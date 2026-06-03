using System;
using System.IO;

public static class SavePaths
{
    public static string SaveDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HellYeah",
        "Saves");

    public static string StatsFilePath => Path.Combine(SaveDirectory, "BasePlayerStats.json");
    public static string RecipeBookFilePath => Path.Combine(SaveDirectory, "RecipeBook.json");
    public static string InventoryFilePath => Path.Combine(SaveDirectory, "Inventory.json");

    public static void EnsureSaveDirectory()
    {
        Directory.CreateDirectory(SaveDirectory);
    }
}
