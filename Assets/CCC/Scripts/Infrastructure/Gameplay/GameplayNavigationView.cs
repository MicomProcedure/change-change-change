using Ccc.Presentation;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面から他画面へ戻る入力を扱うViewです。
    /// </summary>
    public sealed class GameplayNavigationView : MonoBehaviour
    {
        [SerializeField] private Button _returnToMainMenuButton;

        private Subject<GameplayInput> _inputsSubject;
        private UnityEngine.Events.UnityAction _returnToMainMenuAction;

        /// <summary>
        /// Navigation入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs { get; private set; }

        private void Awake()
        {
            _inputsSubject = new Subject<GameplayInput>();
            Inputs = _inputsSubject;
        }

        private void Start()
        {
            if (_returnToMainMenuButton == null || _inputsSubject == null)
            {
                return;
            }

            _returnToMainMenuAction =
                () => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.ReturnToMainMenu));
            _returnToMainMenuButton.onClick.AddListener(_returnToMainMenuAction);
        }

        private void OnDestroy()
        {
            if (_returnToMainMenuButton != null && _returnToMainMenuAction != null)
            {
                _returnToMainMenuButton.onClick.RemoveListener(_returnToMainMenuAction);
            }

            if (_inputsSubject != null)
            {
                _inputsSubject.OnCompleted();
                _inputsSubject.Dispose();
            }
        }
    }
}
