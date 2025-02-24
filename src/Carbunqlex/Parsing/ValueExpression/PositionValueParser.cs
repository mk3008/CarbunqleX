using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class PositionValueParser
{
    public static PositionExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("position");
        tokenizer.Read(TokenType.OpenParen);

        var subString = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read("in");
        var sourceString = ValueExpressionParser.Parse(tokenizer);
        tokenizer.Read(TokenType.CloseParen);
        return new PositionExpression(subString, sourceString);
    }
}

//public class TrimValueParser
//{
//    public static IValueExpression Parse(SqlTokenizer tokenizer)
//    {
//        tokenizer.Read("trim");
//        tokenizer.Read(TokenType.OpenParen);

//        var trimType = "both";

//        if (tokenizer.Peek().Type == TokenType.Command)
//        {
//            trimType = tokenizer.Read().Value;

//            if (tokenizer.Peek().Type == TokenType.Command)
//            {
//                // PostgresTrim (leading from, trailing, from)
//                trimType += " " + tokenizer.Read().Value;
//                var args = ParseArguments(tokenizer);
//                tokenizer.Read(TokenType.CloseParen);

//                return new PostgresTrimExpression(trimType, args);
//            }
//            else
//            {
//                var originalText = ValueExpressionParser.Parse(tokenizer);
//                if (tokenizer.Peek().Type == TokenType.CloseParen)
//                {
//                    // trim(both X)
//                    tokenizer.CommitPeek();
//                    return new TrimExpression(originalText, trimType);
//                }

//                // trim(command X from Y)
//                tokenizer.Read("from");
//                var characters = ValueExpressionParser.Parse(tokenizer);
//                return new TrimExpression(originalText, characters, trimType);
//            }
//        }
//        // omit the trim type
//        else
//        {
//            var originalText = ValueExpressionParser.Parse(tokenizer);
//            if (tokenizer.Peek().Type == TokenType.CloseParen)
//            {
//                // trim(X)
//                tokenizer.CommitPeek();
//                return new TrimExpression(originalText);
//            }
//            // trim(X from Y)
//            tokenizer.Read("from");
//            var characters = ValueExpressionParser.Parse(tokenizer);
//            return new TrimExpression(originalText, characters, trimType);
//        }
//    }

//    private static ArgumentExpression ParseArguments(SqlTokenizer tokenizer)
//    {
//        // no arguments
//        if (tokenizer.Peek().Type == TokenType.CloseParen)
//        {
//            return new ArgumentExpression();
//        }

//        var args = new List<IValueExpression>();

//        while (true)
//        {
//            var expression = ValueExpressionParser.Parse(tokenizer);
//            args.Add(expression);
//            if (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
//            {
//                tokenizer.Read();
//                continue;
//            }
//            break;
//        }

//        return new ArgumentExpression(args);
//    }
//}
