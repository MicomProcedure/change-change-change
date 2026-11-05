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
| `KeepAllHighScoreCompatibilityPolicy` | `Ccc.Core.UseCases` | 全ての記録を使い続ける現在の互換方針 |
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

`HighScoreService` は記録を読み込むたびに `IHighScoreCompatibilityPolicy` を適用する。更新時の扱いは、方針の実装を差し替えて選択すること。

| 決定 | 動作 | 使用例 |
| --- | --- | --- |
| `Keep` | 記録をそのまま使い続ける | 通常の更新 |
| `Discard` | 記録を削除し、記録なしとして扱う | 難易度の調整で、以前の記録と比較できなくなった場合 |
| `Replace` | 変換後の記録を保存して使用する | スコアの定義が変わり、換算できる場合 |

保存形式そのものを変更する場合は、`HighScoreRecordJsonCodec.CurrentFormatVersion` を増やし、旧形式の読み込みを同時に実装すること。

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
| `IHighScoreCompatibilityPolicy` | `KeepAllHighScoreCompatibilityPolicy` |
| `IApplicationVersionProvider` | `UnityApplicationVersionProvider` |
| `ILocalDataEraser` | `PlayerPrefsLocalDataEraser` |
| `HighScoreService` | `HighScoreService` |
