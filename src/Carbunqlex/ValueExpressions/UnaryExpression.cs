namespace Carbunqlex.ValueExpressions;

public class UnaryExpression : IValueExpression
{
    public string Operator { get; set; }
    public IValueExpression Operand { get; set; }
    public UnaryExpression(string @operator, IValueExpression operand)
    {
        Operator = @operator;
        Operand = operand;
    }

    public string DefaultName => Operand.DefaultName;

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Operator, Operator);
        foreach (var lexeme in Operand.GetLexemes())
        {
            yield return lexeme;
        }
    }

    public string ToSql()
    {
        return $"{Operator} {Operand.ToSql()}";
    }
}
