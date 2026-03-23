namespace GuestManager;

public interface IGuestRepository
{
    void Add(Guest guest);
    void Update(Guest guest);
    void Remove(Guest guest);
    List<Guest> GetAll();
    void SaveToFile();
    void LoadFromFile();
}