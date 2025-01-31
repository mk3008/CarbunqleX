namespace Carbunqlex.Parsing.ValueExpressionParsing;

internal static class IdentifierValueParser
{
    private static string ParserName => nameof(IdentifierValueParser);

    public static IEnumerable<string> Parse(SqlTokenizer tokenizer, Token identifier)
    {
        yield return identifier.Value;
        while (tokenizer.TryPeek(out var nextToken) && nextToken.Type == TokenType.Dot)
        {
            tokenizer.CommitPeek();
            var token = tokenizer.Read(ParserName, TokenType.Identifier);
            yield return token.Value;
        }
    }
}
