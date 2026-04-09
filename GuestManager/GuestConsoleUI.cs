using GuestManager;

namespace GuestManager.UI;

public class GuestConsoleUI
{
    private readonly IGuestService _service;
    private readonly IReportGenerator _reportGenerator;
    private readonly IUserInterface _ui;
    private readonly ConsoleMenu _menu;

    public GuestConsoleUI(IGuestService service, IReportGenerator reportGenerator, IUserInterface ui)
    {
        _service = service;
        _reportGenerator = reportGenerator;
        _ui = ui;
        _menu = new ConsoleMenu(_ui);
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

            var action = _menu.ShowMenu("GUESTMANAGER v3.0.0", options);
            action.Invoke();
        }
    }
    
    private void HandleAddGuest()
    {
        _ui.Clear();
        _ui.WriteLine("==============================================");
        _ui.WriteLine("ADD NEW GUEST");
        _ui.WriteLine("==============================================");

        _ui.Write("Enter First Name (or '0' to cancel): ");
        string fName = _ui.ReadLine() ?? "";
        if (fName == "0") return;

        _ui.Write("Enter Last Name: ");
        string lName = _ui.ReadLine() ?? "";

        try
        {
            _service.AddGuest(fName, lName);
            _ui.WriteLine($"\n[SUCCESS] {fName} {lName} added.");
        }
        catch (Exception ex)
        {
            _ui.WriteLine($"\n[ERROR] Failed to add guest: {ex.Message}");
        }
        _ui.WriteLine("Press any key to return to menu...");
        _ui.ReadKey();
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

            int selectedIndex = _menu.ShowMenu("Select a Guest to Manage", options);

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
                        $"Diet: {FormatDietDisplay(selectedGuest.Diet, selectedGuest.CustomDietNote)}\n" +
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

            var action = _menu.ShowMenu(title, options);
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

        var newStatus = _menu.ShowMenu($"UPDATE RSVP: {selectedGuest.FirstName}", options);

        if (newStatus.HasValue)
        {
            _service.UpdateRsvp(selectedGuest.Id, newStatus.Value);
            _ui.WriteLine($"\n[SUCCESS] RSVP updated to: {newStatus.Value}.");
            _ui.ReadKey();
        }
    }

    private void ManagePlusOne(Guest selectedGuest)
    {
        _ui.Clear();
        _ui.WriteLine("==============================================");
        _ui.WriteLine($"MANAGE PLUS-ONE: {selectedGuest.FirstName.ToUpper()}");
        _ui.WriteLine("==============================================");
        
        if (selectedGuest.PlusOne != null)
        {
            _ui.WriteLine($"Currently assigned: {selectedGuest.PlusOne.Name}");
            _ui.Write($"Remove plus-one? (y/n): ");
            if (_ui.ReadLine()?.ToLower() == "y")
            {
                _service.RemoveCompanion(selectedGuest.Id);
                _ui.WriteLine("\n[SUCCESS] Plus-one removed.");
                _ui.ReadKey();
            }
            return;
        }

        _ui.Write("Enter plus-one's name (or '0' to cancel): ");
        string pName = _ui.ReadLine() ?? "";

        if (pName != "0" && !string.IsNullOrWhiteSpace(pName))
        {
            var options = new List<(string, DietaryRestriction)>
            {
                ("None", DietaryRestriction.None),
                ("Vegetarian", DietaryRestriction.Vegetarian),
                ("Vegan", DietaryRestriction.Vegan),
                ("Gluten-Free", DietaryRestriction.GlutenFree),
                ("Nut Allergy", DietaryRestriction.NutAllergy),
                ("Other", DietaryRestriction.Other)
            };

            var pDiet = _menu.ShowMenu($"Select diet for {pName}", options);
            var customDietNote = PromptDietNote(pDiet, pName);

            _service.AddCompanion(selectedGuest.Id, pName, pDiet, customDietNote);
            _ui.WriteLine($"\n[SUCCESS] {pName} added.");
            _ui.ReadKey();
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
            ("Other", DietaryRestriction.Other),
            ("Cancel", null)
        };

        var newDiet = _menu.ShowMenu($"UPDATE DIET: {selectedGuest.FirstName}", options);

        if (newDiet.HasValue)
        {
            var customDietNote = PromptDietNote(newDiet.Value, selectedGuest.FirstName);
            _service.UpdateDiet(selectedGuest.Id, newDiet.Value, customDietNote);
            _ui.WriteLine($"\n[SUCCESS] Diet updated to: {FormatDietDisplay(newDiet.Value, customDietNote)}.");
            _ui.ReadKey();
        }
    }

    private string? PromptDietNote(DietaryRestriction diet, string personName)
    {
        if (diet != DietaryRestriction.Other)
        {
            return null;
        }

        _ui.Write($"Enter custom dietary note for {personName}: ");
        return _ui.ReadLine()?.Trim();
    }

    private static string FormatDietDisplay(DietaryRestriction diet, string? customDietNote)
    {
        if (diet == DietaryRestriction.Other)
        {
            return string.IsNullOrWhiteSpace(customDietNote) ? "Other" : $"Other: {customDietNote}";
        }

        return diet.ToString();
    }

    private bool RemoveGuest(Guest selectedGuest)
    {
        _ui.Clear();
        _ui.WriteLine("==============================================");
        _ui.WriteLine($"REMOVE GUEST: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
        _ui.WriteLine("==============================================");
        _ui.Write("Are you sure you want to remove this guest? (y/n): ");

        string confirm = _ui.ReadLine() ?? "";
        if (confirm.ToLower() == "y")
        {
            _service.RemoveGuest(selectedGuest.Id);
            _ui.WriteLine($"\n[SUCCESS] {selectedGuest.FirstName} {selectedGuest.LastName} removed.");
            _ui.WriteLine("Press any key to return to directory...");
            _ui.ReadKey();
            return true;
        }
        return false;
    }

    private void HandleViewSummary()
    {
        _ui.Clear();
        var allGuests = _service.GetAllGuests();
        
        var summary = _reportGenerator.GenerateSummary(allGuests);

        _ui.WriteLine("==============================================");
        _ui.WriteLine("            EVENT SUMMARY REPORT");
        _ui.WriteLine("==============================================");
        _ui.WriteLine(string.Format("{0,-20} | {1,-10} | {2}", "NAME", "RSVP", "DIET"));
        _ui.WriteLine("----------------------------------------");

        foreach (var guest in allGuests)
        {
            _ui.WriteLine(string.Format("{0,-20} | {1,-10} | {2}", 
                $"{guest.FirstName} {guest.LastName}", 
                guest.Status,
                FormatDietDisplay(guest.Diet, guest.CustomDietNote)));

            if (guest.PlusOne != null)
            {
                _ui.WriteLine(string.Format("  + {0,-16} | {1,-10} | {2}", 
                    guest.PlusOne.Name, 
                    guest.Status,
                    FormatDietDisplay(guest.PlusOne.Diet, guest.PlusOne.CustomDietNote)));
            }
        }
        _ui.WriteLine("==============================================");
        _ui.WriteLine("                  TOTALS");
        _ui.WriteLine("==============================================");
        _ui.WriteLine($"Total Confirmed (incl. Companion): {summary.TotalHeadcount}");
        _ui.WriteLine($"Pending Replies: {summary.PendingCount}");
        
        _ui.WriteLine("\n-- DIETARY BREAKDOWN --");
        foreach (var kvp in summary.DietarySummary)
        {
            if (kvp.Value > 0)
            {
                _ui.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        _ui.WriteLine("\nPress any key to return to menu...");
        _ui.ReadKey();
    }
}
