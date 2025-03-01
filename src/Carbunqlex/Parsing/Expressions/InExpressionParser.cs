using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public static class InExpressionParser
{
    public static InExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isNegated = tokenizer.Read(TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "in" => false,
                "not in" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["in", "not in"], token)
            };
        });

        tokenizer.Read(TokenType.OpenParen);

        var next = tokenizer.Peek();

        if (next.CommandOrOperatorText == "select")
        {
            var query = SelectQueryParser.ParseWithoutEndCheck(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new InExpression(isNegated, left, new SubQueryExpression(query));
        }
        else
        {
            var args = ParseAsArguments(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new InExpression(isNegated, left, args);
        }
    }

    private static InValueGroupExpression ParseAsArguments(SqlTokenizer tokenizer)
    {
        var args = new List<IValueExpression>();
        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            args.Add(expression);
            if (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }

        return new InValueGroupExpression(args);
    }
}
