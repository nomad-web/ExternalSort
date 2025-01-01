namespace FileSorter.Chunks;

public abstract class ChunkFilesStrategy(IComparer<string?> comparer, long minChunkSize, long maxChunkSize)
{
    protected const long DefaultMaxChunkSize = 100 * 1024 * 1024; // 100 MB in bytes
    protected const long DefaultMinChunkSize = 10 * 1024 * 1024; // 1 MB

    protected readonly IComparer<string?> Comparer = comparer;

    public abstract Task<List<string>> Execute(string inputFileName, CancellationToken cancellationToken);

    protected long MinChunkSize { get; } = minChunkSize;

    protected long MaxChunkSize { get; } = maxChunkSize;
}