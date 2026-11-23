using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Godot;

namespace PVZHE.ModEditor.ModSystem;

internal static class XWModSettingsText
{
	internal sealed record Parsed(string Bbcode, string PlainText);

	private sealed class Token
	{
		internal string Raw;

		internal string Name;

		internal string Argument;

		internal bool Closing;

		internal bool Accepted;

		internal int Pair = -1;
	}

	internal static bool TryWebUrl(string value, out string url)
	{
		url = "";
		Uri result = default(Uri);
		bool flag = string.IsNullOrWhiteSpace(value) || value.IndexOfAny(new char[5] { '\r', '\n', '\t', '[', ']' }) >= 0 || !Uri.TryCreate(value, UriKind.Absolute, out result);
		if (!flag)
		{
			string scheme = result.Scheme;
			bool flag2 = ((scheme == "http" || scheme == "https") ? true : false);
			flag = !flag2;
		}
		if (flag || string.IsNullOrWhiteSpace(result.Host))
		{
			return false;
		}
		url = result.AbsoluteUri;
		return true;
	}

	internal static Parsed Parse(string content)
	{
		if (content == null)
		{
			content = "";
		}
		List<Token> list = new List<Token>();
		Stack<int> stack = new Stack<int>();
		int num = 0;
		while (num < content.Length)
		{
			int num2 = content.IndexOf('[', num);
			if (num2 < 0)
			{
				num2 = content.Length;
			}
			int num3;
			if (num2 > num)
			{
				Token token = new Token();
				string text = content;
				num3 = num;
				token.Raw = text.Substring(num3, num2 - num3);
				list.Add(token);
			}
			if (num2 == content.Length)
			{
				break;
			}
			int num4 = content.IndexOf(']', num2 + 1);
			if (num4 < 0)
			{
				Token token2 = new Token();
				string text2 = content;
				num3 = num2;
				token2.Raw = text2.Substring(num3, text2.Length - num3);
				list.Add(token2);
				break;
			}
			string text3 = content;
			num3 = num2 + 1;
			string text4 = text3.Substring(num3, num4 - num3);
			Token token3 = new Token();
			string text5 = content;
			num3 = num2;
			token3.Raw = text5.Substring(num3, num4 + 1 - num3);
			token3.Closing = text4.StartsWith('/');
			Token token4 = token3;
			if (token4.Closing)
			{
				string text2 = text4;
				text4 = text2.Substring(1, text2.Length - 1);
			}
			int num5 = text4.IndexOf('=');
			token4.Name = ((num5 < 0) ? text4 : text4.Substring(0, num5));
			object argument;
			if (num5 >= 0)
			{
				string text2 = text4;
				num3 = num5 + 1;
				argument = text2.Substring(num3, text2.Length - num3);
			}
			else
			{
				argument = null;
			}
			token4.Argument = (string)argument;
			int count = list.Count;
			list.Add(token4);
			bool flag = !token4.Closing;
			if (flag)
			{
				string text2 = token4.Name;
				bool flag2 = ((text2 == "lb" || text2 == "rb") ? true : false);
				flag = flag2;
			}
			int result;
			if (flag && token4.Argument == null)
			{
				token4.Accepted = true;
			}
			else if (token4.Closing && token4.Argument == null && stack.TryPeek(out result) && list[result].Name == token4.Name)
			{
				stack.Pop();
				token4.Pair = result;
				list[result].Pair = count;
				token4.Accepted = (list[result].Accepted = true);
			}
			else if (!token4.Closing && ValidOpening(token4))
			{
				stack.Push(count);
			}
			num = num4 + 1;
		}
		int num6 = -1;
		for (int i = 0; i < list.Count; i++)
		{
			Token token5 = list[i];
			if (token5.Accepted && !token5.Closing && !(token5.Name != "url"))
			{
				if (i < num6)
				{
					token5.Accepted = (list[token5.Pair].Accepted = false);
					continue;
				}
				if (!TryWebUrl(token5.Argument ?? Plain(list, i + 1, token5.Pair), out var url))
				{
					token5.Accepted = (list[token5.Pair].Accepted = false);
					continue;
				}
				token5.Argument = url;
				num6 = token5.Pair;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Token item in list)
		{
			if (!item.Accepted)
			{
				stringBuilder.Append(item.Raw.Replace("[", "[lb]"));
			}
			else if (!item.Closing && item.Name == "url")
			{
				stringBuilder.Append("[url=").Append(item.Argument).Append(']');
			}
			else
			{
				stringBuilder.Append(item.Raw);
			}
		}
		return new Parsed(stringBuilder.ToString(), Plain(list, 0, list.Count));
	}

	private static bool ValidOpening(Token token)
	{
		string name = token.Name;
		if (name != null)
		{
			switch (name.Length)
			{
			case 1:
			{
				char c = name[0];
				if ((uint)c <= 105u)
				{
					if (c != 'b' && c != 'i')
					{
						break;
					}
				}
				else if (c != 'p' && c != 's' && c != 'u')
				{
					break;
				}
				goto IL_0107;
			}
			case 5:
			{
				char c = name[0];
				if (c != 'c')
				{
					if (c != 'r' || !(name == "right"))
					{
						break;
					}
					goto IL_0107;
				}
				if (!(name == "color"))
				{
					break;
				}
				if (token.Argument != null)
				{
					return Color.FromString(token.Argument, new Color(-1f, -1f, -1f)).R >= 0f;
				}
				return false;
			}
			case 6:
				if (!(name == "center"))
				{
					break;
				}
				goto IL_0107;
			case 4:
				if (!(name == "fill"))
				{
					break;
				}
				goto IL_0107;
			case 9:
			{
				if (!(name == "font_size"))
				{
					break;
				}
				if (int.TryParse(token.Argument, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
				{
					if (result > 0)
					{
						return result <= 128;
					}
					return false;
				}
				return false;
			}
			case 3:
				{
					if (!(name == "url"))
					{
						break;
					}
					string url;
					if (token.Argument != null)
					{
						return TryWebUrl(token.Argument, out url);
					}
					return true;
				}
				IL_0107:
				return token.Argument == null;
			}
		}
		return false;
	}

	private static string Plain(List<Token> tokens, int start, int end)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = start; i < end; i++)
		{
			Token token = tokens[i];
			if (!token.Accepted)
			{
				stringBuilder.Append(token.Raw);
				continue;
			}
			if (token.Name == "lb")
			{
				stringBuilder.Append('[');
				continue;
			}
			if (token.Name == "rb")
			{
				stringBuilder.Append(']');
				continue;
			}
			bool flag;
			switch (token.Name)
			{
			case "p":
			case "center":
			case "right":
			case "fill":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag && stringBuilder.Length > 0)
			{
				if (stringBuilder[stringBuilder.Length - 1] != '\n')
				{
					stringBuilder.Append('\n');
				}
			}
		}
		return stringBuilder.ToString();
	}
}
