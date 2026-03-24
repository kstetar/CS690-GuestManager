namespace GuestManager;

public interface IReportGenerator
{
    int CalculateTotalHeadcount(List<Guest> guests);
    Dictionary<DietaryRestriction, int> GetDietarySummary(List<Guest> guests);
}