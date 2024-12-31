using System.Text;

namespace Carbunqlex.Clauses;

public class WindowFunction : IWindowFunction
{
    public IPartitionByClause PartitionBy { get; }
    public IOrderByClause OrderBy { get; }
    public IWindowFrame WindowFrame { get; }

    public bool MightHaveCommonTableClauses =>
        PartitionBy.MightHaveQueries ||
        OrderBy.MightHaveQueries ||
        WindowFrame.MightHaveCommonTableClauses;

    public WindowFunction(IPartitionByClause partitionBy, IOrderByClause orderBy, IWindowFrame windowFrame)
    {
        PartitionBy = partitionBy;
        OrderBy = orderBy;
        WindowFrame = windowFrame;
    }

    public WindowFunction(IPartitionByClause partitionBy, IOrderByClause orderBy)
        : this(partitionBy, orderBy, EmptyWindowFrame.Instance)
    {
    }

    public WindowFunction(IPartitionByClause partitionBy, IWindowFrame windowFrame)
        : this(partitionBy, EmptyOrderByClause.Instance, windowFrame)
    {
    }

    public WindowFunction(IOrderByClause orderBy, IWindowFrame windowFrame)
        : this(EmptyPartitionByClause.Instance, orderBy, windowFrame)
    {
    }

    public WindowFunction(IPartitionByClause partitionBy)
        : this(partitionBy, EmptyOrderByClause.Instance, EmptyWindowFrame.Instance)
    {
    }

    public WindowFunction(IOrderByClause orderBy)
        : this(EmptyPartitionByClause.Instance, orderBy, EmptyWindowFrame.Instance)
    {
    }

    public WindowFunction(IWindowFrame windowFrame)
        : this(EmptyPartitionByClause.Instance, EmptyOrderByClause.Instance, windowFrame)
    {
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();

        sb.Append(PartitionBy.ToSqlWithoutCte());
        if (!string.IsNullOrEmpty(OrderBy.ToSqlWithoutCte()))
        {
            sb.Append(" ").Append(OrderBy.ToSqlWithoutCte());
        }
        if (!string.IsNullOrEmpty(WindowFrame.ToSqlWithoutCte()))
        {
            sb.Append(" ").Append(WindowFrame.ToSqlWithoutCte());
        }

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();

        lexemes.AddRange(PartitionBy.GenerateLexemesWithoutCte());
        lexemes.AddRange(OrderBy.GenerateLexemesWithoutCte());
        lexemes.AddRange(WindowFrame.GenerateLexemesWithoutCte());

        return lexemes;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        queries.AddRange(PartitionBy.GetQueries());
        queries.AddRange(OrderBy.GetQueries());
        queries.AddRange(WindowFrame.GetQueries());

        return queries;
    }
}
