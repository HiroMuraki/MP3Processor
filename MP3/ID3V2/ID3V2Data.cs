using System.Text;

namespace Playground.MP3.ID3V2 {
    /// <summary>
    /// [3][1][1][1][4][10+?][10+?]...[10+?]
    /// [Header][Version][Revision][Flag][Size][Frames1][Frame2]...[Frame3]
    /// </summary>
    public class ID3V2Data {
        public static readonly int HeaderLength = 10;

        /// <summary>
        /// 需为ID3才是ID3V2
        /// </summary>
        public string ID3 { get; } = "ID3";
        /// <summary>
        /// 子版本号
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// 副版本号
        /// </summary>
        public byte Revision { get; set; }
        /// <summary>
        /// 标志字节，一般为 0，定义形如：abc00000
        /// a -- 表示是否使用 Unsynchronisation
        /// b -- 表示是否有扩展头部,一般没有(至少 Winamp 没有记录),所以一般也不设置
        /// c -- 表示是否为测试标签(99.99%的标签都不是测试用的啦,所以一般也不设置)
        /// </summary>
        public byte Flag { get; set; }
        /// <summary>
        /// 帧长度
        /// </summary>
        public int Size => Frames.Sum((frame) => frame.Size) + Frames.Count * 10;
        public List<ID3V2Frame> Frames { get; set; } = new List<ID3V2Frame>();

        public byte[] ToBytes() {
            // 标头数据
            // ID3v2的标签头长度恒为10
            byte[] headerBytes = new byte[HeaderLength];
            // ID3标头[3]
            Array.Copy(Encoding.ASCII.GetBytes(ID3), headerBytes, 3);
            // 子版本号[1]
            headerBytes[3] = Version;
            // 副版本号[1]
            headerBytes[4] = Revision;
            // 标志字节[1]
            headerBytes[5] = Flag;
            // 帧长度
            headerBytes[6] = (byte)((Size >> 21) & 0x7F);
            headerBytes[7] = (byte)((Size >> 14) & 0x7F);
            headerBytes[8] = (byte)((Size >> 7) & 0x7F);
            headerBytes[9] = (byte)((Size) & 0x7F);

            // 帧数据
            var framesBytes = new List<byte>(Size);
            foreach (var frame in Frames) {
                framesBytes.AddRange(frame.ToBytes());
            }

            // 标记的帧总尺寸+标签头的10字长
            var bytes = new byte[Size + 10];
            Array.Copy(headerBytes, bytes, 10);
            Array.Copy(framesBytes.ToArray(), 0, bytes, 10, framesBytes.Count);
            return bytes;
        }
    }
}
