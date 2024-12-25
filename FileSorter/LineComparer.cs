namespace FileSorter;

public class LineComparer : IComparer<string?>
{

    public int Compare(string? line1, string? line2)
    {
        if (line1 == null && line2 == null)
            return 0;
        if (line1 == null)
            return -1;
        if (line2 == null)
            return 1;

        var sLine1 = line1.AsSpan();
        var sLine2 = line2.AsSpan();

        var xFirstDot = sLine1.IndexOf('.');
        var yFirstDot = sLine2.IndexOf('.');

        if (xFirstDot == -1 || yFirstDot == -1)
        {
            throw new FormatException("Invalid line format");
        }

        // Extract string parts
        var xStringPart = sLine1[(xFirstDot + 1)..].Trim();
        var yStringPart = sLine2[(yFirstDot + 1)..].Trim();

        // Compare string parts
        var stringComparison = xStringPart.SequenceCompareTo(yStringPart);
        if (stringComparison != 0)
            return stringComparison;

        // Extract number parts
        var xNumberPart = sLine1.Slice(0, xFirstDot).Trim();
        var yNumberPart = sLine2.Slice(0, yFirstDot).Trim();

        // Parse and compare number parts
        var xNumber = int.Parse(xNumberPart);
        var yNumber = int.Parse(yNumberPart);

        return xNumber.CompareTo(yNumber);
    }
}
