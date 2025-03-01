using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

internal static class IdentifierValueParser
{
    public static IEnumerable<string> Parse(SqlTokenizer tokenizer, Token identifier)
    {
        yield return identifier.Value;
        while (tokenizer.TryPeek(out var nextToken) && nextToken.Type == TokenType.Dot)
        {
            tokenizer.CommitPeek();
            var token = tokenizer.Read(TokenType.Identifier);
            yield return token.Value;
        }
    }
}
