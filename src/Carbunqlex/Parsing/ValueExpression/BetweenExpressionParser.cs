using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class BetweenExpressionParser
{
    public static BetweenExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isNegated = tokenizer.Read(TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "between" => false,
                "not between" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["between", "not between"], token)
            };
        });

        // In the case of "between", "and" should not be treated as an operator
        var start = ValueExpressionParser.Parse(tokenizer, ["and"]);
        tokenizer.Read("and");
        var end = ValueExpressionParser.Parse(tokenizer);
        return new BetweenExpression(isNegated, left, start, end);
    }
}
