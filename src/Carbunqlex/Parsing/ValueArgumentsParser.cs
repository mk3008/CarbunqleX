using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public static class ValueArgumentsParser
{
    private static string ParserName => nameof(ValueArgumentsParser);

    public static ValueArguments Parse(SqlTokenizer tokenizer, TokenType open, TokenType close)
    {
        tokenizer.Read(ParserName, open);

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

        return tokenizer.Peek(token =>
        {
            if (token.Type == close)
            {
                tokenizer.Read();
                return new ValueArguments(args);
            }

            if (token.Identifier == "order by")
            {
                var orderByClause = OrderByClauseParser.Parse(tokenizer);
                var expression = new ValueArguments(args) { OrderByClause = orderByClause };
                tokenizer.Read(ParserName, close);
                return expression;
            }

            throw SqlParsingExceptionBuilder.UnexpectedToken(ParserName, [close.ToString(), "order by"], tokenizer, token);
        });
    }
}
