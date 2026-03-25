using Xunit;
using Moq;
using GuestManager.UI;
using GuestManager;
using System;
using System.Collections.Generic;

namespace GuestManager.Tests;

public class GuestConsoleUITests
{
    private readonly Mock<IGuestService> _mockService;
    private readonly Mock<IReportGenerator> _mockReport;
    private readonly Mock<IUserInterface> _mockUI;

    public GuestConsoleUITests()
    {
        _mockService = new Mock<IGuestService>();
        _mockReport = new Mock<IReportGenerator>();
        _mockUI = new Mock<IUserInterface>();
    }

    [Fact]
    public void Run_AddGuest_CallsService()
    {
        // Arrange
        // Input Sequence:
        // 1. Menu: Enter (Select Add Guest)
        // 2. Add Guest: "John" (First Name)
        // 3. Add Guest: "Doe" (Last Name)
        // 4. Add Guest: Space (Continue)
        // 5. Menu: Down, Down, Down, Enter (Exit)

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Add Guest
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false), // Continue after add
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)  // Select Exit
        });

        var stringSequence = new Queue<string>(new[]
        {
            "John",
            "Doe"
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>()))
               .Returns(() => keySequence.Dequeue());

        _mockUI.Setup(ui => ui.ReadLine())
               .Returns(() => stringSequence.Dequeue());

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockService.Verify(s => s.AddGuest("John", "Doe"), Times.Once);
    }

    [Fact]
    public void Run_AddGuest_HandlesError()
    {
        // Arrange
        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Add Guest
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false), // Continue after error
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)  // Select Exit
        });

        var stringSequence = new Queue<string>(new[]
        {
            "Bad",
            "Name"
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>()))
               .Returns(() => keySequence.Dequeue());

        _mockUI.Setup(ui => ui.ReadLine())
               .Returns(() => stringSequence.Dequeue());

        _mockService.Setup(s => s.AddGuest("Bad", "Name"))
            .Throws(new ArgumentException("Force Failure"));

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("[ERROR] Failed to add guest: Force Failure"))), Times.Once);
    }

    [Fact]
    public void Run_InvalidName_RetriesInput()
    {
        // Sanity Check: If user cancels (enters "0"), service is NOT called.
        
        // Arrange
        // Input Sequence:
        // 1. Menu: Enter (Select Add Guest)
        // 2. Add Guest: "0" (Cancel)
        // 3. Menu: Down, Down, Down, Enter (Exit)

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Add Guest
            // Note: HandleAddGuest returns immediately on "0", no ReadKey pause.
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Menu Down
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)  // Select Exit
        });

        var stringSequence = new Queue<string>(new[]
        {
            "0"
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>()))
               .Returns(() => keySequence.Dequeue());

        _mockUI.Setup(ui => ui.ReadLine())
               .Returns(() => stringSequence.Dequeue());

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockService.Verify(s => s.AddGuest(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Run_ViewSummary_InteractsWithReportGenerator()
    {
        // Arrange
        // Mock data
        var guests = new List<Guest> 
        { 
            new Guest 
            { 
                FirstName = "A", LastName = "B", Status = RSVPStatus.Confirmed, Diet = DietaryRestriction.Vegan, 
                PlusOne = new Companion { Name = "C", Diet = DietaryRestriction.Vegetarian } 
            } 
        };
        _mockService.Setup(s => s.GetAllGuests()).Returns(guests);
        
        var dietDict = new Dictionary<DietaryRestriction, int>
        {
            { DietaryRestriction.Vegan, 1 },
            { DietaryRestriction.Vegetarian, 1 }
        };

        _mockReport.Setup(r => r.GenerateSummary(guests)).Returns(new EventSummary { TotalHeadcount = 2, PendingCount = 0, DietarySummary = dietDict });

        // Input Sequence:
        // 1. Menu: Down, Down, Enter (View Summary)
        // 2. Summary View: Any Key (Return)
        // 3. Menu: Down, Down, Down, Enter (Exit)

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main Menu -> View Summary (Index 2)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            
            // Press any key to return
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),

            // Main Menu -> Exit (Index 3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Dequeue());
        _mockUI.Setup(ui => ui.ReadLine()).Returns(""); // Just in case

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockReport.Verify(r => r.GenerateSummary(guests), Times.Once);
    }

    [Fact]
    public void Run_RemoveGuest_CallsServiceAndUpdatesList()
    {
        // Arrange
        var guest = new Guest { Id = 1, FirstName = "Del", LastName = "Me" };
        
        // Sequence of service calls: 
        // 1. Initial list (contains guest)
        // 2. Post-removal list (empty)
        _mockService.SetupSequence(s => s.GetAllGuests())
            .Returns(new List<Guest> { guest })
            .Returns(new List<Guest>());

        // Input Sequence:
        // 1. Main Menu: Down, Enter (Manage Guest)
        // 2. Select Guest Menu: Enter (Select 'Del Me' at index 0)
        // 3. Manage Single Guest Menu: Down, Down, Down, Enter (Remove Guest at index 3)
        // 4. Confirm Prompt: "y" (ReadLine)
        // 5. Success Message: Any Key (Return to directory)
        // 6. Select Guest Menu (Now empty): Enter (Back to Main Menu at index 0)
        // 7. Main Menu: Down, Down, Down, Enter (Exit)

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main -> Manage (Index 1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Select Guest (Index 0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Single Guest -> Remove (Index 3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Success Pause
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),
            
            // Select Guest (Back is now Index 0 because list is empty)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Main -> Exit (Index 3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });

        var stringSequence = new Queue<string>(new[] { "y" });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(() => stringSequence.Dequeue());

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockService.Verify(s => s.RemoveGuest(guest.Id), Times.Once);
    }

    [Fact]
    public void Run_UpdateRSVP_UpdatesService()
    {
        // Arrange
        var guest = new Guest { Id = 1, FirstName = "Test", LastName = "User", Status = RSVPStatus.Pending };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main -> Manage (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Select Guest (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Single Guest -> Update RSVP (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // RSVP Menu -> Confirmed (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Success Pause
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),

            // Single Guest -> Return (4)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Manage List -> Back (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Main -> Exit (3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(""); 

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockService.Verify(s => s.UpdateRsvp(guest.Id, RSVPStatus.Confirmed), Times.Once);
    }

    [Fact]
    public void Run_AddPlusOne_UpdatesService()
    {
        // Arrange
        var guest = new Guest { Id = 1, FirstName = "Test", LastName = "User" };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main -> Manage (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Select Guest (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Single Guest -> Manage PlusOne (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Diet Menu (after name input) -> Vegetarian (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Success Pause
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),

            // Single Guest -> Return (4)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Manage List -> Back (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Main -> Exit (3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });

        var inputStrings = new Queue<string>(new[] { "PartnerName" });
        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(() => inputStrings.Count > 0 ? inputStrings.Dequeue() : "");

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockService.Verify(s => s.AddCompanion(guest.Id, "PartnerName", DietaryRestriction.Vegetarian), Times.Once);
    }

    [Fact]
    public void Run_UpdateDiet_CallsService()
    {
        // Arrange
        var guest = new Guest { Id = 1, FirstName = "Test", LastName = "User" };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main -> Manage (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Select Guest (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Single Guest -> Update Diet (2)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Diet Menu -> Nut Allergy (4)
            // Options: None, Vegetarian, Vegan, Gluten-Free, Nut Allergy, Cancel
            // Indices: 0, 1, 2, 3, 4, 5
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Success Pause
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),

            // Single Guest -> Return (4)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Manage List -> Back (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Main -> Exit (3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns("");

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockService.Verify(s => s.UpdateDiet(guest.Id, DietaryRestriction.NutAllergy), Times.Once);
    }

    [Fact]
    public void Run_AddGuest_HandlesException()
    {
        // Arrange
        _mockService.Setup(s => s.AddGuest("Fail", "User")).Throws(new Exception("Database error"));

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main -> Add (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Error Pause
            new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),

            // Main -> Exit (3)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });

        var inputStrings = new Queue<string>(new[] { "Fail", "User" });
        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(() => inputStrings.Count > 0 ? inputStrings.Dequeue() : "");

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        // Act
        ui.Run();

        // Assert
        _mockUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("Failed to add guest: Database error"))), Times.Once);
    }

    [Fact]
    public void ConsoleMenu_UpArrow_WrapsToBottom()
    {
        // Arrange
        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        var menu = new ConsoleMenu(_mockUI.Object);
        var options = new List<(string, int)> { ("A", 1), ("B", 2), ("C", 3) };

        // Act
        var result = menu.ShowMenu("Test", options);

        // Assert
        Assert.Equal(3, result); // Starts at 0, UpArrow wraps to 2 (which is value 3)
    }

    [Fact]
    public void ConsoleMenu_DownArrowWrapsAndIgnoresKeys()
    {
        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Up Arrow wraps to bottom (index 2)
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),
            // Down Arrow wraps back to top (index 0)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            // Random ignored key
            new ConsoleKeyInfo('p', ConsoleKey.P, false, false, false),
            // Enter confirms
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        var menu = new ConsoleMenu(_mockUI.Object);
        var options = new List<(string, int)> { ("A", 1), ("B", 2), ("C", 3) };

        var result = menu.ShowMenu("Test", options);

        Assert.Equal(1, result); // Value 1 matches Index 0 
    }

    [Fact]
    public void Run_ManageGuest_CancelOption()
    {
        var guest = new Guest { Id = 1, FirstName = "A", LastName = "B" };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            // Main -> Manage (1)
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Select Guest (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Single Guest -> Update RSVP (0)
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // RSVP Menu -> Cancel (3)
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Wraps to cancel
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Single Guest -> Return (4)
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Select Guest (1) -> Back to Main
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),

            // Main -> Exit (3)
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns("");

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);

        ui.Run();

        _mockService.Verify(s => s.UpdateRsvp(It.IsAny<int>(), It.IsAny<RSVPStatus>()), Times.Never);
    }

    [Fact]
    public void Run_ManageGuest_CancelDiet()
    {
        var guest = new Guest { Id = 1, FirstName = "A" };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Manage
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Guest
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), 
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Update Diet
            
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Wraps Diet cancel
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Cancel

            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Single guest back
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // list back
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false) // Main exit
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns("");
        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);
        ui.Run();
        _mockService.Verify(s => s.UpdateDiet(It.IsAny<int>(), It.IsAny<DietaryRestriction>()), Times.Never);
    }

    [Fact]
    public void Run_RemoveGuest_AnswersNo()
    {
        var guest = new Guest { Id = 1, FirstName = "A" };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Manage
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Guest
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Remove
            
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Single Guest back
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // list back
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Main exit
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)
        });

        var inputStrings = new Queue<string>(new[] { "n" });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(() => inputStrings.Count > 0 ? inputStrings.Dequeue() : "");

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);
        ui.Run();

        _mockService.Verify(s => s.RemoveGuest(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Run_ManagePlusOne_CancelZero()
    {
        var guest = new Guest { Id = 1, FirstName = "A" };
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Manage
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Guest
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Manage Plus One
            
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Single Guest back
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // list back
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Main exit
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)
        });

        var inputStrings = new Queue<string>(new[] { "0" });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(() => inputStrings.Count > 0 ? inputStrings.Dequeue() : "");

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);
        ui.Run();

        _mockService.Verify(s => s.AddCompanion(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DietaryRestriction>()), Times.Never);
    }

    [Fact]
    public void ConsoleMenu_ArrowsWithoutWrapping()
    {
        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // 0 to 1
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false),   // 1 to 0
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)         // Enter on 0
        });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        var menu = new ConsoleMenu(_mockUI.Object);
        var options = new List<(string, int)> { ("A", 1), ("B", 2), ("C", 3) };

        var result = menu.ShowMenu("Test", options);

        Assert.Equal(1, result);
    }

    [Fact]
    public void Run_ManagePlusOne_Remove()
    {
        var guest = new Guest { Id = 1, FirstName = "A", PlusOne = new Companion { Name = "Partner" } }; 
        _mockService.Setup(s => s.GetAllGuests()).Returns(new List<Guest> { guest });

        var keySequence = new Queue<ConsoleKeyInfo>(new[]
        {
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // Manage
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Select Guest
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false),
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Manage Plus One
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false), // Any key to continue after removal
            
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Single Guest back
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false), // list back
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false),
            new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false), // Main exit
            new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)
        });

        var inputStrings = new Queue<string>(new[] { "y" });

        _mockUI.Setup(ui => ui.ReadKey(It.IsAny<bool>())).Returns(() => keySequence.Count > 0 ? keySequence.Dequeue() : new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        _mockUI.Setup(ui => ui.ReadLine()).Returns(() => inputStrings.Count > 0 ? inputStrings.Dequeue() : null);

        var ui = new GuestConsoleUI(_mockService.Object, _mockReport.Object, _mockUI.Object);
        ui.Run();

        _mockService.Verify(s => s.RemoveCompanion(guest.Id), Times.Once); 
    }
}
