using Dapper.Contrib.Extensions;
using FluentAssertions;
using WhipperDapper.Interfaces;

namespace WhipperDapper.Tests;

public class TableGeneratorTests
{
    private readonly TableGenerator _tableGenerator = new();

    [Fact]
    public void Test1()
    {
        var createString = TableGenerator.GenerateCreateQuery<Whipper>();

        createString.Should().Be("CREATE TABLE IF NOT EXISTS whipper (Id INT NOT NULL AUTO_INCREMENT, PRIMARY KEY (Id))");
    }
    
    [Fact]
    public void Test2()
    {
        var createString = TableGenerator.GenerateCreateQuery<WhipperWithEnum>();

        createString.Should().Be("CREATE TABLE IF NOT EXISTS ObjectWithEnum (Id INT NOT NULL AUTO_INCREMENT, Enum INT NOT NULL, TimeHehe DATETIME NULL, PRIMARY KEY (Id))");
    }
    
    [Table("whipper")]
    private class Whipper : IEntity
    {
        public int Id { get; set; }
    }
    
    public enum WhipperEnum
    {
        Lol,
        Lmao
    }
    
    [Table("ObjectWithEnum")]
    private class WhipperWithEnum : IEntity
    {
        public int Id { get; set; }
        
        public WhipperEnum Enum { get; set; }
        
        public DateTime? TimeHehe { get; set; }
    }
}