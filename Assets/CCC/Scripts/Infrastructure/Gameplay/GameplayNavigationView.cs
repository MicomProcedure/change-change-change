using System;
using System.Threading;
using Ccc.Infrastructure.UI;
using Ccc.Presentation;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面から他画面へ戻る入力を扱うViewです。
    /// </summary>
    public sealed class GameplayNavigationView : MonoBehaviour
    {
        [Tooltip("Result画面でスコアの数字だけを表示するText。「Your Score:」などの接頭辞は別のTextとしてPrefabで設定する。")]
        [SerializeField] private TMP_Text _scoreValueText;
        [Tooltip("Result画面で、今回のスコアで到達した評価段階の数を表示するView。")]
        [SerializeField] private ScoreStarsView _scoreStarsView;
        [Tooltip("ハイスコアを更新したSessionのResult画面でのみ表示するText。文言はPrefabで設定する。")]
        [SerializeField] private TMP_Text _highScoreText;
        [Tooltip("ハイスコアを更新したSessionのResult画面でのみ表示するGameObject。")]
        [SerializeField] private GameObject _newHighScoreIndicator;
        [Tooltip("ハイスコアを更新したSessionのResult画面でのみ強調表示するView。")]
        [SerializeField] private ScaleHighlightView _newHighScoreHighlightView;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _returnToMainMenuButton;

        private readonly Subject<GameplayInput> _inputsSubject = new Subject<GameplayInput>();
        private UnityEngine.Events.UnityAction _retryAction;
        private UnityEngine.Events.UnityAction _returnToMainMenuAction;

        /// <summary>
        /// Navigation入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs
        {
            get { return _inputsSubject; }
        }

        /// <summary>
        /// Result画面を表示します。
        /// </summary>
        /// <param name="result">Result画面の表示状態。</param>
        /// <param name="cancellationToken">Result画面の表示演出を中断するトークン。</param>
        /// <returns>評価段階の表示演出の完了を表す非同期処理。</returns>
        public async UniTask RenderResultAsync(
            GameplayResultViewStateDto result,
            CancellationToken cancellationToken)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_scoreValueText != null)
            {
                _scoreValueText.text = result.ScoreText;
            }

            if (_highScoreText != null)
            {
                _highScoreText.gameObject.SetActive(result.IsNewHighScore);
            }

            if (_newHighScoreIndicator != null)
            {
                _newHighScoreIndicator.SetActive(result.IsNewHighScore);
            }

            gameObject.SetActive(true);

            // 非アクティブな間はPlayが無視されるため、表示後に呼ぶ
            if (_newHighScoreHighlightView != null)
            {
                if (result.IsNewHighScore)
                {
                    _newHighScoreHighlightView.Play();
                }
                else
                {
                    _newHighScoreHighlightView.Stop();
                }
            }

            // 表示演出を追加したときに非アクティブなまま再生されないよう、表示後に開始する。
            // 段階数と星の数の不一致などの例外を呼び出し元の非同期失敗として扱えるよう、awaitで受ける
            await _scoreStarsView.RenderAsync(result.AchievedRankCount, cancellationToken);
        }

        /// <summary>
        /// Result画面を非表示にします。
        /// </summary>
        public void HideResult()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            if (_retryButton != null)
            {
                _retryAction =
                    () => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.Retry));
                _retryButton.onClick.AddListener(_retryAction);
            }

            if (_returnToMainMenuButton != null)
            {
                _returnToMainMenuAction =
                    () => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.ReturnToMainMenu));
                _returnToMainMenuButton.onClick.AddListener(_returnToMainMenuAction);
            }
        }

        private void OnDestroy()
        {
            if (_retryButton != null && _retryAction != null)
            {
                _retryButton.onClick.RemoveListener(_retryAction);
            }

            if (_returnToMainMenuButton != null && _returnToMainMenuAction != null)
            {
                _returnToMainMenuButton.onClick.RemoveListener(_returnToMainMenuAction);
            }

            _inputsSubject.OnCompleted();
            _inputsSubject.Dispose();
        }
    }
}
