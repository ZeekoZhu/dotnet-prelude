using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;
using NpgsqlTypes;

namespace Zeeko.SqlSugar.PgVector;

internal class PgVectorTypeHandlerResolver : TypeHandlerResolver
{
  public const string DbTypeName = "vector";
  readonly NpgsqlDatabaseInfo _databaseInfo;
  private readonly PgVectorTypeHandler? _handler;
  private readonly TypeMappingInfo? _mappingInfo;

  public PgVectorTypeHandlerResolver(NpgsqlConnector connector)
  {
    _databaseInfo = connector.DatabaseInfo;
    _handler = PgType(DbTypeName) is { } vectorType ? new PgVectorTypeHandler(vectorType) : null;
    _mappingInfo = CreateMappingByDataTypeName(DbTypeName);
  }

  internal static TypeMappingInfo? CreateMappingByDataTypeName(string dataTypeName)
    => dataTypeName switch
    {
      DbTypeName => new(NpgsqlDbType.Unknown, DbTypeName),
      _ => null
    };

  private PostgresType? PgType(string pgTypeName) =>
    _databaseInfo.TryGetPostgresTypeByName(pgTypeName, out var pgType) ? pgType : null;

  public override NpgsqlTypeHandler? ResolveByDataTypeName(string typeName)
  {
    return typeName == DbTypeName ? _handler : null;
  }

  public override NpgsqlTypeHandler? ResolveByClrType(Type type)
  {
    return type == typeof(PgVector) ? _handler : null;
  }

  public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
  {
    return dataTypeName == DbTypeName ? _mappingInfo : null;
  }
}
