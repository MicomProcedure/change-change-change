# 複合Mission

> [ドキュメント一覧](../../README.md) / 区分: 将来要件

[CCC要件書](../overview.md)から分離した、将来拡張機能として導入する「複合Mission（`CompoundMission`）」の成立条件および判定仕様を定義する。

※現行製品経路では複合ミッションの生成、評価、抽選、表示は一切行わないこと（`Normal`難易度における`CompoundMissionSelectionWeight`は`0`と設定する）。

## 実装フェーズと方針

複合ミッション機能は、単一ミッションの自動生成・Catalog構築・本実装版抽選が安定運用された段階で着手すること。特定のクラス構成はこのドキュメントでは規定せず、実装着手時に本仕様に基づいて設計案を作成すること。

## 候補カタログへの登録要件

`CompoundMissionSelectionWeight > 0` の設定が適用されている場合に限り、Gameplay 開始時の `MissionCatalog` 構築処理において、2つの条件を組み合わせた複合ミッション候補を追加すること。

```text
主条件の実効Tier
AND
主条件より低い実効Tierの追加条件
```

- 追加条件の Tier は、主条件の実効 Tier 未満であれば良く、直前の Tier に限定しないこと。
- 同一条件同士の重複組み合わせは除外すること。
- 両方の条件を同時に達成可能な `ExpressionCandidate` が1つ以上存在するペアのみを採択すること。
- 初期バージョンの複合ミッションでは肯定条件同士（`AND`）の組み合わせのみを対象とし、`NegatedMissionCondition` は含めないこと。
- 複合ミッション全体の実効 Tier は「主条件の実効 Tier」として扱うこと。
- 式適合割合（`MatchRatio`）、最小適合式数（`MinimumExpressionMatchCount`）、および正解例（`AnswerExample`）には単一ミッションと共通の評価規則を適用すること。

## 成否判定ルール

- 2つの条件の論理積（`AND`）として判定すること。
- いずれか片方の条件のみを満たした場合は失敗（不達成）とみなすこと。
- 模範回答例（`AnswerExample`）には、両条件を同時に満たす正常な式を表示すること。
- 新規ミッション出題時の抽選においては、現在の `CurrentExpression` がすでに複合条件を満たしている候補を除外すること。

## 抽選およびフォールバック処理

- 単一ミッションの基準重み `1.00` に対し、`CompoundMissionSelectionWeight` を相対重みとして使用すること。
- 選択された Tier 内に利用可能な複合ミッション候補が存在しない場合は、自動的に単一ミッションの抽選へフォールバックすること。
- `CompoundMissionSelectionWeight == 0` の場合は、複合ミッションの抽選処理自体を実行しないこと。

---
- カタログ・抽選仕様: [Missionカタログ・抽選内部仕様](./catalog-and-draw.md)
- テスト要件: [CCCテスト仕様](../testing/acceptance-specification.md)
