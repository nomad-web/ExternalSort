namespace FileSorter.Merge;

public class MultiChunkMergeStrategy(IComparer<string?> comparer) : MergeStrategy(comparer)
{
    public override async Task Execute(List<string> chunkFiles, string outputFileName,
        CancellationToken cancellationToken)
    {
        if (chunkFiles.Count == 1)
        {
            File.Move(chunkFiles[0], outputFileName);
            return;
        }
        var readers = chunkFiles
            .Select(chunk => new StreamReader(chunk))
            .ToList();

        // Creates sorted set which able to store duplicated lines from different chunks. 
        // Override comparer
        var minHeap = new SortedSet<(string Key, StreamReader Value)>(
            Comparer<(string Key, StreamReader Value)>.Create((x, y) =>
                {
                    var res = Comparer.Compare(x.Key, y.Key);
                    return res == 0
                        ? (x.Value != y.Value ? -1 : 0) // same line from the same reader -> equal 
                        : res;
                }
            ));

        // Initialize the heap with the first line of each chunk
        foreach (var reader in readers.Where(reader => !reader.EndOfStream))
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line != null)
                minHeap.Add((line, reader));
        }

        await using (var writer = new StreamWriter(outputFileName))
        {
            while (minHeap.Count > 0)
            {
                // Get the smallest line from the heap
                var smallest = minHeap.First();
                minHeap.Remove(smallest);

                // Write the smallest line to the output file
                await writer.WriteLineAsync(smallest.Key.AsMemory(), cancellationToken);

                // Read the next line from the corresponding reader
                var reader = smallest.Value;
                if (reader.EndOfStream)
                    continue;
                    
                var nextLine = await reader.ReadLineAsync(cancellationToken);
                if (nextLine != null)
                    minHeap.Add((nextLine, reader));                    
            }
        }

        // Close all readers
        foreach (var reader in readers)
        {
            reader.Close();
        }

        // Cleanup: Delete temporary files
        foreach (var chunkFile in chunkFiles)
        {
            File.Delete(chunkFile);
        }
    }
}