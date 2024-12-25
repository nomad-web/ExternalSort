using FileSorter;
using NUnit.Framework;

namespace FileSorter.Tests;

[TestFixture]
public class FileSorterTests
{
    [Test] 
    public async Task SortFile_CorrectlySortsFile()
    {
        var inputPath = "test_input.txt";
        var outputPath = "test_output.txt";

        // Cleanup
        DeleteFile(inputPath);
        DeleteFile(outputPath);
        
        // Create test input file
        File.WriteAllLines(inputPath, [
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow"
        ]);

        using var cts = new CancellationTokenSource();
        await ExternalMergeSort.SortFile(inputPath, cts.Token, outputPath);

        // Verify output file
        var expectedOutput = new[]
        {
            "1. Apple",
            "415. Apple",
            "2. Banana is yellow",
            "32. Cherry is the best",
            "30432. Something something something"
        };
        var actualOutput = await File.ReadAllLinesAsync(outputPath, cts.Token);

        Assert.That(expectedOutput.SequenceEqual(actualOutput));

        // Cleanup
        DeleteFile(inputPath);
        DeleteFile(outputPath);
    }

    private static void DeleteFile(string fileName)
    {
        if (File.Exists(fileName))
            File.Delete(fileName);
    }
}