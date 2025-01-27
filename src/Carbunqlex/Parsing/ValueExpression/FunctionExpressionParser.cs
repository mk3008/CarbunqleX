using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class FunctionExpressionParser
{
    private static string ParserName => nameof(FunctionExpressionParser);

    public static FunctionExpression Parse(SqlTokenizer tokenizer, Token function)
    {
        tokenizer.Read(ParserName, TokenType.OpenParen);

        // support for distinct, all keyword
        var next = tokenizer.Peek();
        var prefixModifier = string.Empty;
        if (next.Value == "distinct")
        {
            tokenizer.Read();
            prefixModifier = next.Value;
        }
        else if (next.Value == "all")
        {
            tokenizer.Read();
        }

        var args = ParseArguments(tokenizer);

        tokenizer.Read(ParserName, TokenType.CloseParen);

        if (tokenizer.IsEnd)
        {
            return new FunctionExpression(function.Value, prefixModifier, args);
        }

        // TODO: Add support for FILTER, WITHIN GROUP, OVER clauses
        next = tokenizer.Peek();

        if (next.Identifier == "filter")
        {
            var filter = FilterClauseParser.Parse(tokenizer);
            return new FunctionExpression(function.Value, prefixModifier, args, filter);
        }

        if (next.Identifier == "within group")
        {
            var withinGroup = WithinGroupClauseParser.Parse(tokenizer);
            return new FunctionExpression(function.Value, prefixModifier, args, withinGroup);
        }

        if (next.Identifier == "over")
        {
            var over = OverClauseParser.Parse(tokenizer);
            return new FunctionExpression(function.Value, prefixModifier, args, over);
        }

        return new FunctionExpression(function.Value, prefixModifier, args);
    }

    private static ValueArguments ParseArguments(SqlTokenizer tokenizer)
    {
        var args = new List<IValueExpression>();

        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            args.Add(expression);
            if (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue; ;
            }
            break;
        }

        var next = tokenizer.Peek();
        if (next.Identifier == "order by")
        {
            var orderBy = OrderByClauseParser.Parse(tokenizer);
            return new ValueArguments(args, orderBy);
        }

        return new ValueArguments(args);
    }
}
