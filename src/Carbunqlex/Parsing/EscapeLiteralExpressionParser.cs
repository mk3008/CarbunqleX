using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public static class EscapeLiteralExpressionParser
{
    public static EscapeLiteralExpression Parse(SqlTokenizer tokenizer)
    {
        var token = tokenizer.Read(TokenType.EscapedStringConstant);

        if (tokenizer.IsEnd)
        {
            return new EscapeLiteralExpression(token.Value);
        }

        if (tokenizer.Peek().CommandOrOperatorText == "uescape")
        {
            tokenizer.CommitPeek();
            var escapeOption = tokenizer.Read(TokenType.Literal).Value;
            return new EscapeLiteralExpression(token.Value, escapeOption);
        }

        return new EscapeLiteralExpression(token.Value);
    }
}
