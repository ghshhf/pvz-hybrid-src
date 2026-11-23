using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModNetworkService : IDisposable
{
	private sealed record Declaration(int Version, XWModMessageKind Kind, Func<byte[], object> Decode, Action<XWModMessageContext, object> Receive);

	private sealed class PeerState
	{
		internal double Tokens = 60.0;

		internal double Last;

		internal readonly HashSet<string> Seen = new HashSet<string>(StringComparer.Ordinal);
	}

	public const int MaxMessageBytes = 65536;

	private static readonly Dictionary<string, XWModNetworkService> Active = new Dictionary<string, XWModNetworkService>(StringComparer.Ordinal);

	private readonly Dictionary<string, Declaration> _declarations = new Dictionary<string, Declaration>(StringComparer.Ordinal);

	private readonly Dictionary<string, PeerState> _peers = new Dictionary<string, PeerState>(StringComparer.Ordinal);

	private readonly Func<double> _clock;

	private string _session = "";

	private bool _frozen;

	private bool _disposed;

	public string OwnerId { get; }

	internal XWModNetworkService(string owner, Func<double> clock = null)
	{
		OwnerId = XWModSelectionState.Owner(owner);
		if (OwnerId.Length == 0 || Active.ContainsKey(OwnerId))
		{
			throw new InvalidOperationException("Invalid or duplicate network owner.");
		}
		_clock = clock ?? ((Func<double>)(() => (double)Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency));
		Active.Add(OwnerId, this);
	}

	public XWModMessage<T> Declare<T>(string name, int version, XWModMessageKind kind, XWModCodec<T> codec, Action<XWModMessageContext, T> receive)
	{
		Alive();
		if (!_frozen)
		{
			XWModRuntimeSession current = XWModRuntimeSession.Current;
			if (current == null || !current.Frozen)
			{
				if (string.IsNullOrWhiteSpace(name) || name.Length > 128 || name != name.Trim() || name.Contains('\0') || version < 1 || !Enum.IsDefined(kind) || codec == null || receive == null || _declarations.ContainsKey(name))
				{
					throw new ArgumentException("Invalid or duplicate message declaration.");
				}
				_declarations.Add(name, new Declaration(version, kind, delegate(byte[] bytes)
				{
					T val = codec.Decode(bytes.AsMemory());
					if (!codec.Validate(val))
					{
						throw new InvalidOperationException("Message validator rejected payload.");
					}
					return val;
				}, delegate(XWModMessageContext context, object value)
				{
					receive(context, (T)value);
				}));
				return new XWModMessage<T>
				{
					Service = this,
					Name = name,
					Version = version,
					Kind = kind,
					Codec = codec
				};
			}
		}
		throw new InvalidOperationException("Message declarations are frozen.");
	}

	public bool Request<T>(XWModMessage<T> message, T value, out string diagnostic, string requestId = null)
	{
		return Send(message, value, XWModMessageKind.Request, "1", requestId, out diagnostic);
	}

	public bool Notify<T>(XWModMessage<T> message, T value, out string diagnostic, string targetPeerId = "")
	{
		return Send(message, value, XWModMessageKind.Notification, targetPeerId, null, out diagnostic);
	}

	private bool Send<T>(XWModMessage<T> message, T value, XWModMessageKind kind, string target, string requestId, out string diagnostic)
	{
		diagnostic = "";
		try
		{
			Alive();
			MultiPlayerManager manager = MultiPlayerManager.Instance;
			if (message?.Service != this || message.Kind != kind || manager == null || !manager.IsConnect() || manager.CurrentModBattleSessionId.Length == 0 || !manager.IsBattleParticipant(manager.peerId) || (kind == XWModMessageKind.Notification && !manager.isHost) || (kind == XWModMessageKind.Notification && target.Length > 0 && !manager.IsBattleParticipant(target)))
			{
				throw new InvalidOperationException("Message owner, battle membership, or authority rejected.");
			}
			byte[] array;
			using (XWModExtensions.EnterCallback(OwnerId, modifier: true, XWModExtensions.CreateEventContext()))
			{
				if (!message.Codec.Validate(value))
				{
					throw new InvalidOperationException("Message validator rejected payload.");
				}
				array = message.Codec.Encode(value);
			}
			if (array == null || array.Length > 65536)
			{
				throw new InvalidOperationException("Message payload exceeds budget.");
			}
			XWModWireMessage wire = new XWModWireMessage
			{
				Owner = OwnerId,
				Name = message.Name,
				Version = message.Version,
				Session = manager.CurrentModBattleSessionId,
				RequestId = (requestId ?? Guid.NewGuid().ToString("N")),
				Payload = array
			};
			if (!ValidRequestId(wire.RequestId))
			{
				throw new ArgumentException("Invalid request id.");
			}
			string json = JsonSerializer.Serialize(wire, XWModNetworkJsonContext.Default.XWModWireMessage);
			if (Encoding.UTF8.GetByteCount(json) > 65536)
			{
				throw new InvalidOperationException("Encoded message exceeds 64 KiB.");
			}
			XWModCommandResult xWModCommandResult = XWModExtensions.ExecuteCommand(OwnerId, "network.send." + message.Name, delegate
			{
				manager.SendEnvelope(new NetMessageEnvelope
				{
					message_type = ((kind == XWModMessageKind.Request) ? NetMessageType.ModRequest : NetMessageType.ModNotification),
					payload = json
				}, NetDeliveryMode.Reliable, target);
			}, () => manager.CurrentModBattleSessionId == wire.Session && manager.IsBattleParticipant(manager.peerId) && (kind != XWModMessageKind.Notification || manager.isHost));
			diagnostic = xWModCommandResult.Diagnostic;
			return xWModCommandResult.Accepted;
		}
		catch (Exception ex)
		{
			diagnostic = ex.GetBaseException().Message;
			return false;
		}
	}

	internal static void ReceiveAuthenticated(NetMessageContext context)
	{
		MultiPlayerManager manager = MultiPlayerManager.Instance;
		if (manager != null && manager.IsBattleParticipant(manager.peerId))
		{
			Dispatch(context, manager.CurrentModBattleSessionId, manager.isHost, (string peer) => manager.IsBattleParticipant(peer), (string owner) => XWModRuntimeSession.Current?.IsOwnerActive(owner) ?? false);
		}
	}

	internal static bool Dispatch(NetMessageContext context, string session, bool isHost, Func<string, bool> participant, Func<string, bool> ownerActive)
	{
		if (context == null || context.DeliveryMode != NetDeliveryMode.Reliable || string.IsNullOrEmpty(session) || (context.MessageType != NetMessageType.ModRequest && context.MessageType != NetMessageType.ModNotification) || Encoding.UTF8.GetByteCount(context.Payload) > 65536)
		{
			return false;
		}
		string authenticatedSenderPeerId = context.AuthenticatedSenderPeerId;
		if (string.IsNullOrEmpty(authenticatedSenderPeerId) || !participant(authenticatedSenderPeerId) || ((context.MessageType == NetMessageType.ModRequest) ? (!isHost) : (authenticatedSenderPeerId != "1")))
		{
			return false;
		}
		try
		{
			XWModWireMessage xWModWireMessage = JsonSerializer.Deserialize(context.Payload, XWModNetworkJsonContext.Default.XWModWireMessage);
			if (xWModWireMessage == null || xWModWireMessage.Session != session || !ValidRequestId(xWModWireMessage.RequestId) || xWModWireMessage.Owner == null || xWModWireMessage.Name == null || xWModWireMessage.Payload == null || xWModWireMessage.Payload.Length > 65536 || !ownerActive(xWModWireMessage.Owner) || !Active.TryGetValue(xWModWireMessage.Owner, out var value))
			{
				return false;
			}
			return value.Deliver(context.MessageType, authenticatedSenderPeerId, xWModWireMessage);
		}
		catch (JsonException)
		{
			return false;
		}
	}

	private bool Deliver(NetMessageType type, string sender, XWModWireMessage wire)
	{
		if (_disposed || !_declarations.TryGetValue(wire.Name, out var value) || value.Version != wire.Version || value.Kind == XWModMessageKind.Request != (type == NetMessageType.ModRequest))
		{
			return false;
		}
		if (_session != wire.Session)
		{
			_session = wire.Session;
			_peers.Clear();
		}
		if (!_peers.TryGetValue(sender, out var value2))
		{
			if (_peers.Count >= 64)
			{
				return false;
			}
			value2 = (_peers[sender] = new PeerState
			{
				Last = _clock()
			});
		}
		double num = _clock();
		value2.Tokens = Math.Min(60.0, value2.Tokens + Math.Max(0.0, num - value2.Last) * 30.0);
		value2.Last = num;
		if (value2.Tokens < 1.0)
		{
			return false;
		}
		value2.Tokens -= 1.0;
		if (value.Kind == XWModMessageKind.Request && (value2.Seen.Count >= 16384 || !value2.Seen.Add(wire.RequestId)))
		{
			return false;
		}
		using (XWModExtensions.EnterOperation())
		{
			XWModEventContext context = XWModExtensions.CreateEventContext(XWModEventOrigin.Gameplay, (value.Kind != XWModMessageKind.Request) ? XWModAuthority.Local : XWModAuthority.Host);
			object arg;
			try
			{
				using (XWModExtensions.EnterCallback(OwnerId, modifier: true, context))
				{
					arg = value.Decode(wire.Payload);
				}
			}
			catch (Exception)
			{
				return false;
			}
			try
			{
				using (XWModExtensions.EnterCallback(OwnerId, modifier: false, context))
				{
					value.Receive(new XWModMessageContext(sender, wire.RequestId, wire.Session), arg);
					return true;
				}
			}
			catch (Exception error)
			{
				XWModRuntimeSession.Current?.ReportExtensionFault(OwnerId, error);
				return false;
			}
		}
	}

	private static bool ValidRequestId(string id)
	{
		if (id != null)
		{
			int length = id.Length;
			if (length > 0 && length <= 128)
			{
				return !id.Contains('\0');
			}
		}
		return false;
	}

	private void Alive()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("XWModNetworkService");
		}
	}

	internal static void FreezeAll()
	{
		foreach (XWModNetworkService value in Active.Values)
		{
			value._frozen = true;
		}
	}

	internal static IEnumerable<string> GetSchemaSignatures(string owner)
	{
		if (!Active.TryGetValue(XWModSelectionState.Owner(owner), out var value))
		{
			return Array.Empty<string>();
		}
		return (from p in value._declarations.OrderBy<KeyValuePair<string, Declaration>, string>((KeyValuePair<string, Declaration> p) => p.Key, StringComparer.Ordinal)
			select $"message:{p.Key}:{p.Value.Version}:{p.Value.Kind}").ToArray();
	}

	public void Dispose()
	{
		_disposed = true;
		if (Active.GetValueOrDefault(OwnerId) == this)
		{
			Active.Remove(OwnerId);
		}
		_declarations.Clear();
		_peers.Clear();
	}
}
