using GuestManager;
using GuestManager.UI;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            // --- Initialization ---
            IGuestRepository repository = new GuestRepository(); 
            GuestService service = new GuestService(repository);
            service.Initialize();
            
            ReportGenerator reportGenerator = new ReportGenerator();
            
            // --- UI Injection ---
            GuestConsoleUI ui = new GuestConsoleUI(service, reportGenerator);
            
            // --- Run Application ---
            ui.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR] The application crashed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
