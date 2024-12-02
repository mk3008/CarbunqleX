namespace Carbunqlex.QueryModels;

public interface IValueExpression
{
    string DefaultName { get; }
    string ToSql();
    IEnumerable<Lexeme> GetLexemes();
}
