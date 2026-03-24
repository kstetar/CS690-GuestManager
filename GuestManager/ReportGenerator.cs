namespace GuestManager;

public class ReportGenerator : IReportGenerator
{
    public int CalculateTotalHeadcount(List<Guest> guests)
    {
        int count = 0;
        foreach (var guest in guests)
        {
            if (guest.Status == RSVPStatus.Confirmed)
            {
                count++;
                if (guest.PlusOne != null)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public Dictionary<DietaryRestriction, int> GetDietarySummary(List<Guest> guests)
    {
        var summary = new Dictionary<DietaryRestriction, int>();

        foreach (DietaryRestriction diet in System.Enum.GetValues(typeof(DietaryRestriction)))
        {
            summary[diet] = 0;
        }

        foreach (var guest in guests)
        {
            if (guest.Status == RSVPStatus.Confirmed)
            {
                if (summary.ContainsKey(guest.Diet))
                {
                    summary[guest.Diet]++;
                }

                if (guest.PlusOne != null)
                {
                    if (summary.ContainsKey(guest.PlusOne.Diet))
                    {
                        summary[guest.PlusOne.Diet]++;
                    }
                }
            }
        }

        return summary;
    }
}