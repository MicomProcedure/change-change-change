# CCC ローカル永続化クラス設計

> [ドキュメント一覧](../../README.md) / 区分: クラス設計

> **対象範囲**: ハイスコアの記録と参照、保存データのバージョン互換、ゲームリセット

---

## 1. 目的

ローカルに保存するハイスコアの扱いを、「ゲームのルール（更新判定、バージョン互換）」と「保存手段（PlayerPrefs）」に分離すること。

- ハイスコアは `GameMode` と `GameDifficulty` の組ごとに1件保持すること。
- 各記録には、達成時のアプリケーションバージョンを併せて保存すること。
- ゲームリセットでは、ローカルに永続化した情報を保存項目によらず全て消去すること。

---

## 2. 配置

| 型名 | 配置アセンブリ / 名前空間 | 役割概要 |
| --- | --- | --- |
| `HighScoreRecord` / `HighScoreUpdateResult` | `Ccc.Core.Model` | 保存する記録と、スコア記録の結果を表す不変データ |
| `HighScoreService` | `Ccc.Core.UseCases` | バージョン互換方針を適用したハイスコアの参照と更新 |
| `IHighScoreStore` | `Ccc.Core.UseCases` | ハイスコア記録の保存先ポート |
| `IHighScoreCompatibilityPolicy` / `HighScoreCompatibilityDecision` / `HighScoreCompatibilityAction` | `Ccc.Core.UseCases` | 保存済み記録の扱いを決定する方針と決定内容 |
| `VersionEquivalenceGroups` | `Ccc.Core.Model` | 保存データを同一視するバージョングループ |
| `VersionEquivalenceHighScoreCompatibilityPolicy` | `Ccc.Core.UseCases` | バージョングループに基づき、記録を同一視する互換方針 |
| `KeepAllHighScoreCompatibilityPolicy` | `Ccc.Core.UseCases` | 判定せず全ての記録を使い続ける互換方針 |
| `VersionEquivalenceEntryAsset` | `Ccc.Infrastructure.Assets` | 保存データの種類ごとの同一視設定をまとめる `ScriptableObject` |
| `HighScoreVersionEquivalenceAsset` / `HighScoreVersionEquivalenceEntry` / `VersionGroup` | `Ccc.Infrastructure.Assets` | ゲームモードと難易度の組ごとの、ハイスコアの同一視設定 |
| `VersionEquivalenceBuildValidator` | `Ccc.Editor` | ビルド時の設定検証と登録漏れの検出 |
| `IApplicationVersionProvider` | `Ccc.Core.UseCases` | 実行中のアプリケーションバージョンの提供ポート |
| `ILocalDataEraser` | `Ccc.Core.UseCases` | ローカルの永続化情報を全消去するポート |
| `PlayerPrefsHighScoreStore` / `HighScoreRecordJsonCodec` | `Ccc.Infrastructure.Persistence` | PlayerPrefsへの保存と保存用JSONの変換 |
| `NonPersistingHighScoreStore` | `Ccc.Infrastructure.Persistence` | 読み込みのみ委譲し、書き込みを永続化しない保存先 |
| `UnityApplicationVersionProvider` | `Ccc.Infrastructure.Persistence` | `Application.version` の提供 |
| `PlayerPrefsLocalDataEraser` | `Ccc.Infrastructure.Persistence` | `PlayerPrefs.DeleteAll` による全消去 |

---

## 3. ハイスコアの仕様

- スコアは `SessionResult.CorrectAnswerCount` とする。
- 記録は `SessionEndedEvent` の受信時に行うこと。プレイ途中でメインメニューへ戻った場合は記録しない。
- 既存のハイスコアを超えた場合のみ更新し、新記録として扱うこと。同点は更新せず、記録がない状態のスコア0も新記録にしない。
- 記録がない場合のハイスコアは `0` として表示すること。
- `ApplicationLifetimeScope` の DebugMode では `NonPersistingHighScoreStore` を登録し、記録を永続化しないこと。読み込みと新記録の判定は通常と同じように動作する。

---

## 4. 保存形式

| 項目 | 内容 |
| --- | --- |
| キー | `HighScore.{GameMode}.{GameDifficulty}`（例: `HighScore.LifeEndless.Hard`） |
| 値 | `{"formatVersion":1,"score":12,"appVersion":"1.0.1"}` |

