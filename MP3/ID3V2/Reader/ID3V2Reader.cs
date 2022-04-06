using System.Text;

namespace Playground.MP3.ID3V2 {
    public static class ID3V2Reader {
        public static ID3V2Data Read(Span<byte> bytes) {
            var result = new ID3V2Data();

            // 前10个字节作为标签头读取
            var headerData = new ID3V2Header(bytes.Slice(0, 10));
            if (Encoding.ASCII.GetString(headerData.ID3) != "ID3") {
                throw new FormatException("Invalid ID3v2 header");
            }
            result.Version = headerData.Version;
            result.Revision = headerData.Revision;
            result.Flag = headerData.Flag;
            // 获取帧大小，并向后读取帧
            int size = (headerData.Size[0] & 0x7F) << 21
                     | (headerData.Size[1] & 0x7F) << 14
                     | (headerData.Size[2] & 0x7F) << 7
                     | (headerData.Size[3] & 0x7F);

            // 读取标签头后，pos的起始位为10
            int pos = 10;
            while (pos < size) {
                // 读取帧头，pos后移10
                var frameHeaderData = new ID3V2FrameHeader(bytes.Slice(pos, 10));
                pos += 10;
                // 读取帧大小，pos后移
                int frameSize = GetIntFromBytes(frameHeaderData.Size);
                var frame = new ID3V2Frame() {
                    ID = Encoding.ASCII.GetString(frameHeaderData.ID),
                    Flags = (short)(frameHeaderData.Flags[0] << 8 | frameHeaderData.Flags[1]),
                    Data = bytes.Slice(pos, frameSize).ToArray(),
                };
                pos += frameSize;

                result.Frames.Add(frame);
            }

            return result;
        }
        public static ID3V2Data Read(byte[] bytes) {
            return Read(bytes.AsSpan());
        }

        private static int GetIntFromBytes(byte[] bytes) {
            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }
    }
}