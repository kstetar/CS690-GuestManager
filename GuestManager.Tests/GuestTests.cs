using Xunit;
using GuestManager;

namespace GuestManager.Tests;

public class GuestTests
{
    [Fact]
    public void UpdateRSVP_ChangesStatus()
    {
        // Arrange
        var guest = new Guest { FirstName = "Test", LastName = "User", Status = RSVPStatus.Pending };

        // Act
        guest.UpdateRSVP(RSVPStatus.Confirmed);

        // Assert
        Assert.Equal(RSVPStatus.Confirmed, guest.Status);
    }

    [Fact]
    public void AddCompanion_AssignsPlusOne()
    {
        // Arrange
        var guest = new Guest { FirstName = "Main", LastName = "Guest" };
        var companion = new Companion { Name = "Plus One", Diet = DietaryRestriction.None };

        // Act
        guest.AddCompanion(companion);

        // Assert
        Assert.NotNull(guest.PlusOne);
        Assert.Equal("Plus One", guest.PlusOne.Name);
    }
}
