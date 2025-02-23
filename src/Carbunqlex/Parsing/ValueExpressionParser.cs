using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.Parsing;

public static class ValueExpressionParser
{
    public static IValueExpression Parse(string commandText)
    {
        var tokenizer = new SqlTokenizer(commandText);
        return Parse(tokenizer);
    }

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
            throw SqlParsingExceptionBuilder.EndOfInput(tokenizer); ;
        }

        // command (e.g., array, modifier)
        if (token.Type == TokenType.Command)
        {
            //if (token.CommandOrOperatorText == "trim")
            //{
            //    return TrimValueParser.Parse(tokenizer);
            //}
            if (token.CommandOrOperatorText == "array")
            {
                return ArrayExpressionParser.Parse(tokenizer);
            }
            if (token.CommandOrOperatorText == "case")
            {
                return CaseExpressionParser.Parse(tokenizer); ;
            }
            if (token.CommandOrOperatorText == "cast")
            {
                return CastExpressionParser.Parse(tokenizer);
            }
            if (token.CommandOrOperatorText == "not")
            {
                tokenizer.Read();
                return UnaryExpressionParser.Parse(tokenizer, token.CommandOrOperatorText);
            }
            if (token.CommandOrOperatorText is "exists" or "not exists")
            {
                return ExistsExpressionParser.Parse(tokenizer);
            }
            if (token.CommandOrOperatorText == "cube")
            {
                return CubeExpressionParser.Parse(tokenizer);
            }
            if (token.CommandOrOperatorText == "rollup")
            {
                return RollupExpressionParser.Parse(tokenizer);
            }
            if (token.CommandOrOperatorText == "grouping sets")
            {
                return GroupingSetsExpressionParser.Parse(tokenizer);
            }
            if (token.CommandOrOperatorText == "position")
            {
                return PositionValueParser.Parse(tokenizer);
            }
            return ModifierExpressionParser.Parse(tokenizer);
        }

        // literal
        if (token.Type == TokenType.Literal)
        {
            return LiteralExpressionParser.Parse(tokenizer);
        }

        // escaped string constant
        if (token.Type == TokenType.EscapedStringConstant)
        {
            return EscapeLiteralExpressionParser.Parse(tokenizer);
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

            // array[]
            if (tokenizer.TryPeek(out var nextToken3) && nextToken3.Value == "[]")
            {
                tokenizer.CommitPeek();
                return new LiteralExpression(token.Value + nextToken3.Value);
            }

            // column
            return ColumnExpressionParser.Parse(tokenizer, token);
        }

        if (token.Type == TokenType.Operator && token.Value == "*")
        {
            // not operator
            tokenizer.CommitPeek();
            return ColumnExpressionParser.Parse(tokenizer, token);
        }

        if (token.Type == TokenType.Operator && token.Value == "-")
        {
            tokenizer.CommitPeek();

            var next = tokenizer.Peek();
            if (next.Value.Equals("infinity", StringComparison.InvariantCultureIgnoreCase))
            {
                // -infinity
                tokenizer.CommitPeek();
                return new LiteralExpression("-infinity");
            }

            return UnaryExpressionParser.Parse(tokenizer, token.Value);
        }

        if (token.Type == TokenType.OpenParen)
        {
            return ParenthesizedExpressionParser.Parse(tokenizer);
        }

        if (token.Type == TokenType.Parameter)
        {
            return ParameterExpressionParser.Parse(tokenizer);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, TokenType.Identifier, token);
    }

    /// <summary>
    /// Try to parse the following expression.
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <param name="ignoreValues"></param>
    /// <param name="left"></param>
    /// <param name="renewValue"></param>
    /// <returns></returns>
    public static bool TryParseFollowingExpression(SqlTokenizer tokenizer, string[]? ignoreValues, IValueExpression left, [NotNullWhen(true)] out IValueExpression renewValue)
    {
        if (!tokenizer.TryPeek(out var nextToken))
        {
            renewValue = left;
            return false;
        }

        if (nextToken.Type == TokenType.Command)
        {
            // e.g., in the case of the position function, ignore `in` within the syntax.
            if (ignoreValues != null && ignoreValues.Contains(nextToken.Value.ToLower()))
            {
                renewValue = left;
                return false;
            }

            if (nextToken.CommandOrOperatorText == "between" || nextToken.CommandOrOperatorText == "not between")
            {
                renewValue = BetweenExpressionParser.Parse(tokenizer, left);
                return true;
            }
            if (nextToken.CommandOrOperatorText is "like" or "not like" or "ilike" or "not ilike")
            {
                renewValue = LikeExpressionParser.Parse(tokenizer, left);
                return true;
            }
            if (nextToken.CommandOrOperatorText is "in" or "not in")
            {
                var isInExpression = tokenizer.Peek(1).Type == TokenType.OpenParen ? true : false;

                // Only treat as an in-expression if parentheses appear
                if (isInExpression)
                {
                    renewValue = InExpressionParser.Parse(tokenizer, left);
                    return true;
                }
                else
                {
                    renewValue = left;
                    return false;
                }
            }
        }

        if (nextToken.Type == TokenType.Operator)
        {
            // e.g., in the case of "between", ignore `and` as an operator, and if detected, break.
            if (ignoreValues != null && ignoreValues.Contains(nextToken.Value.ToLower()))
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
}
