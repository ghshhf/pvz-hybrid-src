using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModDataService : IDisposable
{
	public const int MaxValueBytes = 65536;

	public const int MaxBattleBytes = 1048576;

	private static readonly System.Collections.Generic.Dictionary<string, XWModDataService> Active = new System.Collections.Generic.Dictionary<string, XWModDataService>(StringComparer.Ordinal);

	private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>> Battle = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>>(StringComparer.Ordinal);

	private static Guid _battleGeneration;

	private static ulong _battleRevision;

	private readonly System.Collections.Generic.Dictionary<string, int> _schemas = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal);

	private readonly string _root;

	private readonly Func<string> _player;

	private bool _frozen;

	private bool _disposed;

	public string OwnerId { get; }

	internal static (Guid BattleGeneration, ulong Revision) BattleStamp
	{
		get
		{
			if (XWModExtensions.SessionId == Guid.Empty)
			{
				return (BattleGeneration: Guid.Empty, Revision: 0uL);
			}
			EnsureBattle();
			return (BattleGeneration: _battleGeneration, Revision: _battleRevision);
		}
	}

	internal XWModDataService(string owner, string root = null, Func<string> player = null)
	{
		OwnerId = XWModSelectionState.Owner(owner);
		if (OwnerId.Length == 0)
		{
			throw new ArgumentException("Missing data owner.");
		}
		_root = root ?? ProjectSettings.GlobalizePath("user://ModData");
		_player = player ?? ((Func<string>)(() => GameSaveManager.Instance?.GetUserCurrent() ?? ""));
		if (Active.ContainsKey(OwnerId))
		{
			throw new InvalidOperationException("Data owner already active.");
		}
		Active.Add(OwnerId, this);
	}

	public XWModDataKey<T> Declare<T>(string name, int version, XWModCodec<T> codec, Func<int, ReadOnlyMemory<byte>, byte[]> migrate = null)
	{
		Alive();
		if (!_frozen)
		{
			XWModRuntimeSession current = XWModRuntimeSession.Current;
			if (current == null || !current.Frozen)
			{
				if (!ValidKey(name) || version <= 0 || codec == null || !_schemas.TryAdd(name, version))
				{
					throw new ArgumentException("Invalid or duplicate data declaration.");
				}
				return new XWModDataKey<T>
				{
					Service = this,
					Name = name,
					Version = version,
					Codec = codec,
					Migrate = migrate
				};
			}
		}
		throw new InvalidOperationException("Data declarations are frozen.");
	}

	public bool TryRead<T>(XWModDataKey<T> key, XWModDataScope scope, out T value, out string diagnostic)
	{
		value = default(T);
		diagnostic = "";
		string path = null;
		try
		{
			Check(key);
			XWModDataEnvelope value3;
			if (scope == XWModDataScope.Battle)
			{
				EnsureBattle();
				if (!Battle.TryGetValue(OwnerId, out var value2) || !value2.TryGetValue(key.Name, out value3))
				{
					return false;
				}
			}
			else
			{
				path = StoragePath(scope, key.Name);
				if (!File.Exists(path))
				{
					return false;
				}
				value3 = ReadEnvelope(File.ReadAllBytes(path));
			}
			if (value3.Version > key.Version)
			{
				diagnostic = "Data was written by a newer schema; preserved unchanged.";
				return false;
			}
			byte[] array = value3.Payload;
			bool flag = value3.Version != key.Version;
			if (flag)
			{
				if (key.Migrate == null)
				{
					diagnostic = "Data migration is required; preserved unchanged.";
					return false;
				}
				using (XWModExtensions.EnterCallback(OwnerId, modifier: true, XWModExtensions.CreateEventContext()))
				{
					array = key.Migrate(value3.Version, array.AsMemory());
				}
			}
			if (array == null || array.Length > 65536)
			{
				throw new InvalidDataException("Data payload exceeds its budget.");
			}
			using (XWModExtensions.EnterCallback(OwnerId, modifier: true, XWModExtensions.CreateEventContext()))
			{
				value = key.Codec.Decode(array.AsMemory());
				if (!key.Codec.Validate(value))
				{
					throw new InvalidDataException("Data validator rejected the value.");
				}
			}
			if (flag && scope != XWModDataScope.Battle && XWModExtensions.TryMutableCallback(OwnerId, out var _))
			{
				XWModDataEnvelope value4 = Encode(key, value);
				Backup(path, "migration");
				Write(path, value4);
			}
			return true;
		}
		catch (Exception ex)
		{
			value = default(T);
			diagnostic = ex.GetBaseException().Message;
			return false;
		}
	}

	public bool TryWrite<T>(XWModDataKey<T> key, XWModDataScope scope, T value, out string diagnostic)
	{
		diagnostic = "";
		try
		{
			Check(key);
			if (!XWModExtensions.TryMutableCallback(OwnerId, out diagnostic))
			{
				return false;
			}
			XWModDataEnvelope envelope = Encode(key, value);
			if (scope == XWModDataScope.Battle)
			{
				EnsureBattle();
				XWModCommandResult xWModCommandResult = XWModExtensions.ExecuteCommand(OwnerId, "data.battle." + key.Name, delegate
				{
					SetBattle(key.Name, envelope);
				}, () => !Global.IsMultiplayerMode || (MultiPlayerManager.Instance?.isHost ?? false));
				diagnostic = xWModCommandResult.Diagnostic;
				return xWModCommandResult.Accepted;
			}
			string path = StoragePath(scope, key.Name);
			if (File.Exists(path))
			{
				try
				{
					if (ReadEnvelope(File.ReadAllBytes(path)).Version > key.Version)
					{
						diagnostic = "Newer data schema cannot be overwritten.";
						return false;
					}
				}
				catch (Exception)
				{
					Backup(path, "corrupt");
				}
			}
			Write(path, envelope);
			return true;
		}
		catch (Exception ex2)
		{
			diagnostic = ex2.GetBaseException().Message;
			return false;
		}
	}

	public Task<XWModCommandResult<bool>> WriteBattleAsync<T>(XWModDataKey<T> key, T value)
	{
		try
		{
			Check(key);
			XWModExtensions.DemandMutableCallback(OwnerId);
			XWModDataEnvelope envelope = Encode(key, value);
			EnsureBattle();
			return XWModExtensions.ExecuteCommand(OwnerId, "data.battle." + key.Name, delegate
			{
				SetBattle(key.Name, envelope);
				return true;
			}, () => !_disposed && (!Global.IsMultiplayerMode || (MultiPlayerManager.Instance?.isHost ?? false)));
		}
		catch (Exception ex)
		{
			return Task.FromResult(new XWModCommandResult<bool>(XWModCommandStatus.Rejected, Value: false, ex.GetBaseException().Message));
		}
	}

	internal string StoragePath(XWModDataScope scope, string key)
	{
		string value = ((scope == XWModDataScope.Player) ? _player() : "");
		string text;
		if (scope != XWModDataScope.Local)
		{
			if (scope != XWModDataScope.Player || string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidOperationException("No current player or invalid disk data scope.");
			}
			text = Path.Combine("players", Digest(value));
		}
		else
		{
			text = "local";
		}
		string path = text;
		return Path.Combine(_root, Digest(OwnerId), path, Digest(key) + ".json");
	}

	private XWModDataEnvelope Encode<T>(XWModDataKey<T> key, T value)
	{
		using (XWModExtensions.EnterCallback(OwnerId, modifier: true, XWModExtensions.CreateEventContext()))
		{
			if (!key.Codec.Validate(value))
			{
				throw new InvalidDataException("Data validator rejected the value.");
			}
			byte[] array = key.Codec.Encode(value);
			if (array == null || array.Length > 65536)
			{
				throw new InvalidDataException("Data payload exceeds its budget.");
			}
			return new XWModDataEnvelope
			{
				Version = key.Version,
				Payload = array.ToArray()
			};
		}
	}

	private void Check<T>(XWModDataKey<T> key)
	{
		Alive();
		if (key?.Service != this)
		{
			throw new InvalidOperationException("Data handle belongs to another owner.");
		}
	}

	private void Alive()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("XWModDataService");
		}
	}

	private static bool ValidKey(string name)
	{
		if (!string.IsNullOrWhiteSpace(name) && name.Length <= 128 && name == name.Trim())
		{
			return !name.Contains('\0');
		}
		return false;
	}

	private static string Digest(string value)
	{
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
	}

	private static XWModDataEnvelope ReadEnvelope(byte[] bytes)
	{
		if (bytes.Length > 131072)
		{
			throw new InvalidDataException("Stored data exceeds its budget.");
		}
		XWModDataEnvelope xWModDataEnvelope = JsonSerializer.Deserialize(bytes, XWModDataJsonContext.Default.XWModDataEnvelope);
		if (xWModDataEnvelope == null || xWModDataEnvelope.Version <= 0 || xWModDataEnvelope.Payload == null || xWModDataEnvelope.Payload.Length > 65536)
		{
			throw new InvalidDataException("Invalid stored data envelope.");
		}
		return xWModDataEnvelope;
	}

	private static void Write(string path, XWModDataEnvelope value)
	{
		XWModStateFile.Write(path, JsonSerializer.SerializeToUtf8Bytes(value, XWModDataJsonContext.Default.XWModDataEnvelope));
	}

	private static void Backup(string path, string reason)
	{
		byte[] array = File.ReadAllBytes(path);
		string path2 = path + "." + reason + "." + Convert.ToHexString(SHA256.HashData(array)) + ".bak";
		if (!File.Exists(path2))
		{
			XWModStateFile.Write(path2, array, overwrite: false);
		}
	}

	private static void EnsureBattle()
	{
		if (_battleGeneration != XWModExtensions.SessionId)
		{
			_battleGeneration = XWModExtensions.SessionId;
			Battle.Clear();
			_battleRevision = 0uL;
		}
		if (_battleGeneration == Guid.Empty)
		{
			throw new InvalidOperationException("No active battle data scope.");
		}
	}

	private void SetBattle(string key, XWModDataEnvelope envelope)
	{
		EnsureBattle();
		if (Battle.TryGetValue(OwnerId, out var value) && value.TryGetValue(key, out var value2) && value2.Version == envelope.Version && value2.Payload.AsSpan().SequenceEqual(envelope.Payload))
		{
			return;
		}
		if ((!Battle.TryGetValue(OwnerId, out var value3) || !value3.ContainsKey(key)) && Battle.Values.Sum((System.Collections.Generic.Dictionary<string, XWModDataEnvelope> values) => values.Count) >= 4096)
		{
			throw new InvalidDataException("Battle data entry budget exceeded.");
		}
		int num = Battle.Values.Sum((System.Collections.Generic.Dictionary<string, XWModDataEnvelope> values) => values.Values.Sum((XWModDataEnvelope xWModDataEnvelope) => xWModDataEnvelope.Payload.Length));
		if (Battle.TryGetValue(OwnerId, out var value4) && value4.TryGetValue(key, out var value5))
		{
			num -= value5.Payload.Length;
		}
		if (num + envelope.Payload.Length > 1048576)
		{
			throw new InvalidDataException("Battle Mod data exceeds its aggregate budget.");
		}
		if (!Battle.TryGetValue(OwnerId, out var value6))
		{
			value6 = (Battle[OwnerId] = new System.Collections.Generic.Dictionary<string, XWModDataEnvelope>(StringComparer.Ordinal));
		}
		value6[key] = envelope;
		_battleRevision++;
	}

	internal static Dictionary CaptureBattle()
	{
		Dictionary dictionary = new Dictionary();
		if (XWModExtensions.SessionId == Guid.Empty)
		{
			return dictionary;
		}
		EnsureBattle();
		foreach (KeyValuePair<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>> item in Battle.OrderBy<KeyValuePair<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>>, string>((KeyValuePair<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>> pair) => pair.Key, StringComparer.Ordinal))
		{
			Dictionary dictionary2 = new Dictionary();
			foreach (KeyValuePair<string, XWModDataEnvelope> item2 in item.Value.OrderBy<KeyValuePair<string, XWModDataEnvelope>, string>((KeyValuePair<string, XWModDataEnvelope> pair) => pair.Key, StringComparer.Ordinal))
			{
				dictionary2[item2.Key] = JsonSerializer.Serialize(item2.Value, XWModDataJsonContext.Default.XWModDataEnvelope);
			}
			dictionary[item.Key] = dictionary2;
		}
		return dictionary;
	}

	internal static bool RestoreBattle(Dictionary snapshot, out string diagnostic)
	{
		diagnostic = "";
		try
		{
			EnsureBattle();
			System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>> dictionary = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>>(StringComparer.Ordinal);
			int num = 0;
			int num2 = 0;
			if (snapshot != null && snapshot.Count > 1024)
			{
				throw new InvalidDataException("Battle data owner budget exceeded.");
			}
			foreach (KeyValuePair<Variant, Variant> item in snapshot ?? new Dictionary())
			{
				string text = XWModSelectionState.Owner(item.Key.AsString());
				if (text.Length == 0 || item.Value.VariantType != Variant.Type.Dictionary || dictionary.ContainsKey(text))
				{
					throw new InvalidDataException("Invalid battle data owner.");
				}
				System.Collections.Generic.Dictionary<string, XWModDataEnvelope> dictionary2 = new System.Collections.Generic.Dictionary<string, XWModDataEnvelope>(StringComparer.Ordinal);
				foreach (KeyValuePair<Variant, Variant> item2 in item.Value.AsGodotDictionary())
				{
					if (++num2 > 4096)
					{
						throw new InvalidDataException("Battle data entry budget exceeded.");
					}
					string text2 = item2.Key.AsString();
					if (!ValidKey(text2) || item2.Value.VariantType != Variant.Type.String)
					{
						throw new InvalidDataException("Invalid battle data key.");
					}
					XWModDataEnvelope xWModDataEnvelope = ReadEnvelope(Encoding.UTF8.GetBytes(item2.Value.AsString()));
					num += xWModDataEnvelope.Payload.Length;
					if (num > 1048576)
					{
						throw new InvalidDataException("Battle data exceeds aggregate budget.");
					}
					dictionary2.Add(text2, xWModDataEnvelope);
				}
				dictionary.Add(text, dictionary2);
			}
			Battle.Clear();
			foreach (KeyValuePair<string, System.Collections.Generic.Dictionary<string, XWModDataEnvelope>> item3 in dictionary)
			{
				Battle.Add(item3.Key, item3.Value);
			}
			_battleRevision++;
			return true;
		}
		catch (Exception ex)
		{
			diagnostic = ex.GetBaseException().Message;
			return false;
		}
	}

	internal static IEnumerable<string> GetSchemaSignatures(string owner)
	{
		if (!Active.TryGetValue(XWModSelectionState.Owner(owner), out var value))
		{
			return System.Array.Empty<string>();
		}
		return (from pair in value._schemas.OrderBy<KeyValuePair<string, int>, string>((KeyValuePair<string, int> pair) => pair.Key, StringComparer.Ordinal)
			select $"data:{pair.Key}:{pair.Value}").ToArray();
	}

	internal static void FreezeAll()
	{
		foreach (XWModDataService value in Active.Values)
		{
			value._frozen = true;
		}
	}

	public void Dispose()
	{
		_disposed = true;
		if (Active.GetValueOrDefault(OwnerId) == this)
		{
			Active.Remove(OwnerId);
		}
		_schemas.Clear();
	}
}
