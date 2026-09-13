# CCCテスト仕様

> [ドキュメント一覧](../../README.md) / 区分: テスト仕様

## ドキュメントの目的

各機能要件および仕様を実行可能な自動・手動テストとして検証できるように、原則1項目につき1つのシステム挙動（Behavior）として整理・定義する。
※特定の C# テストクラス名、テストフレームワーク（NUnit / EditMode / PlayMode）、モック構成には依存しない。簡易版と本実装版の検証項目をパスさせ、未実装の将来機能は機能切り替え時に段階的に有効化すること。

## 正確な数値計算に関する検証要件

- 数学的に同値な分数は、常に等しい有理数（`Rational`）として判定されること。
- 同値な分数は、常に同一のハッシュ値を返却すること。
- 有理数は、常に約分済みの最簡形で保持されること。
- 有理数の分母は、常に正（`> 0`）の値として正規化されること。
- 0 は `0/1` として正規化保持されること。
- 除数が 0 である式（0除算）は、正常な計算結果を返却しないこと。
- 0除算発生時の画面表示文字列は `？` となること。
- Mission の成否判定処理は、表示用に丸めた小数ではなく、正確な有理数を用いること。
- 式適合割合（`MatchRatio`）の算出にあたっては、正常な `ExpressionCandidate` のみを分母として計算すること。

## MissionConditionの判定に関する検証要件

- **Even**: 計算結果が偶数整数の場合にのみ `true` を返すこと。
- **Odd**: 計算結果が奇数整数の場合にのみ `true` を返すこと。
- **Integer**: 計算結果が整数の場合にのみ `true` を返すこと。
- **Decimal**: 計算結果が非整数の場合にのみ `true` を返すこと。
- **Positive**: 計算結果が 0 より大きい場合にのみ `true` を返すこと。
- **Negative**: 計算結果が 0 より小さい場合にのみ `true` を返すこと。
- **AtLeast**: 計算結果が指定された下限値 N 以上の場合にのみ `true` を返すこと。
- **AtMost**: 計算結果が指定された上限値 N 以下の場合にのみ `true` を返すこと。
- **Exact**: 計算結果が指定された目標値 N と等しい場合にのみ `true` を返すこと。
- **Prime**: 計算結果が素数整数の場合にのみ `true` を返すこと。
- **Multiple**: 計算結果が指定値 N の倍数である整数の場合にのみ `true` を返すこと。
- **Zero**: 計算結果が 0 の場合にのみ `true` を返すこと。
- **Square**: 計算結果が 0 以上の平方数整数の場合にのみ `true` を返すこと。
- **Between**: 計算結果が下限 N 以上かつ上限 (N+W) 以下の場合にのみ `true` を返すこと。
- **Absolute**: 整数計算結果の絶対値が指定値 N と等しい場合にのみ `true` を返すこと。
- **OnesDigit**: 整数計算結果の符号を除いた一の位が指定値 N と等しい場合にのみ `true` を返すこと。
- **Composite**: 計算結果が 1 より大きい合成数整数の場合にのみ `true` を返すこと。
- **Modulo**: 整数計算結果が指定した法 N と剰余 M のユークリッド条件を満たす場合にのみ `true` を返すこと。
- **DecimalFirstDigit**: 非整数計算結果の符号を除いた小数第1位が指定値 N と等しい場合にのみ `true` を返すこと。
- 0除算等で正常な計算結果を持たない式は、いかなる `MissionCondition` も成立しない（`false` となる）こと。

## 簡易版：パラメータ候補に関する検証要件

- 簡易版では、[候補プリセット](../missions/candidate-presets.md) に定義された候補のみを評価すること。
- 簡易版では、プリセットに存在しないパラメータを自動生成しないこと。
- パラメータを必要としない Kind は、空パラメータとして正しく評価すること。
- `Between` の上限値は、下限 N と幅 W の和（`N + W`）として算出すること。
- `Between` の幅 W が `1未満` の候補は採用しないこと。
- `Between` は、プリセットで明示された下限 N と幅 W のペアのみを評価すること。
- `Modulo` の剰余 M が `0未満` の候補は採用しないこと。
- `Modulo` の剰余 M が 法 N 以上（`M >= N`）の候補は採用しないこと。
- 同一 Kind かつ同一パラメータを持つ重複候補は、1件に一元正規化すること。
- 簡易版では `NegatedMission` 候補を生成しないこと。
- 簡易版では複合ミッション（`CompoundMission`）候補を生成しないこと。

## 共通：MissionCatalog生成に関する検証要件

- `MissionCatalog` は、Gameplay 開始時に 1回 のみ生成すること。
- `MissionCatalog` は、同一 Session 内で再生成しないこと。
- 各パラメータ候補は、正常なすべての `ExpressionCandidate` に対して評価すること。
- `ExpressionMatchCount` が、条件を満たす正常な `ExpressionCandidate` の正確な件数となること。
- `ExpressionMatchRatio` が、`ExpressionMatchCount / (正常なExpressionCandidate総数)` と一致すること。
- `MinExpressionMatchRatio` 未満の候補は採用しないこと。
- `MaxExpressionMatchRatio` を超える候補は採用しないこと。
- `Zero` は `ExpressionMatchCount == 1` で件数要件をパスすること。
- `Prime` は `ExpressionMatchCount == 1` で件数要件をパスすること。
- `Zero` および `Prime` 以外の Kind は `ExpressionMatchCount == 1` では不採用とすること。
- `Zero` および `Prime` 以外の Kind は `ExpressionMatchCount >= 2` で件数要件をパスすること。
- 正常な `ExpressionCandidate` が 0件 の場合、Gameplay を開始せず設定エラーとすること。
- 有効な `MissionCatalog` が 0件 の場合、Gameplay を開始せず設定エラーとすること。

