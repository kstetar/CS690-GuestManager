namespace GuestManager;

public interface IGuestService
{
    void Initialize();
    void AddGuest(string firstName, string lastName);
    List<Guest> GetAllGuests();
    Guest? GetGuestById(int id);
    void UpdateRsvp(int guestId, RSVPStatus status);
    void UpdateDiet(int guestId, DietaryRestriction diet);
    void AddCompanion(int guestId, string name, DietaryRestriction diet);
    void RemoveCompanion(int guestId);
    void RemoveGuest(int guestId);
}