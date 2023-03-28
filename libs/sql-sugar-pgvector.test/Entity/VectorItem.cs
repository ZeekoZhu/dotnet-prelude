using SqlSugar;
using Zeeko.SqlSugar.PgVector;

namespace DotnetPrelude.SqlSugarPgvector.Test.Entity;

public class VectorItem
{
  [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
  public int Id { get; set; }

  [SugarColumn(ColumnDataType = "vector(3)", SqlParameterDbType = typeof(VectorConverter))]
  public PgVector Vector { get; set; }
}
