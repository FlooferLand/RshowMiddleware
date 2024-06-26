using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class RshowFormat {
    public int[] SignalData { get; set; }
    public byte[] AudioData { get; set; }
    public byte[] VideoData { get; set; }

    public RshowFormat(int[] signalData, byte[] audioData, byte[] videoData) {
        SignalData = signalData;
        AudioData = audioData;
        VideoData = videoData;
    }
    
    public void Save(string filePath) {
        var formatter = new BinaryFormatter();
        using (var stream = File.Open(filePath, FileMode.Create))
            formatter.Serialize(stream, this);
    }
}
