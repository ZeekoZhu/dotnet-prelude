using System.Data;
using SqlSugar;

namespace Zeeko.SqlSugar.PgVector;

public class VectorConverter : ISugarDataConverter
{
  public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
  {
    // null check
    if (columnValue is not PgVector value)
    {
      throw new ArgumentException("columnValue must be a PgVector");
    }

    return value.ToSugarParameter($"@vector_p_{columnIndex}");
  }

  public T QueryConverter<T>(IDataRecord dataRecord, int dataRecordIndex)
  {
    var value = dataRecord.GetValue(dataRecordIndex);
    return (T)Convert.ChangeType(value, typeof(T));
  }
}
