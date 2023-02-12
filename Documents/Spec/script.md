
# ANovelスクリプト

ANovelで利用するマークアップ言語です。  
吉里吉里で利用されるKAG3やKAGEXに強い影響を受けた言語です。  
ただし、KAG3やKAGEXと互換性はありません。  

# 言語仕様

AMLの命令は全て行単位で記述します。  
命令は同じ行に複数記述できません。  
また、テキスト表示命令以外は行頭・行末のタブ・空白は全て無視されます。  
空文字の行も無視されますが、その前の命令の終端として意味を持つ場合があります。

命令は行の一文字によって種類が変わります。  
テキスト表示命令は例外で上記の命令を除いた物はすべてテキスト表示命令として扱われます。

## テキスト表示命令

後述の命令を表す文字以外の場合は全て文字を表示する命令（テキスト表示命令）として扱われます。  

### テキストブロック
テキスト表示命令はANovel上ではテキストブロックという単位で処理されます。  
テキストブロックの終端は別の命令か、空文字の行が来るまでになります。  
ANovelのデフォルトではテキストブロック中の改行はそのまま改行として扱われます。  
空改行や他のタグをそのまま表示したい場合は ` ``` ` で開始と終了を囲うと記述すると、その中身はテキストブロックとして扱われます。

## コメント(;)

`;`から始まる命令はコメントとして扱います。  
コメントの場合は全ての処理がスキップされます。

## プリプロセス命令(#)

`#`から始まる命令はプリプロセス命令として扱います。  
プリプロセス命令は定義ファイルやマクロ・アクションなどのインポートなどを行います。  
プリプロセス命令は必ず最初にすべて実行されます。  
循環参照に対応していますがインポートの順番によっては意図しない状態になりやすいので可能な限り避けてください。  

## 演出命令(@)

`@`から始まる命令は演出命令を扱います。  
原則、演出命令はテキストブロックを跨ぐとすべて停止します。  
演出命令はANovel上のオブジェクトの状態を更新を行い、更新結果はテキストブロック毎にスナップショットが作成されます。  
ユーザー拡張が想定されています。

テキストブロックを超えて演出を行う場合はアクション機能を利用します。  

### パラメーター指定

パラメーターはキーとバリュー形式で`キー="バリュー"`記述します。  
パラメーターの前後には一つ以上のスペースを入れるか`"`で区切られている必要があります。  

例
```
@image name="画像" file="resources/image_file"
```

### 変数展開

パラメーターの=の最初を`$`から開始すると変数が展開されます。  
変数は`{`と`}`で括られた部分が変数名として置換されます。  
通常の`{`と`}`を使用する場合、連続で`{{`と`}}`するとエスケープされます。

例
```
@val name="image" value="image_file"
@image name="画像" file=$"resources/{image}"
```

## システム命令(&)

`&`から始まる命令はシステム命令を扱います。  
内部的に演出命令と違いはないですが特殊な命令と視認しやすくするため分けています。

## ラベル(*)

`*`から始まる命令はラベルとして扱います。  
ラベルはシステム命令のジャンプ先としても扱われます。  

## 特殊テキストブロック(```)

テキスト表示命令の亜種です。  
` ``` `で開始した後に`名前`か`名前:値` を設定し、二行目以降で` ``` `で閉じたものを指します。  
AMLのパーサーはテキストブロックとして扱いますが、ANovel側で名前を指定して値とブロックの内容を受け取れます。  
今後の拡張のために用意されています。  
