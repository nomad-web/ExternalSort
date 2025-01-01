using System.Diagnostics;
using FileSorter.Chunks;
using FileSorter.Merge;

namespace FileSorter;

public static class ExternalMergeSort
{
    public const string DefaultOutputFileName = "Sorted.txt";

    private static readonly LineComparer Comparer = new();

    private static readonly ChunkFilesStrategy ChunkStrategy = new ParallelFileReaderStrategy(Comparer); // Select the fastest method, debugging required! 
    //private static readonly ChunkFilesStrategy ChunkStrategy = new SingleThreadReaderStrategy(Comparer); //single thread

    
    private static readonly MergeStrategy MergeStrategy = new MultiChunkMergeStrategy(Comparer); //single thread
    //private static readonly MergeStrategy MergeStrategy = new ParallelPairwiseStrategy(Comparer); //multiple threads
    //private static readonly MergeStrategy MergeStrategy = new PairwiseMergeStrategy(Comparer); // single thread 

    /// <summary>
    /// Sort lines in the text file
    /// </summary>
    /// <param name="sourceFilePath">File to sort. Full path to a file. </param>
    /// <param name="cancellationToken">Path token to interrupt long running work</param>
    /// <param name="outputFileName">Created new file. If empty then creates "Sorted.txt" in the application folder</param>
    public static async Task SortFile(string sourceFilePath, CancellationToken cancellationToken,
        string? outputFileName = default)
    {
        // 1. Create sorted chunks of files
        var files = await CreateChunks(sourceFilePath, cancellationToken);

        // 2. Merge
        await MergeChunksIntoSingleFile(files, cancellationToken, outputFileName ?? DefaultOutputFileName);
    }

    private static async Task<List<string>> CreateChunks(string filePath, CancellationToken cancellationToken)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            return await ChunkStrategy.Execute(filePath, cancellationToken);
        }
        finally
        {
            stopWatch.Stop();
            Console.WriteLine("chunked creation time:" + stopWatch.Elapsed);
        }
    }

    private static async Task MergeChunksIntoSingleFile(List<string> files, CancellationToken cancellationToken,
        string outputFilePath)
    {
        Console.WriteLine("Start merging...");

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            await MergeStrategy.Execute(files, outputFilePath, cancellationToken);

            Console.WriteLine("File sorting successfully completed!");
        }
        finally
        {
            stopWatch.Stop();
            Console.WriteLine("Merge time:" + stopWatch.Elapsed);
        }
    }
}