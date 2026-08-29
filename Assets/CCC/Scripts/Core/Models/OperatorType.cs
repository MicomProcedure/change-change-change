using System;

namespace Ccc.Core.Model
{
    public enum OperatorType
    {
        Add = 0,
        Subtract,
        Multiply,
        Divide
    }

    public static class OperatorTypeExtensions
    {
        public static OperatorType Next(this OperatorType @operator)
        {
            return @operator switch
            {
                OperatorType.Add => OperatorType.Subtract,
                OperatorType.Subtract => OperatorType.Multiply,
                OperatorType.Multiply => OperatorType.Divide,
                OperatorType.Divide => OperatorType.Add,
                _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null)
            };
        }
    }
}