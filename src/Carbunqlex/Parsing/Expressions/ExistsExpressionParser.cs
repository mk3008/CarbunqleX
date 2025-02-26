using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

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
        var query = SelectQueryParser.ParseWithoutEndCheck(tokenizer);
        tokenizer.Read(TokenType.CloseParen);

        return new ExistsExpression(isNegated, query);
    }
}
