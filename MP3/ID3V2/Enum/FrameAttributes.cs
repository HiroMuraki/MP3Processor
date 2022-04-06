namespace Playground.MP3.ID3V2 {
    [Flags]
    public enum FrameAttributes {
        None = 0b_0000_0000,
        LabelProtected = 0b_1000_0000,
        FileProtected = 0b_0100_0000,
        Readonly = 0b_0010_0000,
        Compressed = 0b_0000_1000,
        Encrypted = 0b_0000_0100,
        Group = 0b_0000_0010,
    }

}