namespace RshowMiddleware;
using static DataConverter;

public enum Ident {
    Signal,
    Audio,
    Video
}

public static class Idents {
    private static readonly byte[] SignalBytes = RawData.MakeIdent(new byte[] { 0xA0 });
    private static readonly byte[] AudioBytes  = RawData.MakeIdent(new byte[] { 0xB0 });
    private static readonly byte[] VideoBytes  = RawData.MakeIdent(new byte[] { 0xC0 });

    public static byte[] toBytes(Ident ident) {
        return ident switch {
            Ident.Signal => SignalBytes,
            Ident.Audio  => AudioBytes,
            Ident.Video  => VideoBytes,
            _ => throw new Exception("Ident byte form not specified")
        };
    }
}