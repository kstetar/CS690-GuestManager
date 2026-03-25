using Xunit;
using GuestManager;
using System.IO;
using System;
using System.Linq;

namespace GuestManager.Tests;

public class GuestRepositoryTests : IDisposable
{
    private readonly string _testFile;
    private readonly GuestRepository _repo;

    public GuestRepositoryTests()
    {
        _testFile = $"test_guests_{Guid.NewGuid()}.json";
        _repo = new GuestRepository(_testFile);
    }

    [Fact]
    public void Add_ShouldPersistGuestToFile()
    {
        // Arrange
        var guest = new Guest { FirstName = "Test", LastName = "Persist" };

        // Act
        _repo.Add(guest);

        // Assert
        Assert.True(File.Exists(_testFile));
        string content = File.ReadAllText(_testFile);
        Assert.Contains("Test", content);
        Assert.Contains("Persist", content);
    }

    [Fact]
    public void LoadFromFile_ShouldRestoreGuests()
    {
        // Arrange
        var guest = new Guest { FirstName = "Restored", LastName = "User" };
        _repo.Add(guest);
        
        // Create a new repo instance pointing to the same file
        var newRepo = new GuestRepository(_testFile);
        
        // Act
        newRepo.LoadFromFile();
        var guests = newRepo.GetAll();

        // Assert
        Assert.Single(guests);
        Assert.Equal("Restored", guests.First().FirstName);
    }

    [Fact]
    public void Remove_ShouldDeleteGuestFromFile()
    {
        // Arrange
        var guest = new Guest { FirstName = "Delete", LastName = "Me" };
        _repo.Add(guest);
        
        // Act
        _repo.Remove(guest);

        // Assert
        var newRepo = new GuestRepository(_testFile);
        newRepo.LoadFromFile();
        Assert.Empty(newRepo.GetAll());
    }

    [Fact]
    public void Update_ShouldSaveToFile()
    {
        // Arrange
        var guest = new Guest { FirstName = "Update", LastName = "User" };
        _repo.Add(guest);
        
        // Act
        guest.FirstName = "Updated";
        _repo.Update(guest);

        // Assert
        var newRepo = new GuestRepository(_testFile);
        newRepo.LoadFromFile();
        Assert.Equal("Updated", newRepo.GetAll().First().FirstName);
    }

    [Fact]
    public void LoadFromFile_ShouldHandleCorruptedFile()
    {
        // Arrange
        File.WriteAllText(_testFile, "{ invalid json ]");
        var newRepo = new GuestRepository(_testFile);

        // Act
        newRepo.LoadFromFile();

        // Assert
        Assert.Empty(newRepo.GetAll());
    }

    [Fact]
    public void LoadFromFile_ShouldDoNothing_WhenFileDoesNotExist()
    {
        // Arrange
        if (File.Exists(_testFile)) File.Delete(_testFile);
        var newRepo = new GuestRepository(_testFile);

        // Act
        newRepo.LoadFromFile();

        // Assert
        Assert.Empty(newRepo.GetAll());
    }

    [Fact]
    public void LoadFromFile_WhenFileIsEmptyArray_ShouldNotSetNextId()
    {
        // Arrange
        File.WriteAllText(_testFile, "[]");
        var newRepo = new GuestRepository(_testFile);

        // Act
        newRepo.LoadFromFile();
        newRepo.Add(new Guest { FirstName = "A" }); // Should get ID 1

        // Assert
        Assert.Equal(1, newRepo.GetAll().First().Id);
    }

    public void Dispose()
    {
        if (File.Exists(_testFile))
        {
            try 
            {
                File.Delete(_testFile);
            }
            catch {}
        }
    }
}