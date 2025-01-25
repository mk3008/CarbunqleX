using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ValueExpressionParser
{
    private static string ParserName => nameof(ValueExpressionParser);

    public static IValueExpression Parse(SqlTokenizer tokenizer, string[]? ignoreOperators = null)
    {
        var current = ParseAsCurrent(tokenizer);

        while (TryParseFollowingExpression(tokenizer, ignoreOperators, current, out var nextExpression))
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

        // command (e.g., array, modifier)
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
            if (token.Identifier == "cast")
            {
                return CastExpressionParser.Parse(tokenizer);
            }
            return ModifierExpressionParser.Parse(tokenizer);
        }

        // constant
        if (token.Type == TokenType.Constant)
        {
            return ConstantExpressionParser.Parse(tokenizer);
        }

        if (token.Type == TokenType.Identifier)
        {
            tokenizer.CommitPeek();

            // function
            if (tokenizer.TryPeek(out var nextToken) && nextToken.Type == TokenType.OpenParen)
            {
                return FunctionExpressionParser.Parse(tokenizer, token);
            }

            // table.column
            if (tokenizer.TryPeek(out var nextToken2) && nextToken2.Type == TokenType.Dot)
            {
                return ColumnExpressionParser.Parse(tokenizer, token);
            }

            // column
            return ColumnExpressionParser.Parse(tokenizer, token);
        }

        if (token.Type == TokenType.Operator && token.Value == "*")
        {
            // not operator, but wildcard
            tokenizer.CommitPeek();
            return ColumnExpressionParser.Parse(tokenizer, token);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(ParserName, TokenType.Identifier, tokenizer, token);
    }

    /// <summary>
    /// Try to parse the following expression.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <param name="ignoreOperators"></param>
    /// <param name="left"></param>
    /// <param name="renewValue"></param>
    /// <returns></returns>
    public static bool TryParseFollowingExpression(SqlTokenizer tokenizer, string[]? ignoreOperators, IValueExpression left, [NotNullWhen(true)] out IValueExpression renewValue)
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
            // ignore operators
            // In the case of "between", "and" is not treated as an operator, and if detected, break.
            if (ignoreOperators != null && ignoreOperators.Contains(nextToken.Value))
            {
                renewValue = left;
                return false;
            }

            renewValue = BinaryExpressionParser.Parse(tokenizer, left);
            return true;
        }

        // no next expression
        renewValue = left;
        return false;
    }

    [Obsolete("Use ParseArguments instead.")]
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
