using System.Diagnostics.CodeAnalysis;

namespace GuestManager;

[ExcludeFromCodeCoverage]
public class ConsoleUserInterface : IUserInterface
{
    public void WriteLine(string message) => Console.WriteLine(message);
    public void Write(string message) => Console.Write(message);
    public string? ReadLine() => Console.ReadLine();
    public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);
    public void Clear() => Console.Clear();
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    public int CursorTop => Console.CursorTop;
    public bool CursorVisible { set => Console.CursorVisible = value; }
    public ConsoleColor BackgroundColor { set => Console.BackgroundColor = value; }
    public ConsoleColor ForegroundColor { set => Console.ForegroundColor = value; }
    public void ResetColor() => Console.ResetColor();
}
