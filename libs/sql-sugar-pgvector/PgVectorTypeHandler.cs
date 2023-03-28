using Npgsql;
using Npgsql.BackendMessages;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;

namespace Zeeko.SqlSugar.PgVector;

internal class PgVectorTypeHandler : NpgsqlSimpleTypeHandler<PgVector>
{
  public override int ValidateObjectAndGetLength(
    object? value,
    ref NpgsqlLengthCache? lengthCache,
    NpgsqlParameter? parameter)
  {
    switch (value)
    {
      case null:
      case DBNull:
        return 0;
      case PgVector vector:
        return ValidateAndGetLength(vector, parameter);
      default:
        throw new ArgumentException("value must be a PgVector");
    }
  }

  public override Task WriteObjectWithLength(
    object? value,
    NpgsqlWriteBuffer buf,
    NpgsqlLengthCache? lengthCache,
    NpgsqlParameter? parameter,
    bool async,
    CancellationToken cancellationToken = new())
  {
    switch (value)
    {
      case PgVector vector:
        return WriteWithLength(vector, buf, lengthCache, parameter, async, cancellationToken);
      case DBNull:
      case null:
        return WriteWithLength(DBNull.Value, buf, lengthCache, parameter, async, cancellationToken);
      default:
        throw new InvalidOperationException($"Can not write '{value.GetType()}' with handler type PgVectorTypeHandler");
    }
  }

  public override PgVector Read(NpgsqlReadBuffer buf, int len, FieldDescription? fieldDescription = null)
  {
    // ushort dimension = bytes[0, 1]
    // unknown bytes[2, 3]
    // float[] bytes[4, len]
    // see pgvector for details
    var dim = buf.ReadUInt16();
    var unused = buf.ReadUInt16();
    if (unused != 0)
    {
      throw new InvalidOperationException("invalid vector, unused bytes must be 0");
    }

    var vector = new float[dim];
    for (var i = 0; i < dim; i++)
    {
      vector[i] = buf.ReadSingle();
    }

    return new PgVector(vector);
  }

  public override int ValidateAndGetLength(PgVector value, NpgsqlParameter? parameter)
  {
    return sizeof(ushort) * 2 + sizeof(float) * value.Value.Length;
  }

  public override void Write(PgVector value, NpgsqlWriteBuffer buf, NpgsqlParameter? parameter)
  {
    var dim = (short)value.Value.Length;
    buf.WriteInt16(dim);
    buf.WriteInt16(0);
    foreach (var v in value.Value)
    {
      buf.WriteSingle(v);
    }
  }

  public PgVectorTypeHandler(PostgresType postgresType) : base(postgresType)
  {
  }
}
