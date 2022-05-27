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
			{"CommandEndBlockTest.anovel", CommandEndBlockTest},
			{"commandendblocktest.anovel", CommandEndBlockTest},
			{"CommandTest.anovel", CommandTest},
			{"commandtest.anovel", CommandTest},
			{"ConductorErrorTest.anovel", ConductorErrorTest},
			{"conductorerrortest.anovel", ConductorErrorTest},
			{"ConductorJumpTest.anovel", ConductorJumpTest},
			{"conductorjumptest.anovel", ConductorJumpTest},
			{"ConductorTest.anovel", ConductorTest},
			{"conductortest.anovel", ConductorTest},
			{"EnvDataHookTest.anovel", EnvDataHookTest},
			{"envdatahooktest.anovel", EnvDataHookTest},
			{"ImportMacroTest.anovel", ImportMacroTest},
			{"importmacrotest.anovel", ImportMacroTest},
			{"KeyValueTest.anovel", KeyValueTest},
			{"keyvaluetest.anovel", KeyValueTest},
			{"MacroSet.anovel", MacroSet},
			{"macroset.anovel", MacroSet},
			{"MetaDataDefineTest.anovel", MetaDataDefineTest},
			{"metadatadefinetest.anovel", MetaDataDefineTest},
			{"ReaderTest.anovel", ReaderTest},
			{"readertest.anovel", ReaderTest},
			{"TagTest.anovel", TagTest},
			{"tagtest.anovel", TagTest},
			{"TextBlockTest.anovel", TextBlockTest},
			{"textblocktest.anovel", TextBlockTest},
		};

		public static string Get(string name) => s_Dic[name];

		/* file:CircleImport.anovel
		
		#import path="MacroSet.anovel"
		
		#if condition="IMMEDIATE_DEFINE" not
			;define_symbolは即時反映される
			#define_symbol name="IMMEDIATE_DEFINE"
			#if condition="IMMEDIATE_DEFINE"
				#macro name="dep_macrolog"
				@macrolog2 m="dep"
				#endmacro
			#endif
		#endif
		*/
		public const string CircleImport = "\n#import path=\"MacroSet.anovel\"\n\n#if condition=\"IMMEDIATE_DEFINE\" not\n	;define_symbolは即時反映される\n	#define_symbol name=\"IMMEDIATE_DEFINE\"\n	#if condition=\"IMMEDIATE_DEFINE\"\n		#macro name=\"dep_macrolog\"\n		@macrolog2 m=\"dep\"\n		#endmacro\n	#endif\n#endif\n";

		/* file:CircleImportTest.anovel
		
		#import path="CircleImport.anovel"
		
		@dep_macrolog
		*/
		public const string CircleImportTest = "\n#import path=\"CircleImport.anovel\"\n\n@dep_macrolog";

		/* file:CommandEndBlockTest.anovel
		
		エンドブロックコマンドテスト
		
		&endblock
		
		デフォルトセーブされる
		
		&endblock cansave=false
		
		ブロックは終了するがその地点でセーブは出来ない
		*/
		public const string CommandEndBlockTest = "\nエンドブロックコマンドテスト\n\n&endblock\n\nデフォルトセーブされる\n\n&endblock cansave=false\n\nブロックは終了するがその地点でセーブは出来ない\n";

		/* file:CommandTest.anovel
		
		
		&async
		@test_sync
		@test_sync
		@test_trigger name="syncend"
		&end
		
		asyncを利用するとスコープ内に同期コマンドがあっても非同期実行される
		
		&async
		@test_sync
		@test_sync
		@test_trigger name="blockfinish"
		&end
		
		非同期コマンド中にブロックを終了する
		
		トリガーが実行されている
		
		&parallel
		@test_sync
		@test_sync
		@test_trigger name="parallelend"
		&end
		
		parallelを利用するとスコープ内を同時に実行できる
		
		&parallel
		@test_trigger name="parallelend"
		&end
		
		parallel内に同期命令がない場合は非同期実行される
		
		&parallel sync=false
		@test_sync
		@test_sync
		@test_trigger name="parallelend"
		&end
		
		parallel内に同期命令があってもsyncフラグをfalseにすると非同期になる
		
		&parallel sync=true
			&async
				@test_sync
				@test_sync
				@test_trigger name="blockfinish"
			&end
			&async
				@test_sync
				@test_sync
				@test_sync
			&end
		&end
		
		nestしたスコープ
		
		
		ジャンプ前
		
		&jump label="jump_label"
		
		スキップ
		
		*jump_label
		
		ジャンプコマンド
		*/
		public const string CommandTest = "\n\n&async\n@test_sync\n@test_sync\n@test_trigger name=\"syncend\"\n&end\n\nasyncを利用するとスコープ内に同期コマンドがあっても非同期実行される\n\n&async\n@test_sync\n@test_sync\n@test_trigger name=\"blockfinish\"\n&end\n\n非同期コマンド中にブロックを終了する\n\nトリガーが実行されている\n\n&parallel\n@test_sync\n@test_sync\n@test_trigger name=\"parallelend\"\n&end\n\nparallelを利用するとスコープ内を同時に実行できる\n\n&parallel\n@test_trigger name=\"parallelend\"\n&end\n\nparallel内に同期命令がない場合は非同期実行される\n\n&parallel sync=false\n@test_sync\n@test_sync\n@test_trigger name=\"parallelend\"\n&end\n\nparallel内に同期命令があってもsyncフラグをfalseにすると非同期になる\n\n&parallel sync=true\n	&async\n		@test_sync\n		@test_sync\n		@test_trigger name=\"blockfinish\"\n	&end\n	&async\n		@test_sync\n		@test_sync\n		@test_sync\n	&end\n&end\n\nnestしたスコープ\n\n\nジャンプ前\n\n&jump label=\"jump_label\"\n\nスキップ\n\n*jump_label\n\nジャンプコマンド\n";

		/* file:ConductorErrorTest.anovel
		
		
		@test_error
		
		*/
		public const string ConductorErrorTest = "\n\n@test_error\n\n";

		/* file:ConductorJumpTest.anovel
		
		ジャンプテスト開始
		
		停止
		
		&stop
		
		*jump1
		
		ジャンプ先1
		
		&stop
		
		*jump2
		
		ジャンプ先2
		
		シーク中
		
		シーク中
		
		*jump2
		
		シーク先1
		
		シーク中
		
		シーク中
		
		*jump2
		
		シーク先2
		
		シーク中
		
		シーク中
		
		停止
		
		&stop
		*/
		public const string ConductorJumpTest = "\nジャンプテスト開始\n\n停止\n\n&stop\n\n*jump1\n\nジャンプ先1\n\n&stop\n\n*jump2\n\nジャンプ先2\n\nシーク中\n\nシーク中\n\n*jump2\n\nシーク先1\n\nシーク中\n\nシーク中\n\n*jump2\n\nシーク先2\n\nシーク中\n\nシーク中\n\n停止\n\n&stop\n";

		/* file:ConductorTest.anovel
		
		
		テキスト表示
		
		@test_prepare_wait time=0.5
		@test_preload path="test1"
		@test_preload path="test2"
		
		【名前】
		名前付きテキスト表示
		
		@test_sync
		
		同期が終わるまでテキストまで進まない
		*/
		public const string ConductorTest = "\n\nテキスト表示\n\n@test_prepare_wait time=0.5\n@test_preload path=\"test1\"\n@test_preload path=\"test2\"\n\n【名前】\n名前付きテキスト表示\n\n@test_sync\n\n同期が終わるまでテキストまで進まない\n";

		/* file:EnvDataHookTest.anovel
		
		#import path="MetaDataDefineTest.anovel"
		
		Test1
		
		Test2
		*/
		public const string EnvDataHookTest = "\n#import path=\"MetaDataDefineTest.anovel\"\n\nTest1\n\nTest2\n";

		/* file:ImportMacroTest.anovel
		;ローカルのシンボルはインポート先には影響しない
		#define_symbol name="SKIP_MACRO_LOG"
		
		#import path="MacroSet.anovel"
		
		#if condition="SKIP_MACRO_LOG_DEFINE" not
		@macrolog m="tt"
		#endif
		
		@macrolog2 m="bb"
		
		@definelog
		
		*/
		public const string ImportMacroTest = ";ローカルのシンボルはインポート先には影響しない\n#define_symbol name=\"SKIP_MACRO_LOG\"\n\n#import path=\"MacroSet.anovel\"\n\n#if condition=\"SKIP_MACRO_LOG_DEFINE\" not\n@macrolog m=\"tt\"\n#endif\n\n@macrolog2 m=\"bb\"\n\n@definelog\n\n";

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
		
		#import path="CircleImport.anovel"
		
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
		public const string MacroSet = "\n#import path=\"CircleImport.anovel\"\n\n#if condition=\"SKIP_MACRO_LOG_DEFINE\" not\n\n#macro name=\"macrolog\"\n#if condition=\"SKIP_MACRO_LOG\" not\n@test_log message=\"%m%\"\n#endif\n#endmacro\n\n#endif\n\n#macro name=\"macrolog2\"\n#if condition=\"SKIP_MACRO_LOG\" not\n@test_log message=\"%m%\"\n#endif\n#endmacro\n\n#macro name=\"definelog\"\n#if condition=\"MACRO_LOG\"\n@test_macro_define message=\"Define MACRO_LOG\"\n#elseif condition=\"SKIP_MACRO_LOG_DEFINE\"\n@test_macro_define message=\"Define SKIP_MACRO_LOG_DEFINE\"\n#elseif condition=\"SKIP_MACRO_LOG\"\n@test_macro_define message=\"Define SKIP_MACRO_LOG\"\n#else\n@test_macro_define message=\"No Define\"\n#endif\n#endmacro\n";

		/* file:MetaDataDefineTest.anovel
		
		
		#test_meta_deine name="key1" value="value1" first
		#test_meta_deine name="key2" value="value2" first
		#test_meta_deine name="key3" value="value3" first
		#test_meta_deine name="key3" value="value3update"
		
		#test_meta_deine value="single1" first
		#test_meta_deine value="single2"
		*/
		public const string MetaDataDefineTest = "\n\n#test_meta_deine name=\"key1\" value=\"value1\" first\n#test_meta_deine name=\"key2\" value=\"value2\" first\n#test_meta_deine name=\"key3\" value=\"value3\" first\n#test_meta_deine name=\"key3\" value=\"value3update\"\n\n#test_meta_deine value=\"single1\" first\n#test_meta_deine value=\"single2\"\n";

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
		#import path="MacroSet.anovel"
		
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
		public const string TextBlockTest = "#import path=\"MacroSet.anovel\"\n\n*開始\n\n一行テキスト\n\n二行テキスト\n二行テキスト\n\n```\n空白ありテキスト\n\n上が空白\n```\n\n@test_log message=\"命令1\"\n@test_log message=\"命令2\"\n\n命令とテキストが一度に取れる\n@test_log message=\"msg\"\n命令でテキストブロックは解除される\n\n\n```test\n特殊テキストブロック\n```\n\n```test : value\n値付き特殊テキストブロック\n```\n\n*終了\n\n最終行テキスト";

	}
}