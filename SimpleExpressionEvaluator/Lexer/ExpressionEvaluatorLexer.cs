using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleExpressionEvaluator.Lexer
{
    public class ExpressionEvaluatorLexer : Lexer
    {
        public ExpressionEvaluatorLexer(string input, int lookaheadCount)
            : base(input, lookaheadCount)
        {

        }

        public override Token LookAhead()
        {
            savedIndex = index;
            while (lookahead[lookaheadIndex] != EOF)
            {
                switch (lookahead[lookaheadIndex])
                {
                    case '.': DonotConsumeItem(); return new Token(TokenType.POINT, ".");
                    //case ',': ConsumeItem(); return new Token(TokenType.COMMA, ",");
                    case '+': DonotConsumeItem(); return Plus();
                    case ',': DonotConsumeItem(); return Comma();
                    case '-': DonotConsumeItem(); return Minus();
                    case '*': DonotConsumeItem(); return Multibly();
                    case '/': DonotConsumeItem(); return Divide();
                    case '%': DonotConsumeItem(); return Modulo();
                    case '<': DonotConsumeItem(); return Smaller();
                    case '>': DonotConsumeItem(); return Greater();
                    case '=': DonotConsumeItem(); return Equal();
                    case '!': DonotConsumeItem(); return Unequal();
                    case '"': DonotConsumeItem(); return String();
                    case '\'': DonotConsumeItem(); return SingleQuoteString();
                    case '|': DonotConsumeItem(); return Or();
                    case '&': DonotConsumeItem(); return And();
                    case '(': DonotConsumeItem(); return new Token(TokenType.OPENBRACKET, "(");
                    case ')': DonotConsumeItem(); return new Token(TokenType.CLOSEBRACKET, ")");
                    default:
                        if (IsLetter())
                        {
                            return Terminal();
                        }
                        if (IsNumber())
                        {
                            return Number();
                        }
                        if (IsWhitespace())
                        {
                            ConsumeItem();
                            continue;
                        }
                        throw new Exception("invalid character." + lookahead[lookaheadIndex]);
                }
            }
            return new Token(TokenType.EOF, "<EOF>");
        }

        public override Token NextToken()
        {
            while (lookahead[lookaheadIndex] != EOF)
            {
                switch (lookahead[lookaheadIndex])
                {
                    case '.': ConsumeItem(); return new Token(TokenType.POINT, ".");
                    //case ',': ConsumeItem(); return new Token(TokenType.COMMA, ",");
                    case '+': ConsumeItem(); return Plus();
                    case ',': ConsumeItem(); return Comma();
                    case '-': ConsumeItem(); return Minus();
                    case '*': ConsumeItem(); return Multibly();
                    case '/': ConsumeItem(); return Divide();
                    case '%': ConsumeItem(); return Modulo();
                    case '<': ConsumeItem(); return Smaller();
                    case '>': ConsumeItem(); return Greater();
                    case '=': ConsumeItem(); return Equal();
                    case '!': ConsumeItem(); return Unequal();
                    case '"': ConsumeItem(); return String();
                    case '\'': ConsumeItem(); return SingleQuoteString();
                    case '|': ConsumeItem(); return Or();
                    case '&': ConsumeItem(); return And();                    
                    case '(': ConsumeItem(); return new Token(TokenType.OPENBRACKET, "(");
                    case ')': ConsumeItem(); return new Token(TokenType.CLOSEBRACKET, ")");
                    default:                        
                        if (IsLetter())
                        {
                            return Terminal();
                        }
                        if (IsNumber())
                        {
                            return Number();
                        }
                        if (IsWhitespace())
                        {
                            ConsumeItem();
                            continue;
                        }
                        throw new Exception("invalid character." + lookahead[lookaheadIndex]);
                }
            }
            return new Token(TokenType.EOF, "<EOF>");
        }

        private Token Number()
        {
            bool hasPoint = false;
            StringBuilder stringBuilder = new StringBuilder();
            while (IsNumber() || IsPoint() || IsExponent())
            {
                if (IsPoint())
                {
                    hasPoint = true;
                }
                stringBuilder.Append(lookahead[lookaheadIndex]);
                ConsumeItem();
            }
            if (hasPoint)
                return new Token(TokenType.DOUBLE, stringBuilder.ToString());
            return new Token(TokenType.INTEGER, stringBuilder.ToString());
        }
       
        private Token String()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (lookahead[lookaheadIndex] != '"')
            {
                if (lookahead[lookaheadIndex] == EOF)
                {
                    break;
                }
                stringBuilder.Append(lookahead[lookaheadIndex]);
                ConsumeItem();
            }
            ConsumeItem();
            return new Token(TokenType.STRING, stringBuilder.ToString());
        }

        private Token SingleQuoteString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (lookahead[lookaheadIndex] != '\'')
            {
                if (lookahead[lookaheadIndex] == EOF)
                {
                    break;
                }
                stringBuilder.Append(lookahead[lookaheadIndex]);
                ConsumeItem();
            }
            ConsumeItem();
            return new Token(TokenType.STRING, stringBuilder.ToString());
        }

        private BoolResult IsBool(string str)
        {
            BoolResult boolResult = new BoolResult();
            StringBuilder stringBuilder = new StringBuilder();            
            if (str == "true")
            {
                boolResult.Value = true;
                boolResult.IsBool = true;
            }
            else if (str == "false")
            {
                boolResult.Value = false;
                boolResult.IsBool = true;
            }
            return boolResult;
        }

        private class BoolResult
        {
            public bool IsBool
            {
                get;
                set;
            }

            public bool Value
            {
                get;
                set;
            }
        }

        private Token Equal()
        {
            return new Token(TokenType.EQUAL, "=");                       
        }

        private Token Unequal()
        {
            if (lookahead[lookaheadIndex] == '=')
            {
                ConsumeItem();
                return new Token(TokenType.UNEQUAL, "!=");
            }
            else
            {
                return new Token(TokenType.UNEQUAL, "!=");
            }   
        }    

        private Token Or()
        {
            if (lookahead[lookaheadIndex] == '|')
            {
                ConsumeItem();
                return new Token(TokenType.OR, "||");
            }
            else
            {
                return new Token(TokenType.OR, "||");
            }            
        }

        private Token And()
        {
            if (lookahead[lookaheadIndex] == '&')
            {
                ConsumeItem();
                return new Token(TokenType.AND, "&&");
            }
            else
            {
                return new Token(TokenType.AND, "&&");
            }
        }

        private Token Plus()
        {
            return new Token(TokenType.PLUS, "+");
        }

        private Token Comma()
        {
            return new Token(TokenType.COMMA, ",");
        }

        private Token Minus()
        {
            return new Token(TokenType.MINUS, "-");
        }

        private Token Multibly()
        {
            return new Token(TokenType.MUL, "*");
        }

        private Token Divide()
        {
            return new Token(TokenType.DIV, "/");
        }

        private Token Modulo()
        {
            return new Token(TokenType.MOD, "%");
        }

        private Token Greater()
        {
            if (lookahead[lookaheadIndex] == '=')
            {
                ConsumeItem();
                return new Token(TokenType.GREATERTHENOREQUAL, ">=");
            }
            else
            {
                return new Token(TokenType.GREATERTHEN, ">");
            }
        }

        private Token Smaller()
        {
            if (lookahead[lookaheadIndex] == '=')
            {
                ConsumeItem();
                return new Token(TokenType.SMALLERTHENOREQUAL, "<=");
            }
            else
            {
                return new Token(TokenType.SMALLERTHEN, "<");
            }
        }

        private Token Terminal()
        {
            StringBuilder stringBuilder = new StringBuilder();
            do
            {
                stringBuilder.Append(lookahead[lookaheadIndex]);
                ConsumeItem();
            } while (IsLetterOrNummer());
            if (IsBool(stringBuilder.ToString()).IsBool)
            {
                return new Token(TokenType.BOOL, stringBuilder.ToString());
            }
            switch (stringBuilder.ToString())
            {
                case "set":
                case "SET":
                case "Set":
                    return new Token(TokenType.SET, stringBuilder.ToString());
                case "then":
                case "THEN":
                case "Then":
                    return new Token(TokenType.THEN, stringBuilder.ToString());
                case "else":
                case "ELSE":
                case "Else":
                    return new Token(TokenType.ELSE, stringBuilder.ToString());
                case "is":
                case "IS":
                case "Is":
                    return new Token(TokenType.IS, stringBuilder.ToString());
                case "null":
                case "NULL":
                case "Null":
                    return new Token(TokenType.NULL, stringBuilder.ToString());
                case "like":
                case "LIKE":
                case "Like":
                    return new Token(TokenType.LIKE, stringBuilder.ToString());
                default:
                    return new Token(TokenType.IDENT, stringBuilder.ToString());
            }
        }

        private void Whitespace()
        {
            while (lookahead[lookaheadIndex] == ' ' || lookahead[lookaheadIndex] == '\t' ||
                lookahead[lookaheadIndex] == '\n' || lookahead[lookaheadIndex] == '\r' ||
                lookahead[lookaheadIndex] == '\b' || lookahead[lookaheadIndex] == '\v')
            {
                ConsumeItem();
            }
        }

        private bool IsWhitespace()
        {
            return lookahead[lookaheadIndex] == ' ' || lookahead[lookaheadIndex] == '\t' ||
                    lookahead[lookaheadIndex] == '\n' || lookahead[lookaheadIndex] == '\r' ||
                    lookahead[lookaheadIndex] == '\b' || lookahead[lookaheadIndex] == '\v';
        }

        private bool IsNumber()
        {
            return (lookahead[lookaheadIndex] >= '0' && lookahead[lookaheadIndex] <= '9') ||
                lookahead[lookaheadIndex] == '-' || lookahead[lookaheadIndex] == '+';
        }

        private bool IsPoint()
        {
            return (lookahead[lookaheadIndex] == '.');
        }

        private bool IsExponent()
        {
            return (lookahead[lookaheadIndex] == 'E' || lookahead[lookaheadIndex] == 'e');
        }

        private bool IsLetter()
        {
            return (lookahead[lookaheadIndex] >= 'a' && lookahead[lookaheadIndex] <= 'z') ||
                (lookahead[lookaheadIndex] >= 'A' && lookahead[lookaheadIndex] <= 'Z') ||
                lookahead[lookaheadIndex] == ';' || lookahead[lookaheadIndex] == '{' ||
                lookahead[lookaheadIndex] == '}' || lookahead[lookaheadIndex] == '[' ||
                lookahead[lookaheadIndex] == ']' || lookahead[lookaheadIndex] == '_' ||
                lookahead[lookaheadIndex] == '=' || lookahead[lookaheadIndex] == '!' ||
                lookahead[lookaheadIndex] == '"' || lookahead[lookaheadIndex] == '%' ||
                lookahead[lookaheadIndex] == '@';
        }

        private bool IsLetterOrNummer()
        {
            return (lookahead[lookaheadIndex] >= 'a' && lookahead[lookaheadIndex] <= 'z') ||
                (lookahead[lookaheadIndex] >= 'A' && lookahead[lookaheadIndex] <= 'Z') ||
                lookahead[lookaheadIndex] == ';' || lookahead[lookaheadIndex] == '{' ||
                lookahead[lookaheadIndex] == '}' || lookahead[lookaheadIndex] == '[' ||
                lookahead[lookaheadIndex] == ']' || lookahead[lookaheadIndex] == '_' ||
                lookahead[lookaheadIndex] == '=' || lookahead[lookaheadIndex] == '!' ||
                lookahead[lookaheadIndex] == '"' || lookahead[lookaheadIndex] == '%' ||
                lookahead[lookaheadIndex] == '@' || (lookahead[lookaheadIndex] >= '0' && lookahead[lookaheadIndex] <= '9');
        }

        public override void Rewind()
        {
            index = savedIndex;            
        }
    }
}
