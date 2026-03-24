namespace GuestManager.UI;

public static class ConsoleMenu
{
    public static T ShowMenu<T>(string title, List<(string Text, T Value)> options)
    {
        int selectedIndex = 0;
        ConsoleKey key;

        // Hide cursor to prevent flickering
        Console.CursorVisible = false;

        // 1. Draw Static Header Once
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine(title);
        Console.WriteLine("==============================================");
        Console.WriteLine("Use Up/Down arrows to navigate, Enter to select.");
        Console.WriteLine("______________________________________________\n");

        // 2. Capture the cursor position where options start
        int optionsStartRow = Console.CursorTop;

        do
        {
            // 3. Reset cursor to the start of the options list
            Console.SetCursorPosition(0, optionsStartRow);

            for (int i = 0; i < options.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"> {options[i].Text}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {options[i].Text}");
                }
            }

            key = Console.ReadKey(true).Key;

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
        Console.CursorVisible = true;
        return options[selectedIndex].Value;
    }
}