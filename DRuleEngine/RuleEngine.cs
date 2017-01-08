using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DRuleEngine
{
    public class RuleEngine
    {
        public Func<T, bool> CompileRule<T>(Rule rule)
        {
            if (string.IsNullOrEmpty(rule.PropertyName))
            {
                ExpressionBuilder expressionBuilder = new ExpressionBuilder();
                var param = Expression.Parameter(typeof(T));
                Expression expression = expressionBuilder.BuildExpression<T>(rule.Operator_, rule.Value, param);
                Func<T, bool> func = Expression.Lambda<Func<T, bool>>(expression, param).Compile();
                return func;
            }
            else
            {
                ExpressionBuilder expressionBuilder = new ExpressionBuilder();
                var param = Expression.Parameter(typeof(T));
                Expression expression = expressionBuilder.BuildExpression<T>(rule.PropertyName, rule.Operator_, rule.Value, param);
                Func<T, bool> func =Expression.Lambda<Func<T, bool>>(expression, param).Compile();
                return func;
            }
        }

        public Func<T, bool>[] CombineRules<T>(Rule[] rules)
        {
            List<Func<T, bool>> list = new List<Func<T, bool>>();
            foreach (Rule rule in rules)
            {
                if (string.IsNullOrEmpty(rule.PropertyName))
                {
                    ExpressionBuilder expressionBuilder = new ExpressionBuilder();
                    var param = Expression.Parameter(typeof(T));
                    Expression expression = expressionBuilder.BuildExpression<T>(rule.Operator_, rule.Value, param);
                    Func<T, bool> func = Expression.Lambda<Func<T, bool>>(expression, param).Compile();
                    list.Add(func);
                }
                else
                {
                    ExpressionBuilder expressionBuilder = new ExpressionBuilder();
                    var param = Expression.Parameter(typeof(T));
                    Expression expression = expressionBuilder.BuildExpression<T>(rule.PropertyName, rule.Operator_, rule.Value, param);
                    Func<T, bool> func = Expression.Lambda<Func<T, bool>>(expression, param).Compile();
                    list.Add(func);
                }
            }
            return list.ToArray();
        }

    }
}
