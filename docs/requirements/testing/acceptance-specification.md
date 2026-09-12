# CCCテスト仕様

> [ドキュメント一覧](../../README.md) / 区分: テスト仕様
>
> 正本: [Cosense「CCCテスト仕様」](https://scrapbox.io/proto-TeamL/CCC%E3%83%86%E3%82%B9%E3%83%88%E4%BB%95%E6%A7%98)
> 取得時点の原文更新日時: 2026-08-26T05:31:24.000Z


## このページの目的

仕様を実行可能なテストへ落とすため、1項目につき原則1つの振る舞いを記述する。
具体的なテストクラス、テストフレームワーク、モック構成は定義しない。
簡易版の項目を先に満たし、本実装版の項目は切替時に有効化する。

## 正確な数値計算

-  同値な分数は等しい有理数として判定される。
-  同値な分数は同じハッシュ値を返す。
-  有理数は約分済みで保持される。
-  有理数の分母は正に正規化される。
-  0は0/1に正規化される。
-  除数が0の式は正常な計算結果を返さない。
-  0除算の表示は？になる。
-  Mission判定は表示用小数ではなく正確な値を使う。
-  式適合割合は正常なExpressionCandidateだけを分母に使う。

## MissionConditionの判定

-  Evenは結果が偶数整数のときだけ成立する。
-  Oddは結果が奇数整数のときだけ成立する。
-  Integerは結果が整数のときだけ成立する。
-  Decimalは結果が非整数のときだけ成立する。
-  Positiveは結果が0より大きいときだけ成立する。
-  Negativeは結果が0より小さいときだけ成立する。
-  AtLeastは結果が下限以上のときだけ成立する。
-  AtMostは結果が上限以下のときだけ成立する。
-  Exactは結果が目標値と等しいときだけ成立する。
-  Primeは結果が素数整数のときだけ成立する。
-  Multipleは結果が指定値の倍数である整数のときだけ成立する。
-  Zeroは結果が0のときだけ成立する。
-  Squareは結果が0以上の平方数整数のときだけ成立する。
-  Betweenは結果が下限以上かつ下限と幅の和以下のときだけ成立する。
-  Absoluteは整数結果の絶対値が指定値と等しいときだけ成立する。
-  OnesDigitは整数結果の符号を除いた一の位が指定値と等しいときだけ成立する。
-  Compositeは結果が1より大きい合成数整数のときだけ成立する。
-  Moduloは整数結果が指定した法と剰余の条件を満たすときだけ成立する。
-  DecimalFirstDigitは非整数結果の符号を除いた小数第1位が指定値と等しいときだけ成立する。
-  正常な計算結果を持たない式はいずれのMissionConditionも成立しない。

## 簡易版：パラメータ候補

-  簡易版はプリセットに記載された候補だけを評価する。
-  簡易版はプリセットにないパラメータを自動生成しない。
-  パラメータを持たないKindは空のパラメータで評価される。
-  Betweenの上限は下限と幅の和から求められる。
-  Betweenの幅が1未満の候補は採用されない。
-  Betweenは明示された下限と幅の組だけを評価する。
-  Moduloの剰余が0未満の候補は採用されない。
-  Moduloの剰余が法以上の候補は採用されない。
-  同じKindと同じパラメータの重複候補は1件に正規化される。
-  簡易版はNegatedMission候補を生成しない。
-  簡易版は複合Mission候補を生成しない。

## 共通：MissionCatalog生成

-  MissionCatalogはGameplay開始時に1回生成される。
-  MissionCatalogは同じSession中に再生成されない。
-  各パラメータ候補はすべての正常なExpressionCandidateで評価される。
-  ExpressionMatchCountはMissionを満たすExpressionCandidateの件数になる。
-  ExpressionMatchRatioはExpressionMatchCountを正常なExpressionCandidate総数で割った値になる。
-  MinExpressionMatchRatio未満の候補は採用されない。
-  MaxExpressionMatchRatioを超える候補は採用されない。
-  ZeroのExpressionMatchCountが1なら件数条件を通過する。
-  PrimeのExpressionMatchCountが1なら件数条件を通過する。
-  ZeroとPrime以外のExpressionMatchCountが1なら採用されない。
-  ZeroとPrime以外のExpressionMatchCountが2なら件数条件を通過する。
-  正常なExpressionCandidateが0件ならGameplayを開始しない。
-  MissionCatalogが0件ならGameplayを開始しない。

## 簡易版：Session中の抽選

-  下位Tierがある場合は現在Tierが相対重み0.60で選ばれる。
-  下位Tierがある場合は下位Tier全体が相対重み0.40で選ばれる。
-  下位Tierがない場合は現在Tierが選ばれる。
-  下位Tier範囲からは候補が存在するTierだけが選ばれる。
-  候補が存在するKindだけが抽選母集団になる。
-  選ばれたTier内のKindは一様に選ばれる。
-  選ばれたKind内のパラメータ候補は一様に選ばれる。
-  簡易版の抽選結果はCurrentExpressionの達成状態に依存しない。
-  簡易版の抽選結果は直前のMissionに依存しない。
-  簡易版の抽選結果にNegatedMissionは含まれない。
-  簡易版の抽選結果に複合Missionは含まれない。
-  新しいMissionには直前より1大きいMissionIdが付く。

## Session進行

-  新しいSessionのSectionは1から始まる。
-  Mission成功時にSectionは1増える。
-  Mission失敗時にSectionは変わらない。
-  Mission失敗時にLifeは1減る。
-  Lifeが0になると次のMissionを出題せず結果画面へ進む。
-  正解数はSectionから max(0, Section - 1) として求められる。
-  正解数は独立した進行状態として保持されない。
-  同じMissionIdに対する2回目のSubmitは無視される。
-  現在と異なるMissionIdのタイマー通知は無視される。
-  遷移中のSubmitは無視される。
-  遷移中の式変更は無視される。

## 入力と表示

-  PLAYER Aの数字操作は左の数字だけを変更する。
-  PLAYER Bの数字操作は右の数字だけを変更する。
-  どちらのプレイヤーの演算子操作も共有演算子を変更する。
-  同一タッチは複数の操作として処理されない。
-  ShowCurrentAnswerが有効ならCurrentExpressionの計算結果が表示される。
-  ShowCurrentAnswerが無効でもMission判定は行われる。
-  CurrentExpressionが0除算なら計算結果欄に？が表示される。
-  Mission成功時の表示が完了するまで次の入力を受け付けない。
-  Mission失敗時の表示が完了するまで次の入力を受け付けない。

## 本実装版：将来有効化するチェックリスト

-  自動生成する境界値は到達可能な整数結果を生成元にする。
-  新Missionは抽選時のCurrentExpressionでは未達成である。
-  肯定MissionとNegatedMissionの相対重みは1.00対設定値になる。
-  NegatedMissionSelectionWeightが0.60なら他条件が同じとき期待出題比率は6対10になる。
-  NegatedMissionSelectionWeightが1.00でもNegatedMissionだけにはならない。
-  直前と同じKindかつ同じパラメータは代替候補があるとき連続しない。
-  Negatedの有無だけが異なるMissionは連続回避上の同種に含まれない。
-  TargetExpressionMatchRatioは難易度Configから取得される。
-  候補0件時は追加した抽選条件が逆順に緩和される。

## 複合Mission：将来有効化するチェックリスト

-  簡易版ではCompoundMissionSelectionWeightが0である。
-  複合Missionはすべての条件を満たしたときだけ成功する。
-  複合Missionの生成失敗時は単一Missionへフォールバックする。
