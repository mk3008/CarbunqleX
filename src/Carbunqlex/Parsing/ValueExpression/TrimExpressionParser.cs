using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class TrimExpressionParser
{
    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Peek(2);

        if (command.Type is not TokenType.Command)
        {
            // omit BOTH command
            // TRIM(string)
            // TRIM(string FROM string)
            // TRIM(string, string) 
            var secondCommand = tokenizer.Peek(3);

            if (secondCommand.CommandOrOperatorText == "from")
            {
                // TRIM(string FROM string)
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                var characters = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read("from");
                var original = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new TrimExpression("both", characters, original);
            }
            else if (secondCommand.Type is TokenType.Comma)
            {
                // TRIM(string, string) /*Postgres only*/
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                var args = ParseArguments(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new PostgresTrimExpression(args);
            }
            else
            {
                // TRIM(string)
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                var value = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new TrimExpression(value);
            }
        }

        if (command.CommandOrOperatorText == "both")
        {
            // TRIM(BOTH string)
            // TRIM(BOTH string FROM string)
            var secondModifier = tokenizer.Peek(4);

            if (secondModifier.CommandOrOperatorText == "from")
            {
                // TRIM(BOTH string FROM string)
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                var characters = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read("from");
                var original = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new TrimExpression("both", characters, original);
            }
            else
            {
                // TRIM(BOTH string)
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                var original = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new TrimExpression("both", original);
            }
        }

        if (command.CommandOrOperatorText is "leading" or "trailing")
        {
            // TRIM(LEADING string)
            // TRIM(TRAILING string)
            // TRIM(LEADING string FROM string)
            // TRIM(TRAILING string FROM string)
            var secondModifier = tokenizer.Peek(4);
            if (secondModifier.CommandOrOperatorText == "from")
            {
                // TRIM(LEADING string FROM string)
                // TRIM(TRAILING string FROM string)
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                command = tokenizer.Read();
                var characters = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read("from");
                var original = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new TrimExpression(command.CommandOrOperatorText, characters, original);
            }
            else
            {
                // TRIM(LEADING string)
                // TRIM(TRAILING string)
                tokenizer.Read("trim");
                tokenizer.Read(TokenType.OpenParen);
                command = tokenizer.Read();
                var original = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new TrimExpression(command.CommandOrOperatorText, original);
            }
        }

        if (command.CommandOrOperatorText == "from")
        {
            // TRIM(FROM string)
            tokenizer.Read("trim");
            tokenizer.Read(TokenType.OpenParen);
            command = tokenizer.Read("from");
            var original = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new TrimExpression("both", original);
        }

        if (command.CommandOrOperatorText is "both from" or "leading from" or "trailing from")
        {
            // PostgresTRIM
            // TRIM(BOTH FROM string, string)
            // TRIM(LEADING FROM string, string)
            // TRIM(TRAILING FROM string, string)
            tokenizer.Read("trim");
            tokenizer.Read(TokenType.OpenParen);
            command = tokenizer.Read();
            var args = ParseArguments(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new PostgresTrimExpression(command.CommandOrOperatorText, args);
        }

        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["both", "leading", "trailing", "from", "both from", "leading from", "trailing from"], command);
    }

    private static ArgumentExpression ParseArguments(SqlTokenizer tokenizer)
    {
        var args = new List<IValueExpression>();
        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            args.Add(expression);
            if (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            break;
        }
        return new ArgumentExpression(args);
    }
}
