using System.Collections.Concurrent;

namespace FileSorter.Chunks;

public class ParallelFileReaderStrategy(
    IComparer<string?> comparer,
    long minChunkSize = ChunkFilesStrategy.DefaultMinChunkSize,
    long maxChunkSize = ChunkFilesStrategy.DefaultMaxChunkSize
) : ChunkFilesStrategy(comparer, minChunkSize, maxChunkSize)
{
    private const int BufferSize = 16384;

    public override async Task<List<string>> Execute(string inputFileName, CancellationToken cancellationToken) =>
        await CreateChunkedFilesAsync(inputFileName, Environment.ProcessorCount,
            cancellationToken); // Use max ProcessorCount

    private async Task<List<string>> CreateChunkedFilesAsync(string filePath, int numberOfThreads,
        CancellationToken cancellationToken)
    {
        // Clean possible temporary files from previous failed runs
        FileHelper.CleanTempFiles();

        var fileSize = new FileInfo(filePath).Length;
        if (fileSize < MinChunkSize)
            return [await ReadChunkAndSaveSorted(filePath, 0, fileSize, cancellationToken)];

        long numberOfChunks = numberOfThreads;
        var chunkSize = fileSize / numberOfThreads;

        if (chunkSize > MaxChunkSize)
        {
            chunkSize = MaxChunkSize;
            numberOfChunks = fileSize / MaxChunkSize + 1;
        }

        var chunkNames = new ConcurrentBag<string>();

        Task<string> CreateSingleChunk(long chunkIndex)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var start = chunkIndex * chunkSize;
            var end = chunkIndex == numberOfChunks - 1 ? fileSize : start + chunkSize;

            return ReadChunkAndSaveSorted(filePath, start, end, cancellationToken);
        }

        try
        {
            //var result = Parallel.For(0, numberOfChunks, parallelOptions, CreateSingleChunk);
            var tasks = Enumerable.Range(0, (int)numberOfChunks)
                .Select(chunkIndex => CreateSingleChunk(chunkIndex)).ToList();

            await Task.WhenAll(tasks);

            Console.WriteLine("Operation finished.");

            return tasks.Select(x=>x.Result).ToList();
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

    private static async Task<List<string>> ReadChunk(string filePath, long start, long end,
        CancellationToken cancellationToken)
    {
        try
        {
            var lines = new List<string>();
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: BufferSize, useAsync: true);
            using var reader = new StreamReader(fileStream);

            // Seek to the start position
            fileStream.Seek(start, SeekOrigin.Begin);

            // Adjust start to the next full line
            if (start > 0)
                await reader.ReadLineAsync(cancellationToken); // Skip the partial line at the beginning

            while (fileStream.Position <= end)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
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

    private async Task<string> ReadChunkAndSaveSorted(string filePath, long start, long end,
        CancellationToken cancellationToken)
    {
        // Read chunk and sort
        var lines = await ReadChunk(filePath, start, end, cancellationToken);
        var sortedLines = lines.OrderBy(x => x, Comparer).ToList();

        var chunkName = $"{Path.GetTempFileName()}.cnk";
        File.WriteAllLines(chunkName, sortedLines);

        return chunkName;
    }
}