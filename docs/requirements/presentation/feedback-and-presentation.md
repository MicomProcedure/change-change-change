# フィードバック・演出

> [ドキュメント一覧](../../README.md) / 区分: 要件
>
> 正本: [Cosense「フィードバック・演出」](https://scrapbox.io/proto-TeamL/%E3%83%95%E3%82%A3%E3%83%BC%E3%83%89%E3%83%90%E3%83%83%E3%82%AF%E3%83%BB%E6%BC%94%E5%87%BA)
> 取得時点の原文更新日時: 2026-08-26T05:26:21.000Z

[CCC要件書](../overview.md)から分離した、入力・成功・失敗時のフィードバック仕様。

## 入力

- 数字変更時は対応する数字カードを[カードめくりのAnimation](https://scrapbox.io/proto-TeamL/%E3%82%AB%E3%83%BC%E3%83%89%E3%82%81%E3%81%8F%E3%82%8A%E3%81%AEAnimation)で強調する。
- タッチ領域は押下Animationでフィードバックする。
- 演算子変更時は共有演算子と両側のOPERATOR領域を短く強調する。

## 成功

- `CORRECT!`をオーバーレイ表示する。
- 最終式と結果を表示する。
- 表示時間は約0.8秒とし、共通設定で管理する。

## 失敗

- Lifeが残っている場合は`MISS!`を表示する。
- Lifeが減る演出を行う。
- Lifeが0の場合は`GAME OVER`を表示する。
- Submitした最終式を表示する。
- 「この式なら正解」と回答例の式・計算結果を表示する。
- ShowCurrentAnswerがfalseでも回答例を表示する。ShowCurrentAnswerはGameplay中のCurrentExpressionの計算結果表示だけを制御する。
- 表示時間は約3秒とする。

外部画像・音声はゲーム判定ロジックから分離する。

関連する内部規則：[CCC内部仕様](../internals/game-invariants.md)
受け入れ条件：[CCCテスト仕様](../testing/acceptance-specification.md)
