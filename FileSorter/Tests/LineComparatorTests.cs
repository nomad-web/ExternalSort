using NUnit.Framework;

namespace FileSorter.Tests;

[TestFixture]
public class LineComparatorTests
{
    [Test]
    public void Compare_DifferentStrings_ReturnsCorrectOrder()
    {
        var comparator = new LineComparer();
        Assert.That(comparator.Compare("1. Apple", "2. Banana"), Is.LessThan(0));
        Assert.That(comparator.Compare("2. Banana", "1. Apple"), Is.GreaterThan(0));
    }

    [Test]
    public void Compare_SameStringsDifferentNumbers_ReturnsCorrectOrder()
    {
        var comparator = new LineComparer();
        Assert.That(comparator.Compare("1. Apple", "415. Apple"), Is.LessThan(0));
        Assert.That(comparator.Compare("415. Apple", "1. Apple"), Is.GreaterThan(0));
    }

    [Test]
    public void Compare_IdenticalLines_ReturnsZero()
    {
        var comparator = new LineComparer();
        Assert.That(comparator.Compare("1. Apple", "1. Apple"), Is.EqualTo(0));
    }

    [Test]
    public void Compare_InvalidLineFormat_ThrowsFormatException()
    {
        var comparator = new LineComparer();
        Assert.Throws<FormatException>(() => comparator.Compare("Invalid line", "1. Valid line"));
    }
}