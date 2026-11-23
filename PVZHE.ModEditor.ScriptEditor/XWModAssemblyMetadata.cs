using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace PVZHE.ModEditor.ScriptEditor;

public sealed class XWModAssemblyMetadata
{
	public string Path { get; }

	public string Identity { get; }

	public string SimpleName { get; }

	public string Sha256 { get; }

	public IReadOnlyList<string> References { get; }

	private XWModAssemblyMetadata(string path, AssemblyName identity, string hash, string[] references)
	{
		Path = path;
		Identity = identity.FullName;
		SimpleName = identity.Name;
		Sha256 = hash;
		References = Array.AsReadOnly(references);
	}

	public static XWModAssemblyMetadata Read(string path)
	{
		using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Read(stream, System.IO.Path.GetFullPath(path));
	}

	public static XWModAssemblyMetadata Read(Stream stream, string logicalPath)
	{
		if (!stream.CanSeek)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				memoryStream.Position = 0L;
				return Read(memoryStream, logicalPath);
			}
		}
		stream.Position = 0L;
		string hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
		stream.Position = 0L;
		using PEReader pEReader = new PEReader(stream, PEStreamOptions.LeaveOpen);
		if (!pEReader.HasMetadata)
		{
			throw new BadImageFormatException("Mod DLL 不是托管程序集：" + logicalPath);
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		if (!metadataReader.IsAssembly)
		{
			throw new BadImageFormatException("Mod DLL 缺少程序集清单：" + logicalPath);
		}
		AssemblyDefinition assemblyDefinition = metadataReader.GetAssemblyDefinition();
		AssemblyName identity = CreateName(metadataReader.GetString(assemblyDefinition.Name), assemblyDefinition.Version, metadataReader.GetString(assemblyDefinition.Culture), metadataReader.GetBlobBytes(assemblyDefinition.PublicKey), fullKey: true);
		List<string> list = new List<string>();
		foreach (AssemblyReferenceHandle assemblyReference2 in metadataReader.AssemblyReferences)
		{
			AssemblyReference assemblyReference = metadataReader.GetAssemblyReference(assemblyReference2);
			list.Add(CreateName(metadataReader.GetString(assemblyReference.Name), assemblyReference.Version, metadataReader.GetString(assemblyReference.Culture), metadataReader.GetBlobBytes(assemblyReference.PublicKeyOrToken), (assemblyReference.Flags & AssemblyFlags.PublicKey) != 0).FullName);
		}
		return new XWModAssemblyMetadata(logicalPath, identity, hash, list.OrderBy<string, string>((string value) => value, StringComparer.Ordinal).ToArray());
	}

	private static AssemblyName CreateName(string name, Version version, string culture, byte[] key, bool fullKey)
	{
		AssemblyName assemblyName = new AssemblyName
		{
			Name = name,
			Version = version,
			CultureName = culture
		};
		if (key.Length != 0)
		{
			if (fullKey)
			{
				assemblyName.SetPublicKey(key);
			}
			else
			{
				assemblyName.SetPublicKeyToken(key);
			}
		}
		else
		{
			assemblyName.SetPublicKeyToken(Array.Empty<byte>());
		}
		return assemblyName;
	}

	public static bool SameIdentity(AssemblyName requested, AssemblyName candidate)
	{
		if (string.Equals(requested.Name, candidate.Name, StringComparison.OrdinalIgnoreCase) && (requested.Version == null || requested.Version == candidate.Version) && string.Equals(requested.CultureName ?? "", candidate.CultureName ?? "", StringComparison.OrdinalIgnoreCase))
		{
			return (requested.GetPublicKeyToken() ?? Array.Empty<byte>()).SequenceEqual(candidate.GetPublicKeyToken() ?? Array.Empty<byte>());
		}
		return false;
	}

	public static IReadOnlyDictionary<string, XWModAssemblyMetadata> BuildIndex(string mainPath, IEnumerable<string> roots)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { System.IO.Path.GetFullPath(mainPath) };
		foreach (string item in roots ?? Array.Empty<string>())
		{
			if (string.IsNullOrWhiteSpace(item) || !Directory.Exists(item))
			{
				continue;
			}
			foreach (string item2 in from path in Directory.EnumerateFiles(item, "*", SearchOption.TopDirectoryOnly)
				where string.Equals(System.IO.Path.GetExtension(path), ".dll", StringComparison.OrdinalIgnoreCase)
				select path)
			{
				hashSet.Add(System.IO.Path.GetFullPath(item2));
			}
		}
		Dictionary<string, XWModAssemblyMetadata> dictionary = new Dictionary<string, XWModAssemblyMetadata>(StringComparer.OrdinalIgnoreCase);
		foreach (string item3 in hashSet.OrderBy<string, string>((string result) => result, StringComparer.Ordinal))
		{
			XWModAssemblyMetadata xWModAssemblyMetadata = Read(item3);
			if (dictionary.TryGetValue(xWModAssemblyMetadata.SimpleName, out var value))
			{
				if (!string.Equals(value.Identity, xWModAssemblyMetadata.Identity, StringComparison.Ordinal) || !string.Equals(value.Sha256, xWModAssemblyMetadata.Sha256, StringComparison.Ordinal))
				{
					throw new InvalidDataException($"同一 Mod 内存在不同的同名程序集 '{xWModAssemblyMetadata.SimpleName}'：{value.Path} / {item3}");
				}
			}
			else
			{
				dictionary.Add(xWModAssemblyMetadata.SimpleName, xWModAssemblyMetadata);
			}
		}
		return new ReadOnlyDictionary<string, XWModAssemblyMetadata>(dictionary);
	}

	public static IReadOnlyDictionary<string, XWModAssemblyMetadata> ReadPackage(string path)
	{
		using ZipArchive zipArchive = ZipFile.OpenRead(path);
		Dictionary<string, XWModAssemblyMetadata> dictionary = new Dictionary<string, XWModAssemblyMetadata>(StringComparer.OrdinalIgnoreCase);
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			string text = entry.FullName.Replace('\\', '/');
			if (!(text == "Runtime/ModAssembly.dll") && (!text.StartsWith("Runtime/Dependencies/", StringComparison.Ordinal) || !text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}
			if (entry.Length > 67108864)
			{
				throw new InvalidDataException("程序集超过包内大小限制：" + text);
			}
			using Stream stream = entry.Open();
			XWModAssemblyMetadata xWModAssemblyMetadata = Read(stream, text);
			if (dictionary.TryGetValue(xWModAssemblyMetadata.SimpleName, out var value) && (value.Identity != xWModAssemblyMetadata.Identity || value.Sha256 != xWModAssemblyMetadata.Sha256))
			{
				throw new InvalidDataException("同一包中存在不同的同名程序集：" + xWModAssemblyMetadata.SimpleName);
			}
			if (value == null || text == "Runtime/ModAssembly.dll")
			{
				dictionary[xWModAssemblyMetadata.SimpleName] = xWModAssemblyMetadata;
			}
		}
		return new ReadOnlyDictionary<string, XWModAssemblyMetadata>(dictionary);
	}
}
