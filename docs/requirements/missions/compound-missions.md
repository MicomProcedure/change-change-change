# 複合Mission

> [ドキュメント一覧](../../README.md) / 区分: 将来要件
>
> 正本: [Cosense「複合Mission」](https://scrapbox.io/proto-TeamL/%E8%A4%87%E5%90%88Mission)
> 取得時点の原文更新日時: 2026-08-26T05:39:16.000Z


[CCC要件書](../overview.md)から分離した、将来実装する複合Missionの成立条件。
簡易版では複合Missionの生成、評価、抽選、表示を行わない。
NormalのCompoundMissionSelectionWeightは0とする。

## 実装時期

複合Missionは単一Missionの簡易版と本実装版が安定した後に着手する。
クラス構成はこのページでは定義しない。
実装着手時に、この仕様から設計案を生成する。

## 候補カタログへの追加条件

CompoundMissionSelectionWeightが0より大きい設定でのみ、Gameplay開始時のMissionCatalogへ合計2条件の複合Mission候補を追加する。

```text
主条件の実効Tier
AND
主条件より低い実効Tierの追加条件
```

追加条件は主条件の実効Tier未満であればよく、直前のTierに限定しない。
同じ条件の重複を避ける。
両条件を同時に達成できるExpressionCandidateが存在する組だけを採用する。
最初の複合Mission仕様では肯定条件同士だけを組み合わせ、NegatedMissionConditionを含めない。
複合Missionの実効Tierは主条件の実効Tierとする。
式適合割合、最小適合式数、AnswerExampleには単一Missionと共通の評価規則を適用する。

## 判定

- 2条件のANDとする。
- 片方だけを満たしても失敗とする。
- 回答例は両条件を同時に満たす正常な式とする。
- 新Mission選定時は、CurrentExpressionが複合条件をすでに満たす候補を除外する。

## 抽選とフォールバック

単一Missionの基準重み1.00に対してCompoundMissionSelectionWeightを相対重みとして使用する。
選択Tierに有効な複合Missionがなければ単一Missionへフォールバックする。
CompoundMissionSelectionWeightが0の場合は複合Mission抽選を行わない。

## 関連仕様

- [Missionカタログ・抽選内部仕様](./catalog-and-draw.md)
- [CCCテスト仕様](../testing/acceptance-specification.md)
