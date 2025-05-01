using System;

namespace USBVault.Utils
{
    public static class ConsoleHelper
    {
        public static void PrintWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  _    _  _____ ____   __      __         _ _   ");
            Console.WriteLine(" | |  | |/ ____|  _ \\  \\ \\    / /        | | |  ");
            Console.WriteLine(" | |  | | (___ | |_) |  \\ \\  / /_ _ _   _| | |_ ");
            Console.WriteLine(" | |  | |\\___ \\|  _ <    \\ \\/ / _` | | | | | __|");
            Console.WriteLine(" | |__| |____) | |_) |    \\  / (_| | |_| | | |_ ");
            Console.WriteLine("  \\____/|_____/|____/      \\/ \\__,_|\\__,_|_|\\__|\n");
            Console.WriteLine("Безопасное хранилище заметок\n");
            Console.ResetColor();
        }

        public static void PrintMenu(params string[] items)
        {
            Console.WriteLine("\n     ------------ Меню ------------");
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            Console.Write("> ");
        }

        public static string ReadMultiLineInput()
        {
            var sb = new System.Text.StringBuilder();
            string line;
            while ((line = Console.ReadLine()) != "//end//")
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}