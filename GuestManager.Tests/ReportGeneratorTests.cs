using Xunit;
using GuestManager;
using System.Collections.Generic;

namespace GuestManager.Tests;

public class ReportGeneratorTests
{
    private readonly ReportGenerator _generator;

    public ReportGeneratorTests()
    {
        _generator = new ReportGenerator();
    }

    [Fact]
    public void GenerateSummary_ShouldAggregateCorrectly()
    {
        // Arrange
        var guests = new List<Guest>
        {
            new Guest { Status = RSVPStatus.Confirmed, Diet = DietaryRestriction.Vegetarian },
            new Guest { Status = RSVPStatus.Confirmed, Diet = DietaryRestriction.Vegan },
            new Guest { Status = RSVPStatus.Confirmed, Diet = DietaryRestriction.None, 
                        PlusOne = new Companion { Diet = DietaryRestriction.Vegan } },
            new Guest { Status = RSVPStatus.Pending, Diet = DietaryRestriction.GlutenFree },
            new Guest { Status = RSVPStatus.Declined, Diet = DietaryRestriction.Vegetarian }
        };

        // Act
        var summary = _generator.GenerateSummary(guests);

        // Assert
        // Headcount: 3 confirmed guests + 1 companion = 4
        Assert.Equal(4, summary.TotalHeadcount);
        
        // Pending logic
        Assert.Equal(1, summary.PendingCount);

        // Diet logic (Only confirmed guests + companions)
        Assert.Equal(1, summary.DietarySummary[DietaryRestriction.Vegetarian]);
        Assert.Equal(2, summary.DietarySummary[DietaryRestriction.Vegan]);
        Assert.Equal(1, summary.DietarySummary[DietaryRestriction.None]);
        Assert.Equal(0, summary.DietarySummary[DietaryRestriction.GlutenFree]);
    }
}