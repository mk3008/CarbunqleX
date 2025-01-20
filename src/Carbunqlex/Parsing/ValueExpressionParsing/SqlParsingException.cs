namespace Carbunqlex.Parsing.ValueExpressionParsing;

public class SqlParsingException : Exception
{
    public SqlParsingException(string message, string parser, int position, Token token) : base(message)
    {
        Parser = parser;
        Position = position;
        Token = token;
    }

    public string Parser { get; }
    public int Position { get; }
    public Token Token { get; }
}

public static class SqlParsingExceptionBuilder
{
    public static SqlParsingException EndOfInput(string parser, SqlTokenizer tokenizer)
    {
        return new SqlParsingException("Unexpected end of input.", parser, tokenizer.Position, Token.Empty);
    }
    public static SqlParsingException UnexpectedTokenType(string parser, TokenType expectedType, SqlTokenizer tokenizer, Token actualToken)
    {
        return new SqlParsingException($"Unexpected token type encountered. Expected: {expectedType}, Actual: {actualToken.Type}, Position: {tokenizer.Position}", parser, tokenizer.Position, actualToken);
    }
    public static SqlParsingException UnexpectedTokenIdentifier(string parser, string expectedIdentifier, SqlTokenizer tokenizer, Token actualToken)
    {
        return new SqlParsingException($"Unexpected token identifier encountered. Expected: {expectedIdentifier}, Actual: {actualToken.Identifier}, Position: {tokenizer.Position}", parser, tokenizer.Position, actualToken);
    }

    internal static Exception UnexpectedTokenType(string sender, TokenType[] expectedTokenTypes, SqlTokenizer sqlTokenizer, Token token)
    {
        return new SqlParsingException($"Unexpected token type encountered. Expected: {string.Join(" or ", expectedTokenTypes)}, Actual: {token.Type}, Position: {sqlTokenizer.Position}", sender, sqlTokenizer.Position, token);
    }
}