## 簡易版：Session中の抽選に関する検証要件

- 解放済みの下位 Tier が存在する場合、現在 Tier が相対重み `0.60` で抽出されること。
- 解放済みの下位 Tier が存在する場合、下位 Tier 全体が相対重み `0.40` で抽出されること。
- 解放済みの下位 Tier が存在しない場合、現在 Tier が確実に抽出されること。
- 下位 Tier 範囲からは、候補が存在する Tier のみが抽出対象となること。
- 候補が存在する Kind のみが抽選の母集団となること。
- 選択された Tier 内の Kind は、等確率でランダム選定すること。
- 選択された Kind 内のパラメータ候補は、等確率でランダム選定すること。
- 簡易版の抽選結果は、`CurrentExpression` の達成・未達成状態に依存しないこと。
- 簡易版の抽選結果は、直前の出題履歴・ミッションに依存しないこと。
- 簡易版の抽選結果に `NegatedMission` が含まれないこと。
- 簡易版の抽選結果に複合ミッションが含まれないこと。
- 新しく出題される Mission には、直前より `1` 加算された `MissionId` が割り当てられること。

## Session進行に関する検証要件

- 新規 Session 開始時の `Section` は `1` から始まること。
- Mission 成功時、`Section` が `1` 加算されること。
- Mission 失敗時、`Section` は変化しないこと。
- Mission 失敗時、`Life` が `1` 減算されること。
- `Life` が 0 に達した場合、次の Mission を出題せず結果画面へ遷移すること。
- 正解問題数が `max(0, Section - 1)` として正常に算出されること。
- 正解問題数が独立した変数として保持されないこと。
- 同一の `MissionId` に対する 2回目以降の Submit が無視されること。
- 現在処理中と異なる `MissionId` を対象とするタイマー通知が無視されること。
- 画面遷移中（`IsTransitioning == true`）の Submit 操作が無視されること。
- 画面遷移中（`IsTransitioning == true`）の式の変更操作が無視されること。

## 入力と表示に関する検証要件

- PLAYER A の数字変更操作は、左側の数字のみを繰り上げ変更すること。
- PLAYER B の数字変更操作は、右側の数字のみを繰り上げ変更すること。
- いずれのプレイヤーの演算子操作も、同一の共有演算子を変更すること。
- 1回のタッチ操作が、重複して複数の操作として処理されないこと。
- `ShowCurrentAnswer == true` の場合、`CurrentExpression` の計算結果が画面に表示されること。
- `ShowCurrentAnswer == false` の場合でも、内部的に Mission の成否判定処理が正しく行われること。
- `CurrentExpression` が 0除算となった場合、計算結果表示欄に `？` が表示されること。
- Mission 成功演出の表示再生が完了するまで、次の入力操作を受け付けないこと。
- Mission 失敗演出の表示再生が完了するまで、次の入力操作を受け付けないこと。

## 本実装版：Session中の抽選に関する検証要件

- Session初回は初期式によるMission候補の除外を行わないこと。
- 2問目以降は、抽選時点の `CurrentExpression` では未達成のMissionを優先すること。
- 解放済みTierの未達成Missionが0件の場合に限り、`CurrentExpression`による除外条件を緩和すること。未解放Tierの候補はこの判定に含めないこと。
- 肯定 Mission と `NegatedMission` の相対重みが `1.00` 対 Config設定値 となること。
- `NegatedMissionSelectionWeight == 0.60` の場合、他条件が同等であれば期待出題比率が `6 : 10` となること。
- `NegatedMissionSelectionWeight == 1.00` の場合でも、`NegatedMission` のみに偏らないこと。
- `NegatedMissionSelectionWeight == 0` の場合、`NegatedMission` が抽選されないこと。
- 直前と同一の Kind かつ同一パラメータのミッションが、代替候補が存在する場合に連続出題されないこと。
- `Negated` の有無のみが異なるミッションは、連続出題回避上の同種に含まれないこと。
- `TargetExpressionMatchRatio` が難易度 Config から正しく取得・適用されること。
- 直前と同一条件しか存在しない場合、連続出題回避条件を適用しないこと。

## Mission候補自動生成：将来有効化する検証チェックリスト

- 自動生成する境界値が、到達可能な整数結果集合を生成元とすること。

## 複合Mission：将来有効化する検証チェックリスト

- 簡易版における `CompoundMissionSelectionWeight` が `0` であること。
- 複合ミッションは、含まれるすべての構成条件を満たした場合にのみ成功となること。
- 複合ミッションの生成・抽出に失敗した場合、単一ミッションへ自動フォールバックすること。

---
- ドキュメント一覧: [README](../../README.md)
- 仕様全般: [CCC要件書](../overview.md)
- 内部仕様: [CCC内部仕様](../internals/game-invariants.md)
