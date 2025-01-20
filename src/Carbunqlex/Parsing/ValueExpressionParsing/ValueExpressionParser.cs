using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ValueExpressionParser
{
    private static string Name => nameof(ValueExpressionParser);

    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        if (!tokenizer.TryPeek(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(Name, tokenizer); ;
        }

        // command
        if (token.Type == TokenType.Command)
        {
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
            var expression = ColumnExpressionParser.Parse(tokenizer);

            if (!tokenizer.TryPeek(out var nextToken))
            {
                return expression;
            }
            if (nextToken.Type == TokenType.Command)
            {
                if (nextToken.Identifier == "between" || nextToken.Identifier == "not between")
                {
                    return BetweenExpressionParser.Parse(tokenizer, expression);
                }
                if (nextToken.Identifier == "like" || nextToken.Identifier == "not like")
                {
                    return LikeExpressionParser.Parse(tokenizer, expression);
                }
                if (nextToken.Identifier == "in" || nextToken.Identifier == "not in")
                {
                    return InExpressionParser.Parse(tokenizer, expression);
                }
            }

            return expression;
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(Name, TokenType.Identifier, tokenizer, token);
    }

    public static bool TryParse(SqlTokenizer tokenizer, [NotNullWhen(true)] out IValueExpression? expression)
    {
        try
        {
            expression = Parse(tokenizer);
            return true;
        }
        catch (SqlParsingException)
        {
            expression = null;
            return false;
        }
    }
}
