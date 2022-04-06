using System.Text;

namespace MP3.ID3V1 {
    public class ID3V1Data {
        readonly struct HeadData {
            public readonly byte[] Tag;
            public readonly byte[] Title;
            public readonly byte[] Artist;
            public readonly byte[] Album;
            public readonly byte[] Year;
            public readonly byte[] Comment;
            public readonly byte Type;

            public HeadData(Span<byte> data) {
                if (data.Length != 128) {
                    throw new ArgumentException("Invalid ID3V1 data length");
                }
                Tag = data.Slice(0, 3).ToArray();
                Title = data.Slice(3, 30).ToArray();
                Artist = data.Slice(33, 30).ToArray();
                Album = data.Slice(63, 30).ToArray();
                Year = data.Slice(93, 4).ToArray();
                Comment = data.Slice(97, 30).ToArray();
                Type = data[127];
            }
        }
    }
}