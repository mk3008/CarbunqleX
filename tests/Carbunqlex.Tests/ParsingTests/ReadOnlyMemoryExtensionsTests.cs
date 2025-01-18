using Carbunqlex.Parsing;

namespace Carbunqlex.Tests.ParsingTests;

public class ReadOnlyMemoryExtensionsTests
{
    [Fact]
    public void ReadLexeme_Comma_ReturnsCommaToken()
    {
        var memory = new ReadOnlyMemory<char>(", test".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Comma, token.Type);
        Assert.Equal(",", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_Dot_ReturnsDotToken()
    {
        var memory = new ReadOnlyMemory<char>(". test".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Dot, token.Type);
        Assert.Equal(".", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_SingleQuote_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("'test'".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Value, token.Type);
        Assert.Equal("'test'", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_DoubleQuote_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("\"test\"".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Value, token.Type);
        Assert.Equal("\"test\"", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_BackQuote_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("`test`".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Value, token.Type);
        Assert.Equal("`test`", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_SquareBrackets_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("[test]".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Value, token.Type);
        Assert.Equal("[test]", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_OpenParen_ReturnsOpenParenToken()
    {
        var memory = new ReadOnlyMemory<char>("(test".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.OpenParen, token.Type);
        Assert.Equal("(", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_CloseParen_ReturnsCloseParenToken()
    {
        var memory = new ReadOnlyMemory<char>(")test".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.CloseParen, token.Type);
        Assert.Equal(")", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_ParameterAt_ReturnsParameterToken()
    {
        var memory = new ReadOnlyMemory<char>("@param".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Parameter, token.Type);
        Assert.Equal("@param", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_ParameterColon_ReturnsParameterToken()
    {
        var memory = new ReadOnlyMemory<char>(":param".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Parameter, token.Type);
        Assert.Equal(":param", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_ParameterDollar_ReturnsParameterToken()
    {
        var memory = new ReadOnlyMemory<char>("$param".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Parameter, token.Type);
        Assert.Equal("$param", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_Symbol_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("!@#".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Value, token.Type);
        Assert.Equal("!@#", token.Value);
        Assert.Equal(3, end);
    }

    [Fact]
    public void ReadLexeme_Digit_ReturnsConstantToken()
    {
        var memory = new ReadOnlyMemory<char>("123".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Constant, token.Type);
        Assert.Equal("123", token.Value);
        Assert.Equal(3, end);
    }

    [Fact]
    public void ReadLexeme_Float_ReturnsConstantToken()
    {
        var memory = new ReadOnlyMemory<char>("123.456".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Constant, token.Type);
        Assert.Equal("123.456", token.Value);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_Word_ReturnsIdentifierToken()
    {
        var memory = new ReadOnlyMemory<char>("test word".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("test", token.Value);
        Assert.Equal(5, end);
    }

    [Fact]
    public void ReadLexeme_SelectKeyword_ReturnsKeywordToken()
    {
        var memory = new ReadOnlyMemory<char>("select 1".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal(7, end);

        var nextToken = memory.ReadLexeme(end, out end);
        Assert.Equal(TokenType.Constant, nextToken.Type);
        Assert.Equal("1", nextToken.Value);
        Assert.Equal(8, end);
    }

    [Fact]
    public void ReadLexeme_SelectDistinctKeyword_ReturnsKeywordToken()
    {
        var memory = new ReadOnlyMemory<char>("select distinct 1".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("select distinct", token.Value);
        Assert.Equal("select distinct ", token.RawValue);
        Assert.Equal(16, end);

        var nextToken = memory.ReadLexeme(end, out end);
        Assert.Equal(TokenType.Constant, nextToken.Type);
        Assert.Equal("1", nextToken.Value);
        Assert.Equal(17, end);
    }

    [Fact]
    public void ReadLexeme_SelectDistinctOnKeyword_ReturnsKeywordToken()
    {
        var memory = new ReadOnlyMemory<char>("select distinct on ()".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("select distinct on", token.Value);
        Assert.Equal("select distinct on ", token.RawValue);
        Assert.Equal(19, end);
    }

    [Fact]
    public void ReadLexeme_Normalize_WhiteSpace()
    {
        var memory = new ReadOnlyMemory<char>("select\tdistinct\non ()".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("select distinct on", token.Value);
        Assert.Equal("select\tdistinct\non ", token.RawValue);
        Assert.Equal(19, end);
    }

    [Fact]
    public void ReadLexeme_Normalize_Comment()
    {
        var memory = new ReadOnlyMemory<char>("select/*comment*/distinct--comment\non ()".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("select distinct on", token.Value);
        Assert.Equal("select/*comment*/distinct--comment\non ", token.RawValue);
        Assert.Equal(38, end);
    }

    [Fact]
    public void ReadLexeme_HintClause()
    {
        var memory = new ReadOnlyMemory<char>("select/*+ hint */ 1".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal("select/*+ hint */ ", token.RawValue);
        Assert.Equal(18, end);
    }

    [Fact]
    public void ReadLexeme_Identifier()
    {
        var memory = new ReadOnlyMemory<char>("SELECT/*comment*/DISTINCT--comment\nON ()".ToCharArray());
        var token = memory.ReadLexeme(0, out int end);
        Assert.Equal(TokenType.Keyword, token.Type);
        Assert.Equal("SELECT DISTINCT ON", token.Value);
        Assert.Equal("SELECT/*comment*/DISTINCT--comment\nON ", token.RawValue);
        Assert.Equal("select distinct on", token.Identifier);
        Assert.Equal(38, end);
    }

    [Fact]
    public void ReadLexeme_NotSupportedException()
    {
        var memory = new ReadOnlyMemory<char>("Inner/*comment*/Test".ToCharArray());
        var exception = Assert.Throws<NotSupportedException>(() => memory.ReadLexeme(0, out int end));
        Assert.Equal("Unsupported keyword 'Inner Test' found at position 0.", exception.Message);
    }
}
