namespace FileSorter.Merge;

public abstract class MergeStrategy(IComparer<string?> comparer)
{
    protected readonly IComparer<string?> Comparer = comparer;

    public abstract Task Execute(List<string> chunkFiles, string outputFileName, CancellationToken cancellationToken);
}