namespace GuestManager;

public interface IUserInterface
{
    void WriteLine(string message);
    void Write(string message);
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept = false);
    void Clear();
    void SetCursorPosition(int left, int top);
    int CursorTop { get; }
    bool CursorVisible { set; }
    ConsoleColor BackgroundColor { set; }
    ConsoleColor ForegroundColor { set; }
    void ResetColor();
}
