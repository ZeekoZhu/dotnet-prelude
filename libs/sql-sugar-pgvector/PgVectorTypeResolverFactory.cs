using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;

namespace Zeeko.SqlSugar.PgVector;

internal class PgVectorTypeResolverFactory : TypeHandlerResolverFactory
{
  public override TypeHandlerResolver Create(NpgsqlConnector connector)
  {
    return new PgVectorTypeHandlerResolver(connector);
  }

  public override string? GetDataTypeNameByClrType(Type clrType)
  {
    return clrType == typeof(PgVector) ? PgVectorTypeHandlerResolver.DbTypeName : null;
  }

  public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
  {
    return PgVectorTypeHandlerResolver.CreateMappingByDataTypeName(dataTypeName);
  }
}
