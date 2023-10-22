using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ANovel.Core
{
	public class LocalizeDefineConverter
	{
		public static void ApplyDefault(string source, string[] langs)
		{
			var text = File.ReadAllText(source);
			var converter = new LocalizeDefineConverter(text);
			converter.ApplyLangData(Path.ChangeExtension(source, "lang.anovellang"), langs);
			converter.ApplySource(source);
		}

		string m_RawText;
		string m_Text;
		Dictionary<string, string> m_KeyAndValues;

		public string RawText => m_RawText;

		public string Text => m_Text;

		public IReadOnlyDictionary<string, string> KeyAndValues => m_KeyAndValues;


		public LocalizeDefineConverter(string text)
		{
			m_RawText = text;
			m_Text = Insertkey(text);
			m_KeyAndValues = GetKeyAndValues(m_Text);
		}

		public void ApplySource(string sourcePath)
		{
			File.WriteAllText(sourcePath, m_Text);
		}

		public void ApplyLangData(string langPath, string[] langs)
		{
			if (File.Exists(langPath))
			{
				var langData = File.ReadAllText(langPath);
				LocalizeData current;
				if (langData.StartsWith("{"))
				{
					current = JsonUtility.FromJson<LocalizeData>(langData);
				}
				else
				{
					current = LocalizeData.Create(langData, tab: false);
				}

				LocalizeData data = new();
				data.Keys = KeyAndValues.Keys.ToArray();

				foreach (var oldLang in current.List)
				{
					var newLang = new LanguageData();
					newLang.Default = oldLang.Default;
					newLang.Language = oldLang.Language;

					List<string> lines = new();
					foreach (var key in data.Keys)
					{
						var index = Array.IndexOf(current.Keys, key);
						if (index >= 0)
						{
							lines.Add(oldLang.Lines[index]);
						}
						else
						{
							lines.Add(KeyAndValues[key]);
						}
					}
					newLang.Lines = lines.ToArray();
					data.List.Add(newLang);

				}
				var json = JsonUtility.ToJson(data, true);
				File.WriteAllText(langPath, json);
			}
			else
			{
				LocalizeData data = new();
				data.Keys = KeyAndValues.Keys.ToArray();

				foreach (var langName in langs)
				{
					LanguageData lang = new();
					lang.Default = data.List.Count == 0;
					lang.Language = langName;
					if (lang.Default)
					{
						lang.Lines = KeyAndValues.Values.ToArray();
					}
					else
					{
						lang.Lines = KeyAndValues.Values.Select(x => "").ToArray();
					}
					data.List.Add(lang);
				}

				var json = JsonUtility.ToJson(data, true);
				File.WriteAllText(langPath, json);
			}
		}


		string Insertkey(string text)
		{
			System.Random random = new();
			byte[] buffer = new byte[6];

			var keys = GetKeys(text);

			var reader = new LineReader("", text);
			reader.InsertLastStop = false;
			LineData data = default;

			StringBuilder builder = new();
			List<string> queue = new();
			bool hasKey = false;

			while (reader.TryRead(ref data, skipNoneAndComment: false))
			{
				if (data.Type == LineType.Text)
				{
					queue.Add(data.Line);
					ReadText();
					foreach (var item in queue)
					{
						builder.Append(item).Append("\n");
					}
					queue.Clear();
					hasKey = false;
				}
				else
				{
					builder.Append(data.Line).Append("\n");
					if (data.Type == LineType.SystemCommand)
					{
						if (data.ReadName(out _) == "localize_key")
						{
							hasKey = true;
						}
					}
				}
			}

			builder.Length--;

			return builder.ToString();

			void ReadText()
			{
				bool extension = queue[0].StartsWith(Token.TextBlockScope, StringComparison.Ordinal);
				LineData data = default;
				while (reader.TryRead(ref data, skipNoneAndComment: false))
				{
					if (data.Type == LineType.Text)
					{
						queue.Add(data.Line);
						if (extension && data.Line.StartsWith(Token.TextBlockScope, StringComparison.Ordinal))
						{
							if (!hasKey) InsertKey();
							return;
						}
					}
					else if (extension)
					{
						queue.Add(data.Line);
					}
					else
					{
						if (!hasKey) InsertKey();
						queue.Add(data.Line);
						return;
					}
				}

				if (!hasKey) InsertKey();

				void InsertKey()
				{
					while (true)
					{
						random.NextBytes(buffer);
						var key = string.Join("", buffer.Select(x => x.ToString("x2")));
						if (keys.Add(key))
						{
							queue.Insert(0, $"&localize_key key=\"{key}\"");
							return;
						}
					}
				}
			}
		}

		HashSet<string> GetKeys(string text)
		{
			HashSet<string> keys = new();
			var reader = new LineReader("", text);
			TagParam param = new();
			List<IParamConverter> converters = new();
			LineData data = default;
			while (reader.TryRead(ref data))
			{
				if (data.Type == LineType.SystemCommand)
				{
					if (data.ReadName(out _) == "localize_key")
					{
						param.Set(data, converters);
						var key = param.GetValue("key");
						if (!string.IsNullOrEmpty(key))
						{
							keys.Add(key);
						}
					}
				}
			}
			return keys;
		}

		Dictionary<string, string> GetKeyAndValues(string text)
		{
			Dictionary<string, string> ret = new();
			var reader = new LineReader("", text);
			TagParam param = new();
			List<IParamConverter> converters = new();
			LineData data = default;

			string key = null;
			string value = null;
			bool extension = false;
			while (reader.TryRead(ref data, skipNoneAndComment: false))
			{
				if (data.Type == LineType.Text || extension)
				{
					if (extension && data.Line.StartsWith(Token.TextBlockScope, StringComparison.Ordinal))
					{
						ret.Add(key, value);
						extension = false;
						key = null;
						value = null;
						continue;
					}
					if (value == null)
					{
						if (data.Line.StartsWith(Token.TextBlockScope, StringComparison.Ordinal))
						{
							extension = true;
							continue;
						}
						value = data.Line;
					}
					else
					{
						value += "\n" + data.Line;
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(key) && value != null)
					{
						ret.Add(key, value);
						extension = false;
						key = null;
						value = null;
					}
					if (data.Type == LineType.SystemCommand)
					{
						if (data.ReadName(out _) == "localize_key")
						{
							param.Set(data, converters);
							key = param.GetValue("key");
						}
					}
				}
			}
			return ret;
		}


	}
}