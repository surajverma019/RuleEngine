using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DRuleEngine
{
    public class ExpressionBuilder
    {
        public Expression BuildExpression<T>(Operator ruleOperator, object value, ParameterExpression parameterExpression)
        {
            ExpressionType expressionType = new ExpressionType();
            var leftOperand = parameterExpression;
            var rightOperand = Expression.Constant(Convert.ChangeType(value, typeof(T)));
            var expressionTypeValue = (ExpressionType)expressionType.GetType()
                .GetField(Enum.GetName(typeof(Operator), ruleOperator)).GetValue(ruleOperator);

            var binaryExpression = Expression.MakeBinary(expressionTypeValue, leftOperand, rightOperand);
            return binaryExpression;
        }

        public Expression BuildExpression<T>(string propertyName, Operator ruleOperator, object value, ParameterExpression parameterExpression)
        {
            ExpressionType expressionType = new ExpressionType();
            var leftOperand = MemberExpression.Property(parameterExpression, propertyName);
            var rightOperand = Expression.Constant(Convert.ChangeType(value, value.GetType()));
            FieldInfo fieldInfo = expressionType.GetType().GetField(Enum.GetName(typeof(Operator), ruleOperator));
            var expressionTypeValue = (ExpressionType)fieldInfo.GetValue(ruleOperator);
            var binaryExpression = Expression.MakeBinary(expressionTypeValue, leftOperand, rightOperand);
            return binaryExpression;
        }

        public Expression BuildExpression(Type type, Operator ruleOperator, object value, ParameterExpression parameterExpression)
        {
            ExpressionType expressionType = new ExpressionType();
            var leftOperand = parameterExpression;
            var rightOperand = Expression.Constant(Convert.ChangeType(value, type));
            var expressionTypeValue = (ExpressionType)expressionType.GetType().GetField(Enum.GetName(typeof(Operator), ruleOperator)).GetValue(ruleOperator);
            return CastBuildExpression(expressionTypeValue, value, leftOperand, rightOperand);
        }

        private Expression CastBuildExpression(ExpressionType expressionTypeValue, object value, Expression leftOperand, ConstantExpression rightOperand)
        {
            if (leftOperand.Type == rightOperand.Type)
            {
                return Expression.MakeBinary(expressionTypeValue, leftOperand, rightOperand);
            }
            else if (CanChangeType(value, leftOperand.Type))
            {
                if (rightOperand.Type != typeof(bool))
                {
                    rightOperand = Expression.Constant(Convert.ChangeType(value, leftOperand.Type));
                }
                else
                {
                    leftOperand = Expression.Constant(Convert.ChangeType(value, rightOperand.Type));
                }
                return Expression.MakeBinary(expressionTypeValue, leftOperand, rightOperand);
            }
            return null;
        }

        private bool CanChangeType(object sourceType, Type targetType)
        {
            try
            {
                Convert.ChangeType(sourceType, targetType);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}