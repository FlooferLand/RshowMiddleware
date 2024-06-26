using System;

[Serializable]
public class rshwFormat {
    public byte[] audioData { get; set; }
    public int[] signalData { get; set; }
    public byte[] videoData { get; set; }
}
