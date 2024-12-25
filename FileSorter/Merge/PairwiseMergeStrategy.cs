using FileSorter;

namespace FileSorter.Merge;

public class PairwiseMergeStrategy(IComparer<string?> comparer) : MergeStrategy(comparer)
{
    public override async Task Execute(List<string> chunkFiles, string outputFileName, CancellationToken cancellationToken)
    {
        if (chunkFiles.Count == 0)
            return;

        await Merge(chunkFiles, outputFileName, cancellationToken);

        FileHelper.CleanTempFiles(chunkFiles);
    }

    protected async Task MergeFiles(string file1, string file2, string outputFile, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(file1) || string.IsNullOrEmpty(file2) || string.IsNullOrEmpty(outputFile))
            throw new ArgumentException("File paths cannot be null or empty.");

        try
        {
            using var reader1 = new StreamReader(file1);
            using var reader2 = new StreamReader(file2);
            await using var writer = new StreamWriter(outputFile, append: false);

            string? line1 = null, line2 = null;

            // Read the first lines from both files
            if (!reader1.EndOfStream) 
                line1 = await reader1.ReadLineAsync(cancellationToken);
            
            if (!reader2.EndOfStream)
                line2 = await reader2.ReadLineAsync(cancellationToken);

            while (line1 != null || line2 != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (line1 != null && (line2 == null || Comparer.Compare(line1, line2) <= 0))
                {
                    // Write line1 to the output file
                    await writer.WriteLineAsync(line1);
                    line1 = await reader1.ReadLineAsync(cancellationToken); // Read next line from file1
                }
                else if (line2 != null)
                {
                    // Write line2 to the output file
                    await writer.WriteLineAsync(line2);
                    line2 = await reader2.ReadLineAsync(cancellationToken); // Read next line from file2
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Merge operation was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private async Task Merge(IReadOnlyList<string> files, string outputFileName, CancellationToken cancellationToken)
    {
        List<string> outputFiles = [];
        try
        {
            var isFinalFile = files.Count == 2;
            for (var i = 0; i < files.Count - 1; i += 2)
            {
                var file1 = files[i];
                var file2 = files[i + 1];
                var output = isFinalFile ? outputFileName : $"{Path.GetTempFileName()}.cnk";

                await MergeFiles(file1, file2, output, cancellationToken);

                outputFiles.Add(output);
                   
                File.Delete(file1);
                File.Delete(file2);
            }

            if (files.Count / 2 >= 1)
                await Merge(outputFiles, outputFileName, cancellationToken);
                
            Console.WriteLine("All files inside the folder have been deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw new Exception($"Can't clean temporary folder! Exception:{ex}");
        }
    }
}