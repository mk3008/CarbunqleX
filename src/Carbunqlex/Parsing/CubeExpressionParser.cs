using Carbunqlex.Lexing;
using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public class CubeExpressionParser
{
    public static CubeExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("cube");
        tokenizer.Read(TokenType.OpenParen);
        var expressions = new List<IValueExpression>();
        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            expressions.Add(expression);
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            break;
        }
        tokenizer.Read(TokenType.CloseParen);
        return new CubeExpression(expressions);
    }
}
