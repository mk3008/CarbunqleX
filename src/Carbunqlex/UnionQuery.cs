using System.Text;

namespace Carbunqlex.Clauses;

public enum UnionType
{
    Union,
    UnionAll,
    Intersect,
    Except
}

internal static class UnionTypeExtensions
{
    public static string ToSqlString(this UnionType unionType)
    {
        return unionType switch
        {
            UnionType.Union => "union",
            UnionType.UnionAll => "union all",
            UnionType.Intersect => "intersect",
            UnionType.Except => "except",
            _ => throw new ArgumentOutOfRangeException(nameof(unionType), unionType, null)
        };
    }
}

public class UnionQuery : IQuery
{
    public IQuery Left { get; }
    public IQuery Right { get; }
    public UnionType UnionType { get; }

    public UnionQuery(IQuery left, IQuery right, UnionType unionType)
    {
        Left = left;
        Right = right;
        UnionType = unionType;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSql());
        sb.Append(" ");
        sb.Append(UnionType.ToSqlString());
        sb.Append(" ");
        sb.Append(Right.ToSql());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>();
        lexemes.AddRange(Left.GetLexemes());
        lexemes.Add(new Lexeme(LexType.Keyword, UnionType.ToSqlString()));
        lexemes.AddRange(Right.GetLexemes());
        return lexemes;
    }
}
