using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRuleEngine
{
    class Part1
    {
        static void Main(string[] args)
        {
            RuleLoader ruleLoader = new RuleLoader();
            Rule firstRule = ruleLoader.Load(1);
            Rule secondRule = ruleLoader.Load(2);
            Rule thirdRule = ruleLoader.Load(3);

            Person person = new Person() { Name = "Mathias", Age = 35, Children = 2 };

            RuleEngine ruleEngine = new RuleEngine();
            var firstRuleFunc = ruleEngine.CompileRule<Person>(firstRule);
            var secondRuleFunc = ruleEngine.CompileRule<Person>(secondRule);
            var thirdRuleFunc = ruleEngine.CompileRule<Person>(thirdRule);
            var result = firstRuleFunc(person) && secondRuleFunc(person) && thirdRuleFunc(person);

            Console.WriteLine(firstRuleFunc(person));
            Console.WriteLine(secondRuleFunc);
            Console.WriteLine(thirdRuleFunc);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
