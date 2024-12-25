using System.Collections.Concurrent;

namespace FileSorter.Chunks;

public class ParallelFileReaderStrategy(IComparer<string?> comparer) : ChunkFilesStrategy(comparer)
{
    private const int BufferSize = 4096;

    public override async Task<List<string>> Execute(string inputFileName, CancellationToken cancellationToken) =>
        await CreateChunkedFilesAsync(inputFileName, Environment.ProcessorCount, cancellationToken); // Use max ProcessorCount

    private async Task<List<string>> CreateChunkedFilesAsync(string filePath, int numberOfThreads, CancellationToken cancellationToken)
    {
        // Clean possible temporary files from previous failed runs
        FileHelper.CleanTempFiles(); 

        var fileSize = new FileInfo(filePath).Length;
        if (fileSize < MinFileSizePerChunk)
            return [ReadChunkAndSaveSorted(filePath, 0, fileSize)];
 
        long numberOfChunks = numberOfThreads;
        var chunkSize = fileSize / numberOfThreads;

        if (chunkSize > MaxSizePerChunkBytes)
        {
            chunkSize = MaxSizePerChunkBytes;
            numberOfChunks = fileSize / MaxSizePerChunkBytes + 1;
        }

        var chunkNames = new ConcurrentBag<string>();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = numberOfThreads 
        };

        try
        {
            await Task.Run(() =>
            {
                Parallel.For(0, numberOfChunks, parallelOptions, chunkIndex =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var start = chunkIndex * chunkSize;
                    var end = chunkIndex == numberOfChunks - 1 ? fileSize : start + chunkSize;
                    
                    var chunkName = ReadChunkAndSaveSorted(filePath, start, end);
                    chunkNames.Add(chunkName);
                });

            }, cancellationToken);
                
            Console.WriteLine("Operation finished.");

            return chunkNames.ToList();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    private static List<string> ReadChunk(string filePath, long start, long end)
    {
        try
        {
            var lines = new List<string>();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: BufferSize, useAsync: true);
            using var reader = new StreamReader(fileStream);

            // Seek to the start position
            fileStream.Seek(start, SeekOrigin.Begin);

            // Adjust start to the next full line
            if (start > 0)
                reader.ReadLine(); // Skip the partial line at the beginning

            while (fileStream.Position <= end)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break; // End of file
                lines.Add(line);
            }

            return lines;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    private string ReadChunkAndSaveSorted(string filePath, long start, long end)
    {
        // Read chunk and sort
        var lines = ReadChunk(filePath, start, end);
        var sortedLines = lines.OrderBy(x => x, Comparer).ToList();

        var chunkName = $"{Path.GetTempFileName()}.cnk";
        File.WriteAllLines(chunkName, sortedLines);

        return chunkName;
    }
}