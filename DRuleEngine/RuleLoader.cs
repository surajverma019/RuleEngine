using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRuleEngine
{
    public class RuleLoader
    {
        public Rule Load(int id)
        {
            switch (id)
            {
                case 1:
                    return new Rule("Name", Operator.NotEqual, "test");
                case 2:
                    return new Rule("Age", Operator.LessThanOrEqual, 50);
                case 3:
                    return new Rule("Children", Operator.GreaterThan, 0);
                case 4:
                    return new Rule("City", Operator.Equal, "New York");
                case 5:
                    return new Rule("ActiveState", Operator.Equal, true);
                case 6:
                    return new Rule("DecimalValue", Operator.GreaterThanOrEqual, 1);
                case 7:
                    return new Rule("DecimalValue", Operator.GreaterThanOrEqual, 1);
                case 8:
                    return new Rule("Married", Operator.Equal, true);
                default:
                    return null;
            }
        }

        //        // Loading dataTable with values from Database
        //        var property = [0].Field(“Property”);
        //        var operatorString = [0].Field(“Operator”);
        //        var operator = ), operatorString);
        //var value = [0].Field(“Value”);
        //return new Rule(property, operator, value);
    }
}
