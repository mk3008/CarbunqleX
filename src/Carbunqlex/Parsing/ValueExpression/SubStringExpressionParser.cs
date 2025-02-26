using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class SubStringExpressionParser
{
    public static IValueExpression Parse(SqlTokenizer tokenizer)
    {
        // check splitter token
        var splitter = tokenizer.Peek(3);

        if (splitter.Type == TokenType.Comma)
        {
            // common format
            // substring('hello world', 1, 5)
            var function = tokenizer.Read("substring");
            return FunctionExpressionParser.Parse(tokenizer, function);
        }

        if (splitter.CommandOrOperatorText == "from")
        {
            // substring('hello world' from 1 for 5)
            // substring('hello world' from 1)
            tokenizer.Read("substring");
            tokenizer.Read(TokenType.OpenParen);
            var source = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read("from");
            var from = ValueExpressionParser.Parse(tokenizer);

            if (tokenizer.Peek().CommandOrOperatorText == "for")
            {
                // from and for
                tokenizer.CommitPeek();
                var forExpression = ValueExpressionParser.Parse(tokenizer);
                tokenizer.Read(TokenType.CloseParen);
                return new SubStringExpression(source, from, forExpression);
            }

            // from only
            tokenizer.Read(TokenType.CloseParen);
            return new SubStringExpression(source, from, null);
        }

        if (splitter.CommandOrOperatorText == "for")
        {
            // substring('hello world' for 5)
            tokenizer.Read("substring");
            tokenizer.Read(TokenType.OpenParen);
            var source = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read("for");
            var forExpression = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new SubStringExpression(source, null, forExpression);
        }

        if (splitter.CommandOrOperatorText == "similar")
        {
            // substring('Thomas' similar '%#\"o_a#\"_' escape '#')
            tokenizer.Read("substring");
            tokenizer.Read(TokenType.OpenParen);
            var source = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read("similar");
            var pattern = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read("escape");
            var escape = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new SubStringSimilarExpression(source, pattern, escape);
        }

        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, [",", "from", "for", "similar"], splitter);
    }
}
