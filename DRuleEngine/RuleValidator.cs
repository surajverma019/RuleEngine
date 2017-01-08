using SimpleExpressionEvaluator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DRuleEngine
{
    public class RuleValidator
    {
        public bool ValidateExpressionRules<T>(T value, Rule rule)
        {
            if (rule.ProcessingRule != string.Empty)
            {
                Evaluator evaluator = new Evaluator();
                return evaluator.Evaluate<T>(rule.ProcessingRule, value);
            }
            return false;
        }
        public bool ValidateRulesAll<T>(T value, Func<T, bool>[] rules)
        {
            foreach (var rule in rules)
            {
                if (!rule(value))
                    return false;
            }
            return true;
        }

        public bool ValidateRulesAny<T>(T value, Func<T, bool>[] rules)
        {
            foreach (var rule in rules)
            {
                if (rule(value))
                    return true;
            }
            return false;
        }

        public bool ValidateRulesAll<T>(T[] values, Func<T, bool>[] rules)
        {
            foreach (var value in values)
            {
                foreach (var rule in rules)
                {
                    if (!rule(value))
                        return false;
                }
            }
            return true;
        }

        public bool ValidateRulesAny<T>(T[] values, Func<T, bool>[] rules)
        {
            foreach (var value in values)
            {
                bool validated = false;
                foreach (var rule in rules)
                {
                    if (rule(value))
                    {
                        validated = true;
                        break;
                    }
                }
                if (!validated)
                    return false;
            }
            return true;
        }

        public bool ValidateRulesSum<T>(IEnumerable<T> values, IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                // necessary to create the type dynamic so i could use the + operator to 
                // build the sum on an Int32 or Double or Decimal
                dynamic sum = Activator.CreateInstance(values.GetType().GetElementType().GetProperty(rule.PropertyName).PropertyType);
                foreach (var value in values)
                {
                    dynamic innerValue = value.GetType().GetProperty(rule.PropertyName).GetValue(value, null);
                    // creating the sum
                    sum += innerValue;
                }
                // building the Func
                dynamic func = BuildGenericFunction(rule, sum);
                // checking the rule Func with the sum value
                if (!func(sum))
                    return false;
            }
            return true;
        }

        public bool ValidateRuleAvg<T>(IEnumerable<T> values, IEnumerable<Rule> rules)
        {
            foreach (var rule in rules)
            {
                dynamic sum = Activator.CreateInstance(values.GetType().GetElementType().GetProperty(rule.PropertyName).PropertyType);
                var counter = 0;
                foreach (var value in values)
                {
                    dynamic innerValue = value.GetType().GetProperty(rule.PropertyName).GetValue(value, null);
                    sum += innerValue;
                    counter++;
                }
                dynamic avg = sum / counter;
                dynamic func = BuildGenericFunction(rule, avg);
                if (!func(avg))
                    return false;
            }
            return true;
        }

        public bool ValidateValuesAny<T>(List<T> values, Func<T, bool> rule)
        {
            foreach (var value in values)
            {
                if (rule(value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ValidateValuesAny<T>(List<T> values, Func<T, bool>[] rules)
        {
            foreach (var value in values)
            {
                bool validated = false;
                foreach (var rule in rules)
                {
                    if (rule(value))
                    {
                        validated = true;
                    }
                    else
                    {
                        validated = false;
                        break;
                    }
                }
                if (validated)
                    return true;
            }
            return true;
        }

        private object BuildGenericFunction(Rule rule, object sum)
        {
            ExpressionBuilder expressionBuilder = new ExpressionBuilder();
            System.Type specificType = sum.GetType();
            var param = Expression.Parameter(specificType);
            Expression expression = expressionBuilder.BuildExpression(specificType, rule.Operator_, rule.Value, param);
            MethodInfo method = this.GetType().GetMethod("BuildLambdaFunc",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(specificType);
            object func = generic.Invoke(this, new object[] { expression, param });
            return func;
        }

        private Func<T, bool> BuildLambdaFunc<T>(Expression expression, ParameterExpression param)
        {
            // building the lambda function
            Func<T, bool> func = Expression.Lambda<Func<T, bool>>(expression, param).Compile();
            return func;
        }
    }
}
