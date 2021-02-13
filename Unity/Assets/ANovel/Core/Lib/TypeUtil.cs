using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ANovel.Core
{

	public static class TypeUtil
	{

#if !ANOVEL_FAST_TAG_CHECK_DISABLE
		static readonly string[] s_IgnoreAssemblyPrefix = new string[]
		{
			"Unity.",
			"UnityEngine",
			"UnityEditor",
			"System.",
			"mscorlib",
			"Mono.",
			"SyntaxTree.",
			"netstandard",
			"nunit.",
			"ReportGeneratorMerged",
			"ExCSS.Unity",
		};

#endif

		static Assembly[] s_Assemblies;
		static Assembly[] s_CheckAssemblies;
		static Dictionary<string, Type> s_NameToTypeCatch = new Dictionary<string, Type>();

		static TypeUtil()
		{
			s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
			s_CheckAssemblies = GetCheckAssemblies().ToArray();
		}

		static IEnumerable<Assembly> GetCheckAssemblies()
		{
			foreach (var assembly in s_Assemblies)
			{
#if !ANOVEL_FAST_TAG_CHECK_DISABLE
				var name = assembly.GetName().Name;
				bool ignore = false; ;
				foreach (var prefix in s_IgnoreAssemblyPrefix)
				{
					if (name.IndexOf(prefix) == 0)
					{
						ignore = true;
						break;
					}
				}
				if (ignore || name == "System")
				{
					continue;
				}
#endif
				yield return assembly;
			}
		}

		public static IEnumerable<Type> GetTypes(bool all = false)
		{
			if (all)
			{
				return s_Assemblies.SelectMany(x => x.GetTypes());
			}
			else
			{
				return s_CheckAssemblies.SelectMany(x => x.GetTypes());
			}
		}

		public static Type GetType(string name)
		{
			if (!s_NameToTypeCatch.TryGetValue(name, out var type))
			{
				foreach (var assembly in s_CheckAssemblies)
				{
					type = assembly.GetType(name, throwOnError: false);
					if (type != null)
					{
						s_NameToTypeCatch[name] = type;
						return type;
					}
				}
				foreach (var assembly in s_Assemblies)
				{
					type = assembly.GetType(name, throwOnError: false);
					if (type != null)
					{
						s_NameToTypeCatch[name] = type;
						return type;
					}
				}
				s_NameToTypeCatch[name] = null;
			}
			return type;
		}

		public static string GetTypeName(Type type)
		{
			return type.FullName;
		}

	}

}