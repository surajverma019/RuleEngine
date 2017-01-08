using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleExpressionEvaluator.AbstractSyntaxTree;
using SimpleExpressionEvaluator.Lexer;

namespace SimpleExpressionEvaluator.Parser
{
    public class ExpressionEvaluatorParser : Parser
    {
         //implementation with List and Operator Stack convert infix espression (5 + 3 * 2 - 1) to postfix expression and then calculate value ( 10 )
         //first convert infix to postfix expression and save in list (5 + 3 * 2 - 1 => 5 3 2 * 1 - +)
         //5 => to List ( 5 )
         //+ => to Operator Stack ( + )
         //3 => to List ( 5, 3 )
         //* => to Operator Stack ( +, * )
         //2 => to List ( 5, 3, 2 )
         //- => lower precedence than * on Stack => to List ( 5, 3, 2, * ) => Operator Stack ( +, - )
         //1 => to List ( 5, 3, 2, *, 1 )
         //pop Operator Stack ( +, - ) => to List ( 5, 3, 2, *, 1, -, + )
         //second calculate the result out of the genenerated list with the postfix expression with a Stack
         //5 => Stack ( 5 )
         //3 => Stack ( 5, 3 )
         //2 => Stack ( 5, 3, 2 )
         //* => 2 from Stack 3 from Stack change operators ( 5 ) => calculate 3 * 2 = 6 => Stack ( 5, 6 )
         //1 => Stack ( 5, 6, 1 )
         //- => 1 from Stack 6 from Stack change operators ( 5 ) => calculate 6 - 1 = 5 => Stack ( 5, 5 )
         //+ => 5 from Stack 5 from Stack change operators ( ) => calculate 5 + 5 => Stack ( 10 )
        //public Stack<AbstractSyntaxTreeNode> lastNodes = new Stack<AbstractSyntaxTreeNode>();
        //public Stack<AbstractSyntaxTreeNode> numberStack = new Stack<AbstractSyntaxTreeNode>();
        //public Stack<AbstractSyntaxTreeNode> operatorStack = new Stack<AbstractSyntaxTreeNode>();
        private Dictionary<string, AbstractSyntaxTreeNode> symbolTable = new Dictionary<string, AbstractSyntaxTreeNode>();

        public Dictionary<string, AbstractSyntaxTreeNode> SymbolTable
        {
            get { return symbolTable; }
            set { symbolTable = value; }
        }
        
        public ExpressionEvaluatorParser(SimpleExpressionEvaluator.Lexer.Lexer lexer)
            : base(lexer, 1)
        {

        }

        public List<AbstractSyntaxTreeNode> BuildParseTree()
        {
            if (parserTree == null)
            {
                parserTree = new ParseTree();
            }
            Stack<AbstractSyntaxTreeNode> operatorStack = new Stack<AbstractSyntaxTreeNode>();
            while (lookahead[lookaheadIndex].Type_ != TokenType.EOF)
            {
                parserTree.AddChild(BuildAST());
            }
            InfixToPostfix infixToPostfix = new InfixToPostfix();
            var infixToPostfixResult = infixToPostfix.ConvertInfixToPostfix(parserTree.RootNodes);
            return infixToPostfixResult;
        }

