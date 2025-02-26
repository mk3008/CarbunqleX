using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public static class ModifierExpressionParser
{
    public static ModifierExpression Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Read(TokenType.Command);
        var right = ValueExpressionParser.Parse(tokenizer);
        return new ModifierExpression(command.Value, right);
    }
}
