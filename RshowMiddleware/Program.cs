using System.Runtime.Serialization.Formatters.Binary;
using RshowMiddleware;

// TODO: Rewrite in Rust :3

// TODO: Put the file inside an actual 7z file instead of compressing it,
//       so people can more easily open and modify the interior file

internal static class Program {
    private const string InputPath = "C:/Users/FlooferLand/Desktop/looney.rshw";
    private const string OutputFolder = $"C:/Users/FlooferLand/Desktop/";

    private static void Main(string[] stringArgs) {
        // Argument parsing
        foreach (string arg in stringArgs) {
            string?[] split = arg.Split('=');
            string? key = split[0];
            string? value = split[1];
            if (key == null || value == null)
                throw new Exception($"Argument \"{key}={value}\" is not valid");
            
            switch (split.Length) {
                case 2:
                    Args.AddFromUser(key, value);
                    break;
                case 1:
                    Args.AddFromUser(key, "true");
                    break;
            }
        }
        
        // Printing arguments
        foreach (var arg in Args.UserProvidedArgs.Values) {
            Console.WriteLine($"{arg.GetName()} = {arg.Get()}");
        }
        Console.WriteLine();
        
        // Arguments
        var outputFilePath = Args.Register(
            "outputFile", "",
            Args.GetString
        );
        var archiveFeature = Args.RegisterFeature(
            "archive",
            "Whenever to use an archive instead of a binary file",
            false
        );
        var compressionFeature = Args.RegisterFeature(
            "compression", 
            $"Whenever to compress the output file (does nothing with '{archiveFeature.GetName()}')",
            false
        );

        // Reading the rshow file
        Console.WriteLine("Reading input rshow file..");
        ShowFormat? show = null;
        var formatter = new BinaryFormatter();
        FileInfo inputInfo = new FileInfo(InputPath);
        using (var inputStream = inputInfo.OpenRead()) {
            if (inputStream.Length != 0) {
                inputStream.Position = 0;

                #pragma warning disable SYSLIB0011
                rshwFormat surrogate = (rshwFormat)formatter.Deserialize(inputStream);
                #pragma warning restore SYSLIB0011
                
                show = new ShowFormat(
                    signalData: surrogate.signalData,
                    audioData: surrogate.audioData,
                    videoData: surrogate.videoData
                );
            }
        }

        // Safety
        if (show == null) {
            throw new Exception($"Show file at \"{InputPath}\" is null");
        }
        
        // Main stuff
        // SAFETY: It's impossible for outputFilePath.Value to be null. C# language server is shit
        string outputFile = Path.Combine(OutputFolder, outputFilePath.GetString());
        FileInfo outputInfo = archiveFeature.IsEnabled()
            ? DataConverter.RshowTo7zBin(show, outputFile)
            : DataConverter.RshowToBin(show, outputFile, compressionFeature.IsEnabled());

        // Display
        long oldFileSize = Util.ByteToMegabyte(inputInfo.Length);
        long newFileSize = Util.ByteToMegabyte(outputInfo.Length);
        Console.WriteLine("\nFinished.");
        if (archiveFeature.IsEnabled()) {
            Console.WriteLine($"Archived {oldFileSize} MB into {newFileSize} MB ({oldFileSize - newFileSize} MB less)");
        } else {
            Console.WriteLine(compressionFeature.IsEnabled()
                ? $"Wrote {oldFileSize} MB to {newFileSize} MB ({oldFileSize - newFileSize} MB less)"
                : $"Wrote {newFileSize} MB (uncompressed)");
        }
    }
}