        public AbstractSyntaxTreeNode BuildAST()
        {                                    
            if (lookahead[lookaheadIndex].Type_ == TokenType.EQUAL)
            {
                return Equal();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.UNEQUAL)
            {
                return Unequal();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.SMALLERTHEN)
            {
                return Smaller();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.SMALLERTHENOREQUAL)
            {
                return Smallerequal();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.GREATERTHEN)
            {
                return Greater();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.GREATERTHENOREQUAL)
            {
                return Greaterequal();
            }            
            else if (lookahead[lookaheadIndex].Type_ == TokenType.OPENBRACKET)
            {
                return OpenBracket();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.CLOSEBRACKET)
            {
                return CloseBracket();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.PLUS)
            {
                return Plus();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.MINUS)
            {
                return Minus();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.MUL)
            {
                return Mulibly();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.DIV)
            {
                return Divide();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.MOD)
            {
                return Modulo();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.IDENT)
            {
                var methodCall = CheckMethodCall(lookahead[lookaheadIndex].Text);
                if (methodCall.Item1)
                {
                    return methodCall.Item2;
                }
                return Ident(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.SET)
            {
                return Set(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.THEN)
            {
                return Then(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.ELSE)
            {
                return Else(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.IS)
            {
                return Is(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.LIKE)
            {
                return Like(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.NULL)
            {
                return Null(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.INTEGER)
            {
                return Integer(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.DOUBLE)
            {
                return Double(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.BOOL)
            {
                return Bool(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.STRING)
            {
                return String(lookahead[lookaheadIndex].Text);
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.OR)
            {
                return Or();
            }            
            else if (lookahead[lookaheadIndex].Type_ == TokenType.AND)
            {
                return And();
            }
            else if (lookahead[lookaheadIndex].Type_ == TokenType.COMMA)
            {
                return Comma();
            }
            return null;
        }

        public Tuple<bool, MethodCallNode> CheckMethodCall(string value)
        {
            MethodCallNode methodCallNode = new MethodCallNode();
            methodCallNode.Name = value;
            var currentToken = NextToken();
            if (currentToken.Type_ == TokenType.OPENBRACKET)
            {
                Match(TokenType.IDENT);
                return new Tuple<bool, MethodCallNode>(true, methodCallNode);
            }
            else if(currentToken.Type_ == TokenType.LIKE)
            {
                Rewind();
                return new Tuple<bool, MethodCallNode>(false, null);
            }
            else
            {                
                return new Tuple<bool, MethodCallNode>(false, null);
            }         
        }

        public VariableNode Ident(string value)
        {
            VariableNode identNode = new VariableNode();
            identNode.Name = value;
            Match(TokenType.IDENT);
            var currentToken = CurrentToken();            
            if (!symbolTable.ContainsKey(value))
                symbolTable.Add(value, identNode);
            return identNode;
        }

        public SetNode Set(string value)
        {
            SetNode setNode = new SetNode();
            Match(TokenType.SET);
            return setNode;
        }

        public ThenNode Then(string value)
        {
            ThenNode thenNode = new ThenNode();
            Match(TokenType.THEN);
            return thenNode;
        }

        public ElseNode Else(string value)
        {
            ElseNode elseNode = new ElseNode();
            Match(TokenType.ELSE);
            return elseNode;
        }

        public IsNode Is(string value)
        {
            IsNode isNode = new IsNode();
            Match(TokenType.IS);
            return isNode;
        }

        public LikeNode Like(string value)
        {
            LikeNode likeNode = new LikeNode();
            Match(TokenType.LIKE);
            return likeNode;
        }

        public NullNode Null(string value)
        {
            NullNode nullNode = new NullNode();
            Match(TokenType.NULL);
            return nullNode;
        }

        public IntegerNode Integer(string value)
        {
            IntegerNode integerNode = new IntegerNode();
            Match(TokenType.INTEGER);            
            integerNode.Value = int.Parse(value);
            return integerNode;
        }

        public BooleanNode Bool(string value)
        {
            BooleanNode booleanNode = new BooleanNode();
            Match(TokenType.BOOL);
            booleanNode.Value = bool.Parse(value);
            return booleanNode;
        }

        public StringNode String(string value)
        {
            StringNode stringNode = new StringNode();
            Match(TokenType.STRING);
            stringNode.Value = value;
            return stringNode;
        }

        public DoubleNode Double(string value)
        {
            DoubleNode doubleNode = new DoubleNode();
            Match(TokenType.DOUBLE);            
            doubleNode.Value = double.Parse(value);
            return doubleNode;
        }

        public AddNode Plus()
        {
            AddNode addNode = new AddNode();
            Match(TokenType.PLUS);                      
            return addNode;
        }
        
        public SubNode Minus()
        {
            SubNode subNode = new SubNode();
            Match(TokenType.MINUS);            
            return subNode;
        }

        public MulNode Mulibly()
        {
            MulNode mulNode = new MulNode();
            Match(TokenType.MUL);            
            return mulNode;
        }

        public DivNode Divide()
        {
            DivNode divNode = new DivNode();
            Match(TokenType.DIV);           
            return divNode;
        }

        public ModuloNode Modulo()
        {
            ModuloNode modNode = new ModuloNode();
            Match(TokenType.MOD);
            return modNode;
        }

        public OpenBracketNode OpenBracket()
        {
            OpenBracketNode bracketNode = new OpenBracketNode();
            Match(TokenType.OPENBRACKET);
            return bracketNode;
        }

        public CloseBracketNode CloseBracket()
        {
            CloseBracketNode closeBracketNode = new CloseBracketNode();            
            Match(TokenType.CLOSEBRACKET);
            return closeBracketNode;
        }

        public EqualNode Equal()
        {
            EqualNode equalNode = new EqualNode();
            Match(TokenType.EQUAL);
            return equalNode;
        }

        public UnEqualNode Unequal()
        {
            UnEqualNode unequalNode = new UnEqualNode();
            Match(TokenType.UNEQUAL);
            return unequalNode;
        }

        public SmallerThenNode Smaller()
        {
            SmallerThenNode smallerThenNode = new SmallerThenNode();
            Match(TokenType.SMALLERTHEN);
            return smallerThenNode;
        }

        public CommaNode Comma()
        {
            var commaNode = new CommaNode();
            Match(TokenType.COMMA);
            return commaNode;
        }

        public SmallerThenOrEqualNode Smallerequal()
        {
            SmallerThenOrEqualNode smallerThenOrEqualNode = new SmallerThenOrEqualNode();
            Match(TokenType.SMALLERTHENOREQUAL);
            return smallerThenOrEqualNode;
        }

        public GreaterThenNode Greater()
        {
            GreaterThenNode greaterThenNode = new GreaterThenNode();
            Match(TokenType.GREATERTHEN);
            return greaterThenNode;
        }

        public GreaterThenOrEqualNode Greaterequal()
        {
            GreaterThenOrEqualNode greaterThenOrEqualNode = new GreaterThenOrEqualNode();
            Match(TokenType.GREATERTHENOREQUAL);
            return greaterThenOrEqualNode;
        }

        public OrNode Or()
        {
            OrNode orNode = new OrNode();
            Match(TokenType.OR);            
            return orNode;
        }

        public AndNode And()
        {
            AndNode andNode = new AndNode();
            Match(TokenType.AND);
            return andNode;
        }

        //public AndNode And()
        //{
        //    AndNode andNode = new AndNode();
        //    Match(TokenType.COMMA);
        //    return andNode;
        //}
    }
}
