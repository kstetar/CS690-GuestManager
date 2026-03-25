using System.Diagnostics.CodeAnalysis;
using GuestManager;
using GuestManager.UI;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            // Initialize application
            IGuestRepository repository = new GuestRepository(); 
            IGuestService service = new GuestService(repository);
            service.Initialize();
            
            IReportGenerator reportGenerator = new ReportGenerator();
            IUserInterface userInterface = new ConsoleUserInterface();

            // Inject UI components
            GuestConsoleUI ui = new GuestConsoleUI(service, reportGenerator, userInterface);
            
            // Run application main loop
            ui.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR] The application crashed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
