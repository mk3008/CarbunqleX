namespace Carbunqlex.Clauses;

public class WindowFunction : ISqlComponent
{
    public PartitionByClause? PartitionBy { get; }
    public OrderByClause? OrderBy { get; }
    public WindowFrame? WindowFrame { get; }

    public WindowFunction(PartitionByClause? partitionBy = null, OrderByClause? orderBy = null, WindowFrame? windowFrame = null)
    {
        PartitionBy = partitionBy;
        OrderBy = orderBy;
        WindowFrame = windowFrame;
    }

    public string ToSql()
    {
        var components = new List<string>();

        if (PartitionBy != null)
        {
            components.Add(PartitionBy.ToSql());
        }
        if (OrderBy != null)
        {
            components.Add(OrderBy.ToSql());
        }
        if (WindowFrame != null)
        {
            components.Add(WindowFrame.ToSql());
        }

        return string.Join(" ", components);
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>();

        if (PartitionBy != null)
        {
            lexemes.AddRange(PartitionBy.GetLexemes());
        }

        if (OrderBy != null)
        {
            lexemes.AddRange(OrderBy.GetLexemes());
        }

        if (WindowFrame != null)
        {
            lexemes.AddRange(WindowFrame.GetLexemes());
        }

        return lexemes;
    }
}
