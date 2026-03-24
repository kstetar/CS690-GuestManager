namespace GuestManager;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _repository;

    public GuestService(IGuestRepository repository)
    {
        _repository = repository;
    }

    public void Initialize()
    {
        _repository.LoadFromFile();
    }

    public void AddGuest(string firstName, string lastName)
    {
        var guest = new Guest
        {
            FirstName = firstName,
            LastName = lastName,
            Status = RSVPStatus.Pending,
            Diet = DietaryRestriction.None
        };
        _repository.Add(guest);
    }

    public List<Guest> GetAllGuests()
    {
        return _repository.GetAll();
    }

    public Guest? GetGuestById(int id)
    {
        return _repository.GetAll().FirstOrDefault(g => g.Id == id);
    }

    public void UpdateRsvp(int guestId, RSVPStatus status)
    {
        var guest = GetGuestById(guestId);
        if (guest != null)
        {
            guest.UpdateRSVP(status);
            _repository.Update(guest);
        }
    }

    public void UpdateDiet(int guestId, DietaryRestriction diet)
    {
        var guest = GetGuestById(guestId);
        if (guest != null)
        {
            guest.Diet = diet;
            _repository.Update(guest);
        }
    }

    public void AddCompanion(int guestId, string name, DietaryRestriction diet)
    {
        var guest = GetGuestById(guestId);
        if (guest != null)
        {
            var companion = new Companion { Name = name, Diet = diet };
            guest.AddCompanion(companion);
            _repository.Update(guest);
        }
    }

    public void RemoveCompanion(int guestId)
    {
        var guest = GetGuestById(guestId);
        if (guest != null)
        {
            guest.PlusOne = null;
            _repository.Update(guest);
        }
    }

    public void RemoveGuest(int guestId)
    {
        var guest = GetGuestById(guestId);
        if (guest != null)
        {
            _repository.Remove(guest);
        }
    }
}