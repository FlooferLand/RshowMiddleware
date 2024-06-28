using System.ComponentModel;

namespace RshowMiddleware.Formats;

[Serializable]
public record ShowFormatMetadata(FormatVersion Version, Show Show, Song Song) {
    public byte[] toBytes() {
        using MemoryStream stream = new MemoryStream();

        // Version
        stream.Write(BitConverter.GetBytes(Version.Major).Concat(BitConverter.GetBytes(Version.Minor)).ToArray());

        // Show
        
        // Song
        
        return stream.ToArray();
    }
}

[Serializable]
public record FormatVersion(ushort Major,  ushort Minor);

[Serializable]
public record Show(string Name);

[Serializable]
public record Song(string Title, string Author);
