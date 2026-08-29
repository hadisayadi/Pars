using FluentAssertions;
using Pars.Application.Common.Filtering;
using Pars.Domain.Entities;
using Xunit;

namespace Pars.Tests.Filtering;

public class QueryBuilderTests
{
    private readonly List<Personal> _testData = new()
    {
        new Personal { Id = "1", FirstName = "علی", LastName = "محمدی", CodeMelli = "001", Jensiat = "مرد", Company = "نفت" },
        new Personal { Id = "2", FirstName = "رضا", LastName = "کریمی", CodeMelli = "002", Jensiat = "مرد", Company = "گاز" },
        new Personal { Id = "3", FirstName = "زهرا", LastName = "رضایی", CodeMelli = "003", Jensiat = "زن", Company = "نفت" },
        new Personal { Id = "4", FirstName = "مریم", LastName = "احمدی", CodeMelli = "004", Jensiat = "زن", Company = "پخش" },
    };

    [Fact]
    public void Apply_WithEqualsFilter_ShouldFilterCorrectly()
    {
        var request = new QueryRequest
        {
            Filters = new List<FilterDescriptor>
            {
                new() { Field = "Jensiat", Operator = FilterOperator.Equals, Value = "مرد" }
            }
        };

        var result = QueryBuilder<Personal>.Apply(_testData.AsQueryable(), request).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Jensiat == "مرد");
    }

    [Fact]
    public void Apply_WithContainsFilter_ShouldFilterBySubstring()
    {
        var request = new QueryRequest
        {
            Filters = new List<FilterDescriptor>
            {
                new() { Field = "FirstName", Operator = FilterOperator.Contains, Value = "لی" }
            }
        };

        var result = QueryBuilder<Personal>.Apply(_testData.AsQueryable(), request).ToList();

        result.Should().ContainSingle();
        result[0].FirstName.Should().Be("علی");
    }

    [Fact]
    public void Apply_WithSorting_ShouldSortAscending()
    {
        var request = new QueryRequest
        {
            Sorts = new List<SortDescriptor>
            {
                new() { Field = "LastName", Descending = false }
            }
        };

        var result = QueryBuilder<Personal>.Apply(_testData.AsQueryable(), request).ToList();

        result[0].LastName.Should().Be("احمدی");
        result[^1].LastName.Should().Be("محمدی");
    }

    [Fact]
    public void Apply_WithPaging_ShouldReturnCorrectPage()
    {
        var request = new QueryRequest
        {
            Page = 2,
            PageSize = 2,
            Sorts = new List<SortDescriptor> { new() { Field = "Id" } }
        };

        var result = QueryBuilder<Personal>.Apply(_testData.AsQueryable(), request).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("3");
    }

    [Fact]
    public void Apply_WithInFilter_ShouldFilterMultipleValues()
    {
        var request = new QueryRequest
        {
            Filters = new List<FilterDescriptor>
            {
                new() { Field = "Company", Operator = FilterOperator.In, Value = new List<string> { "نفت", "گاز" } }
            }
        };

        var result = QueryBuilder<Personal>.Apply(_testData.AsQueryable(), request).ToList();

        result.Should().HaveCount(3);
    }
}