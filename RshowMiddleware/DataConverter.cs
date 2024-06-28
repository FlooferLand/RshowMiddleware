using System.IO.Compression;
using System.Text;
using Aspose.Zip.Saving;
using Aspose.Zip.SevenZip;
using Newtonsoft.Json;

namespace RshowMiddleware;

public static class DataConverter {
    public const string FormatExt = "bin";

    public static class RawData {
        public const short Version = 1;
        public static readonly byte[] FileHeader      = Encoding.ASCII.GetBytes("SHOWBIN\0");
        public static readonly byte[] IdentIdent      = Encoding.ASCII.GetBytes("BLOCK\0");
        public static readonly byte[] NoContentIdent  = Encoding.ASCII.GetBytes("EMPTY_CONTENT\0");
        private static readonly byte[] DataEndIdent   = { 0x00 };
        
        public static byte[] MakeIdent(byte[] bytes) {
            return IdentIdent.Concat(bytes).Concat(DataEndIdent).ToArray();
        }

        private static byte[] MakeInfo(uint length) {
            byte[] intBytes = BitConverter.GetBytes(length);
            byte[] reservedMetadata = { 0x0, 0x0, 0x0, 0x0 };
            
            return intBytes.Concat(reservedMetadata).ToArray();
        }

        public delegate void DataWriterDelegate(BinaryWriter writer);
        public static void Write(BinaryWriter writer, Ident ident, DataWriterDelegate dataWriter) {
            // Getting the data length via a simulation
            uint dataByteSize;
            using (var fakeStream = new MemoryStream()) {
                using (var fakeWriter = new BinaryWriter(fakeStream)) {
                    dataWriter(fakeWriter);
                    dataByteSize = Convert.ToUInt32(fakeWriter.BaseStream.Length);
                }
            }

            // Writing the data
            writer.Write(Idents.toBytes(ident));
            writer.Write(MakeInfo(dataByteSize));
            dataWriter(writer);
            
            Console.WriteLine($"Wrote {dataByteSize} bytes to ident '{ident}'");
        }
    }

    public static FileInfo RshowToBin(ShowFormat show, string outputFile, bool compressionFeature) {
        FileInfo outputInfo = new FileInfo(outputFile);
        using var fileStream = outputInfo.Open(FileMode.Create);
        
        // Compression
        Stream stream;
        if (compressionFeature)
            stream = new GZipStream(fileStream, CompressionLevel.SmallestSize);
        else
            stream = fileStream;

        // Converting the rshow format and writing it to the file
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        Console.WriteLine($"Converting *.rshow data to the *.{DataConverter.FormatExt} format..");

        // Header
        writer.Write(RawData.FileHeader);
        writer.Write(RawData.Version);
        writer.Write(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }); // Padding

        // Signal
        RawData.Write(writer, Ident.Signal, w => {
            foreach (int signal in show.SignalData) {
                byte[] intAsBytes = BitConverter.GetBytes(signal);
                w.Write(intAsBytes);
            }
        });

        // Audio
        RawData.Write(writer, Ident.Audio, w => { w.Write(show.AudioData); });

        // Video
        if (show.VideoData.LetSome(out byte[] videoData)) {
            RawData.Write(writer, Ident.Video, w => {
                foreach (int video in videoData) {
                    w.Write(BitConverter.GetBytes(video));
                }
            });
        }
        else {
            writer.Write(RawData.NoContentIdent);
        }

        // File output info
        return outputInfo;
    }

    public static FileInfo RshowTo7zBin(ShowFormat show, string outputFile) {
        FileInfo outputInfo = new FileInfo(outputFile);
        using var fileStream = outputInfo.Open(FileMode.Create);
        using var archive = new SevenZipArchive();
        
        // Signal
        archive.CreateEntry(
            "signal.bin",
            new MemoryStream(show.SignalData.SelectMany(BitConverter.GetBytes).ToArray()),
            new SevenZipEntrySettings(new SevenZipLZMA2CompressionSettings())
        );

        // Audio
        // TODO: Find the format. Only WAVs are supported rn
        archive.CreateEntry(
            "audio.wav",
            new MemoryStream(show.AudioData)
        );
        
        // Video
        if (show.VideoData.LetSome(out byte[] videoData)) {
            archive.CreateEntry(
                "video.mp4",
                new MemoryStream(videoData),
                new SevenZipEntrySettings(new SevenZipLZMA2CompressionSettings())
            );
        }
        
        // Metadata
        using MemoryStream metadataStream = new MemoryStream();
        using StreamWriter metadataWriter = new StreamWriter(metadataStream);
        var metadataSerializer = JsonSerializer.Create();
        if (show.Metadata.LetSome(out var metadata))
            metadataSerializer.Serialize(metadataWriter, metadata);
        if (metadataStream.Length <= 1)
            metadataSerializer.Serialize(metadataWriter, "{\n\n}");
        
        archive.CreateEntry(
            "metadata.json",
            metadataStream,
            new SevenZipEntrySettings(new SevenZipLZMA2CompressionSettings())
        );

        // Saving the archive
        // TODO: Add support for archive.SaveSplit();
        archive.Save(fileStream);
        return outputInfo;
    }
}