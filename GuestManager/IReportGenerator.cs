namespace GuestManager;

public interface IReportGenerator
{
    EventSummary GenerateSummary(List<Guest> guests);
}