using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Parsing.QuerySources;

/// <summary>
/// Parses function datasource expressions from SQL tokens.
/// e.g. function(arg1, arg2)
/// </summary>
public class FunctionDatasourceParser
{
    public static FunctionSource Parse(SqlTokenizer tokenizer, string functionName)
    {
        tokenizer.Read(TokenType.OpenParen);
        var next = tokenizer.Peek();
        if (next.Type == TokenType.CloseParen)
        {
            // no arguments
            tokenizer.CommitPeek();

            if (tokenizer.IsEnd)
            {
                return new FunctionSource(functionName);
            }

            var hasKeyword = HasWithOrdinalityKeyword(tokenizer);
            return new FunctionSource(functionName) { HasWithOrdinalityKeyword = hasKeyword };
        }

        var parameters = new List<IValueExpression>();
        bool hasWithOrdinalityKeyword = false;

        while (true)
        {
            var parameter = ValueExpressionParser.Parse(tokenizer);
            parameters.Add(parameter);

            next = tokenizer.Peek();
            if (next.Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            if (next.Type == TokenType.CloseParen)
            {
                tokenizer.CommitPeek();

                hasWithOrdinalityKeyword = HasWithOrdinalityKeyword(tokenizer);
                break;
            }

            throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, [TokenType.Comma, TokenType.CloseParen], next);
        }

        return new FunctionSource(functionName, parameters, hasWithOrdinalityKeyword);
    }

    /// <summary>
    /// Checks if the next token is the "with ordinality" keyword.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    private static bool HasWithOrdinalityKeyword(SqlTokenizer tokenizer)
    {
        if (tokenizer.IsEnd)
        {
            return false;
        }

        var next = tokenizer.Peek();
        if (next.CommandOrOperatorText == "with ordinality")
        {
            tokenizer.CommitPeek();
            return true;
        }
        return false;
    }
}
