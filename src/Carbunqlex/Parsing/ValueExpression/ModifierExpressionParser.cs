using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ModifierExpressionParser
{
    private static string ParserName => nameof(ModifierExpressionParser);

    public static ModifierExpression Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Read(ParserName, TokenType.Command);
        var right = ValueExpressionParser.Parse(tokenizer);
        return new ModifierExpression(command.Value, right);
    }
}
