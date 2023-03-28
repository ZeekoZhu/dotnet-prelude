namespace Zeeko.SqlSugar.PgVector;

public class PgVector
{
  public PgVector(float[] value)
  {
    Value = value;
  }

  public PgVector(double[] value)
  {
    Value = value.Select(x => (float)x).ToArray();
  }

  public float[] Value { get; set; }
}
