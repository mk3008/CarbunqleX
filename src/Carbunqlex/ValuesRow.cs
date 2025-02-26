using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex;

public class ValuesRow : ISqlComponent
{
    public List<IValueExpression> Columns { get; } = new();

    public ValuesRow()
    {
    }
    public ValuesRow(IEnumerable<IValueExpression> columns)
    {
        Columns.AddRange(columns);
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder("(");
        for (int i = 0; i < Columns.Count; i++)
        {
            sb.Append(Columns[i].ToSqlWithoutCte());
            if (i < Columns.Count - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(")");
        return sb.ToString();
    }

    /// <summary>
    /// The number of tokens required to represent this row.
    /// </summary>
    internal int Capacity => Columns.Count * 2 - 1 + 2;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>(Capacity) { new Token(TokenType.OpenParen, "(") };

        for (int i = 0; i < Columns.Count; i++)
        {
            tokens.AddRange(Columns[i].GenerateTokensWithoutCte());

            if (i < Columns.Count - 1)
            {
                tokens.Add(new Token(TokenType.Comma, ","));
            }
        }

        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Columns
            .Where(column => column.MightHaveQueries)
            .SelectMany(column => column.GetQueries());
    }
}
