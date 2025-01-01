using FileSorter.Chunks;
using FluentAssertions;
using NUnit.Framework;

namespace FileSorter.Tests
{
    [TestFixture]
    public class ParallelFileReaderStrategyTests
    {
        private const int MinChunkSize = 1024; // Example value
        private const int MaxSizePerChunkBytes = 1024 * 1024; // 10 MB
        private readonly IComparer<string?> _comparer = StringComparer.Ordinal;
        private static readonly string[] Expectation = ["apple", "banana", "cherry"];

        private static readonly ParallelFileReaderStrategy Strategy
            = new(StringComparer.Ordinal, MinChunkSize, MaxSizePerChunkBytes);
  
        [TearDown]
        public void TearDown()
        {
            var testFilePath = Path.GetTempFileName();
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [Test]
        public async Task Execute_ShouldCreateSingleChunk_ForSmallFile()
        {
            // Arrange
            var testFilePath = Path.GetTempFileName();
            var content = new[] {"banana", "apple", "cherry"};
            await File.WriteAllLinesAsync(testFilePath, content);

            // Act
            var result = await Strategy.Execute(testFilePath, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            var sortedLines = await File.ReadAllLinesAsync(result[0]);
            sortedLines.Should().BeInAscendingOrder(x => x, _comparer);
        }

        [Test]
        public async Task Execute_ShouldSplitFileIntoChunks_ForLargeFile()
        {
            // Arrange
            var testFilePath = Path.GetTempFileName();
            var content = Enumerable.Range(0, 100000)
                .Select(i => $"{i}. Apple  Apple  Apple  Apple  Apple  Apple  Apple  Apple  Apple ");
            await File.WriteAllLinesAsync(testFilePath, content);

            // Act
            var result = await Strategy.Execute(testFilePath, CancellationToken.None);

            // Assert
            result.Should().HaveCountGreaterThan(1);
            foreach (var chunkFile in result)
            {
                var sortedLines = await File.ReadAllLinesAsync(chunkFile);
                sortedLines.Should().BeInAscendingOrder(x => x, _comparer);
            }
        }

        [Test]
        public async Task Execute_ShouldThrowOperationCanceledException_WhenCancelled()
        {
            // Arrange
            var testFilePath = Path.GetTempFileName();
            var content = Enumerable.Range(0, 10000)
                .Select(i => $"{i}. Apple Apple Apple Apple Apple Apple Apple Apple Apple Apple Apple Apple Apple ");
            await File.WriteAllLinesAsync(testFilePath, content);
            
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act & Assert
            Func<Task> act = async () => await Strategy.Execute(testFilePath, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task ReadChunkAndSaveSorted_ShouldSortChunkCorrectly()
        {
            // Arrange
            var testFilePath = Path.GetTempFileName();
            var content = new[] {"banana", "apple", "cherry"};
            await File.WriteAllLinesAsync(testFilePath, content);

            // Act
            var chunkFile = await Strategy.Execute(testFilePath, CancellationToken.None);

            // Assert
            var sortedLines = await File.ReadAllLinesAsync(chunkFile.First());
            sortedLines.Should()
                .BeEquivalentTo(Expectation, options => options.WithStrictOrdering());
        }
    }
}