namespace PersonalKnowledgeHub.Storage.Validators;

public static class FileSignatures
{
    public static readonly Dictionary<string, (byte[] Signature, int Offset)> FileSignature = new()
    {
        { "pdf", (new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2d }, 0) },
        { "png", (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0) },
        { "mp4", (new byte[] { 0x66, 0x74, 0x79, 0x70 }, 4) },
    };

    public static readonly Dictionary<string, int> FileBrandBytes = new()
    {
        { "pdf", 0 },
        { "png", 0 },
        { "mp4", 4 },
    };
    
    public static readonly HashSet<string> AllowedMp4Brands =
    [
        "isom",
        "mp41",
        "mp42",
        "avc1"
    ];
}