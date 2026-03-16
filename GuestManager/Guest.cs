namespace GuestManager;

public class Guest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public RSVPStatus Status { get; set; } = RSVPStatus.Pending;
    public DietaryRestriction Diet { get; set; } = DietaryRestriction.None;
    public Companion? PlusOne { get; set; }

    public void UpdateRSVP(RSVPStatus status) => Status = status;
    
    public void AddCompanion(Companion companion) => PlusOne = companion;
}