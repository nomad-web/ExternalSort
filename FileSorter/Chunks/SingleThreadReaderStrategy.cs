namespace FileSorter.Chunks;

public class SingleThreadReaderStrategy(IComparer<string?> comparer) 
    : ChunkFilesStrategy(comparer, DefaultMinChunkSize, DefaultMaxChunkSize)
{
    public override async Task<List<string>> Execute(string inputFileName, CancellationToken cancellationToken)
    {
        // Clean possible temporary files from previous failed runs
        FileHelper.CleanTempFiles();
        
        var chunkFiles = new List<string>();
        var lines = new List<string>();
        long currentSize = 0;
        
        using var reader = new StreamReader(inputFileName);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) 
                continue;

            lines.Add(line);
            currentSize += line.Length;

            // If chunk size exceeds the limit, sort and save the chunk
            if (currentSize > MaxChunkSize)
            {
                chunkFiles.Add(await SaveSortedChunk(lines, cancellationToken));
                lines = [];
                currentSize = 0;
            }
        }

        // Save any remaining lines
        if (lines.Count > 0)
            chunkFiles.Add(await SaveSortedChunk(lines, cancellationToken));

        return chunkFiles;
    }

    private async Task<string> SaveSortedChunk(List<string> lines, CancellationToken cancellationToken)
    {
        lines.Sort(Comparer); // Sort the lines in memory
        var chunkFile = Path.GetTempFileName() + ".cnk";
        await File.WriteAllLinesAsync(chunkFile, lines, cancellationToken); 

        return chunkFile;
    }
}