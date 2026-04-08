namespace GuestManager;

public class Companion
{
    public string Name { get; set; } = string.Empty;
    public DietaryRestriction Diet { get; set; } = DietaryRestriction.None;
    public string? CustomDietNote { get; set; }
}