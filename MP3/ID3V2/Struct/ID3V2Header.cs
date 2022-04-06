namespace Playground.MP3.ID3V2 {
    /// <summary>
    /// [3][1][1][1][4]
    /// [Header][Version][Revision][Flag][Size]
    /// </summary>
    public readonly struct ID3V2Header {
        /// <summary>
        /// 需为ID3才是ID3V2
        /// </summary>
        public readonly byte[] ID3;
        /// <summary>
        /// 子版本号
        /// </summary>
        public readonly byte Version;
        /// <summary>
        /// 副版本号
        /// </summary>
        public readonly byte Revision;
        /// <summary>
        /// 标志字节，一般为 0，定义形如：abc00000
        /// a -- 表示是否使用 Unsynchronisation
        /// b -- 表示是否有扩展头部,一般没有(至少 Winamp 没有记录),所以一般也不设置
        /// c -- 表示是否为测试标签(99.99%的标签都不是测试用的啦,所以一般也不设置)
        /// </summary>
        public readonly byte Flag;
        /// <summary>
        /// 帧长度
        /// </summary>
        public readonly byte[] Size;

        public int GetSize() {
            return (Size[0] & 0x7F) << 21
                 | (Size[1] & 0x7F) << 14
                 | (Size[2] & 0x7F) << 7
                 | (Size[3] & 0x7F);
        }

        public ID3V2Header(Span<byte> bytesSpan) {
            ID3 = bytesSpan.Slice(0, 3).ToArray();
            Version = bytesSpan[3];
            Revision = bytesSpan[4];
            Flag = bytesSpan[5];
            Size = bytesSpan.Slice(6, 4).ToArray();
        }
    }
}