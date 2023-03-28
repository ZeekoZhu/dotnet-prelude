using DotnetPrelude.SqlSugarPgvector.Test.Entity;
using Npgsql;
using SqlSugar;
using Zeeko.SqlSugar.PgVector;

namespace DotnetPrelude.SqlSugarPgvector.Test;

public class Tests
{
  private ISqlSugarClient _db;
  private NpgsqlDataSource _datasource;

  [OneTimeSetUp]
  public void Setup()
  {
    _db = new SqlSugarClient(
      new ConnectionConfig
      {
        ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres",
        DbType = DbType.PostgreSQL,
      });
    _datasource =
      new NpgsqlDataSourceBuilder(_db.CurrentConnectionConfig.ConnectionString).UsePgVector().Build();
    _db.Ado.Connection = _datasource.CreateConnection();
    _db.EnableVectorExtension();
    _db.UsePgVector();
    _db.CodeFirst.InitTables(typeof(VectorItem));
  }

  [OneTimeTearDown]
  public void TearDown()
  {
    _db.Ado.ExecuteCommand("DROP TABLE IF EXISTS vectoritem");
    _db.Dispose();
    _datasource.Dispose();
  }

  [Test]
  [Order(1)]
  public void CanInsert()
  {
    var item = new VectorItem
    {
      Id = 1,
      Vector = new PgVector(new[] { 1.0f, 2.0f, 3.0f }),
    };
    _db.Insertable(item).ExecuteCommand();
    Assert.That(_db.Queryable<VectorItem>().Count(), Is.EqualTo(1));
  }

  [Test]
  [Order(2)]
  public void CanQuery()
  {
    var item = _db.Queryable<VectorItem>().First();
    Assert.That(item, Is.Not.Null);
    Assert.That(item.Vector.Value, Is.EqualTo(new[] { 1.0f, 2.0f, 3.0f }));
  }

  [Test]
  [Order(3)]
  public void CanQueryWithWhere()
  {
    var item = _db.Queryable<VectorItem>().Where(x => x.Vector == new PgVector(new[] { 1.0f, 2.0f, 3.0f })).First();
    Assert.That(item, Is.Not.Null);
    Assert.That(item.Vector.Value, Is.EqualTo(new[] { 1.0f, 2.0f, 3.0f }));
  }
}
