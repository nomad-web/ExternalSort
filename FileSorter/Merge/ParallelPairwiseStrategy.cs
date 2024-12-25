using System.Collections.Concurrent;

namespace FileSorter.Merge;

public class ParallelPairwiseStrategy(IComparer<string?> comparer) : PairwiseMergeStrategy(comparer)
{
    public override async Task Execute(List<string> chunkFiles, string outputFileName,
        CancellationToken cancellationToken)
    {
        if (chunkFiles.Count == 0)
            return;

        if (chunkFiles.Count == 1)
        {
            File.Move(chunkFiles[0], outputFileName);
            return;
        }

        var queueOfChunks = new ConcurrentQueue<string>(chunkFiles);
        await MergeChunksAsync(queueOfChunks, outputFileName, cancellationToken);

        if (queueOfChunks.Count == 2)
            
        FileHelper.CleanTempFiles(chunkFiles);
    }

    private async Task MergeChunksAsync(ConcurrentQueue<string> chunkFiles, string outputFile,
        CancellationToken cancellationToken)
    {
        if (chunkFiles == null || chunkFiles.Count < 2)
            throw new ArgumentException("The queue must contain at least two files to merge.");

        var mergeTasks = new List<Task>();

        try
        {
            while (chunkFiles.Count > 1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Dequeue two files for merging
                if (chunkFiles.TryDequeue(out var file1))
                {
                    if (chunkFiles.TryDequeue(out var file2))
                    {
                        var task = Task.Run(async () =>
                        {
                            var newChunk = chunkFiles.IsEmpty ? outputFile : $"{Path.GetTempFileName()}.cnk";
                            await MergeFiles(file1, file2, outputFile, cancellationToken);
                            chunkFiles.Enqueue(newChunk); // Add the merged file back to the queue

                            // Optionally delete the old files to save space
                            File.Delete(file1);
                            File.Delete(file2);
                        }, cancellationToken);

                        mergeTasks.Add(task);
                    }
                    else
                    {
                        //dequeued first but could not dequeue the second, enqueue
                        chunkFiles.Enqueue(file1);
                        return;
                    }
                }
            }

            // Wait for all merging tasks to complete
            await Task.WhenAll(mergeTasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Merge operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during chunk merging: {ex.Message}");
            throw;
        }
    }
}