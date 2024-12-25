using System.CommandLine;
using System.Text;

namespace FileGenerator;

public static class TextFileGenerator
{
    private const int MaxNumberOfWordsInRow = 5;
    private const long DefaultFileSize = 1024L * 1024L; // 1 MB
    private const string DefaultFileName = "FileToSort_";
    private const int FileBufferSize = 1024 * 1024; // 1 MB
    
    private static readonly string[] Words =
    [
        "Apple",
        "Mango",
        "Banana",
        "Grape",
        "Cherry",
        "Pineapple",
        "Pear",
        "Orange",
        "Kiwi",
        "Lemon",
        "Plum",
        "Peach",
        "Papaya",
        "Watermelon",
        "Strawberry"
    ];

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Usage: dotnet run <File_Name> <File_Size_In_Bytes>");

        var fileNameOption = new Argument<string>(
            name: "fileName",
            description: $"Define your file name.",
            getDefaultValue: () => DefaultFileName + DateTime.Now.Ticks + ".txt");

        var fileSizeOption = new Argument<long>(
            name: "fileSize",
            description: $"The size of the file in bytes.",
            getDefaultValue: () => DefaultFileSize);

        var rootCommand = new RootCommand
        {
            fileNameOption,
            fileSizeOption
        };
        rootCommand.SetHandler(GenerateFile, fileNameOption, fileSizeOption);
        
        await rootCommand.InvokeAsync(args);
    }

    public static void GenerateFile(string fileName, long fileSize)
    {
        Console.WriteLine($"Started file generation. File name '{fileName}', file size: {fileSize} bytes.");
   
        var random = new Random();
        
        long totalBytes = 0;
        using var streamWriter = new StreamWriter(fileName, false, Encoding.UTF8, FileBufferSize);
        try
        {
            while (totalBytes < fileSize)
            {
                var number = random.Next(10000); // Choose random number from 1 to 10000
                var wordsInLine = random.Next(1, MaxNumberOfWordsInRow);
                var sb = new StringBuilder(wordsInLine * 2 + 2);
                sb.Append(number);
                sb.Append(". ");

                for (var i = 0; i < wordsInLine; i++)
                {
                    sb.Append(Words[random.Next(Words.Length)]);
                    sb.Append(' ');
                }

                var strLine = sb.ToString();
                streamWriter.WriteLine(strLine);
                totalBytes += strLine.Length;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured: {ex}");
        }
        
        Console.WriteLine("File has been generated.");
    }
}


