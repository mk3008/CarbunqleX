using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ReadOnlyMemoryExtensionsTests
{
    public ReadOnlyMemoryExtensionsTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void ReadLexeme_Comma_ReturnsCommaToken()
    {
        var memory = new ReadOnlyMemory<char>(", test".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Comma, token.Type);
        Assert.Equal(",", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_Dot_ReturnsDotToken()
    {
        var memory = new ReadOnlyMemory<char>(". test".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Dot, token.Type);
        Assert.Equal(".", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_SingleQuote_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("'test'".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("'test'", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_DoubleQuote_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("\"test\"".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("\"test\"", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_BackQuote_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("`test`".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("`test`", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_SquareBrackets_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("[test]".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("[test]", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_OpenParen_ReturnsOpenParenToken()
    {
        var memory = new ReadOnlyMemory<char>("(test".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.OpenParen, token.Type);
        Assert.Equal("(", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_CloseParen_ReturnsCloseParenToken()
    {
        var memory = new ReadOnlyMemory<char>(")test".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.CloseParen, token.Type);
        Assert.Equal(")", token.Value);
        Assert.Equal(1, end);
    }

    [Fact]
    public void ReadLexeme_ParameterAt_ReturnsParameterToken()
    {
        var memory = new ReadOnlyMemory<char>("@param".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Parameter, token.Type);
        Assert.Equal("@param", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_ParameterColon_ReturnsParameterToken()
    {
        var memory = new ReadOnlyMemory<char>(":param".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Parameter, token.Type);
        Assert.Equal(":param", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_ParameterDollar_ReturnsParameterToken()
    {
        var memory = new ReadOnlyMemory<char>("$param".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Parameter, token.Type);
        Assert.Equal("$param", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_Symbol_ReturnsValueToken()
    {
        var memory = new ReadOnlyMemory<char>("!@#".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Operator, token.Type);
        Assert.Equal("!@#", token.Value);
        Assert.Equal(3, end);
    }

    [Fact]
    public void ReadLexeme_Digit_ReturnsConstantToken()
    {
        var memory = new ReadOnlyMemory<char>("123".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("123", token.Value);
        Assert.Equal(3, end);
    }

    [Fact]
    public void ReadLexeme_Float_ReturnsConstantToken()
    {
        var memory = new ReadOnlyMemory<char>("123.456".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("123.456", token.Value);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_Underscore_ReturnsConstantToken()
    {
        var memory = new ReadOnlyMemory<char>("123_456".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("123_456", token.Value);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_Word_ReturnsIdentifierToken()
    {
        var memory = new ReadOnlyMemory<char>("test word".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("test", token.Value);
        Assert.Equal(5, end);
    }

    [Fact]
    public void ReadLexeme_SelectKeyword_ReturnsKeywordToken()
    {
        var memory = new ReadOnlyMemory<char>("select 1".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal(7, end);

        var nextToken = memory.ReadLexeme(null, end, out end);
        Assert.Equal(TokenType.Literal, nextToken.Type);
        Assert.Equal("1", nextToken.Value);
        Assert.Equal(8, end);
    }

    [Fact]
    public void ReadLexeme_SelectDistinctKeyword_ReturnsKeywordToken()
    {
        var memory = new ReadOnlyMemory<char>("select distinct 1".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal("select ", token.RawValue);
        Assert.Equal(7, end);

        var nextToken = memory.ReadLexeme(null, end, out end);
        Assert.Equal(TokenType.Command, nextToken.Type);
        Assert.Equal("distinct", nextToken.Value);
        Assert.Equal(16, end);
    }

    [Fact]
    public void ReadLexeme_SelectDistinctOnKeyword_ReturnsKeywordToken()
    {
        var memory = new ReadOnlyMemory<char>("select distinct on ()".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal("select ", token.RawValue);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_Normalize_WhiteSpace()
    {
        var memory = new ReadOnlyMemory<char>("select\tdistinct\non ()".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal("select\t", token.RawValue);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_Normalize_Comment()
    {
        var memory = new ReadOnlyMemory<char>("select/*comment*/distinct--comment\non ()".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal("select/*comment*/", token.RawValue);
        Assert.Equal(17, end);
    }

    [Fact]
    public void ReadLexeme_HintClause()
    {
        var memory = new ReadOnlyMemory<char>("select/*+ hint */ 1".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("select", token.Value);
        Assert.Equal("select/*+ hint */ ", token.RawValue);
        Assert.Equal(18, end);
    }

    [Fact]
    public void ReadLexeme_Identifier()
    {
        var memory = new ReadOnlyMemory<char>("SELECT/*comment*/DISTINCT--comment\nON ()".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Command, token.Type);
        Assert.Equal("SELECT", token.Value);
        Assert.Equal("SELECT/*comment*/", token.RawValue);
        Assert.Equal("select", token.CommandOrOperatorText);
        Assert.Equal(17, end);
    }

    [Fact]
    public void ReadLexeme_NotSupportedException()
    {
        var memory = new ReadOnlyMemory<char>("Inner/*comment*/Test ".ToCharArray());
        var exception = Assert.Throws<NotSupportedException>(() => memory.ReadLexeme(null, 0, out int end));
        Assert.Equal("Unsupported keyword 'Inner Test' of type 'Command' found between positions 0 and 20.", exception.Message);
    }

    [Fact]
    public void ReadLexeme_DotStartDigit()
    {
        var memory = new ReadOnlyMemory<char>(".001".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal(".001", token.Value);
        Assert.Equal(4, end);
    }

    [Fact]
    public void ReadLexeme_DigitDot()
    {
        var memory = new ReadOnlyMemory<char>("4.".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("4.", token.Value);
        Assert.Equal(2, end);
    }

    [Fact]
    public void ReadLexeme_ScientificNotation()
    {
        var memory = new ReadOnlyMemory<char>("1.23e-4".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("1.23e-4", token.Value);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_ScientificNotation2()
    {
        var memory = new ReadOnlyMemory<char>("1.23e+4".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("1.23e+4", token.Value);
        Assert.Equal(7, end);
    }

    [Fact]
    public void ReadLexeme_ScientificNotation3()
    {
        var memory = new ReadOnlyMemory<char>("5e2".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("5e2", token.Value);
        Assert.Equal(3, end);
    }

    [Fact]
    public void ReadLexeme_UnicodeEscape()
    {
        var memory = new ReadOnlyMemory<char>("""
            U&'d\0061t\+000061'
            """.ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.EscapedStringConstant, token.Type);
        Assert.Equal("""
            U&'d\0061t\+000061'
            """, token.Value);
        Assert.Equal(19, end);
    }

    [Fact]
    public void ReadLexeme_BinaryConstant()
    {
        var memory = new ReadOnlyMemory<char>("0b1101".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("0b1101", token.Value);
        Assert.Equal(6, end);
    }

    [Fact]
    public void ReadLexeme_OctalConstant()
    {
        var memory = new ReadOnlyMemory<char>("0o123".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("0o123", token.Value);
        Assert.Equal(5, end);
    }

    [Fact]
    public void ReadLexeme_HexConstant()
    {
        var memory = new ReadOnlyMemory<char>("0x123".ToCharArray());
        var token = memory.ReadLexeme(null, 0, out int end);
        Assert.Equal(TokenType.Literal, token.Type);
        Assert.Equal("0x123", token.Value);
        Assert.Equal(5, end);
    }

    [Fact]
    public void DisplayAllSqlKeywords()
    {
        var keywords = SqlKeyword.CommandKeywordNodes.Values;

        foreach (var keyword in keywords)
        {
            Output.WriteLine(keyword.ToTreeString());
        }
    }
}
