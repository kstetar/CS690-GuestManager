using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GuestManager; 

public class GuestRepository : IGuestRepository
{
    private List<Guest> _guests = new List<Guest>();
    private readonly string _filePath = "guests.json";

    public void Add(Guest guest)
    {
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
            string json = File.ReadAllText(_filePath);
            _guests = JsonSerializer.Deserialize<List<Guest>>(json) ?? new List<Guest>();
        }
    }
}
