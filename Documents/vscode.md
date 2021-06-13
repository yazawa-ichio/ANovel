# VSCode拡張
まともな開発環境がないとつらいのでVisualStudioCodeの拡張を用意してあります。  
ユーザーが作成したタグも補完が出来ます。  
https://github.com/yazawa-ichio/anovel-vscode

## インストール
https://github.com/yazawa-ichio/anovel-vscode/releases  
リリースページから「anovel-vscode.zip」をダウンロードします。  
展開した中にあるanovel-vscode.vsixをVSCodeの拡張機能のメニューから[VSIXからインストール]でインストールが可能です。

## 定義ファイルの出力
デフォルトではコマンドの補完は出来ません。  
定義ファイルを出力する必要があります。  
ユーザーが新しく定義したコマンドも出力されます。  

### ワークスペースの設定
`EngineConfig.assets` を通して定義ファイルを出力出来ます。
EngineConfigは[Assets/Create/ANovel/EngineConfig]から作成できます。  
Project Exportの項目に出力先を記載してください。  
最低限の設定は`Project Root`だけで問題ありませんが、`Resource Path`等を設定しておくと補完候補に現れます。

設定したら `Export Define` ボタンで定義ファイル`ANovelProject`が出力されます。

## プロジェクトを開く
先ほどの定義ファイルがワークスペース内にある状態で開くとanovelファイルでコマンドの補完が出来るようになります。
