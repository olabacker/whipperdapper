using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WhipperDapper.Interfaces;

namespace WhipperDapper.Tests;

public class PredicateQueryTranslatorTests
{
    [Fact]
    public void Test1()
    {
        Expression<Func<Test,bool>> func = (Test test) => test.Id == 1 && test.Id != 2;

        var translator = new PredicateQueryTranslator();
        string whereClause = translator.Translate(func);
        
        whereClause.Should().Be("((Id = 1) AND (Id != 2))");
    }
    
    
    [Fact]
    public void Test2()
    {
        Expression<Func<Test,bool>> func = (Test test) => test.SomeOtherStuff > 55;

        var translator = new PredicateQueryTranslator();
        string whereClause = translator.Translate(func);
        
        whereClause.Should().Be("(SomeOtherStuff > 55)");
    }
    
    [Fact]
    public void Test66()
    {
        Expression<Func<Whipper,bool>> func = test => test.Text == "lol";

        var translator = new PredicateQueryTranslator();
        string whereClause = translator.Translate(func);
        
        whereClause.Should().Be("(Text = 'lol')");
    }
    

    [Table("whipper")]
    private class Whipper : IEntity
    {
        public int Id { get; set; }
        
        public string Text { get; set; }
    }
    
    private class Test
    {
        public int Id { get; set; }
        
        public int SomeOtherStuff { get; set; }
    }
}