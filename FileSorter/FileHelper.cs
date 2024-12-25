namespace FileSorter;

public static class FileHelper
{
    public static void CleanTempFiles(IEnumerable<string>? files = null)
    {
        try
        {
            files ??= Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.cnk");

            foreach (var file in files)
            {
                File.Delete(file);
            }

            Console.WriteLine("All files inside the folder have been deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw new Exception($"Can't clean temporary folder! Exception:{ex}");
        }
    }
}