using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenumFiles
{
    class Converter
    {
        public string? Format { get; set; }
        public string? TargetFile { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Format) && !string.IsNullOrWhiteSpace(TargetFile);

        public void Run()
        {
            var (directoryPath, fileNames) = GetTargetFiles(TargetFile);
            Console.WriteLine($"Directory: {directoryPath}");
            foreach (var targetFile in fileNames) {
                var renumberedFileName = Renumber(targetFile, Format);
                Console.WriteLine($"{targetFile} -> {renumberedFileName}");
                try {
                    File.Move(sourceFileName: $@"{directoryPath}\{targetFile}", destFileName: $@"{directoryPath}\{renumberedFileName}");
                } catch (Exception ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            static (string directoryPath, IEnumerable<string> fileNames) GetTargetFiles(string? targetFile)
            {
                var normalizedTargetFile = NormalizePath(targetFile);
                var directoryPath        = Path.GetDirectoryName(normalizedTargetFile) ?? "";
                var fileName             = Path.GetFileName(normalizedTargetFile) ?? "";
                var filePathNames        = Directory.GetFileSystemEntries(directoryPath, fileName);
                return (directoryPath, filePathNames.Select(filePath => Path.GetFileName(filePath)));

                static string NormalizePath(string? filePath)
                    => string.IsNullOrWhiteSpace(filePath) ? Directory.GetCurrentDirectory() + @"\*"
                                                           : Path.GetFullPath(filePath);
            }

            static string Renumber(string text, string? format)
            {
                var stringBuilder       = new StringBuilder();
                var numberStringBuilder = new StringBuilder();

                foreach (var character in text) {
                    if (char.IsDigit(character)) {
                        numberStringBuilder.Append(character);
                    } else {
                        var numberString = numberStringBuilder.ToString();
                        numberStringBuilder.Clear();
                        if (int.TryParse(numberString, out var number)) {
                            try {
                                numberString = number.ToString(format);
                            } catch {
                            }
                        }
                        stringBuilder.Append(numberString);
                        stringBuilder.Append(character);
                    }
                }
                return stringBuilder.ToString();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var converter = AnalyzeCommandLine(args);
            if (converter.IsValid)
                converter.Run();
            else
                Usage();

            static Converter AnalyzeCommandLine(string[] args)
            {
                var converter = new Converter();
                foreach (var arg in args) {
                    if (IsFormat(arg, out var format))
                        converter.Format = format;
                    else
                        converter.TargetFile ??= arg;
                }
                return converter;

                static bool IsFormat(string text, out string format)
                {
                    var upperText = text.ToUpper();
                    if (upperText.StartsWith("/F") || upperText.StartsWith("-F")) {
                        format = text.Substring(2);
                        return true;
                    }
                    format = "";
                    return false;
                }
            }

            static void Usage()
            {
                const string directoryName = @"C:\MyPictures\";
                const string extension     = ".png";
                var usage                  = "RenumFiles [targetPath] /F[format]\n" +
                    "ex.\n" +
                    "\t" + $@"RenumFiles {directoryName}*{extension} /F000\0" + "\n\n" +
                    "\tBefore:\n" +
                    "\t\t" + $@"{directoryName}picture1{extension}" + "\n" +
                    "\t\t" + $@"{directoryName}picture10.jpg" + "\n" +
                    "\t\t" + $@"{directoryName}picture10{extension}" + "\n" +
                    "\t\t" + $@"{directoryName}picture2{extension}" + "\n\n" +
                    "\tAfter:\n" +
                    "\t\t" + $@"{directoryName}picture0010{extension}" + "\n" +
                    "\t\t" + $@"{directoryName}picture10.jpg" + "\n" +
                    "\t\t" + $@"{directoryName}picture0100{extension}" + "\n" +
                    "\t\t" + $@"{directoryName}picture0020{extension}" + "\n\n" +
                    "See also: Standard numeric format strings | Microsoft Docs\nhttps://docs.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings";
                Console.WriteLine(usage);
            }
        }
    }
}
