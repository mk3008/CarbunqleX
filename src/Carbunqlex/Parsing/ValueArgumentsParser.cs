using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public static class ValueArgumentsParser
{
    public static ValueArguments Parse(SqlTokenizer tokenizer, TokenType openToken, TokenType closeToken)
    {
        tokenizer.Read(openToken);

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
            if (token.Type == closeToken)
            {
                tokenizer.Read();
                return new ValueArguments(args);
            }

            if (token.CommandOrOperatorText == "order by")
            {
                var orderByClause = OrderByClauseParser.Parse(tokenizer);
                var expression = new ValueArguments(args) { OrderByClause = orderByClause };
                tokenizer.Read(closeToken);
                return expression;
            }

            throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, [closeToken.ToString(), "order by"], token);
        });
    }
}
