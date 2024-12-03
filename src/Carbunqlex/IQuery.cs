namespace Carbunqlex;

public interface IQuery
{
    string ToSql();
    IEnumerable<Lexeme> GetLexemes();
}