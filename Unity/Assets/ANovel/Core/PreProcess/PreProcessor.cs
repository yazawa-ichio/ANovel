using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ANovel.Core
{

	public class PreProcessor
	{
		public class Result
		{
			public readonly List<string> Symbols;
			public readonly MacroDefine MacroDefine;
			public readonly List<MacroDefine> DependMacros;
			public readonly List<IParamConverter> Converters;
			public readonly List<SkipScope> SkipScopes;
			public Result(string[] symbols)
			{
				Symbols = new List<string>(symbols);
				DependMacros = new List<MacroDefine>();
				Converters = new List<IParamConverter>();
				MacroDefine = new MacroDefine(Converters, DependMacros);
				SkipScopes = new List<SkipScope>();
			}
		}

		class Entry
		{
			public TagProvider Provider;
			public LineReader Reader;
			public Queue<LineData> Temp = new Queue<LineData>();
			public Result Result;

			public Entry(string path, List<string> symbols, string text, Result result)
			{
				Provider = new TagProvider(symbols);
				Reader = new LineReader(path, text);
				Result = result;
			}

		}

		IScenarioLoader m_Loader;
		Dictionary<string, Result> m_Cache = new Dictionary<string, Result>();
		string[] m_Symbols;
		List<Tag> m_Tags = new List<Tag>();
		Stack<Entry> m_Stack = new Stack<Entry>();

		Entry Current => m_Stack.Peek();

		public PreProcessor(IScenarioLoader loader, string[] symbols)
		{
			m_Loader = loader;
			m_Symbols = symbols ?? Array.Empty<string>();
		}

		public Task<Result> Run(string path, string text)
		{
			try
			{
				return RunImpl(path, text);
			}
			finally
			{
				m_Stack.Clear();
			}
		}

		async Task<Result> RunImpl(string path, string text)
		{
			if (m_Cache.TryGetValue(path, out var result))
			{
				return result;
			}
			result = m_Cache[path] = new Result(m_Symbols);
			if (text == null)
			{
				text = await m_Loader.Load(path);
			}
			var entry = new Entry(path, result.Symbols, text, result);
			m_Stack.Push(entry);
			LineData data = default;
			while (TryReadData(ref data))
			{
				if (data.Type != LineType.PreProcess)
				{
					continue;
				}
				var process = ReadTag(in data);
				CheckStartIfScope(in data, process);
				if (process.GetType() == typeof(IfScope))
				{
					ProcessIfScope(data, (IfScope)process);
					continue;
				}
				if (process is IImportPreProcess import)
				{
					import.Import(await RunImpl(import.Path, null));
				}
				if (process is MacroScope macro)
				{
					ProcessMacroScope(data, macro);
				}
				process.Result(result);
			}
			m_Stack.Pop();
			return result;
		}


		bool TryReadData(ref LineData data)
		{
			if (Current.Temp.Count > 0)
			{
				data = Current.Temp.Dequeue();
				return true;
			}
			return Current.Reader.TryRead(ref data);
		}

		PreProcess ReadTag(in LineData data)
		{
			m_Tags.Clear();
			Current.Provider.Provide(in data, m_Tags);
			return (PreProcess)m_Tags[0];
		}

		void CheckStartIfScope(in LineData data, PreProcess process)
		{
			if (process is ElseIfScope || process is EndIfScope || process is ElseScope)
			{
				throw new LineDataException(in data, $"not start if scope");
			}
		}

		void CheckElseStartScope(in LineData data, PreProcess process)
		{
			if (process is ElseIfScope || process is ElseScope)
			{
				throw new LineDataException(in data, $"not start if scope");
			}
		}


		void ProcessIfScope(LineData cur, IfScope scope)
		{
			var reader = Current.Reader;
			var symbols = Current.Result.Symbols;
			var temp = Current.Temp;

			bool elseScope = false;

			// 無効なスコープ
			if (!scope.IsCondition(symbols))
			{
				LineData data = default;
				int skipStart = cur.Index;
				bool condition = false;
				int ifCount = 1;
				while (reader.TryRead(ref data))
				{
					if (data.Type != LineType.PreProcess)
					{
						continue;
					}
					var process = ReadTag(in data);
					if (process.GetType() == typeof(IfScope))
					{
						ifCount++;
					}
					else if (process is EndIfScope)
					{
						ifCount--;
						if (ifCount == 0)
						{
							Current.Result.SkipScopes.Add(new SkipScope(skipStart, data.Index));
							return;
						}
					}
					else if (process is ElseScope || (process is ElseIfScope elseIfScope && elseIfScope.IsCondition(symbols)))
					{
						elseScope = process is ElseScope;
						cur = data;
						condition = true;
						Current.Result.SkipScopes.Add(new SkipScope(skipStart, data.Index));
						break;
					}
				}
				if (!condition)
				{
					throw new LineDataException(cur, "if not end");
				}
			}
			// 有効なスコープ
			{
				LineData data = default;
				int skipStart = -1;
				int ifCount = 1;
				while (reader.TryRead(ref data))
				{
					if (data.Type != LineType.PreProcess)
					{
						if (skipStart == -1)
						{
							temp.Enqueue(data);
						}
						continue;
					}
					var process = ReadTag(in data);
					if (elseScope)
					{
						CheckElseStartScope(in data, process);
					}
					if (process is EndIfScope)
					{
						ifCount--;
						if (ifCount == 0)
						{
							if (skipStart >= 0)
							{
								Current.Result.SkipScopes.Add(new SkipScope(skipStart, data.Index));
							}
							return;
						}
					}
					else if (process.GetType() == typeof(IfScope))
					{
						if (skipStart == -1)
						{
							ProcessIfScope(data, (IfScope)process);
						}
						else
						{
							ifCount++;
						}
					}
					else if (skipStart == -1)
					{
						if (process is ElseScope || process is ElseIfScope)
						{
							skipStart = data.Index;
						}
						else if (process is IDefineSymbol define)
						{
							//シンボルは即時評価する必要がある
							define.Result(Current.Result);
						}
						else
						{
							temp.Enqueue(data);
						}
					}
				}
				throw new LineDataException(cur, "if not end");
			}
		}

		void ProcessMacroScope(LineData cur, MacroScope scope)
		{
			LineData data = default;
			int skipStart = cur.Index;
			while (TryReadData(ref data))
			{
				if (data.Type == LineType.Label || data.Type == LineType.Text)
				{
					throw new LineDataException(in data, $"not allowed {data.Type} in macro scope");
				}
				else if (data.Type == LineType.PreProcess)
				{
					var process = ReadTag(in data);
					if (process is EndMacroScope)
					{
						Current.Result.SkipScopes.Add(new SkipScope(skipStart, data.Index));
						return;
					}
					else if (process.GetType() == typeof(IfScope))
					{
						ProcessIfScope(data, (IfScope)process);
						continue;
					}
					throw new LineDataException(in data, "PreProcess is not allowed except for if scope in macro scope");
				}
				else if (data.Type == LineType.Command || data.Type == LineType.SystemCommand)
				{
					scope.Add(in data);
				}
			}
			throw new LineDataException(cur, "macro not end");
		}
	}
}