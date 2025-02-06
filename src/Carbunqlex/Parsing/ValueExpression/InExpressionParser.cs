using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

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
            var query = SelectQueryParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new InExpression(isNegated, left, query);
        }
        else
        {
            var args = ParseAsArguments(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new InExpression(isNegated, left, args);
        }
    }

    public static ValueArguments ParseAsArguments(SqlTokenizer tokenizer)
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

        return new ValueArguments(args);
    }
}



public static class ExistsExpressionParser
{
    public static ExistsExpression Parse(SqlTokenizer tokenizer)
    {
        var isNegated = tokenizer.Read(TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "exists" => false,
                "not exists" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["exists", "not exists"], token)
            };
        });

        tokenizer.Read(TokenType.OpenParen);
        var query = SelectQueryParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

        return new ExistsExpression(isNegated, query);
    }
}
