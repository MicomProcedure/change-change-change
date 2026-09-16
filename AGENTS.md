## Base I/O Regulation

- 常に日本語で回答する
- 出力の頭に`[Applying Local AGENTS.md]`と出力する

## 実装方針

- 指示したクラスを作成する際、不足している依存や参照元があれば作成を止め一度ユーザーに指示を仰ぐこと
- クラスやpublic method, public propertyはドキュメントコメント（<summary>や<param>）を記載する
- ハードコードを禁止する
- コンパイルチェックはユーザーに依頼する
- EditorScriptはtddを実施せず、静的な検証と手動確認のみ

## プロジェクト情報
- 仕様や要件の正本はdocs以下のドキュメント群とする
- デザインや画面構成案などはCosenceを正本とする
  - https://scrapbox.io/ChangeChangeChange/
- 作業前に @design-principles.md を参照すること

## 命名規則

### 基本方針
 名前だけで役割や意味が分かる、具体的で検索しやすい語を使う
 同じ概念には同じ単語を使い、類義語を混在させない
 意味の伝わらない省略、ローマ字、日本語と英語の混在を避ける
 スコープが広い名前ほど具体的にし、一時変数など狭いスコープでは簡潔にする
 真偽値には `is`、`has`、`can`、`should` など、真偽が分かる接頭辞を付ける
 関数・メソッドは動詞から始め、何をするかが分かる名前にする
 単位を持つ値は、必要に応じて `timeoutMs` や `sizeBytes` のように単位を名前へ含める

### クラス名Suffix
 DTOクラスはクラス名最後に`Dto`をつける
		`GameplayViewStateDto`


### 大文字・小文字
 ローカル変数、引数、プロパティ、関数、メソッドは `lowerCamelCase`
  例: `userName`、`calculateTotal`
 クラスのprivateフィールドには頭にアンダースコアをつける
 	例: `_hasRequest`、`_gameplayScore`
 クラス、構造体、列挙型、インターフェース、型は `PascalCase`
  例: `UserRepository`、`ConnectionState`
 GLOBAL な定数は `UPPER_SNAKE_CASE`
  例: `MAX_RETRY_COUNT`、`DEFAULT_TIMEOUT_MS`
 GLOBAL ではない通常の定数は `PascalCase`
  例: `MaxRetryCount`、`DefaultTimeoutMs`


### 略称
 2文字の略称は、すべて大文字のまま記述する
  例: `IO`、`UI`
  	ただし、idについては.NET公式ガイドより、一般語彙として`Id`を用いること
  複合名の例: `UserId`、`SectionUI`、`IOManager`
 3文字以上の略称は、通常の単語として先頭だけを大文字にする
  例: `USB` → `Usb`、`API` → `Api`、`URL` → `Url`
  複合名の例: `UsbDevice`、`usbDevice`、`ApiClient`

### 避ける名前
 `data`、`info`、`value`、`temp` など、文脈なしでは意味が曖昧な名前
 1文字だけの名前。ただし、短いループの `i` や座標の `x`、`y` など慣例が明確な場合は除く
 型名を不必要に含むハンガリアン記法（`pData`, `iCount` ）
 否定形を重ねた分かりにくい真偽値名