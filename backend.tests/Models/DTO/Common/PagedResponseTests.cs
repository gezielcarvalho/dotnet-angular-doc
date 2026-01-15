using Backend.Models.DTO.Common;
using FluentAssertions;

namespace backend.tests.Models.DTO.Common;

public class PagedResponseTests
{
    [Fact]
    public void PagedResponse_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var response = new PagedResponse<string>();

        // Assert
        response.Items.Should().NotBeNull();
        response.Items.Should().BeEmpty();
        response.PageNumber.Should().Be(0);
        response.PageSize.Should().Be(0);
        response.TotalPages.Should().Be(0);
        response.TotalCount.Should().Be(0);
        response.HasPrevious.Should().BeFalse();
        response.HasNext.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_ShouldSetProperties()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };

        // Act
        var response = new PagedResponse<string>
        {
            Items = items,
            PageNumber = 2,
            PageSize = 10,
            TotalPages = 5,
            TotalCount = 50
        };

        // Assert
        response.Items.Should().BeEquivalentTo(items);
        response.PageNumber.Should().Be(2);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(5);
        response.TotalCount.Should().Be(50);
    }

    [Fact]
    public void HasPrevious_ShouldBeTrue_WhenPageNumberGreaterThanOne()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            PageNumber = 2,
            TotalPages = 5
        };

        // Assert
        response.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasPrevious_ShouldBeFalse_WhenPageNumberIsOne()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            PageNumber = 1,
            TotalPages = 5
        };

        // Assert
        response.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_ShouldBeFalse_WhenPageNumberIsZero()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            PageNumber = 0,
            TotalPages = 5
        };

        // Assert
        response.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasNext_ShouldBeTrue_WhenPageNumberLessThanTotalPages()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            PageNumber = 2,
            TotalPages = 5
        };

        // Assert
        response.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_ShouldBeFalse_WhenPageNumberEqualsTotalPages()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            PageNumber = 5,
            TotalPages = 5
        };

        // Assert
        response.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_ShouldBeFalse_WhenPageNumberGreaterThanTotalPages()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            PageNumber = 6,
            TotalPages = 5
        };

        // Assert
        response.HasNext.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_ShouldWorkWithDifferentTypes()
    {
        // Arrange
        var intResponse = new PagedResponse<int>();
        var objectResponse = new PagedResponse<object>();

        // Act & Assert
        intResponse.Items.Should().NotBeNull();
        intResponse.Items.Should().BeEmpty();
        objectResponse.Items.Should().NotBeNull();
        objectResponse.Items.Should().BeEmpty();
    }
}