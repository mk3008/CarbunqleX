namespace Carbunqlex.DatasourceExpressions;

public interface IDatasource : ISqlComponent
{
    string Alias { get; set; }
}
