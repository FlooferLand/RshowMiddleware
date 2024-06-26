using System.Text;

namespace RshowMiddleware;

public static class DataConverter {
    public const string FormatExt = "bin";

    public static class RawData {
        public static readonly byte[] Version         = { 0x00, 0x01 };
        public static readonly byte[] FileHeader      = Encoding.ASCII.GetBytes("SHOWBIN\0");
        public static readonly byte[] IdentIdent      = Encoding.ASCII.GetBytes("BLOCK\0");
        public static readonly byte[] NoContentIdent  = Encoding.ASCII.GetBytes("EMPTY_CONTENT\0");
        private static readonly byte[] DataEndIdent   = { 0x00 };
        
        public static byte[] MakeIdent(byte[] bytes) {
            return IdentIdent.Concat(bytes).Concat(DataEndIdent).ToArray();
        }

        private static byte[] MakeInfo(int length) {
            byte[] intBytes = BitConverter.GetBytes(length+2);
            byte[] reservedMetadata = { 0x0, 0x0, 0x0, 0x0 };
            return intBytes.Concat(reservedMetadata).Concat(DataEndIdent).ToArray();
        }

        public delegate void DataWriterDelegate(BinaryWriter writer);
        public static void Write(BinaryWriter writer, Ident ident, DataWriterDelegate dataWriter, int length) {
            writer.Write(Idents.toBytes(ident));
            writer.Write(MakeInfo(length));
            dataWriter(writer);
            writer.Write(DataEndIdent);
            
            Console.WriteLine($"Wrote {length} bytes to ident '{ident}'");
        }
    }

    public static void RshowToBin(BinaryWriter writer, RshowFormat show) {
        // Header
        writer.Write(RawData.FileHeader);
        writer.Write(RawData.Version);
        writer.Write(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }); // Padding
        
        // Signal
        RawData.Write(writer, Ident.Signal, w => {
            foreach (int signal in show.SignalData) {
                w.Write(BitConverter.GetBytes(signal));
            }
        }, Buffer.ByteLength(show.SignalData));

        // Audio
        RawData.Write(writer, Ident.Audio, w => {
            w.Write(show.AudioData);
        }, Buffer.ByteLength(show.AudioData));

        // Video
        if (show.VideoData != null) {
            RawData.Write(writer, Ident.Video, w => {
                foreach (int video in show.VideoData) {
                    w.Write(BitConverter.GetBytes(video));
                }
            }, Buffer.ByteLength(show.VideoData));
        }
        else {
            writer.Write(RawData.NoContentIdent);
        }
    }
}