using ANovel.Core.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ANovel.Core
{
	internal partial class TagEntry
	{
		static readonly BindingFlags s_BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

		static Dictionary<string, TagEntry[]>[] s_Dic;
		static Dictionary<Type, TagFieldEntry[]> s_Fields = new Dictionary<Type, TagFieldEntry[]>();
		static Dictionary<Type, InjectEntry[]> s_NestParams = new Dictionary<Type, InjectEntry[]>();

		static TagEntry()
		{
			s_Dic = new Dictionary<string, TagEntry[]>[(int)LineType.Max];
			var tempDic = new Dictionary<string, List<TagEntry>>[(int)LineType.Max];
			foreach (var entry in GetEntries())
			{
				var type = (int)entry.Type;
				var dic = tempDic[type] ?? (tempDic[type] = new Dictionary<string, List<TagEntry>>());
				if (!dic.TryGetValue(entry.Name, out var list))
				{
					dic[entry.Name] = list = new List<TagEntry>();
				}
				list.Add(entry);
			}
			for (int i = 0; i < tempDic.Length; i++)
			{
				var dic = tempDic[i];
				if (dic == null)
				{
					continue;
				}
				var ret = s_Dic[i] = new Dictionary<string, TagEntry[]>();
				foreach (var kvp in dic)
				{
					ret[kvp.Key] = kvp.Value.OrderByDescending(x => x.m_Attr.Priority).ToArray();
				}
			}
		}

		static IEnumerable<TagEntry> GetEntries()
		{
			foreach (var type in TypeUtil.GetTypes())
			{
				if (!typeof(Tag).IsAssignableFrom(type))
				{
					continue;
				}
				foreach (var attr in type.GetCustomAttributes(typeof(TagNameAttribute), inherit: false))
				{
					yield return new TagEntry(type, attr as TagNameAttribute);
				}
			}
		}

		public static bool TryGet(in LineData data, string name, List<string> symbols, out TagEntry ret)
		{
			var dic = s_Dic[(int)data.Type];
			if (dic != null && dic.TryGetValue(name, out var entries))
			{
				foreach (var entry in entries)
				{
					if (entry.IsDefineSymbol(symbols))
					{
						ret = entry;
						return true;
					}
				}
			}
			else
			{
				throw new LineDataException(in data, $"not found {data.Type} TagName : {name}");
			}
			ret = null;
			return false;
		}

		internal static TagFieldEntry[] GetFields(Type type)
		{
			if (!s_Fields.TryGetValue(type, out var ret))
			{
				s_Fields[type] = ret = GetFieldsImpl(type);
			}
			return ret;
		}

		static TagFieldEntry[] GetFieldsImpl(Type type)
		{
			using (ListPool<TagFieldEntry>.Use(out var list))
			{
				foreach (var info in type.GetFields(s_BindingFlags))
				{
					if (!info.IsDefined(typeof(ArgumentAttribute), inherit: false))
					{
						continue;
					}
					foreach (var attr in info.GetCustomAttributes(typeof(ArgumentAttribute), false))
					{
						list.Add(new TagFieldEntry(info, attr as ArgumentAttribute));
					}
				}
				foreach (var info in type.GetProperties(s_BindingFlags))
				{
					if (!info.IsDefined(typeof(ArgumentAttribute), inherit: false))
					{
						continue;
					}
					foreach (var attr in info.GetCustomAttributes(typeof(ArgumentAttribute), false))
					{
						list.Add(new TagFieldEntry(info, attr as ArgumentAttribute));
					}
				}
				return list.ToArray();
			}
		}

		internal static InjectEntry[] GetInjects(Type type)
		{
			if (!s_NestParams.TryGetValue(type, out var ret))
			{
				s_NestParams[type] = ret = GetInjectInjectsImpl(type);
			}
			return ret;
		}

		static InjectEntry[] GetInjectInjectsImpl(Type type)
		{
			using (ListPool<InjectEntry>.Use(out var list))
			{
				foreach (var info in type.GetFields(s_BindingFlags))
				{
					if (!info.IsDefined(typeof(InjectArgumentAttribute), inherit: false))
					{
						continue;
					}
					foreach (var attr in info.GetCustomAttributes(typeof(InjectArgumentAttribute), inherit: false))
					{
						list.Add(new InjectEntry(info, attr as InjectArgumentAttribute));
					}
				}
				foreach (var info in type.GetProperties(s_BindingFlags))
				{
					if (!info.IsDefined(typeof(InjectArgumentAttribute), inherit: false))
					{
						continue;
					}
					foreach (var attr in info.GetCustomAttributes(typeof(InjectArgumentAttribute), inherit: false))
					{
						list.Add(new InjectEntry(info, attr as InjectArgumentAttribute));
					}
				}
				return list.ToArray();
			}
		}

		internal static IEnumerable<TagDefine> GetDefines(List<string> symbols)
		{
			foreach (var tag in s_Dic.Where(x => x != null).SelectMany(x => x.Values).SelectMany(x => x))
			{
				if (tag.IsDefineSymbol(symbols))
				{
					yield return tag.CreateDefine();
				}
			}
		}

	}
}