namespace GuestManager;

public class EventSummary
{
    public int TotalHeadcount { get; set; }
    public int PendingCount { get; set; }
    public Dictionary<DietaryRestriction, int> DietarySummary { get; set; } = new();
}
