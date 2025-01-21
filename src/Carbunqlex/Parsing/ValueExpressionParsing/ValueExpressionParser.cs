using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ValueExpressionParser
{
    private static string ParserName => nameof(ValueExpressionParser);

    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        if (!tokenizer.TryPeek(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(ParserName, tokenizer); ;
        }

        var current = ParseAsCurrent(tokenizer);

        while (TryParseFollowingExpression(tokenizer, current, out var nextExpression))
        {
            current = nextExpression;
        }

        return current;
    }

    private static IValueExpression ParseAsCurrent(SqlTokenizer tokenizer)
    {
        if (!tokenizer.TryPeek(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(ParserName, tokenizer); ;
        }

        // command (eg. array, modifier)
        if (token.Type == TokenType.Command)
        {
            if (token.Identifier == "array")
            {
                return ArrayExpressionParser.Parse(tokenizer);
            }
            if (token.Identifier == "case")
            {
                return CaseExpressionParser.Parse(tokenizer); ;
            }

            return ModifierExpressionParser.Parse(tokenizer);

        }

        // constant
        if (token.Type == TokenType.Constant)
        {
            return ConstantExpressionParser.Parse(tokenizer);
        }

        // column or table identifier
        if (token.Type == TokenType.Identifier)
        {
            return ColumnExpressionParser.Parse(tokenizer);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(ParserName, TokenType.Identifier, tokenizer, token);
    }

    /// <summary>
    /// 次の式をパースし、新しい式を返します。
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <param name="left"></param>
    /// <param name="renewValue"></param>
    /// <returns></returns>
    public static bool TryParseFollowingExpression(SqlTokenizer tokenizer, IValueExpression left, [NotNullWhen(true)] out IValueExpression renewValue)
    {
        if (!tokenizer.TryPeek(out var nextToken))
        {
            renewValue = left;
            return false;
        }

        if (nextToken.Type == TokenType.Command)
        {
            if (nextToken.Identifier == "between" || nextToken.Identifier == "not between")
            {
                renewValue = BetweenExpressionParser.Parse(tokenizer, left);
                return true;
            }
            if (nextToken.Identifier == "like" || nextToken.Identifier == "not like")
            {
                renewValue = LikeExpressionParser.Parse(tokenizer, left);
                return true;
            }
            if (nextToken.Identifier == "in" || nextToken.Identifier == "not in")
            {
                renewValue = InExpressionParser.Parse(tokenizer, left);
                return true;
            }
        }

        if (nextToken.Type == TokenType.Operator)
        {
            renewValue = BinaryExpressionParser.Parse(tokenizer, left);
            return true;
        }

        // no next expression
        renewValue = left;
        return false;
    }

    public static IEnumerable<IValueExpression> ParseArguments(SqlTokenizer tokenizer, TokenType openTokenType, TokenType closeTokenType)
    {
        tokenizer.Read(ParserName, openTokenType);
        while (true)
        {
            yield return Parse(tokenizer);

            var token = tokenizer.Read(ParserName, closeTokenType, TokenType.Comma);
            if (token.Type == closeTokenType)
            {
                break;
            }
        }
    }
}
