using System.Text;

namespace Carbunqlex.Clauses;

public class NamelessWindowDefinition : IWindowDefinition
{
    public PartitionByClause? PartitionBy { get; }
    public OrderByClause? OrderBy { get; }
    public BetweenWindowFrame? WindowFrame { get; }

    public bool MightHaveCommonTableClauses =>
        (PartitionBy?.MightHaveQueries ?? false) ||
        (OrderBy?.MightHaveQueries ?? false) ||
        (WindowFrame?.MightHaveCommonTableClauses ?? false);

    public NamelessWindowDefinition(PartitionByClause? partitionBy, OrderByClause? orderBy, BetweenWindowFrame? windowFrame)
    {
        PartitionBy = partitionBy;
        OrderBy = orderBy;
        WindowFrame = windowFrame;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();

        sb.Append("(");

        if (PartitionBy != null)
        {
            sb.Append(PartitionBy.ToSqlWithoutCte());
        }
        if (OrderBy != null)
        {
            if (sb.Length != 1)
            {
                sb.Append(' ');
            }
            sb.Append(OrderBy.ToSqlWithoutCte());
        }
        if (WindowFrame != null)
        {
            if (sb.Length != 1)
            {
                sb.Append(' ');
            }
            sb.Append(WindowFrame.ToSqlWithoutCte());
        }

        sb.Append(")");

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();

        tokens.Add(new Token(TokenType.OpenParen, "("));

        if (PartitionBy != null)
        {
            tokens.AddRange(PartitionBy.GenerateTokensWithoutCte());
        }
        if (OrderBy != null)
        {
            tokens.AddRange(OrderBy.GenerateTokensWithoutCte());
        }
        if (WindowFrame != null)
        {
            tokens.AddRange(WindowFrame.GenerateTokensWithoutCte());
        }

        tokens.Add(new Token(TokenType.CloseParen, ")"));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (PartitionBy != null)
        {
            queries.AddRange(PartitionBy.GetQueries());
        }
        if (OrderBy != null)
        {
            queries.AddRange(OrderBy.GetQueries());
        }
        if (WindowFrame != null)
        {
            queries.AddRange(WindowFrame.GetQueries());
        }

        return queries;
    }
}
