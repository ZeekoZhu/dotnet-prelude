using System.Data;
using Newtonsoft.Json;
using SqlSugar;
using DbType = System.Data.DbType;

namespace Zeeko.SqlSugar.PgVector;

public class VectorConverter : ISugarDataConverter
{
  public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
  {
    var pgVector = columnValue as PgVector;
    // null check
    if (pgVector == null)
    {
      throw new ArgumentException("columnValue must be a PgVector");
    }
    var name = $"@vector_p_{columnIndex}";
    var value = columnValue as PgVector;
    var parameter = new SugarParameter(name, null)
    {
      DbType = DbType.Object,
      Value = value
    };
    return parameter;
  }

  public T QueryConverter<T>(IDataRecord dataRecord, int dataRecordIndex)
  {
    var value = dataRecord.GetValue(dataRecordIndex);
    return (T)Convert.ChangeType(value, typeof(T));
  }
}
