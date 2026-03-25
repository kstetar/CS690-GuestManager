using GuestManager;

namespace GuestManager.UI;

public class ConsoleMenu
{
    private readonly IUserInterface _ui;

    public ConsoleMenu(IUserInterface ui)
    {
        _ui = ui;
    }

    public T ShowMenu<T>(string title, List<(string Text, T Value)> options)
    {
        int selectedIndex = 0;
        ConsoleKey key;

        // Hide cursor to prevent flickering
        _ui.CursorVisible = false;

        // 1. Draw Static Header Once
        _ui.Clear();
        _ui.WriteLine("==============================================");
        _ui.WriteLine(title);
        _ui.WriteLine("==============================================");
        _ui.WriteLine("Use Up/Down arrows to navigate, Enter to select.");
        _ui.WriteLine("______________________________________________\n");

        // 2. Capture the cursor position where options start
        int optionsStartRow = _ui.CursorTop;

        do
        {
            // 3. Reset cursor to the start of the options list
            _ui.SetCursorPosition(0, optionsStartRow);

            for (int i = 0; i < options.Count; i++)
            {
                if (i == selectedIndex)
                {
                    _ui.BackgroundColor = ConsoleColor.Gray;
                    _ui.ForegroundColor = ConsoleColor.Black;
                    _ui.WriteLine($"> {options[i].Text}");
                    _ui.ResetColor();
                }
                else
                {
                    _ui.WriteLine($"  {options[i].Text}");
                }
            }

            key = _ui.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex == 0) ? options.Count - 1 : selectedIndex - 1;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex == options.Count - 1) ? 0 : selectedIndex + 1;
                    break;
            }

        } while (key != ConsoleKey.Enter);

        // Restore cursor visibility
        _ui.CursorVisible = true;
        return options[selectedIndex].Value;
    }
}
