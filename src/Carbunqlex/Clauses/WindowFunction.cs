using System.Text;

namespace Carbunqlex.Clauses;

public class WindowFunction : ISqlComponent
{
    public PartitionByClause? PartitionBy { get; }
    public OrderByClause? OrderBy { get; }
    public WindowFrame? WindowFrame { get; }

    public bool MightHaveCommonTableClauses => (PartitionBy?.MightHaveCommonTableClauses ?? false) || (OrderBy?.MightHaveCommonTableClauses ?? false) || (WindowFrame?.MightHaveCommonTableClauses ?? false);

    public WindowFunction(PartitionByClause? partitionBy = null, OrderByClause? orderBy = null, WindowFrame? windowFrame = null)
    {
        PartitionBy = partitionBy;
        OrderBy = orderBy;
        WindowFrame = windowFrame;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();

        if (PartitionBy != null)
        {
            sb.Append(PartitionBy.ToSqlWithoutCte());
        }
        if (OrderBy != null)
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(OrderBy.ToSqlWithoutCte());
        }
        if (WindowFrame != null)
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(WindowFrame.ToSqlWithoutCte());
        }

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();

        if (PartitionBy != null)
        {
            lexemes.AddRange(PartitionBy.GenerateLexemesWithoutCte());
        }

        if (OrderBy != null)
        {
            lexemes.AddRange(OrderBy.GenerateLexemesWithoutCte());
        }

        if (WindowFrame != null)
        {
            lexemes.AddRange(WindowFrame.GenerateLexemesWithoutCte());
        }

        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<CommonTableClause>();
        if (PartitionBy != null)
        {
            commonTableClauses.AddRange(PartitionBy.GetCommonTableClauses());
        }
        if (OrderBy != null)
        {
            commonTableClauses.AddRange(OrderBy.GetCommonTableClauses());
        }
        if (WindowFrame != null)
        {
            commonTableClauses.AddRange(WindowFrame.GetCommonTableClauses());
        }
        return commonTableClauses;
    }
}
