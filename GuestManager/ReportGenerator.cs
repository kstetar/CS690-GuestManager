namespace GuestManager;

public class ReportGenerator : IReportGenerator
{
    public EventSummary GenerateSummary(List<Guest> guests)
    {
        var summary = new EventSummary();

        foreach (DietaryRestriction diet in System.Enum.GetValues(typeof(DietaryRestriction)))
        {
            summary.DietarySummary[diet] = 0;
        }

        foreach (var guest in guests)
        {
            if (guest.Status == RSVPStatus.Pending)
            {
                summary.PendingCount++;
            }
            else if (guest.Status == RSVPStatus.Confirmed)
            {
                summary.TotalHeadcount++;
                summary.DietarySummary[guest.Diet]++;

                if (guest.PlusOne != null)
                {
                    summary.TotalHeadcount++;
                    summary.DietarySummary[guest.PlusOne.Diet]++;
                }
            }
        }

        return summary;
    }
}