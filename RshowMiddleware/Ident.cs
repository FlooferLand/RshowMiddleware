namespace RshowMiddleware;
using static DataConverter;

public enum Ident {
    Signal,
    Audio,
    Video
}

public static class Idents {
    private static readonly byte[] SignalBytes = RawData.MakeIdent(new byte[] { 0x1A, /* Reserved */ 0x00 });
    private static readonly byte[] AudioBytes  = RawData.MakeIdent(new byte[] { 0x2A, /* Reserved */ 0x00 });
    private static readonly byte[] VideoBytes  = RawData.MakeIdent(new byte[] { 0x3A, /* Reserved */ 0x00 });

    public static byte[] toBytes(Ident ident) {
        return ident switch {
            Ident.Signal => SignalBytes,
            Ident.Audio  => AudioBytes,
            Ident.Video  => VideoBytes,
            _ => throw new Exception("Ident byte form not specified")
        };
    }
}