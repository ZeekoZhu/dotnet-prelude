using Npgsql;
using SqlSugar;

namespace Zeeko.SqlSugar.PgVector;

public static class SqlSugarClientExtension
{
  public static void EnableVectorExtension(this ISqlSugarClient client)
  {
    client.Ado.ExecuteCommand("CREATE EXTENSION IF NOT EXISTS vector");
    (client.Ado.Connection as NpgsqlConnection)?.ReloadTypes();
  }

  public static Task EnableVectorExtensionAsync(this ISqlSugarClient client)
  {
    return client.Ado.ExecuteCommandAsync("CREATE EXTENSION IF NOT EXISTS vector");
  }

  public static NpgsqlDataSourceBuilder UsePgVector(this NpgsqlDataSourceBuilder mapper)
  {
    mapper.AddTypeResolverFactory(new PgVectorTypeResolverFactory());
    return mapper;
  }

  public static void UsePgVector(this ISqlSugarClient client)
  {
  }
}
