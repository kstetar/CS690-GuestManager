using GuestManager;

namespace GuestManager.UI;

public class GuestConsoleUI
{
    private readonly IGuestService _service;
    private readonly IReportGenerator _reportGenerator;

    public GuestConsoleUI(IGuestService service, IReportGenerator reportGenerator)
    {
        _service = service;
        _reportGenerator = reportGenerator;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            var options = new List<(string, Action)>
            {
                ("Add New Guest", HandleAddGuest),
                ("Manage Guest", HandleManageGuest),
                ("View Event Summary", HandleViewSummary),
                ("Exit", () => running = false)
            };

            var action = ConsoleMenu.ShowMenu("GUESTMANAGER v1.0", options);
            action.Invoke();
        }
    }
    
    // ShowMainMenu removed, replaced by ConsoleMenu


    private void HandleAddGuest()
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine("ADD NEW GUEST");
        Console.WriteLine("==============================================");

        Console.Write("Enter First Name (or '0' to cancel): ");
        string fName = Console.ReadLine() ?? "";
        if (fName == "0") return;

        Console.Write("Enter Last Name: ");
        string lName = Console.ReadLine() ?? "";

        _service.AddGuest(fName, lName);

        Console.WriteLine($"\n[SUCCESS] {fName} {lName} added.");
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey();
    }

    private void HandleManageGuest()
    {
        while (true)
        {
            var guests = _service.GetAllGuests();
            var options = new List<(string, int)>();

            for (int i = 0; i < guests.Count; i++)
            {
                options.Add(($"{guests[i].FirstName} {guests[i].LastName} ({guests[i].Status})", i));
            }
            options.Add(("Back to Main Menu", -1));

            int selectedIndex = ConsoleMenu.ShowMenu("Select a Guest to Manage", options);

            if (selectedIndex == -1) break;

            ManageSingleGuest(guests[selectedIndex]);
        }
    }

    private void ManageSingleGuest(Guest selectedGuest)
    {
        bool inProfile = true;
        while (inProfile)
        {
            var title = $"MANAGING: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}\n" +
                        $"-- Status --\n" +
                        $"RSVP: {selectedGuest.Status}\n" +
                        $"Diet: {selectedGuest.Diet}\n" +
                        $"Plus-One: {(selectedGuest.PlusOne != null ? selectedGuest.PlusOne.Name : "None")}";

            var options = new List<(string, Action)>
            {
                ("Update RSVP", () => UpdateRsvp(selectedGuest)),
                ("Manage Plus-One", () => ManagePlusOne(selectedGuest)),
                ("Update Dietary Preference", () => UpdateDiet(selectedGuest)),
                ("Remove Guest", () => 
                {
                    if(RemoveGuest(selectedGuest)) inProfile = false; 
                }),
                ("Return to Guest Directory", () => inProfile = false)
            };

            var action = ConsoleMenu.ShowMenu(title, options);
            action.Invoke();
        }
    }

    private void UpdateRsvp(Guest selectedGuest)
    {
        var options = new List<(string, RSVPStatus?)>
        {
            ("Pending", RSVPStatus.Pending),
            ("Confirmed", RSVPStatus.Confirmed),
            ("Declined", RSVPStatus.Declined),
            ("Cancel", null)
        };

        var newStatus = ConsoleMenu.ShowMenu($"UPDATE RSVP: {selectedGuest.FirstName}", options);

        if (newStatus.HasValue)
        {
            _service.UpdateRsvp(selectedGuest.Id, newStatus.Value);
            Console.WriteLine($"\n[SUCCESS] RSVP updated to: {newStatus.Value}.");
            Console.ReadKey();
        }
    }

    private void ManagePlusOne(Guest selectedGuest)
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine($"MANAGE PLUS-ONE: {selectedGuest.FirstName.ToUpper()}");
        Console.WriteLine("==============================================");
        
        if (selectedGuest.PlusOne != null)
        {
            Console.WriteLine($"Currently assigned: {selectedGuest.PlusOne.Name}");
            Console.Write($"Remove plus-one? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                _service.RemoveCompanion(selectedGuest.Id);
                Console.WriteLine("\n[SUCCESS] Plus-one removed.");
                Console.ReadKey();
            }
            return;
        }

        Console.Write("Enter plus-one's name (or '0' to cancel): ");
        string pName = Console.ReadLine() ?? "";

        if (pName != "0" && !string.IsNullOrWhiteSpace(pName))
        {
            var options = new List<(string, DietaryRestriction)>
            {
                ("None", DietaryRestriction.None),
                ("Vegetarian", DietaryRestriction.Vegetarian),
                ("Vegan", DietaryRestriction.Vegan),
                ("Gluten-Free", DietaryRestriction.GlutenFree),
                ("Nut Allergy", DietaryRestriction.NutAllergy)
            };

            var pDiet = ConsoleMenu.ShowMenu($"Select diet for {pName}", options);

            _service.AddCompanion(selectedGuest.Id, pName, pDiet);
            Console.WriteLine($"\n[SUCCESS] {pName} added.");
            Console.ReadKey();
        }
    }

    private void UpdateDiet(Guest selectedGuest)
    {
        var options = new List<(string, DietaryRestriction?)>
        {
            ("None", DietaryRestriction.None),
            ("Vegetarian", DietaryRestriction.Vegetarian),
            ("Vegan", DietaryRestriction.Vegan),
            ("Gluten-Free", DietaryRestriction.GlutenFree),
            ("Nut Allergy", DietaryRestriction.NutAllergy),
            ("Cancel", null)
        };

        var newDiet = ConsoleMenu.ShowMenu($"UPDATE DIET: {selectedGuest.FirstName}", options);

        if (newDiet.HasValue)
        {
            _service.UpdateDiet(selectedGuest.Id, newDiet.Value);
            Console.WriteLine($"\n[SUCCESS] Diet updated to: {newDiet.Value}.");
            Console.ReadKey();
        }
    }

    private bool RemoveGuest(Guest selectedGuest)
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine($"REMOVE GUEST: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
        Console.WriteLine("==============================================");
        Console.Write("Are you sure you want to remove this guest? (y/n): ");

        string confirm = Console.ReadLine() ?? "";
        if (confirm.ToLower() == "y")
        {
            _service.RemoveGuest(selectedGuest.Id);
            Console.WriteLine($"\n[SUCCESS] {selectedGuest.FirstName} {selectedGuest.LastName} removed.");
            Console.WriteLine("Press any key to return to directory...");
            Console.ReadKey();
            return true;
        }
        return false;
    }

    private void HandleViewSummary()
    {
        Console.Clear();
        var allGuests = _service.GetAllGuests();
        
        int totalHeadcount = _reportGenerator.CalculateTotalHeadcount(allGuests);
        var dietSummary = _reportGenerator.GetDietarySummary(allGuests);

        Console.WriteLine("==============================================");
        Console.WriteLine("            EVENT SUMMARY REPORT");
        Console.WriteLine("==============================================");
        Console.WriteLine(string.Format("{0,-20} | {1,-10} | {2}", "NAME", "RSVP", "DIET"));
        Console.WriteLine("----------------------------------------");

        int pendingCount = 0;
        foreach (var guest in allGuests)
        {
                if(guest.Status == RSVPStatus.Pending) pendingCount++;

            // Display Guest info
            Console.WriteLine(string.Format("{0,-20} | {1,-10} | {2}", 
                $"{guest.FirstName} {guest.LastName}", 
                guest.Status,
                guest.Diet));

            if (guest.PlusOne != null)
            {
                Console.WriteLine(string.Format("  + {0,-16} | {1,-10} | {2}", 
                    guest.PlusOne.Name, 
                    "Confirmed", 
                    guest.PlusOne.Diet));
            }
        }
        Console.WriteLine("==============================================");
        Console.WriteLine("                  TOTALS");
        Console.WriteLine("==============================================");
        Console.WriteLine($"Total Headcount (Confirmed +1s): {totalHeadcount}");
        Console.WriteLine($"Pending Replies: {pendingCount}");
        
        Console.WriteLine("\n-- DIETARY BREAKDOWN --");
        foreach (var kvp in dietSummary)
        {
            if (kvp.Value > 0)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
    }
}