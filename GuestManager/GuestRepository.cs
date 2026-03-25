using System.Text.Json;

namespace GuestManager; 

public class GuestRepository : IGuestRepository
{
    private List<Guest> _guests = new List<Guest>();
    private readonly string _filePath;
    private int _nextId = 1;

    public GuestRepository(string filePath = "guests.json")
    {
        _filePath = filePath;
    }

    public void Add(Guest guest)
    {
        guest.Id = _nextId++;
        _guests.Add(guest);
        SaveToFile(); 
    }

    public void Update(Guest guest)
    {
        SaveToFile();
    }

    public void Remove(Guest guest)
    {
        _guests.Remove(guest);
        SaveToFile();
    }

    public List<Guest> GetAll() => _guests;

    public void SaveToFile()
    {
        string json = JsonSerializer.Serialize(_guests);
        File.WriteAllText(_filePath, json);
    }

    public void LoadFromFile()
    {
        if (File.Exists(_filePath))
        {
            try 
            {
                string json = File.ReadAllText(_filePath);
                _guests = JsonSerializer.Deserialize<List<Guest>>(json) ?? new List<Guest>();
                
                // Set _nextId to be 1 more than the highest existing ID
                if (_guests.Count > 0)
                {
                    _nextId = _guests.Max(g => g.Id) + 1;
                }
            }
            catch (Exception)
            {
                // If file is corrupted or empty, start fresh
                _guests = new List<Guest>();
                _nextId = 1;
            }
        }
    }
}
