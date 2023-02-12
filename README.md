# ANovel (Atra Novel)

Unity用のノベルパートを作成する為のフレームワークです。  
先行プリロードやロールバックなどの機能を持ちます。  
※現在開発中でリリースの段階にまで至っていません。

## 概要

より詳細な内容は[ドキュメント](Documents/index.md)に記載してあります。

### スクリプト
ANovelのスクリプトはKAG3/KAGEXを参考にした文法を採用しています。

```
@image name="画像" path="file_name"

テキストの表示に特別な命令は必要ありません。

デフォルトでは空白の行があると改ページとして扱われます。
```
※現在、デフォルトの挙動しかありません！

#### [VSCode拡張](Documents/vscode.md)
まともな開発環境がないとつらいのでVisualStudioCodeの拡張を用意してあります。  
ユーザーが作成したタグも補完が出来ます。  
https://github.com/yazawa-ichio/anovel-vscode

### コマンドを作成する
コマンドを作成は簡単に出来ます。  
登録等は不要でCommandクラスを継承しTagNameを付けていれば自動で登録されます。  

```cs
//@sample key="value" flag="false"

[TagName("sample")]
public class SampleCommand : Command
{
	// デフォルトでは「_」と「m_」が除かれた変数名がkeyになります。
	[Argument]
	string m_Key;
	[Argument(Required = true)]
	bool m_Flag = true;

	protected override void Execute()
	{
		// 事前にContainerに登録して使います。
		var service = Container.Get<ISampleService>();
		service.Execute(m_Key, _flag);
	}
}
```

## QuickStart

### インストール
upmで以下のURLでパッケージのインストールが出来ます。  
`https://github.com/yazawa-ichio/ANovel.git?path=Unity/Assets/ANovel`

### インポート

メニューの`[ANovel/Import/QuickStart]`でQuickStartのプロジェクトがインポートできます。
`Assets/ANovel.QuickStart/QuickStart.unity` を開くと実行できます。
