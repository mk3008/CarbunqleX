namespace Carbunqlex.Parsing.ValueExpression;

public class SqlParsingException : Exception
{
    public SqlParsingException(string message, int position, Token token) : base(message)
    {
        Position = position;
        Token = token;
    }

    public int Position { get; }
    public Token Token { get; }
}

public static class SqlParsingExceptionBuilder
{
    public static SqlParsingException EmptyArgument(SqlTokenizer tokenizer)
    {
        return new SqlParsingException("Empty argument.", tokenizer.Position, Token.Empty);
    }

    public static SqlParsingException EndOfInput(SqlTokenizer tokenizer)
    {
        return new SqlParsingException("Unexpected end of input.", tokenizer.Position, Token.Empty);
    }
    public static SqlParsingException UnexpectedTokenType(SqlTokenizer tokenizer, TokenType expectedType, Token actualToken)
    {
        return new SqlParsingException($"Unexpected token type encountered. Expected: {expectedType}, Actual: {actualToken.Type}({actualToken.Value}), Position: {tokenizer.Position}", tokenizer.Position, actualToken);
    }

    public static SqlParsingException UnexpectedTokenType(SqlTokenizer sqlTokenizer, IEnumerable<TokenType> expectedTokenTypes, Token actualToken)
    {
        return new SqlParsingException($"Unexpected token type encountered. Expected: {string.Join(" or ", expectedTokenTypes)}({actualToken.Type}), Value: {actualToken.Value},Position: {sqlTokenizer.Position}", sqlTokenizer.Position, actualToken);
    }

    public static SqlParsingException UnexpectedToken(SqlTokenizer sqlTokenizer, string expectedToken, Token actualToken)
    {
        return new SqlParsingException($"Unexpected token encountered. Expected: {expectedToken}, Actual: {actualToken.Value}, Position: {sqlTokenizer.Position}", sqlTokenizer.Position, actualToken);
    }

    public static SqlParsingException UnexpectedToken(SqlTokenizer sqlTokenizer, IEnumerable<string> expectedTokens, Token actualToken)
    {
        return new SqlParsingException($"Unexpected token encountered. Expected: {string.Join(" or ", expectedTokens)}, Actual: {actualToken.Value}, Position: {sqlTokenizer.Position}", sqlTokenizer.Position, actualToken);
    }
}
