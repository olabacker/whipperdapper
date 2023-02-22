using Dapper.Contrib.Extensions;
using FluentAssertions;

namespace WhipperDapper.Tests;

public class TableGeneratorTests
{
    private readonly TableGenerator _tableGenerator = new();

    [Fact]
    public void Test1()
    {
        var createString = _tableGenerator.GenerateCreateQuery<Whipper>();

        createString.Should().Be("CREATE TABLE IF NOT EXISTS whipper (Id INT NOT NULL AUTO_INCREMENT, PRIMARY KEY (Id))");
    }
    
    
    [Table("whipper")]
    private class Whipper : IEntity
    {
        public int Id { get; set; }
    }
}