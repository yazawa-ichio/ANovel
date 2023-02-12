# マクロ機能

マクロ機能はよく使うスクリプトを事前定義して呼び出す機能です。  
マクロの使用側でパラメーターを渡すような事も可能です。  
アクションのマクロの一種です。  

## マクロ定義する

マクロの定義は`#macro`から`#endmacro`内に記述したスクリプトがマクロとなります。  
マクロ内にはラベルとテキストを含めることは出来ません。  

例
```
#macro name="image_file"
@image name="画像" file="resources/image_file"
#endmacro

@image_file
```

### パラメーターを定義する
`%`と`%`で括られた物がパラメーターとなります。  
`%`自体使う場合は`%%`と連続するとエスケープされます。  

例
```
#macro name="image_file"
@image name="画像" file="resources/%val%"
#endmacro

@image_file val="image_file"
```

### 分岐と併用する
マクロ内でも`&if`などの分岐を利用する事が可能です。  
変数なども組み合わせるとより柔軟に対応が可能です。  

例
```
#macro name="image_file"
&if left="%pos%" right="l"
	@image name="画像" file="resources/%val%" x=-300
&if left="%pos%" right="r"
	@image name="画像" file="resources/%val%" x=300
&else
	@image name="画像" file="resources/%val%"
&endif
#endmacro

#左に表示
@image_file val="image_file" pos=l
#右に表示
@image_file val="image_file" pos=l
```

## アクションについて
アクションは特殊なマクロの一種です。  
アクションはブロックを超えて継続を扱うために用意された方法になります。  


