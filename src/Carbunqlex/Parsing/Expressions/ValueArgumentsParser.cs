using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Clauses;

namespace Carbunqlex.Parsing.Expressions;

public static class ValueArgumentsParser
{
    public static ArgumentExpression Parse(SqlTokenizer tokenizer, TokenType openToken, TokenType closeToken)
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
                continue;
            }
            break;
        }

        var next = tokenizer.Peek();

        if (next.Type == closeToken)
        {
            tokenizer.Read();
            return new ArgumentExpression(args);
        }

        if (next.CommandOrOperatorText == "order by")
        {
            var orderByClause = OrderByClauseParser.Parse(tokenizer);
            tokenizer.Read(closeToken);
            return new ArgumentExpression(args) { OrderByClause = orderByClause };
        }

        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, [closeToken.ToString(), "order by"], next);
    }
}
