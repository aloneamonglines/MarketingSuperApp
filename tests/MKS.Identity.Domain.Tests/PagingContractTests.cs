using System.ComponentModel.DataAnnotations;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Extensions;

namespace MKS.Identity.Domain.Tests;

public sealed class PagingContractTests
{
    [Fact]
    public void PageRequestSupportsStandardFilterSortAndBounds()
    {
        var request = new PagedAndSortedResultRequestDto
        {
            SkipCount = 5,
            MaxResultCount = 20,
            Sorting = "CreatedAt DESC",
            Filter = "campaign"
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.True(isValid);
        Assert.Equal(5, request.SkipCount);
        Assert.Equal(20, request.MaxResultCount);
        Assert.Equal("CreatedAt DESC", request.Sorting);
        Assert.Equal("campaign", request.Filter);
    }

    [Fact]
    public void PageByAppliesOffsetAndMaximumSize()
    {
        var request = new PagedAndSortedResultRequestDto
        {
            SkipCount = 2,
            MaxResultCount = 2
        };

        var page = Enumerable.Range(1, 10)
            .AsQueryable()
            .PageBy(request)
            .ToArray();

        Assert.Equal([3, 4], page);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 0)]
    [InlineData(0, 1001)]
    public void PageRequestValidationRejectsInvalidBounds(int skipCount, int maxResultCount)
    {
        var request = new PagedAndSortedResultRequestDto
        {
            SkipCount = skipCount,
            MaxResultCount = maxResultCount
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
    }

    [Fact]
    public void PageByClampsUnvalidatedBounds()
    {
        var request = new PagedAndSortedResultRequestDto
        {
            SkipCount = -1,
            MaxResultCount = 1001
        };

        var page = Enumerable.Range(1, 1100)
            .AsQueryable()
            .PageBy(request)
            .ToArray();

        Assert.Equal(1000, page.Length);
        Assert.Equal(1, page[0]);
        Assert.Equal(1000, page[^1]);
    }

    [Fact]
    public void PagedResultIncludesTotalCountAndItems()
    {
        var items = new[] { "one", "two" };
        var result = new PagedResultDto<string>(5, items);

        Assert.Equal(5, result.TotalCount);
        Assert.Same(items, result.Items);
    }
}
