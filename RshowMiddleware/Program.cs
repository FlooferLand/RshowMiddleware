using System.Text;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using RshowMiddleware;

// TODO: Rewrite in Rust :3

// TODO: Put the file inside an actual 7z file instead of compressing it,
//       so people can more easily open and modify the interior file

internal static class Program {
    private const string InputPath = "D:/Creative Engineering/Show tapes/Monkees Medley.rshw";
    private const string OutputPath = $"C:/Users/FlooferLand/Desktop/";

    private static void Main(string[] stringArgs) {
        // Argument parsing
        foreach (string arg in stringArgs) {
            string[] split = arg.Split('=');
            switch (split.Length) {
                case 2:
                    Args.Add(split[0], split[1]);
                    break;
                case 1:
                    Args.Add(split[0], "true");
                    break;
            }
        }
        
        // Arguments
        bool compressionFeature = Args.IsFeatureEnabled("compression");

        // Reading the rshow file
        Console.WriteLine("Reading input rshow file..");
        RshowFormat? show = null;
        var formatter = new BinaryFormatter();
        FileInfo inputInfo = new FileInfo(InputPath);
        using (var inputStream = inputInfo.OpenRead()) {
            if (inputStream.Length != 0) {
                inputStream.Position = 0;

                rshwFormat surrogate = (rshwFormat)formatter.Deserialize(inputStream);
                show = new RshowFormat(
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
        FileInfo outputInfo = new FileInfo(Path.Combine(OutputPath, Args.Get("filename")?.GetString() ?? $"ShowTape.{DataConverter.FormatExt}"));
        using (var fileStream = outputInfo.Open(FileMode.Create)) {
            // Compression
            Stream stream;
            if (compressionFeature) {
                stream = new GZipStream(fileStream, CompressionLevel.SmallestSize);
            } else {
                stream = fileStream;
            }
            
            // Converting the rshow format and writing it to the file
            using var binaryWriter = new BinaryWriter(stream, Encoding.UTF8);
            Console.WriteLine($"Converting *.rshow data to the *.{DataConverter.FormatExt} format..");
            DataConverter.RshowToBin(binaryWriter, show);
        }

        // Display
        long oldFileSize = Util.ByteToMegabyte(inputInfo.Length);
        long newFileSize = Util.ByteToMegabyte(outputInfo.Length);
        Console.WriteLine("\nFinished.");
        Console.WriteLine(compressionFeature
            ? $"Wrote {oldFileSize} MB to {newFileSize} MB ({oldFileSize - newFileSize} MB less)"
            : $"Wrote {newFileSize} MB (uncompressed)");
    }
}
