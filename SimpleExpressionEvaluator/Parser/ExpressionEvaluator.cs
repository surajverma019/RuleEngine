using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleExpressionEvaluator.AbstractSyntaxTree;
using LikeParser;

namespace SimpleExpressionEvaluator
{
    public class ThenElse
    {
        public AbstractSyntaxTreeNode ThenNode;
        public AbstractSyntaxTreeNode ElseNode;
    }

    public class ExpressionEvaluatorExecutor
    {
        public Stack<AbstractSyntaxTreeNode> valueStack = new Stack<AbstractSyntaxTreeNode>();
        private Dictionary<string, AbstractSyntaxTreeNode> setList = new Dictionary<string, AbstractSyntaxTreeNode>();
        private Dictionary<Tuple<string, string>, List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>> thenList = 
            new Dictionary<Tuple<string, string>, List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>>();
        private Dictionary<Tuple<string, string>, List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>> elseList =
            new Dictionary<Tuple<string, string>, List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>>();

        public bool Evaluate<T>(List<AbstractSyntaxTreeNode> postfixList,
            Dictionary<string, AbstractSyntaxTreeNode> symbolTable, T objectValue)
        {
            int setNameValue = 0;
            bool thenNode = false;
            bool elseNode = false;
            foreach (var item in postfixList)
            {
                if (item is IntegerNode || item is DoubleNode || item is DateTimeNode ||
                    item is BooleanNode || item is StringNode || item is NullNode)
                {
                    valueStack.Push(item);
                }
                else if (item is SetNode)
                {
                    var equalNode = postfixList[setNameValue - 1];
                    var valueNode = postfixList[setNameValue - 2];
                    var nameNode = postfixList[setNameValue - 3];
                    if (equalNode is EqualNode)
                    {
                        var pop = valueStack.Pop();
                        setList.Add(((VariableNode)nameNode).Name, valueNode);
                    }
                    else
                    {
                        var pop = valueStack.Pop();
                        var methodValueNode = postfixList[setNameValue - 1];
                        var methodNode = postfixList[setNameValue - 2];
                        if(methodNode is VariableNode)
                        {
                            setList.Add(((VariableNode)methodNode).Name, methodValueNode);
                        }
                    }
                }
                else if (item is ThenNode)
                {
                    thenNode = true;                   
                }
                else if(item is ElseNode)
                {
                    elseNode = true;                    
                }
                else if (item is VariableNode)
                {
                    if (symbolTable.ContainsKey(((VariableNode)item).Name))
                    {
                        var property = objectValue.GetType().GetProperty(((VariableNode)item).Name);
                        if (property != null)
                        {
                            object value = property.GetValue(objectValue, null);
                            int intOutValue;
                            double doubleOutValue;
                            bool boolOutValue;
                            DateTime dateTimeOutValue;
                            if(value == null)
                            {
                                NullNode nullNode = new NullNode();
                                valueStack.Push(nullNode);
                            }
                            else if (int.TryParse(value.ToString(), out intOutValue))
                            {
                                IntegerNode integerNode = new IntegerNode();
                                integerNode.Value = intOutValue;
                                valueStack.Push(integerNode);
                            }
                            else if (double.TryParse(value.ToString(), out doubleOutValue))
                            {
                                DoubleNode doubleNode = new DoubleNode();
                                doubleNode.Value = doubleOutValue;
                                valueStack.Push(doubleNode);
                            }
                            else if (bool.TryParse(value.ToString(), out boolOutValue))
                            {
                                BooleanNode booleanNode = new BooleanNode();
                                booleanNode.Value = boolOutValue;
                                valueStack.Push(booleanNode);
                            }                                
                            else if (DateTime.TryParse(value.ToString(), out dateTimeOutValue))
                            {
                                DateTimeNode dateTimeNode = new DateTimeNode();
                                dateTimeNode.Value = dateTimeOutValue;
                                valueStack.Push(dateTimeNode);
                            }
                            else
                            {
                                StringNode stringNode = new StringNode();
                                stringNode.Value = value.ToString();
                                valueStack.Push(stringNode);
                            }
                        }                       
                    }
                }
                else if (item is MethodCallNode)
                {
                    if(!thenNode && !elseNode)
                    {
                        MethodCallNodeExecuteCall(objectValue, item);
                    }
                    else if(thenNode)
                    {
                        var methodNodeName = ((MethodCallNode)item).Name;
                        var list = new List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>();
                        list.Add(new Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>(item, item));
                        thenList.Add(new Tuple<string, string>(methodNodeName, null), list);
                        thenNode = false;
                    }
                    else if(elseNode)
                    {
                        var methodNodeName = ((MethodCallNode)item).Name;
                        var list = new List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>();
                        list.Add(new Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>(item, item));
                        elseList.Add(new Tuple<string, string>(methodNodeName, null), list);
                        elseNode = false;
                    }
                }
                else if (item is AddNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value +
                            ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value + ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value + ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value + ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is SubNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value - ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value - ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value - ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value - ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is MulNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value * ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value * ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value * ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value * ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is DivNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value / ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value / ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value / ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value / ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is ModuloNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value % ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value % ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value % ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value % ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is LikeNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is StringNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((StringNode)firstoperand).Value;
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((IntegerNode)firstoperand).Value.ToString();
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((DoubleNode)firstoperand).Value.ToString();
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((DateTimeNode)firstoperand).Value.ToString();
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is EqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value == ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value == ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value == ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value == ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value == ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value == ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value == ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value == ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value == ((DateTimeNode)secondoperand).Value.ToString();
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value.ToString() == ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is UnEqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value != ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value != ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value != ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value != ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value != ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value != ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value != ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value != ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value != ((DateTimeNode)secondoperand).Value.ToString();
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value.ToString() != ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is SmallerThenNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value < ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value < ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value < ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value < ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value < ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) < ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value < DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is SmallerThenOrEqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value <= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value <= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value <= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value <= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value <= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) <= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value <= DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is GreaterThenNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value > ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value > ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value > ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value > ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value > ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) > ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value > DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is GreaterThenOrEqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value >= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value >= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value >= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value >= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value >= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) >= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value >= DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is IsNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is NullNode && firstoperand is NullNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = true;
                        valueStack.Push(booleanNode);
                    }
                    else
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = false;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is OrNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value || ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is AndNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value && ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                setNameValue++;
            }
            var lastValue = false;
            var reveredStack = new Stack<AbstractSyntaxTreeNode>();
            while (valueStack.Count > 0)
            {
                reveredStack.Push(valueStack.Pop());
            }
            var ebene = reveredStack.Count - 1;
            while (reveredStack.Count > 0)
            {
                var endValue = reveredStack.Pop();
                if (endValue is BooleanNode)
                {
                    if (((BooleanNode)endValue).Value)
                    {
                        if (setList.Count > 0)
                        {
                            foreach (var item in setList)
                            {
                                var propertyName = item.Key;
                                var objectToSet = item.Value;
                                var objectCastValue = new object();
                                if (objectToSet is IntegerNode)
                                {
                                    objectCastValue = ((IntegerNode)objectToSet).Value;
                                }
                                else if (objectToSet is DoubleNode)
                                {
                                    objectCastValue = ((DoubleNode)objectToSet).Value;
                                }
                                else if (objectToSet is BooleanNode)
                                {
                                    objectCastValue = ((BooleanNode)objectToSet).Value;
                                }
                                else if (objectToSet is StringNode)
                                {
                                    objectCastValue = ((StringNode)objectToSet).Value;
                                }
                                else if (objectToSet is DateTimeNode)
                                {
                                    objectCastValue = ((DateTimeNode)objectToSet).Value;
                                }
                                var property = objectValue.GetType().GetProperty(propertyName);
                                if (property != null)
                                    property.SetValue(objectValue, objectCastValue, null);
                                else
                                {
                                    var method = objectValue.GetType().GetMethod(propertyName);
                                    if (method != null)
                                    {
                                        method.Invoke(objectValue, new object[] { objectCastValue });
                                    }
                                }
                            }
                        }
                    }
                    if (((BooleanNode)endValue).Value)
                    {
                        if (ebene < thenList.Count)
                        {
                            var item = thenList.ToList()[ebene];
                            MethodCallNodeExecuteCall(objectValue, item.Value.First().Item2);
                            lastValue = ((BooleanNode)endValue).Value;
                            break;
                        }                        
                    }
                    else
                    {
                        if (ebene < elseList.Count)
                        {
                            var item = elseList.ToList()[ebene];
                            MethodCallNodeExecuteCall(objectValue, item.Value.First().Item2);
                            lastValue = ((BooleanNode)endValue).Value;
                            break;
                        }                                        
                    }
                    lastValue = ((BooleanNode)endValue).Value;                    
                }                
                ebene--;
            }
            return lastValue;
        }

        public R EvaluateNonBoolean<T,R>(List<AbstractSyntaxTreeNode> postfixList,
           Dictionary<string, AbstractSyntaxTreeNode> symbolTable, T objectValue)
        {
            int setNameValue = 0;
            bool thenNode = false;
            bool elseNode = false;
            foreach (var item in postfixList)
            {
                if (item is IntegerNode || item is DoubleNode || item is DateTimeNode ||
                    item is BooleanNode || item is StringNode || item is NullNode)
                {
                    valueStack.Push(item);
                }
                else if (item is SetNode)
                {
                    var equalNode = postfixList[setNameValue - 1];
                    var valueNode = postfixList[setNameValue - 2];
                    var nameNode = postfixList[setNameValue - 3];
                    if (equalNode is EqualNode)
                    {
                        var pop = valueStack.Pop();
                        setList.Add(((VariableNode)nameNode).Name, valueNode);
                    }
                    else
                    {
                        var pop = valueStack.Pop();
                        var methodValueNode = postfixList[setNameValue - 1];
                        var methodNode = postfixList[setNameValue - 2];
                        if (methodNode is VariableNode)
                        {
                            setList.Add(((VariableNode)methodNode).Name, methodValueNode);
                        }
                    }
                }
                else if (item is ThenNode)
                {
                    thenNode = true;
                }
                else if (item is ElseNode)
                {
                    elseNode = true;
                }
                else if (item is VariableNode)
                {
                    if (symbolTable.ContainsKey(((VariableNode)item).Name))
                    {
                        var property = objectValue.GetType().GetProperty(((VariableNode)item).Name);
                        if (property != null)
                        {
                            object value = property.GetValue(objectValue, null);
                            int intOutValue;
                            double doubleOutValue;
                            bool boolOutValue;
                            DateTime dateTimeOutValue;
                            if (value == null)
                            {
                                NullNode nullNode = new NullNode();
                                valueStack.Push(nullNode);
                            }
                            else if (int.TryParse(value.ToString(), out intOutValue))
                            {
                                IntegerNode integerNode = new IntegerNode();
                                integerNode.Value = intOutValue;
                                valueStack.Push(integerNode);
                            }
                            else if (double.TryParse(value.ToString(), out doubleOutValue))
                            {
                                DoubleNode doubleNode = new DoubleNode();
                                doubleNode.Value = doubleOutValue;
                                valueStack.Push(doubleNode);
                            }
                            else if (bool.TryParse(value.ToString(), out boolOutValue))
                            {
                                BooleanNode booleanNode = new BooleanNode();
                                booleanNode.Value = boolOutValue;
                                valueStack.Push(booleanNode);
                            }
                            else if (DateTime.TryParse(value.ToString(), out dateTimeOutValue))
                            {
                                DateTimeNode dateTimeNode = new DateTimeNode();
                                dateTimeNode.Value = dateTimeOutValue;
                                valueStack.Push(dateTimeNode);
                            }
                            else
                            {
                                StringNode stringNode = new StringNode();
                                stringNode.Value = value.ToString();
                                valueStack.Push(stringNode);
                            }
                        }
                    }
                }
                else if (item is MethodCallNode)
                {
                    if (!thenNode && !elseNode)
                    {
                        MethodCallNodeExecuteCall(objectValue, item);
                    }
                    else if (thenNode)
                    {
                        var methodNodeName = ((MethodCallNode)item).Name;
                        var list = new List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>();
                        list.Add(new Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>(item, item));
                        thenList.Add(new Tuple<string, string>(methodNodeName, null), list);
                        thenNode = false;
                    }
                    else if (elseNode)
                    {
                        var methodNodeName = ((MethodCallNode)item).Name;
                        var list = new List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>>();
                        list.Add(new Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>(item, item));
                        elseList.Add(new Tuple<string, string>(methodNodeName, null), list);
                        elseNode = false;
                    }
                }
                else if (item is AddNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value +
                            ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value + ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value + ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value + ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is SubNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value - ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value - ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value - ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value - ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is MulNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value * ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value * ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value * ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value * ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is DivNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value / ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value / ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value / ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value / ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is ModuloNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        IntegerNode integerNode = new IntegerNode();
                        integerNode.Value = ((IntegerNode)firstoperand).Value % ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((DoubleNode)firstoperand).Value % ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        DoubleNode integerNode = new DoubleNode();
                        integerNode.Value = ((DoubleNode)firstoperand).Value % ((IntegerNode)secondoperand).Value;
                        valueStack.Push(integerNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        DoubleNode doubleNode = new DoubleNode();
                        doubleNode.Value = ((IntegerNode)firstoperand).Value % ((DoubleNode)secondoperand).Value;
                        valueStack.Push(doubleNode);
                    }
                }
                else if (item is LikeNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is StringNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((StringNode)firstoperand).Value;
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((IntegerNode)firstoperand).Value.ToString();
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((DoubleNode)firstoperand).Value.ToString();
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var firstoperatorString = ((DateTimeNode)firstoperand).Value.ToString();
                        var secondperatorString = ((StringNode)secondoperand).Value;
                        var result = Like(firstoperatorString, secondperatorString);
                        booleanNode.Value = result;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is EqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value == ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value == ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value == ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value == ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value == ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value == ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value == ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value == ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value == ((DateTimeNode)secondoperand).Value.ToString();
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value.ToString() == ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is UnEqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value != ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value != ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value != ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value != ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value != ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value != ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value != ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value != ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((StringNode)firstoperand).Value != ((DateTimeNode)secondoperand).Value.ToString();
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value.ToString() != ((StringNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is SmallerThenNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value < ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value < ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value < ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value < ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value < ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) < ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value < DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is SmallerThenOrEqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value <= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value <= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value <= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value <= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value <= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) <= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value <= DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is GreaterThenNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value > ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value > ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value > ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value > ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value > ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) > ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value > DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is GreaterThenOrEqualNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is IntegerNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value >= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value >= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is IntegerNode && firstoperand is DoubleNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DoubleNode)firstoperand).Value >= ((IntegerNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DoubleNode && firstoperand is IntegerNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((IntegerNode)firstoperand).Value >= ((DoubleNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value >= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is DateTimeNode && firstoperand is StringNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)firstoperand).Value;
                        booleanNode.Value = DateTime.Parse(str) >= ((DateTimeNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                    else if (secondoperand is StringNode && firstoperand is DateTimeNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        var str = ((StringNode)secondoperand).Value;
                        booleanNode.Value = ((DateTimeNode)firstoperand).Value >= DateTime.Parse(str);
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is IsNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is NullNode && firstoperand is NullNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = true;
                        valueStack.Push(booleanNode);
                    }
                    else
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = false;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is OrNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value || ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                else if (item is AndNode)
                {
                    var secondoperand = valueStack.Pop();
                    var firstoperand = valueStack.Pop();
                    if (secondoperand is BooleanNode && firstoperand is BooleanNode)
                    {
                        BooleanNode booleanNode = new BooleanNode();
                        booleanNode.Value = ((BooleanNode)firstoperand).Value && ((BooleanNode)secondoperand).Value;
                        valueStack.Push(booleanNode);
                    }
                }
                setNameValue++;
            }
            var lastValue = false;
            var reveredStack = new Stack<AbstractSyntaxTreeNode>();
            while (valueStack.Count > 0)
            {
                reveredStack.Push(valueStack.Pop());
            }
            var ebene = reveredStack.Count - 1;
            while (reveredStack.Count > 0)
            {
                var endValue = reveredStack.Pop();
                if (endValue is BooleanNode)
                {
                    if (((BooleanNode)endValue).Value)
                    {
                        if (setList.Count > 0)
                        {
                            foreach (var item in setList)
                            {
                                var propertyName = item.Key;
                                var objectToSet = item.Value;
                                var objectCastValue = new object();
                                if (objectToSet is IntegerNode)
                                {
                                    objectCastValue = ((IntegerNode)objectToSet).Value;
                                }
                                else if (objectToSet is DoubleNode)
                                {
                                    objectCastValue = ((DoubleNode)objectToSet).Value;
                                }
                                else if (objectToSet is BooleanNode)
                                {
                                    objectCastValue = ((BooleanNode)objectToSet).Value;
                                }
                                else if (objectToSet is StringNode)
                                {
                                    objectCastValue = ((StringNode)objectToSet).Value;
                                }
                                else if (objectToSet is DateTimeNode)
                                {
                                    objectCastValue = ((DateTimeNode)objectToSet).Value;
                                }
                                var property = objectValue.GetType().GetProperty(propertyName);
                                if (property != null)
                                    property.SetValue(objectValue, objectCastValue, null);
                                else
                                {
                                    var method = objectValue.GetType().GetMethod(propertyName);
                                    if (method != null)
                                    {
                                        method.Invoke(objectValue, new object[] { objectCastValue });
                                    }
                                }
                            }
                        }
                    }
                    if (((BooleanNode)endValue).Value)
                    {
                        if (ebene < thenList.Count)
                        {
                            var item = thenList.ToList()[ebene];
                            MethodCallNodeExecuteCall(objectValue, item.Value.First().Item2);
                            lastValue = ((BooleanNode)endValue).Value;
                            break;
                        }
                    }
                    else
                    {
                        if (ebene < elseList.Count)
                        {
                            var item = elseList.ToList()[ebene];
                            MethodCallNodeExecuteCall(objectValue, item.Value.First().Item2);
                            lastValue = ((BooleanNode)endValue).Value;
                            break;
                        }
                    }
                    lastValue = ((BooleanNode)endValue).Value;
                }
                else if (endValue is IntegerNode)
                {
                    return (R)Convert.ChangeType(((IntegerNode)endValue).Value, typeof(R));
                }
                else if (endValue is DoubleNode)
                {
                    return (R)Convert.ChangeType(((IntegerNode)endValue).Value, typeof(R));
                }
                ebene--;
            }
            return (R)Convert.ChangeType(lastValue, typeof(R));
        }

        private void MethodCallNodeExecuteCall<T>(T objectValue, AbstractSyntaxTreeNode item)
        {
            var method = objectValue.GetType().GetMethod(((MethodCallNode)item).Name);
            if (method != null)
            {
                var objectList = new List<object>();
                for (int parameterCount = 0; parameterCount < ((MethodCallNode)item).parameterList.Count; parameterCount++)
                {
                    var parameter = ((MethodCallNode)item).parameterList[parameterCount];
                    if (parameter is StringNode)
                    {
                        var value = ((StringNode)parameter).Value;
                        DateTime dateTime = new DateTime();
                        if (DateTime.TryParse(value, out dateTime))
                        {
                            objectList.Add(dateTime);
                        }
                        else
                        {
                            objectList.Add(value);
                        }
                    }
                    else if (parameter is DateTimeNode)
                    {
                        var value = ((DateTimeNode)parameter).Value;
                        objectList.Add(value);
                    }
                    else if (parameter is BooleanNode)
                    {
                        var value = ((BooleanNode)parameter).Value;
                        objectList.Add(value);
                    }
                    else if (parameter is DoubleNode)
                    {
                        var value = ((DoubleNode)parameter).Value;
                        objectList.Add(value);
                    }
                    else if (parameter is IntegerNode)
                    {
                        var value = ((IntegerNode)parameter).Value;
                        objectList.Add(value);
                    }
                }
                var result = method.Invoke(objectValue, objectList.ToArray());
                int intOutValue;
                double doubleOutValue;
                bool boolOutValue;
                DateTime dateTimeOutValue;
                if (result == null)
                {
                    NullNode nullNode = new NullNode();
                    valueStack.Push(nullNode);
                }
                else if (int.TryParse(result.ToString(), out intOutValue))
                {
                    IntegerNode integerNode = new IntegerNode();
                    integerNode.Value = intOutValue;
                    valueStack.Push(integerNode);
                }
                else if (double.TryParse(result.ToString(), out doubleOutValue))
                {
                    DoubleNode doubleNode = new DoubleNode();
                    doubleNode.Value = doubleOutValue;
                    valueStack.Push(doubleNode);
                }
                else if (bool.TryParse(result.ToString(), out boolOutValue))
                {
                    BooleanNode booleanNode = new BooleanNode();
                    booleanNode.Value = boolOutValue;
                    valueStack.Push(booleanNode);
                }
                else if (DateTime.TryParse(result.ToString(), out dateTimeOutValue))
                {
                    DateTimeNode dateTimeNode = new DateTimeNode();
                    dateTimeNode.Value = dateTimeOutValue;
                    valueStack.Push(dateTimeNode);
                }
                else
                {
                    StringNode stringNode = new StringNode();
                    stringNode.Value = result.ToString();
                    valueStack.Push(stringNode);
                }
            }
        }

        private bool Like(string firstoperatorString, string secondperatorString)
        {
            StateMaschineBuilder stateMaschineBuilder = new StateMaschineBuilder();
            StateMaschine stateMaschine = stateMaschineBuilder.Match(secondperatorString);
            var result = stateMaschineBuilder.Recognize(firstoperatorString, stateMaschine);
            return result;
        }

        private string FindParameterList(List<Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>> list, 
            List<AbstractSyntaxTreeNode> postfixList,
            int actualPosition, Stack<AbstractSyntaxTreeNode> valueStack, bool firstCall)
        {
            var actualParameter = postfixList[actualPosition];
            if(actualParameter is ThenNode)
            {
                return FindParameterList(list, postfixList, actualPosition - 1, valueStack, true);
            }
            else if (actualParameter is ElseNode)
            {
                var pop = valueStack.Pop();
                return FindParameterList(list, postfixList, actualPosition - 1, valueStack, true);
            }
            else if(actualParameter is IntegerNode || actualParameter is StringNode || actualParameter is BooleanNode || 
                                actualParameter is DateTimeNode || actualParameter is DoubleNode)
            {
                AbstractSyntaxTreeNode pop = null;
                if(!firstCall)
                    pop = valueStack.Pop();
                list.Add(new Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>(actualParameter, null));
                return FindParameterList(list, postfixList, actualPosition - 1, valueStack, false);
            }
            else
            {
                if (actualParameter is VariableNode)
                    return ((VariableNode)actualParameter).Name;
                else if(actualParameter is MethodCallNode)
                {
                    foreach (var parameter in ((MethodCallNode)actualParameter).parameterList)
                    {
                        list.Add(new Tuple<AbstractSyntaxTreeNode, AbstractSyntaxTreeNode>(parameter, null));
                    }
                    return ((MethodCallNode)actualParameter).Name;
                }
                else
                {
                    return actualParameter.GetType().Name;
                }
            }
        }
    }
}
