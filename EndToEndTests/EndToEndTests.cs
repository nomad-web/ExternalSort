using FileGenerator;
using FileSorter;
using FluentAssertions;
using NUnit.Framework;

namespace EndToEndTests;

[TestFixture]
public class EndToEndTests
{
    private static readonly string SourceFileName = $"{Path.GetTempFileName()}.txt";
    private static readonly string OutputFileName = $"{SourceFileName}_sorted.txt";

    [TestCase(1024)]
    [TestCase(1024L * 1024L)]
    [TestCase(1024L * 1024L * 1024L)]
    public async Task GenerateFile_Sort_CheckFileSorted(long desiredFileSize)
    {
        // Generate file
        TextFileGenerator.GenerateFile(SourceFileName, desiredFileSize);

        // Assert
        File.Exists(SourceFileName).Should().BeTrue();
        var fileSizeInBytes = new FileInfo(SourceFileName).Length;
        fileSizeInBytes.Should().BeGreaterThan(desiredFileSize);

        // Sort
        using var cts = new CancellationTokenSource();
        await ExternalMergeSort.SortFile(SourceFileName, cts.Token, OutputFileName);

        // Assert
        File.Exists(OutputFileName).Should().BeTrue();
        var sortedFileSizeInBytes = new FileInfo(OutputFileName).Length;

        var sourceLines = await File.ReadAllLinesAsync(SourceFileName, cts.Token);
        var outputLines = await File.ReadAllLinesAsync(OutputFileName, cts.Token);
        var missingLines = sourceLines.Except(outputLines);
        var list = missingLines.ToList();
        
        missingLines.Should().BeEmpty();
        
        missingLines = outputLines.Except(sourceLines);
        missingLines.Should().BeEmpty();
        
        // Cleanup
        File.Delete(SourceFileName);
        File.Delete(OutputFileName);
    }
}