# Missionパラメータ候補PresetDebug

> [ドキュメント一覧](../../README.md) / 区分: 設定仕様

## ドキュメントの目的

`MissionPresetAsset`は製品経路の候補供給元ではなく、新Mode・新Mission・個別条件の限定確認に使用するデバッグ資産とする。製品経路は`MissionParameterSourceMode.Automatic`へ一本化する。

## SourceMode契約

- `Automatic`では`MissionParameterGenerationProfileAsset`を必須とし、Presetは検証も参照もしないこと。
- `PresetDebug`ではProfileとPresetの両方を必須とすること。
- `PresetDebug`は`IRuntimeMode.IsDebugMode == true`でのみ許可すること。
- 非Debug環境では設定エラーとしてGameplayを開始せず、`Automatic`へフォールバックしないこと。
- Preset候補もProfileの有効Kind、適合率ポリシー、固定Tier規則を通してからCatalogへ採用すること。

## Entry形式

表の1行はTier未確定の`MissionParameterCandidate` 1件に対応する。

| 項目 | 内容 |
| --- | --- |
| `Kind` | Mission種別 |
| `ParameterA` | Singleの値、Betweenの下限、Moduloの法 |
| `ParameterB` | Betweenの幅、Moduloの剰余 |
| `IsNegated` | 判定反転の有無 |

`Tier`は入力しないこと。肯定固定Tier、Betweenの分析済み適合率、DivisorのNの到達可能性、Negated既定Tierから`MissionCatalogBuilder`が確定する。

## パラメータ構造

- `None`: Even、Odd、Integer、Decimal、Positive、Negative、Prime、Zero、Square、Composite、BetweenZeroAndOne、TwoDigit、RepeatingDecimal
- `Single`: AtLeast、AtMost、Exact、Multiple、Absolute、OnesDigit、DecimalFirstDigit、Divisor、Power、TensDigit、DigitSum
- `Pair`: Between、Modulo

`None`はParameterA/Bを0、`Single`はParameterBを0とする。`Between`のParameterBは幅であり1以上、`Modulo`は法2以上かつ`0 <= remainder < divisor`とする。

## 重複と失敗

同一`MissionCondition`の重複行はAsset検証エラーにしない。`MissionCatalogBuilder`が入力順の最初の1件へ決定的に正規化する。Presetが空、不正Kind、不正パラメータ、許可されないNegatedは設定エラーとする。

---
- Mission種別とTier: [生成するMission](./mission-types.md)
- Catalog準備と自動生成: [Missionカタログ・抽選内部仕様](./catalog-and-draw.md)
