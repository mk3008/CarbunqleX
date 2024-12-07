using System.Text;

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
        var sb = new StringBuilder();

        if (PartitionBy != null)
        {
            sb.Append(PartitionBy.ToSql());
        }
        if (OrderBy != null)
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(OrderBy.ToSql());
        }
        if (WindowFrame != null)
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(WindowFrame.ToSql());
        }

        return sb.ToString();
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
