using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ANovel.TestTools
{

	public static class ScriptDataConverter
	{
		static string s_DataPath = "ANovel/Tests/TestData~";
		static string s_OutputPath = "ANovel/Tests/TestData.cs";

		[MenuItem("ANovel/TestTools/ScriptDataConvert")]
		public static void Convert()
		{
			var dir = Path.Combine(Application.dataPath, s_DataPath);
			Debug.Log("TargetDir:" + dir);

			var sb = new StringBuilder();

			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine();

			sb.AppendLine("namespace ANovel.Core.Tests");
			sb.AppendLine("{");
			sb.AppendLine("\tpublic static class TestData");
			sb.AppendLine("\t{");

			sb.AppendLine("\t\tstatic Dictionary<string, string> s_Dic = new Dictionary<string, string>");
			sb.AppendLine("\t\t{");
			foreach (var file in Directory.GetFiles(dir, "*.anovel"))
			{
				var filename = Path.GetFileName(file);
				var name = Path.GetFileNameWithoutExtension(file);
				sb.AppendLine("\t\t\t{\"" + filename + "\", " + name + "},");
				sb.AppendLine("\t\t\t{\"" + filename.ToLower() + "\", " + name + "},");
				//sb.AppendLine("\t\t\t{\"" + name + "\", " + name + "},");
			}
			sb.AppendLine("\t\t};");

			sb.AppendLine();
			sb.AppendLine("\t\tpublic static string Get(string name) => s_Dic[name];");

			foreach (var file in Directory.GetFiles(dir, "*.anovel"))
			{
				Debug.Log("File:" + file);
				sb.AppendLine();
				var name = Path.GetFileNameWithoutExtension(file);
				sb.AppendLine("\t\t/* file:" + name + ".anovel");
				foreach (var line in File.ReadLines(file))
				{
					sb.AppendLine("\t\t" + line);
				}
				sb.AppendLine("\t\t*/");
				var text = File.ReadAllText(file).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
				sb.AppendLine($"\t\tpublic const string {name} = \"{text}\";");
			}

			sb.AppendLine("");
			sb.AppendLine("\t}");
			sb.Append("}");


			var output = Path.Combine(Application.dataPath, s_OutputPath);
			Debug.Log("Output:" + output + "\n" + sb.ToString());
			File.WriteAllText(output, sb.ToString().Replace("\r\n", "\n"));

		}
	}

}