using RshowMiddleware;
using RshowMiddleware.Formats;

[Serializable]
public class ShowFormat {
    public int[] SignalData { get; set; }
    public byte[] AudioData { get; set; }
    public Option<byte[]> VideoData { get; set; }
    public Option<ShowFormatMetadata> Metadata { get; set; }

    public ShowFormat(int[] signalData, byte[] audioData)
        : this(signalData, audioData, Option<byte[]>.None(), Option<ShowFormatMetadata>.None()) {}

    public ShowFormat(int[] signalData, byte[] audioData, byte[]? videoData)
        : this(signalData, audioData, Option<byte[]>.Some(videoData), Option<ShowFormatMetadata>.None()) {}
    
    public ShowFormat(int[] signalData, byte[] audioData, Option<ShowFormatMetadata> metadata)
        : this(signalData, audioData, Option<byte[]>.None(), metadata) {}
    
    public ShowFormat(int[] signalData, byte[] audioData, byte[]? videoData, Option<ShowFormatMetadata> metadata)
        : this(signalData, audioData, Option<byte[]>.Some(videoData), metadata) {}
    
    public ShowFormat(int[] signalData, byte[] audioData, Option<byte[]> videoData, Option<ShowFormatMetadata> metadata) {
        SignalData = signalData;
        AudioData = audioData;
        VideoData = videoData;
        Metadata = metadata;
    }
}
