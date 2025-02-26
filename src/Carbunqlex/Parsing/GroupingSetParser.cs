using Carbunqlex.Lexing;
using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

public static class GroupingSetParser
{
    public static GroupingSetExpression Parse(SqlTokenizer tokenizer)
    {
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
        return new GroupingSetExpression(expressions);
    }
}
