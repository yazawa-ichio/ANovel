using System.Collections.Generic;

namespace ANovel.Core.Tests
{
	public static class TestData
	{
		static Dictionary<string, string> s_Dic = new Dictionary<string, string>
		{
			{"CircleImport.anovel", CircleImport},
			{"circleimport.anovel", CircleImport},
			{"CircleImportTest.anovel", CircleImportTest},
			{"circleimporttest.anovel", CircleImportTest},
			{"ImportMacroTest.anovel", ImportMacroTest},
			{"importmacrotest.anovel", ImportMacroTest},
			{"KeyValueTest.anovel", KeyValueTest},
			{"keyvaluetest.anovel", KeyValueTest},
			{"MacroSet.anovel", MacroSet},
			{"macroset.anovel", MacroSet},
			{"ReaderTest.anovel", ReaderTest},
			{"readertest.anovel", ReaderTest},
			{"TagTest.anovel", TagTest},
			{"tagtest.anovel", TagTest},
			{"TextBlockTest.anovel", TextBlockTest},
			{"textblocktest.anovel", TextBlockTest},
		};

		public static string Get(string name) => s_Dic[name];

		/* file:CircleImport.anovel
		
		#importmacro path="MacroSet.anovel"
		
		#if condition="IMMEDIATE_DEFINE" not
			;defineは即時反映される
			#define name="IMMEDIATE_DEFINE"
			#if condition="IMMEDIATE_DEFINE"
				#macro name="dep_macrolog"
				@macrolog2 m="dep"
				#endmacro
			#endif
		#endif
		*/
		public const string CircleImport = "\n#importmacro path=\"MacroSet.anovel\"\n\n#if condition=\"IMMEDIATE_DEFINE\" not\n	;defineは即時反映される\n	#define name=\"IMMEDIATE_DEFINE\"\n	#if condition=\"IMMEDIATE_DEFINE\"\n		#macro name=\"dep_macrolog\"\n		@macrolog2 m=\"dep\"\n		#endmacro\n	#endif\n#endif\n";

		/* file:CircleImportTest.anovel
		
		#importmacro path="CircleImport.anovel"
		
		@dep_macrolog
		*/
		public const string CircleImportTest = "\n#importmacro path=\"CircleImport.anovel\"\n\n@dep_macrolog";

		/* file:ImportMacroTest.anovel
		;ローカルのシンボルはインポート先には影響しない
		#define name="SKIP_MACRO_LOG"
		
		#importmacro path="MacroSet.anovel"
		
		#if condition="SKIP_MACRO_LOG_DEFINE" not
		@macrolog m="tt"
		#endif
		
		@macrolog2 m="bb"
		
		@definelog
		
		*/
		public const string ImportMacroTest = ";ローカルのシンボルはインポート先には影響しない\n#define name=\"SKIP_MACRO_LOG\"\n\n#importmacro path=\"MacroSet.anovel\"\n\n#if condition=\"SKIP_MACRO_LOG_DEFINE\" not\n@macrolog m=\"tt\"\n#endif\n\n@macrolog2 m=\"bb\"\n\n@definelog\n\n";

		/* file:KeyValueTest.anovel
		@command1
			@command1
		@command2 key1="Value1" key2="Value2"
		  @command2 key1="Value1"key2="Value2"
			@command2 key1=Value1 key2="Value2"
		@command2 key1  = "Value1" key2=  "Value2"
		@command3 key1 key2
		@command4 key1="escape\"test"
		@command4 key1=escape"test
		
		#nexterror
		
		@
			@
		@error1 key=
			@error2 key="test
			@ error3
		*/
		public const string KeyValueTest = "@command1\n	@command1\n@command2 key1=\"Value1\" key2=\"Value2\"\n  @command2 key1=\"Value1\"key2=\"Value2\"\n	@command2 key1=Value1 key2=\"Value2\"\n@command2 key1  = \"Value1\" key2=  \"Value2\"\n@command3 key1 key2\n@command4 key1=\"escape\\\"test\"\n@command4 key1=escape\"test\n\n#nexterror\n\n@\n	@\n@error1 key=\n	@error2 key=\"test\n	@ error3\n";

