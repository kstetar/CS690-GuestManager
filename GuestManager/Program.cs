using GuestManager;

internal class Program
{
    private static void Main(string[] args)
    {
        GuestRepository repository = new GuestRepository();
        repository.LoadFromFile();
        
        ReportGenerator reportGenerator = new ReportGenerator();

        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("GUESTMANAGER v1.0");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Add New Guest");
            Console.WriteLine("2. Manage Guest");
            Console.WriteLine("3. View Event Summary");
            Console.WriteLine("0. Exit");
            Console.WriteLine("________________________________________");
            Console.Write("\nSelect an option: ");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.Clear();
                    Console.WriteLine("========================================");
                    Console.WriteLine("ADD NEW GUEST");
                    Console.WriteLine("========================================");

                    Console.Write("Enter First Name (or '0' to cancel): ");
                    string fName = Console.ReadLine() ?? "";
                    if (fName == "0") break;

                    Console.Write("Enter Last Name: ");
                    string lName = Console.ReadLine() ?? "";

                    Guest newGuest = new Guest 
                    { 
                        Id = repository.GetAll().Count + 1,
                        FirstName = fName, 
                        LastName = lName,
                        Status = RSVPStatus.Pending,
                        Diet = DietaryRestriction.None
                    };
            
                    repository.Add(newGuest); 

                    Console.WriteLine($"\n[SUCCESS] {fName} {lName} added.");
                    Console.WriteLine("Press any key to return to menu...");
                    Console.ReadKey();
                    break;
                case "2":
                    while (true)
                    {
                        Console.Clear();
                        var guests = repository.GetAll();
                        Console.WriteLine("========================================");
                        Console.WriteLine("GUEST DIRECTORY");
                        Console.WriteLine("========================================");
                        for (int i = 0; i < guests.Count; i++)
                        {
                            string fullName = $"{guests[i].FirstName} {guests[i].LastName}";
                            // Ensure name takes up 25 chars for alignment
                            Console.WriteLine($"{i + 1}. {fullName,-25} ({guests[i].Status})");
                        }
                        Console.WriteLine("0. Back to Main Menu");
                        Console.WriteLine("________________________________________");
                        Console.Write("\nSelect a guest to manage: ");
                        
                        if (int.TryParse(Console.ReadLine(), out int choice))
                        {
                            if (choice == 0) break;
                            if (choice > 0 && choice <= guests.Count)
                            {
                                var selectedGuest = guests[choice - 1];
                                bool inProfile = true;
                                while (inProfile)
                                {
                                    Console.Clear();
                                    Console.WriteLine("========================================");
                                    Console.WriteLine($"MANAGING: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
                                    Console.WriteLine("========================================");
                                    Console.WriteLine("-- Current Status --");
                                    Console.WriteLine($"RSVP: {selectedGuest.Status}");
                                    Console.WriteLine($"Diet: {selectedGuest.Diet}");
                                    Console.WriteLine($"Plus-One: {(selectedGuest.PlusOne != null ? selectedGuest.PlusOne.Name : "None")}");
                                    Console.WriteLine("\n-- Actions --");
                                    Console.WriteLine("1. Update RSVP");
                                    Console.WriteLine("0. Return to Guest Directory");
                                    Console.WriteLine("________________________________________");
                                    Console.Write("\nSelect an action (0-1): ");

                                    string actionInput = Console.ReadLine() ?? "";
                                    if (actionInput == "0") 
                                    {
                                        inProfile = false;
                                    }
                                    else if (actionInput == "1")
                                    {
                                        Console.Clear();
                                        Console.WriteLine("========================================");
                                        Console.WriteLine($"UPDATE RSVP: {selectedGuest.FirstName.ToUpper()} {selectedGuest.LastName.ToUpper()}");
                                        Console.WriteLine("========================================");
                                        Console.WriteLine("Select RSVP Status:");
                                        Console.WriteLine("1. Pending");
                                        Console.WriteLine("2. Confirmed");
                                        Console.WriteLine("3. Declined");
                                        Console.WriteLine("0. Cancel and Return");
                                        Console.WriteLine("________________________________________");
                                        Console.Write("\nChoice: ");

                                        string rsvpInput = Console.ReadLine() ?? "";
                                        if (rsvpInput == "1") selectedGuest.UpdateRSVP(RSVPStatus.Pending);
                                        else if (rsvpInput == "2") selectedGuest.UpdateRSVP(RSVPStatus.Confirmed);
                                        else if (rsvpInput == "3") selectedGuest.UpdateRSVP(RSVPStatus.Declined);
                                        
                                        if (rsvpInput == "1" || rsvpInput == "2" || rsvpInput == "3")
                                        {
                                            repository.Update(selectedGuest);
                                            Console.WriteLine($"\n[SUCCESS] {selectedGuest.FirstName}'s RSVP updated to: {selectedGuest.Status}.");
                                            Console.WriteLine("Press any key to return to guest profile...");
                                            Console.ReadKey();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid selection. Press any key...");
                                Console.ReadKey();
                            }
                        }
                    }
                    break;
                case "3":
                    Console.Clear();
                    var allGuests = repository.GetAll();
                    
                    int totalHeadcount = reportGenerator.CalculateTotalHeadcount(allGuests);
                    var dietSummary = reportGenerator.GetDietarySummary(allGuests);

                    Console.WriteLine("========================================");
                    Console.WriteLine("         EVENT SUMMARY REPORT");
                    Console.WriteLine("========================================");
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
                    Console.WriteLine("========================================");
                    Console.WriteLine("               TOTALS");
                    Console.WriteLine("========================================");
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

        Console.WriteLine("\nGoodbye!");
    }
}