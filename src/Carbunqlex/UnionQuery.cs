namespace Carbunqlex.Clauses;

public enum UnionType : byte
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
        return $"{Left.ToSql()} {UnionType.ToSqlString()} {Right.ToSql()}";
    }

    public IEnumerable<Lexeme> GenerateLexemes()
    {
        var leftLexemes = Left.GenerateLexemes().ToList();
        var rightLexemes = Right.GenerateLexemes().ToList();

        // Initial capacity is set to accommodate the lexemes from Left, Right, and the UnionType keyword.
        var lexemes = new List<Lexeme>(leftLexemes.Count + rightLexemes.Count + 1);
        lexemes.AddRange(leftLexemes);
        lexemes.Add(new Lexeme(LexType.Keyword, UnionType.ToSqlString()));
        lexemes.AddRange(rightLexemes);
        return lexemes;
    }

    public string ToSqlWithoutCte()
    {
        return $"{Left.ToSqlWithoutCte()} {UnionType.ToSqlString()} {Right.ToSqlWithoutCte()}";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var leftLexemes = Left.GenerateLexemesWithoutCte().ToList();
        var rightLexemes = Right.GenerateLexemesWithoutCte().ToList();

        // Initial capacity is set to accommodate the lexemes from Left, Right, and the UnionType keyword.
        var lexemes = new List<Lexeme>(leftLexemes.Count + rightLexemes.Count + 1);
        lexemes.AddRange(leftLexemes);
        lexemes.Add(new Lexeme(LexType.Keyword, UnionType.ToSqlString()));
        lexemes.AddRange(rightLexemes);
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<CommonTableClause>();
        commonTableClauses.AddRange(Left.GetCommonTableClauses());
        commonTableClauses.AddRange(Right.GetCommonTableClauses());
        return commonTableClauses;
    }
}
