namespace Playground.MP3.ID3V2 {
    /// <summary>
    /// [4][4][2]
    /// [FrameID][Size][Flags]
    /// </summary>
    public readonly struct ID3V2FrameHeader {
        /// <summary>
        /// 帧标识
        /// </summary>
        public readonly byte[] ID;
        /// <summary>
        /// 帧大小
        /// </summary>
        public readonly byte[] Size;
        /// <summary>
        /// 标志位，只定义了 6 位,另外的 10 位为 0,但大部分的情况下 16 位都为 0 就可以了。格式如下:
        /// abc00000 ijk00000
        /// a -- 标签保护标志,设置时认为此帧作废
        /// b -- 文件保护标志,设置时认为此帧作废
        /// c -- 只读标志,设置时认为此帧不能修改(但我没有找到一个软件理会这个标志)
        /// i -- 压缩标志,设置时一个字节存放两个 BCD 码表示数字
        /// j -- 加密标志(没有见过哪个 MP3 文件的标签用了加密)
        /// k -- 组标志,设置时说明此帧和其他的某帧是一组
        /// 值得一提的是 winamp 在保存和读取帧内容的时候会在内容前面加个'\0',并把这个字节计算在帧内容的大小中。
        /// </summary>
        public readonly byte[] Flags;

        public byte[] ToBytes() {
            byte[] output = new byte[10];
            output[0] = ID[0];
            output[1] = ID[1];
            output[2] = ID[2];
            output[3] = ID[3];

            output[4] = Size[0];
            output[5] = Size[1];
            output[6] = Size[2];
            output[7] = Size[3];

            output[8] = Flags[0];
            output[9] = Flags[1];
            return output;
        }
        public ID3V2FrameHeader(Span<byte> bytesSpan) {
            ID = bytesSpan.Slice(0, 4).ToArray();
            Size = bytesSpan.Slice(4, 4).ToArray();
            Flags = bytesSpan.Slice(8, 2).ToArray();
        }
    }
}