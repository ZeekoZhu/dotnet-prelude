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
  public void CanQueryWithDistance()
  {
    var other = new PgVector(new[] { 1.0f, 2.0f, 3.0f });
    var item = _db.Queryable<VectorItem>()
      .Select(x => VectorOperator.L2Distance(x.Vector, other))
      .First();
    Assert.That(item, Is.EqualTo(0.0f));
  }

  [Test]
  [Order(4)]
  public void CanFilterWithDistance()
  {
    var other = new PgVector(new[] { 1.0f, 2.0f, 3.0f });
    var l2Distance = _db.Queryable<VectorItem>()
      .Where(x => VectorOperator.L2Distance(x.Vector, other) < 1.0f)
      .First();
    Assert.That(l2Distance, Is.Not.Null);
    var cosineDistance = _db.Queryable<VectorItem>()
      .Where(x => VectorOperator.CosineDistance(x.Vector, other) < 1.0f)
      .First();
    Assert.That(cosineDistance, Is.Not.Null);
    var innerProduct = _db.Queryable<VectorItem>()
      .Where(x => VectorOperator.InnerProduct(x.Vector, other) < 1.0f)
      .First();
    Assert.That(innerProduct, Is.Not.Null);
  }

  [Test]
  [Order(5)]
  public void CanQueryAvg()
  {
    var item = _db.Queryable<VectorItem>()
      .Select(x => new VectorItem { Vector = SqlFunc.AggregateAvg(x.Vector) })
      .First();
    Assert.That(item, Is.Not.Null);
    Assert.That(item.Vector.Value, Is.EquivalentTo(new[] { 1.0f, 2.0f, 3.0f }));
  }
}