		/* file:MacroSet.anovel
		
		#importmacro path="CircleImport.anovel"
		
		#if condition="SKIP_MACRO_LOG_DEFINE" not
		
		#macro name="macrolog"
		#if condition="SKIP_MACRO_LOG" not
		@test_log message="%m%"
		#endif
		#endmacro
		
		#endif
		
		#macro name="macrolog2"
		#if condition="SKIP_MACRO_LOG" not
		@test_log message="%m%"
		#endif
		#endmacro
		
		#macro name="definelog"
		#if condition="MACRO_LOG"
		@test_macro_define message="Define MACRO_LOG"
		#elseif condition="SKIP_MACRO_LOG_DEFINE"
		@test_macro_define message="Define SKIP_MACRO_LOG_DEFINE"
		#elseif condition="SKIP_MACRO_LOG"
		@test_macro_define message="Define SKIP_MACRO_LOG"
		#else
		@test_macro_define message="No Define"
		#endif
		#endmacro
		*/
		public const string MacroSet = "\n#importmacro path=\"CircleImport.anovel\"\n\n#if condition=\"SKIP_MACRO_LOG_DEFINE\" not\n\n#macro name=\"macrolog\"\n#if condition=\"SKIP_MACRO_LOG\" not\n@test_log message=\"%m%\"\n#endif\n#endmacro\n\n#endif\n\n#macro name=\"macrolog2\"\n#if condition=\"SKIP_MACRO_LOG\" not\n@test_log message=\"%m%\"\n#endif\n#endmacro\n\n#macro name=\"definelog\"\n#if condition=\"MACRO_LOG\"\n@test_macro_define message=\"Define MACRO_LOG\"\n#elseif condition=\"SKIP_MACRO_LOG_DEFINE\"\n@test_macro_define message=\"Define SKIP_MACRO_LOG_DEFINE\"\n#elseif condition=\"SKIP_MACRO_LOG\"\n@test_macro_define message=\"Define SKIP_MACRO_LOG\"\n#else\n@test_macro_define message=\"No Define\"\n#endif\n#endmacro\n";

		/* file:ReaderTest.anovel
		#preprocess
		;comment
			@command key="Value"
		  &systemcommand key="Value"
		*label name="Label"
		テストテキスト
		
		@command key2="Value2"
		*/
		public const string ReaderTest = "#preprocess\n;comment\n	@command key=\"Value\"\n  &systemcommand key=\"Value\"\n*label name=\"Label\"\nテストテキスト\n\n@command key2=\"Value2\"";

		/* file:TagTest.anovel
		@test_default_formatter Key1="test1" key2="10" key3="2.5" key4=false
		@test_default_formatter Key1="test1" key2="error"
		@test_custom_formatter key1="1,2,3" key2="1,2,3" key3="四五"
		@test_custom_formatter key1="1,2,3" key2="1,2,3" key3="四五"
		@test_required required optional
		@test_required required
		@test_required optional
		@replace_test_default_formatter Key1="test1" key2_replace key3="2.5" key4=false
		*/
		public const string TagTest = "@test_default_formatter Key1=\"test1\" key2=\"10\" key3=\"2.5\" key4=false\n@test_default_formatter Key1=\"test1\" key2=\"error\"\n@test_custom_formatter key1=\"1,2,3\" key2=\"1,2,3\" key3=\"四五\"\n@test_custom_formatter key1=\"1,2,3\" key2=\"1,2,3\" key3=\"四五\"\n@test_required required optional\n@test_required required\n@test_required optional\n@replace_test_default_formatter Key1=\"test1\" key2_replace key3=\"2.5\" key4=false";

		/* file:TextBlockTest.anovel
		#importmacro path="MacroSet.anovel"
		
		*開始
		
		一行テキスト
		
		二行テキスト
		二行テキスト
		
		```
		空白ありテキスト
		
		上が空白
		```
		
		@test_log message="命令1"
		@test_log message="命令2"
		
		命令とテキストが一度に取れる
		@test_log message="msg"
		命令でテキストブロックは解除される
		
		
		```test
		特殊テキストブロック
		```
		
		```test : value
		値付き特殊テキストブロック
		```
		
		*終了
		
		最終行テキスト
		*/
		public const string TextBlockTest = "#importmacro path=\"MacroSet.anovel\"\n\n*開始\n\n一行テキスト\n\n二行テキスト\n二行テキスト\n\n```\n空白ありテキスト\n\n上が空白\n```\n\n@test_log message=\"命令1\"\n@test_log message=\"命令2\"\n\n命令とテキストが一度に取れる\n@test_log message=\"msg\"\n命令でテキストブロックは解除される\n\n\n```test\n特殊テキストブロック\n```\n\n```test : value\n値付き特殊テキストブロック\n```\n\n*終了\n\n最終行テキスト";

	}
}