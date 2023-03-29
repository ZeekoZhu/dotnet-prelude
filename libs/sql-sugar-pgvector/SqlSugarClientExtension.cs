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

  public static async Task EnableVectorExtensionAsync(this ISqlSugarClient client)
  {
    await client.Ado.ExecuteCommandAsync("CREATE EXTENSION IF NOT EXISTS vector");
    if (client.Ado.Connection is NpgsqlConnection conn)
    {
      await conn.ReloadTypesAsync();
    }
  }

  private static KeyValuePair<string, SugarParameter[]?> Identity(string sql, SugarParameter[]? pars) => new(sql, pars);

  public static NpgsqlDataSourceBuilder UsePgVector(this NpgsqlDataSourceBuilder mapper)
  {
    mapper.AddTypeResolverFactory(new PgVectorTypeResolverFactory());
    return mapper;
  }


  public static void UsePgVector(
    this ISqlSugarClient client,
    Func<string, SugarParameter[]?, KeyValuePair<string, SugarParameter[]?>>? onExecutingChangeSql = null)
  {
    var finalOnExecutingChangeSql = onExecutingChangeSql ?? Identity;
    client.Aop.OnExecutingChangeSql = (sql, pars) =>
    {
      if (pars != null)
      {
        foreach (var sugarParameter in pars)
        {
          if (sugarParameter.Value is PgVector)
          {
            sugarParameter.DbType = System.Data.DbType.Object;
          }
        }
      }

      return finalOnExecutingChangeSql(sql, pars);
    };

    var expMethods = new List<SqlFuncExternal>()
    {
      new()
      {
        UniqueMethodName = nameof(VectorOperator.L2Distance),
        MethodValue = CreateBinaryOperator("<->")
      },
      new()
      {
        UniqueMethodName = nameof(VectorOperator.CosineDistance),
        MethodValue = CreateBinaryOperator("<=>")
      },
      new()
      {
        UniqueMethodName = nameof(VectorOperator.InnerProduct),
        MethodValue = CreateBinaryOperator("<#>")
      }
    };

    client.CurrentConnectionConfig.ConfigureExternalServices.SqlFuncServices ??= new();
    client.CurrentConnectionConfig.ConfigureExternalServices.SqlFuncServices.AddRange(expMethods);
  }

  private static Func<MethodCallExpressionModel, DbType, ExpressionContext, string> CreateBinaryOperator(string op)
  {
    return (expInfo, dbType, _) =>
    {
      return dbType switch
      {
        DbType.PostgreSQL => $"({expInfo.Args[0].MemberName} {op} {expInfo.Args[1].MemberName})",
        _ => throw new NotSupportedException("Only PostgreSQL is supported.")
      };
    };
  }

  public static SugarParameter ToSugarParameter(this PgVector vector, string name)
  {
    return new SugarParameter(name, null)
    {
      Value = vector,
      DbType = System.Data.DbType.Object
    };
  }
}
