using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Clauses;

namespace Carbunqlex.Parsing.Expressions;

public static class FunctionExpressionParser
{
    public static FunctionExpression Parse(SqlTokenizer tokenizer, Token function)
    {
        tokenizer.Read(TokenType.OpenParen);

        // support for distinct, all keyword
        var next = tokenizer.Peek();
        var prefixModifier = string.Empty;
        if (next.CommandOrOperatorText == "distinct")
        {
            tokenizer.Read();
            prefixModifier = next.Value;
        }
        else if (next.CommandOrOperatorText == "all")
        {
            tokenizer.Read();
        }

        var args = ParseArguments(tokenizer);

        tokenizer.Read(TokenType.CloseParen);

        if (tokenizer.IsEnd)
        {
            return new FunctionExpression(function.Value, prefixModifier, args);
        }

        next = tokenizer.Peek();
        if (next.CommandOrOperatorText == "filter")
        {
            var filter = FilterClauseParser.Parse(tokenizer);
            return new FunctionExpression(function.Value, prefixModifier, args, filter);
        }
        if (next.CommandOrOperatorText == "within group")
        {
            var withinGroup = WithinGroupClauseParser.Parse(tokenizer);
            return new FunctionExpression(function.Value, prefixModifier, args, withinGroup);
        }
        if (next.CommandOrOperatorText == "over")
        {
            var over = OverClauseParser.Parse(tokenizer);
            return new FunctionExpression(function.Value, prefixModifier, args, over);
        }

        return new FunctionExpression(function.Value, prefixModifier, args);
    }

    private static ArgumentExpression ParseArguments(SqlTokenizer tokenizer)
    {
        // no arguments
        if (tokenizer.Peek().Type == TokenType.CloseParen)
        {
            return new ArgumentExpression();
        }

        var args = new List<IValueExpression>();

        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            args.Add(expression);
            if (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            break;
        }

        // check for ORDER BY clause
        // e.g. array_agg(value order by sort_column)
        var next = tokenizer.Peek();
        if (next.CommandOrOperatorText == "order by")
        {
            var orderBy = OrderByClauseParser.Parse(tokenizer);
            return new ArgumentExpression(args, orderBy);
        }

        return new ArgumentExpression(args);
    }
}