- `formatVersion` は保存形式の版数、`appVersion` は記録を達成したアプリケーションバージョンとする。
- JSONが不正、スコアが負、`appVersion` が空白、`formatVersion` が未対応のいずれかに該当する値は、警告ログを出して記録なしとして扱うこと。
- 音量設定はバージョン管理の対象外とする。

---

## 5. バージョン更新時の扱い

`HighScoreService` は記録を読み込むたびに `IHighScoreCompatibilityPolicy` を適用する。

| 決定 | 動作 | 使用例 |
| --- | --- | --- |
| `Keep` | 記録をそのまま使い続ける | 達成時のバージョンと同一視する場合 |
| `Ignore` | 記録を残したまま記録なしとして扱い、1以上のスコアで上書きする | 達成時のバージョンと同一視しない場合 |
| `Discard` | 記録を削除し、記録なしとして扱う | 記録を確実に消したい場合 |
| `Replace` | 変換後の記録を保存して使用する | スコアの定義が変わり、換算できる場合 |

### 5.1 バージョングループによる同一視

`ApplicationLifetimeScope` に `VersionEquivalenceEntryAsset` を設定すると、`VersionEquivalenceHighScoreCompatibilityPolicy` を使用する。未設定の場合は `KeepAllHighScoreCompatibilityPolicy` を使用し、判定を行わない。

- 記録の `appVersion` と実行中のバージョンが同じグループにある場合は `Keep`、それ以外は `Ignore` とすること。
- 設定はゲームモードと難易度の組ごとに持つこと。
- 同一視の設定を誤っても記録が失われないよう、`Discard` は使用しない。バージョンをグループへ追加すれば、残っている記録は再び使用される。
- 同じバージョンは、未登録でも常に同一視すること。

### 5.2 バージョン更新時の手順

1. `HighScoreVersionEquivalenceAsset` の全ての組に、新しいバージョンを登録すること。スコアを引き継ぐ組は既存のグループへ、引き継がない組は新しいグループへ追加する。
2. ビルドすると `VersionEquivalenceBuildValidator` が設定を検証し、不正な設定または未登録のバージョンがあればビルドを中止する。

| 検出箇所 | 対象 | 動作 |
| --- | --- | --- |
| ビルド時 | 設定の不正、ビルド対象バージョンの未登録 | `BuildFailedException` でビルドを中止 |
| 起動時 | 設定の不正 | `InvalidOperationException` で起動を中止 |
| 起動時 | 実行中バージョンの未登録 | エラーログを出し、同じバージョンの記録だけを同一視して継続 |

### 5.3 同一視では対応できない変更

- スコアを換算する場合は、`Replace` を返す互換方針をコードで実装すること。
- 保存形式そのものを変更する場合は、`HighScoreRecordJsonCodec.CurrentFormatVersion` を増やし、旧形式の読み込みを同時に実装すること。

---

## 6. ゲームリセット

1. `OptionMenu` から確認画面（`ResetConfirmation`）へ遷移すること。
2. 確認後に `ILocalDataEraser.EraseAll` を実行すること。
3. `TitleScene` を `TitleScreen` から読み込み直すこと。

各 Model を個別に初期化せず、Scene の再読み込みで消去後の保存内容から再構築する。これにより、保存項目が増えてもリセット処理の変更を不要とし、音量は `TitleEntryPoint` の購読開始時に Audio へ反映される。

- `PlayerPrefs.DeleteAll` は、Unity が PlayerPrefs へ保存するウィンドウサイズなどの情報も消去する。
- `Logs/Ccc` のログと診断CSVはゲームデータではないため、消去の対象外とする。
- PlayerPrefs 以外の保存手段を追加する場合は、`ILocalDataEraser` の実装でその保存先も消去すること。

---

## 7. DI仕様

`ApplicationLifetimeScope` が全て Singleton で登録し、TitleScene と GameScene の双方から解決すること。

| 解決する型 | 実装 |
| --- | --- |
| `IHighScoreStore` | 通常は `PlayerPrefsHighScoreStore`、DebugMode は `NonPersistingHighScoreStore` |
| `IHighScoreCompatibilityPolicy` | `VersionEquivalenceEntryAsset` の設定時は `VersionEquivalenceHighScoreCompatibilityPolicy`、未設定時は `KeepAllHighScoreCompatibilityPolicy` |
| `IApplicationVersionProvider` | `UnityApplicationVersionProvider` |
| `ILocalDataEraser` | `PlayerPrefsLocalDataEraser` |
| `HighScoreService` | `HighScoreService` |
