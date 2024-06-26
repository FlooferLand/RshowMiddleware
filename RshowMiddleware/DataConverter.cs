using System.Text;

namespace RshowMiddleware;

public static class DataConverter {
    public const string FormatExt = "bin";

    public static class RawData {
        private static readonly byte[] IdentIdent    = Encoding.ASCII.GetBytes("SHWID\0");
        public static readonly byte[] NoContentIdent = Encoding.ASCII.GetBytes("EMPTY_CONTENT\0");
        private static readonly byte[] DataEndIdent   = { 0x00 };
        
        public static byte[] MakeIdent(byte[] bytes) {
            return IdentIdent.Concat(bytes).ToArray();
        }

        private static byte[] MakeInfo(long length) {
            byte[] intBytes = BitConverter.GetBytes(length);
            return intBytes.Concat(new byte[] { 0x00 }).ToArray();
        }

        public delegate void DataWriterDelegate(BinaryWriter writer);
        public static void Write(BinaryWriter writer, Ident ident, DataWriterDelegate dataWriter, long length) {
            byte[] identBytes = Idents.toBytes(ident);
            writer.Write(identBytes);
            writer.Write(MakeInfo(length));
            dataWriter(writer);
            writer.Write(DataEndIdent);
            Console.WriteLine($"Wrote {length} bytes to ident '{ident}'");
        }
    }

    public static void RshowToBin(BinaryWriter writer, RshowFormat show) {
        // Signal
        RawData.Write(writer, Ident.Signal, w => {
            foreach (int signal in show.SignalData) {
                w.Write(BitConverter.GetBytes(signal));
            }
        }, show.SignalData.LongLength);

        // Audio
        RawData.Write(writer, Ident.Audio, w => {
            w.Write(show.AudioData);
        }, show.AudioData.LongLength);

        // Video
        if (show.VideoData != null) {
            RawData.Write(writer, Ident.Video, w => {
                foreach (int video in show.VideoData) {
                    w.Write(BitConverter.GetBytes(video));
                }
            }, show.VideoData.LongLength);
        }
        else {
            writer.Write(RawData.NoContentIdent);
        }
    }
}