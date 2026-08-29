using System;
using Ccc.Core.Model;
using R3;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Gameplay で提出式を編集・公開する mutable な入力値所有者です。
    /// </summary>
    public sealed class ExpressionInput : IDisposable
    {
        private readonly GameplayConfig _config;
        private readonly Subject<Expression> _expressionsSubject;
        private Expression _current;
        private bool _isDisposed;

        /// <summary>
        /// 現在の提出式を取得します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public Expression Current
        {
            get
            {
                ThrowIfDisposed();
                return _current;
            }
        }

        /// <summary>
        /// 変更後の提出式を同期的に公開する Observable を取得します。
        /// 初期値は公開せず、Dispose 時に完了します。
        /// </summary>
        public Observable<Expression> Expressions { get; }

        /// <summary>
        /// GameplayConfig の初期式を現在値として入力を生成します。
        /// </summary>
        /// <param name="config">数字範囲と初期式を含む Gameplay 設定。</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> が null の場合。</exception>
        public ExpressionInput(GameplayConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _current = config.InitialExpression;
            _expressionsSubject = new Subject<Expression>();
            Expressions = _expressionsSubject;
        }

        /// <summary>
        /// 指定された側の数値を1つ進め、最大値の次で最小値へ戻します。
        /// </summary>
        /// <param name="side">変更対象の左右。</param>
        /// <exception cref="ArgumentOutOfRangeException">未定義の <paramref name="side"/> の場合。</exception>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void ChangeNumber(PlayerSide side)
        {
            ThrowIfDisposed();

            if (!Enum.IsDefined(typeof(PlayerSide), side))
            {
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            var nextNumber = side == PlayerSide.Left
                ? GetNextNumber(_current.Left)
                : GetNextNumber(_current.Right);
            var nextExpression = side == PlayerSide.Left
                ? new Expression(nextNumber, _current.Operator, _current.Right)
                : new Expression(_current.Left, _current.Operator, nextNumber);

            PublishIfChanged(nextExpression);
        }

        /// <summary>
        /// 演算子を定義された順序で1段階進めます。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void ChangeOperator()
        {
            ThrowIfDisposed();

            var nextExpression = new Expression(
                _current.Left,
                _current.Operator.Next(),
                _current.Right);
            PublishIfChanged(nextExpression);
        }

        /// <summary>
        /// 指定された式を現在値へ設定します。
        /// </summary>
        /// <param name="expression">設定する提出式。</param>
        /// <exception cref="ArgumentOutOfRangeException">左右の数値が設定範囲外の場合。</exception>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void Reset(Expression expression)
        {
            ThrowIfDisposed();

            if (!IsInConfiguredNumberRange(expression.Left) ||
                !IsInConfiguredNumberRange(expression.Right))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(expression),
                    expression,
                    "Expression numbers must be within the configured range.");
            }

            PublishIfChanged(expression);
        }

        /// <summary>
        /// 入力を破棄し、公開中の Observable を完了します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _expressionsSubject.OnCompleted();
            _expressionsSubject.Dispose();
        }

        private int GetNextNumber(int currentNumber)
        {
            return currentNumber >= _config.MaximumNumber
                ? _config.MinimumNumber
                : currentNumber + 1;
        }

        private bool IsInConfiguredNumberRange(int number)
        {
            return number >= _config.MinimumNumber && number <= _config.MaximumNumber;
        }

        private void PublishIfChanged(Expression nextExpression)
        {
            if (_current == nextExpression)
            {
                return;
            }

            _current = nextExpression;
            _expressionsSubject.OnNext(_current);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ExpressionInput));
            }
        }
    }
}
