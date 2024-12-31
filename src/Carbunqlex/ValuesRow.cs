using Carbunqlex.ValueExpressions;
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
    /// The number of lexemes required to represent this row.
    /// </summary>
    internal int Capacity => Columns.Count * 2 - 1 + 2;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>(Capacity) { new Lexeme(LexType.OpenParen, "(") };

        for (int i = 0; i < Columns.Count; i++)
        {
            lexemes.AddRange(Columns[i].GenerateLexemesWithoutCte());

            if (i < Columns.Count - 1)
            {
                lexemes.Add(new Lexeme(LexType.Comma, ","));
            }
        }

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        return lexemes;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Columns
            .Where(column => column.MightHaveQueries)
            .SelectMany(column => column.GetQueries());
    }
}
