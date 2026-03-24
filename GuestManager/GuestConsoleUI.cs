using GuestManager;

namespace GuestManager.UI;

public class GuestConsoleUI
{
    private readonly GuestService _service;
    private readonly ReportGenerator _reportGenerator;

    public GuestConsoleUI(GuestService service, ReportGenerator reportGenerator)
    {
        _service = service;
        _reportGenerator = reportGenerator;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            ShowMainMenu();
            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    HandleAddGuest();
                    break;
                case "2":
                    HandleManageGuest();
                    break;
                case "3":
                    HandleViewSummary();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("\nInvalid option, try again.");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine("GUESTMANAGER v1.0");
        Console.WriteLine("==============================================");
        Console.WriteLine("1. Add New Guest");
        Console.WriteLine("2. Manage Guest");
        Console.WriteLine("3. View Event Summary");
        Console.WriteLine("0. Exit");
        Console.WriteLine("______________________________________________");
        Console.Write("\nSelect an option: ");
    }

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
            Console.Clear();
            var guests = _service.GetAllGuests();
            Console.WriteLine("==============================================");
            Console.WriteLine("GUEST DIRECTORY");
            Console.WriteLine("==============================================");
            for (int i = 0; i < guests.Count; i++)
            {
                string fullName = $"{guests[i].FirstName} {guests[i].LastName}";
                Console.WriteLine($"{i + 1}. {fullName,-25} ({guests[i].Status})");
            }
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine("______________________________________________");
            Console.Write("\nSelect a guest to manage: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0) break;
                if (choice > 0 && choice <= guests.Count)
                {
                    ManageSingleGuest(guests[choice - 1]);
                }
                else
                {
                    Console.WriteLine("Invalid selection. Press any key...");
                    Console.ReadKey();
                }
            }
        }
    }

    private void ManageSingleGuest(Guest selectedGuest)
    {
        bool inProfile = true;
        while (inProfile)
        {
            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine($"MANAGING: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
            Console.WriteLine("==============================================");
            Console.WriteLine("-- Current Status --");
            Console.WriteLine($"RSVP: {selectedGuest.Status}");
            Console.WriteLine($"Diet: {selectedGuest.Diet}");
            Console.WriteLine($"Plus-One: {(selectedGuest.PlusOne != null ? selectedGuest.PlusOne.Name : "None")}");
            Console.WriteLine("\n-- Actions --");
            Console.WriteLine("1. Update RSVP");
            Console.WriteLine("2. Manage Plus-One");
            Console.WriteLine("3. Update Dietary Preference");
            Console.WriteLine("4. Remove Guest");
            Console.WriteLine("0. Return to Guest Directory");
            Console.WriteLine("______________________________________________");
            Console.Write("\nSelect an action (0-4): ");

            string actionInput = Console.ReadLine() ?? "";
            
            switch (actionInput)
            {
                case "0":
                    inProfile = false;
                    break;
                case "1":
                    UpdateRsvp(selectedGuest);
                    break;
                case "2":
                    ManagePlusOne(selectedGuest);
                    break;
                case "3":
                    UpdateDiet(selectedGuest);
                    break;
                case "4":
                    if (RemoveGuest(selectedGuest)) inProfile = false;
                    break;
            }
        }
    }

    private void UpdateRsvp(Guest selectedGuest)
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine($"UPDATE RSVP: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
        Console.WriteLine("==============================================");
        Console.WriteLine("Select RSVP Status:");
        Console.WriteLine("1. Pending");
        Console.WriteLine("2. Confirmed");
        Console.WriteLine("3. Declined");
        Console.WriteLine("0. Cancel and Return");
        Console.WriteLine("______________________________________________");
        Console.Write("\nChoice: ");

        string rsvpInput = Console.ReadLine() ?? "";
        RSVPStatus? newStatus = null;
        if (rsvpInput == "1") newStatus = RSVPStatus.Pending;
        else if (rsvpInput == "2") newStatus = RSVPStatus.Confirmed;
        else if (rsvpInput == "3") newStatus = RSVPStatus.Declined;

        if (newStatus.HasValue)
        {
            _service.UpdateRsvp(selectedGuest.Id, newStatus.Value);
            Console.WriteLine($"\n[SUCCESS] {selectedGuest.FirstName}'s RSVP updated to: {newStatus.Value}.");
            Console.WriteLine("Press any key to return to guest profile...");
            Console.ReadKey();
        }
    }

    private void ManagePlusOne(Guest selectedGuest)
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine($"MANAGE PLUS-ONE: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
        Console.WriteLine("==============================================");
        Console.WriteLine($"Currently assigned: {(selectedGuest.PlusOne != null ? selectedGuest.PlusOne.Name : "None")}");
        Console.WriteLine();
        Console.Write($"Is {selectedGuest.FirstName} bringing a plus-one? (y/n) (or '0' to cancel): ");

        string plusOneInput = Console.ReadLine() ?? "";

        if (plusOneInput == "0") return;
        
        if (plusOneInput.ToLower() == "n")
        {
            if (selectedGuest.PlusOne != null)
            {
                _service.RemoveCompanion(selectedGuest.Id);
                Console.WriteLine($"\n[SUCCESS] Plus-one removed.");
            }
            else
            {
                Console.WriteLine($"\n[info] No plus-one to remove.");
            }
            Console.WriteLine("Press any key to return to guest profile...");
            Console.ReadKey();
        }
        else if (plusOneInput.ToLower() == "y")
        {
            Console.WriteLine("\n==============================================");
            Console.WriteLine($"PLUS-ONE DETAILS: {selectedGuest.FirstName.ToUpper()}");
            Console.WriteLine("==============================================");

            Console.Write("Enter plus-one's name (or '0' to cancel): ");
            string pName = Console.ReadLine() ?? "";

            if (pName != "0")
            {
                Console.WriteLine("\nSelect plus-one's dietary restriction:");
                Console.WriteLine("1. None");
                Console.WriteLine("2. Vegetarian");
                Console.WriteLine("3. Vegan");
                Console.WriteLine("4. Gluten-Free");
                Console.WriteLine("5. Nut Allergy");
                Console.WriteLine("0. Cancel");
                Console.Write("\nChoice: ");

                string pDietInput = Console.ReadLine() ?? "";
                DietaryRestriction pDiet = DietaryRestriction.None;
                bool validDiet = true;

                switch (pDietInput)
                {
                    case "1": pDiet = DietaryRestriction.None; break;
                    case "2": pDiet = DietaryRestriction.Vegetarian; break;
                    case "3": pDiet = DietaryRestriction.Vegan; break;
                    case "4": pDiet = DietaryRestriction.GlutenFree; break;
                    case "5": pDiet = DietaryRestriction.NutAllergy; break;
                    case "0": validDiet = false; break;
                }

                if (validDiet)
                {
                    _service.AddCompanion(selectedGuest.Id, pName, pDiet);
                    Console.WriteLine($"\n[SUCCESS] {pName} added as a +1 for {selectedGuest.FirstName}.");
                    Console.WriteLine("Press any key to return to guest profile...");
                    Console.ReadKey();
                }
            }
        }
    }

    private void UpdateDiet(Guest selectedGuest)
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine($"UPDATE DIET: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
        Console.WriteLine("==============================================");
        Console.WriteLine("Select Dietary Restriction:");
        Console.WriteLine("1. None");
        Console.WriteLine("2. Vegetarian");
        Console.WriteLine("3. Vegan");
        Console.WriteLine("4. Gluten-Free");
        Console.WriteLine("5. Nut Allergy");
        Console.WriteLine("0. Cancel and Return");
        Console.WriteLine("______________________________________________");
        Console.Write("\nChoice: ");

        string dietInput = Console.ReadLine() ?? "";
        DietaryRestriction? newDiet = null;

        switch (dietInput)
        {
            case "1": newDiet = DietaryRestriction.None; break;
            case "2": newDiet = DietaryRestriction.Vegetarian; break;
            case "3": newDiet = DietaryRestriction.Vegan; break;
            case "4": newDiet = DietaryRestriction.GlutenFree; break;
            case "5": newDiet = DietaryRestriction.NutAllergy; break;
        }

        if (newDiet.HasValue)
        {
            _service.UpdateDiet(selectedGuest.Id, newDiet.Value);
            Console.WriteLine($"\n[SUCCESS] {selectedGuest.FirstName}'s diet updated to: {newDiet.Value}.");
            Console.WriteLine("Press any key to return to guest profile...");
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