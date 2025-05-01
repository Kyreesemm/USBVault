using System;
using System.IO;
using System.Linq;
using USBVault.Models;
using USBVault.Services;
using USBVault.Utils;

class Program
{
    private static Vault _vault = new Vault();
    private static byte[]? _currentKey;
    private static string? _currentDrive;
    private static string? _currentVaultId;

    static void Main()
    {
        Console.Title = "USB Vault - Secure Note Storage";
        ConsoleHelper.PrintWelcome();

        while (true)
        {
            if (_currentKey == null)
            {
                ShowNoKeyMenu();
            }
            else
            {
                ShowMainMenu();
            }
        }
    }

    public static void ClearConsole()
    {
        Console.Clear();
        ConsoleHelper.PrintWelcome();
        ShowMainMenu();
    }

    private static bool IsKeyStillValid()
    {
        if (_currentDrive == null) return false;

        var keyInfo = UsbKeyManager.TryReadKey(_currentDrive);
        return keyInfo != null &&
               keyInfo.Value.Key.SequenceEqual(_currentKey) &&
               keyInfo.Value.VaultId == _currentVaultId;
    }

    private static void ShowNoKeyMenu()
    {
        ConsoleHelper.PrintMenu(
            "     1. Создать новый USB-ключ",
            "     2. Вставить существующий USB-ключ",
            "     3. Выход"
        );

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                CreateNewKey();
                break;
            case "2":
                HandleUsbKeyInsertion();
                break;
            case "3":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }
    }

    private static void CreateNewKey()
    {
        Console.WriteLine("\nВставьте флешку, которую хотите использовать как ключ.");
        Console.WriteLine("Список доступных дисков:\n");

        var drives = DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable)
            .ToList();

        if (drives.Count == 0)
        {
            Console.WriteLine("Не найдено USB-флешек. Вставьте флешку и попробуйте снова.");
            return;
        }

        for (int i = 0; i < drives.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {drives[i].Name} (Свободно: {drives[i].AvailableFreeSpace / 1024 / 1024} МБ)");
        }

        Console.Write("Выберите флешку (1-{0}): ", drives.Count);
        if (int.TryParse(Console.ReadLine(), out int selectedDrive) && selectedDrive > 0 && selectedDrive <= drives.Count)
        {
            var driveLetter = drives[selectedDrive - 1].Name[0].ToString();
            UsbKeyManager.CreateKey(driveLetter, out var key, out var vaultId);
            _currentKey = key;
            _currentVaultId = vaultId;
            _currentDrive = driveLetter;

            UsbWatcher.StartWatching(_currentDrive);
            Console.WriteLine("[USB-V] Ключ успешно создан и записан на флешку!");
        }
        else
        {
            Console.WriteLine("Неверный выбор!");
        }
    }

    private static void HandleUsbKeyInsertion()
    {
        var driveLetter = UsbWatcher.WaitForUsbKey();
        var keyInfo = UsbKeyManager.TryReadKey(driveLetter);

        if (keyInfo != null)
        {
            _currentKey = keyInfo.Value.Key;
            _currentVaultId = keyInfo.Value.VaultId;
            _currentDrive = driveLetter;

            UsbWatcher.StartWatching(_currentDrive);
            _vault.LoadNotes(_currentKey, _currentVaultId);
            Console.WriteLine("[USB-V] Ключ найден! Доступ разрешён.");
        }
    }

    private static void ShowMainMenu()
    {
        ConsoleHelper.PrintMenu(
            "     1. Создать заметку",
            "     2. Просмотреть заметки",
            "     3. Удалить заметку",
            "     4. Очистить консоль",
            "     5. Выход"
        );

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                if (IsKeyStillValid())
                    CreateNote();
                else
                    Console.WriteLine("[USB-V] Ошибка: USB-ключ не найден или недействителен!");
                break;
            case "2":
                if (IsKeyStillValid())
                    ViewNotes();
                else
                    Console.WriteLine("[USB-V] Ошибка: USB-ключ не найден или недействителен!");
                break;
            case "3":
                if (IsKeyStillValid())
                    DeleteNote();
                else
                    Console.WriteLine("[USB-V] Ошибка: USB-ключ не найден или недействителен!");
                break;
            case "4":
                ClearConsole();
                break;
            case "5":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }
    }

    private static void CreateNote()
    {
        Console.WriteLine();

        Console.Write("Название заметки: ");
        var title = Console.ReadLine();

        Console.WriteLine("Введите текст (завершите строкой //end//):");
        var text = ConsoleHelper.ReadMultiLineInput();

        var note = new Note(title, text);
        _vault.AddNote(note, _currentKey);
        Console.WriteLine("Заметка сохранена!");
    }

    private static void ViewNotes()
    {
        Console.WriteLine();

        var notes = _vault.GetNotes(_currentKey);
        if (notes == null || notes.Count == 0)
        {
            Console.WriteLine("Нет заметок.");
            return;
        }

        for (int i = 0; i < notes.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {notes[i].Title} ({notes[i].CreatedDate})");
        }

        Console.Write("\nВыберите заметку (0 - назад): ");
        if (int.TryParse(Console.ReadLine(), out int index))
        {
            if (index > 0 && index <= notes.Count)
            {
                Console.WriteLine($"\n--- {notes[index - 1].Title} ---");
                Console.WriteLine(notes[index - 1].Text);
                Console.WriteLine($"\nИзменено: {notes[index - 1].LastModifiedDate}");
            }
        }
    }

    private static void DeleteNote()
    {
        var notes = _vault.GetNotes(_currentKey);
        if (notes == null || notes.Count == 0)
        {
            Console.WriteLine("\nНет заметок.\n");
            return;
        }

        for (int i = 0; i < notes.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {notes[i].Title}");
        }

        Console.Write("\nВыберите заметку для удаления (0 - назад): ");
        if (int.TryParse(Console.ReadLine(), out int index))
        {
            if (index > 0 && index <= notes.Count)
            {
                _vault.RemoveNote(notes[index - 1], _currentKey);
                Console.WriteLine("\nЗаметка удалена!");
            }
        }
    }
}