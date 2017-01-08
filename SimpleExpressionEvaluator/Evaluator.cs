using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleExpressionEvaluator.Lexer;
using SimpleExpressionEvaluator.Parser;

namespace SimpleExpressionEvaluator
{
    public class Evaluator
    {
        public bool Evaluate<T>(string evaluationText, T objectValue)
        {
            ExpressionEvaluatorLexer expressionEvaluatorLexer = new ExpressionEvaluatorLexer(evaluationText, 1);
            ExpressionEvaluatorParser expressionEvaluatorParser = new ExpressionEvaluatorParser(expressionEvaluatorLexer);
            var AbstractSyntaxTreeNodeList = expressionEvaluatorParser.BuildParseTree();
            ExpressionEvaluatorExecutor expressionEvaluator = new ExpressionEvaluatorExecutor();
            var result = expressionEvaluator.Evaluate<T>(AbstractSyntaxTreeNodeList, expressionEvaluatorParser.SymbolTable, objectValue);
            return result;
        }

        public R EvaluateNonBoolean<T,R>(string evaluationText, T objectValue)
        {
            ExpressionEvaluatorLexer expressionEvaluatorLexer = new ExpressionEvaluatorLexer(evaluationText, 1);
            ExpressionEvaluatorParser expressionEvaluatorParser = new ExpressionEvaluatorParser(expressionEvaluatorLexer);
            var AbstractSyntaxTreeNodeList = expressionEvaluatorParser.BuildParseTree();
            ExpressionEvaluatorExecutor expressionEvaluator = new ExpressionEvaluatorExecutor();
            var result = expressionEvaluator.EvaluateNonBoolean<T,R>(AbstractSyntaxTreeNodeList, expressionEvaluatorParser.SymbolTable, objectValue);
            return result;
        }

        public Tuple<List<AbstractSyntaxTree.AbstractSyntaxTreeNode>, Dictionary<string, AbstractSyntaxTree.AbstractSyntaxTreeNode>> 
            PreEvaluate<T>(string evaluationText)
        {
            ExpressionEvaluatorLexer expressionEvaluatorLexer = new ExpressionEvaluatorLexer(evaluationText, 1);
            ExpressionEvaluatorParser expressionEvaluatorParser = new ExpressionEvaluatorParser(expressionEvaluatorLexer);
            var AbstractSyntaxTreeNodeList = expressionEvaluatorParser.BuildParseTree();
            ExpressionEvaluatorExecutor expressionEvaluator = new ExpressionEvaluatorExecutor();
            var symbolTable = expressionEvaluatorParser.SymbolTable;
            return new Tuple<List<AbstractSyntaxTree.AbstractSyntaxTreeNode>, Dictionary<string, AbstractSyntaxTree.AbstractSyntaxTreeNode>>(
                AbstractSyntaxTreeNodeList, symbolTable
                );            
        }

        public bool ExecuteEvaluate<T>(Tuple<List<AbstractSyntaxTree.AbstractSyntaxTreeNode>, 
            Dictionary<string, AbstractSyntaxTree.AbstractSyntaxTreeNode>> values, T objectValue)
        {
            ExpressionEvaluatorExecutor expressionEvaluator = new ExpressionEvaluatorExecutor();
            var result = expressionEvaluator.Evaluate<T>(values.Item1, values.Item2, objectValue);
            return result;
        }
    }
}
