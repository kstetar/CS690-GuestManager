using Moq;
using Xunit;
using GuestManager;
using System.Collections.Generic;
using System.Linq;

namespace GuestManager.Tests;

public class GuestServiceTests
{
    private readonly Mock<IGuestRepository> _mockRepo;
    private readonly GuestService _service;

    public GuestServiceTests()
    {
        _mockRepo = new Mock<IGuestRepository>();
        _service = new GuestService(_mockRepo.Object);
    }

    [Fact]
    public void AddGuest_ShouldCallRepositoryAdd()
    {
        // Arrange
        string firstName = "John";
        string lastName = "Doe";

        // Act
        _service.AddGuest(firstName, lastName);

        // Assert
        _mockRepo.Verify(r => r.Add(It.Is<Guest>(g => 
            g.FirstName == firstName && 
            g.LastName == lastName &&
            g.Status == RSVPStatus.Pending &&
            g.Diet == DietaryRestriction.None)), Times.Once);
    }

    [Fact]
    public void UpdateRsvp_ShouldUpdateStatus_WhenGuestExists()
    {
        // Arrange
        var guest = new Guest { Id = 1, Status = RSVPStatus.Pending };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        // Act
        _service.UpdateRsvp(1, RSVPStatus.Confirmed);

        // Assert
        Assert.Equal(RSVPStatus.Confirmed, guest.Status);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }

    [Fact]
    public void AddCompanion_ShouldAddPlusOne_WhenGuestExists()
    {
        // Arrange
        var guest = new Guest { Id = 1, FirstName = "Jane" };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        // Act
        _service.AddCompanion(1, "Partner", DietaryRestriction.Vegan);

        // Assert
        Assert.NotNull(guest.PlusOne);
        Assert.Equal("Partner", guest.PlusOne.Name);
        Assert.Equal(DietaryRestriction.Vegan, guest.PlusOne.Diet);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }

    [Fact]
    public void RemoveGuest_ShouldCallRepositoryRemove_WhenGuestExists()
    {
        // Arrange
        var guest = new Guest { Id = 1, FirstName = "RemoveMe" };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        // Act
        _service.RemoveGuest(1);

        // Assert
        _mockRepo.Verify(r => r.Remove(guest), Times.Once);
    }

    [Fact]
    public void UpdateRsvp_ShouldNotCallUpdate_WhenGuestNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest>());

        // Act
        _service.UpdateRsvp(99, RSVPStatus.Confirmed);

        // Assert
        _mockRepo.Verify(r => r.Update(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public void Initialize_ShouldCallRepositoryLoad()
    {
        _service.Initialize();
        _mockRepo.Verify(r => r.LoadFromFile(), Times.Once);
    }

    [Fact]
    public void GetAllGuests_ShouldCallRepositoryGetAll()
    {
        _service.GetAllGuests();
        _mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void UpdateDiet_ShouldUpdateDiet_WhenGuestExists()
    {
        var guest = new Guest { Id = 1, Diet = DietaryRestriction.None };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        _service.UpdateDiet(1, DietaryRestriction.Vegan);

        Assert.Equal(DietaryRestriction.Vegan, guest.Diet);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }

    [Fact]
    public void UpdateDiet_ShouldNotCallUpdate_WhenGuestNotFound()
    {
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest>());
        _service.UpdateDiet(99, DietaryRestriction.Vegan);
        _mockRepo.Verify(r => r.Update(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public void RemoveCompanion_ShouldRemovePlusOne_WhenGuestExists()
    {
        var guest = new Guest { Id = 1, PlusOne = new Companion { Name = "Partner" } };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        _service.RemoveCompanion(1);

        Assert.Null(guest.PlusOne);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }

    [Fact]
    public void RemoveCompanion_ShouldNotCallUpdate_WhenGuestNotFound()
    {
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest>());
        _service.RemoveCompanion(99);
        _mockRepo.Verify(r => r.Update(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public void AddCompanion_ShouldNotCallUpdate_WhenGuestNotFound()
    {
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest>());
        _service.AddCompanion(99, "Partner", DietaryRestriction.Vegan);
        _mockRepo.Verify(r => r.Update(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public void RemoveGuest_ShouldNotCallRemove_WhenGuestNotFound()
    {
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest>());
        _service.RemoveGuest(99);
        _mockRepo.Verify(r => r.Remove(It.IsAny<Guest>()), Times.Never);
    }

    [Fact]
    public void UpdateDiet_ShouldSetCustomDietNote_WhenDietIsOther()
    {
        var guest = new Guest { Id = 1, Diet = DietaryRestriction.None };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        _service.UpdateDiet(1, DietaryRestriction.Other, "No mushrooms");

        Assert.Equal(DietaryRestriction.Other, guest.Diet);
        Assert.Equal("No mushrooms", guest.CustomDietNote);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }

    [Fact]
    public void UpdateDiet_ShouldClearCustomDietNote_WhenDietIsNotOther()
    {
        var guest = new Guest
        {
            Id = 1,
            Diet = DietaryRestriction.Other,
            CustomDietNote = "No dairy"
        };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        _service.UpdateDiet(1, DietaryRestriction.Vegan, "ignored");

        Assert.Equal(DietaryRestriction.Vegan, guest.Diet);
        Assert.Null(guest.CustomDietNote);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }

    [Fact]
    public void AddCompanion_ShouldSetCustomDietNote_WhenDietIsOther()
    {
        var guest = new Guest { Id = 1, FirstName = "Jane" };
        _mockRepo.Setup(r => r.GetAll()).Returns(new List<Guest> { guest });

        _service.AddCompanion(1, "Partner", DietaryRestriction.Other, "Allergic to sesame");

        Assert.NotNull(guest.PlusOne);
        Assert.Equal(DietaryRestriction.Other, guest.PlusOne.Diet);
        Assert.Equal("Allergic to sesame", guest.PlusOne.CustomDietNote);
        _mockRepo.Verify(r => r.Update(guest), Times.Once);
    }
}