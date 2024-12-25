namespace FileSorter.Chunks;

public abstract class ChunkFilesStrategy(IComparer<string?> comparer)
{
    protected const int ChunkSize = 100 * 1024 * 1024; // 100 MB in bytes
    protected const long MaxSizePerChunkBytes = 100 * 1024 * 1024; // 100 MB
    protected const long MinFileSizePerChunk = 1024 * 1024; // 1 MB

    protected readonly IComparer<string?> Comparer = comparer;

    public abstract Task<List<string>> Execute(string inputFileName, CancellationToken cancellationToken);
}