namespace FileSorter;

public static class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run Input_File_Name [Output_File_Name]");
            return;
        }

        var sourceFilePath = args[0];

        Console.WriteLine("Application has started. Ctrl-C to end");

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancelling execution.");
            cts.Cancel();
            eventArgs.Cancel = true;
        };

        var outputFileName = args.Length == 2 ? args[1] : ExternalMergeSort.DefaultOutputFileName;

        await ExternalMergeSort.SortFile(sourceFilePath, cancellationToken, outputFileName);
    }
}