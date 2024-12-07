namespace Carbunqlex;

public interface ISqlComponent
{
    string ToSql();
    IEnumerable<Lexeme> GetLexemes();
}
