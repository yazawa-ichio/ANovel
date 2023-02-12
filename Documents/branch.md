# 分岐処理
ANovelの分岐処理にはプリプロセス時に呼び出される物と実行時に呼び出される物の２つがあります。  
どちらも以下の命令を持ちます。

|タグ||
|---|---|
|if|分岐条件の最初に定義します|
|elseif|最初以外の分岐条件で使用します|
|eles|分岐条件に失敗した場合に使用します|
|end|分岐の終端に定義します|

## プリプロセス命令の分岐
プリプロセス命令の分岐はシンボルによる分岐しか行なえません。  
シンボルは`#define`で設定するかANovelEngienの起動時に設定します。  

```
#if condition="DEFINE"
@val name="val1" value="true"
#endif

;notを付けると条件が反転します
#if condition="DEFINE" not
@val name="val1" value="false"
#endif
```

## 実行時分岐
実行分岐はマクロなどの中でも行なえます。  
分岐命令は先行先読み時に評価されます。  
設定方法は以下の４つになります。notを使用すると結果が反転します。

### condition
値を評価します。`>`や`<`などの比較演算子も使用できます。
1の場合も成功とみなします。

### leftとright
LeftとRightの内容が一致した場合に分岐します。

### flag
flagに設定された変数が有効値(true)であれば分岐します。

#### has_val
変数が設定されている場合に実行します。  