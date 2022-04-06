using System.Text;

namespace Playground.MP3.ID3V2 {
    /// <summary>
    /// [4][4][2][?]
    /// [FrameID][Size][Flags][data]
    /// </summary>
    public class ID3V2Frame {
        public static readonly int FrameHeaderLength = 10;

        /// <summary>
        /// 帧标识
        /// </summary>
        public string ID { get; set; } = "0000";
        /// <summary>
        /// 帧大小
        /// </summary>
        public int Size => Data.Length;
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
        public short Flags { get; set; }
        /// <summary>
        /// 数据区
        /// </summary>
        public byte[] Data { get; set; } = new byte[1] { 0 };

        public byte[] ToBytes() {
            // 数据大小至少为1
            if (Data.Length == 0) {
                Data = new byte[1] { 0 };
            }

            // 最终输出为：帧头长度+数据长度
            byte[] bytes = new byte[FrameHeaderLength + Size];

            // 写入帧头
            byte[] frameHeader = new byte[FrameHeaderLength];
            byte[] id = Encoding.ASCII.GetBytes(ID);
            // ID信息
            frameHeader[0] = id[0];
            frameHeader[1] = id[1];
            frameHeader[2] = id[2];
            frameHeader[3] = id[3];
            // 大小信息
            frameHeader[4] = (byte)(Size >> 24);
            frameHeader[5] = (byte)(Size >> 16);
            frameHeader[6] = (byte)(Size >> 8);
            frameHeader[7] = (byte)(Size);
            // 标志位
            frameHeader[8] = (byte)(Flags >> 8);
            frameHeader[9] = (byte)(Flags);

            Array.Copy(frameHeader, bytes, frameHeader.Length);
            Array.Copy(Data, 0, bytes, 10, Data.Length);
            // 写入数据

            return bytes;
        }
        public override string ToString() {
            return $"[{ID}][{Size}][{Flags}]";
        }

        /// <summary>
        /// 获取编码的字符串字节流
        /// </summary>
        /// <param name="str">字符串信息</param>
        /// <param name="encoding">编码信息</param>
        /// <returns>编码后的字符串信息</returns>
        public static byte[] GetEncodedString(string str, ID3V2Encoding encoding) {
            byte[] strBytes;
            byte[] bytes;
            // 第一位为编码指示
            // 0：ISO
            // 1：Unicode
            // 2：UTF8
            switch (encoding) {
                default:
                case ID3V2Encoding.UTF16:
                    // 如果是Unicode编码，则第二、三位为UTF16的字节序编码指示
                    // 其中FEFF为Big Endian，FFFE为Little Endian
                    // 这里直接使用Little Endian
                    strBytes = Encoding.Unicode.GetBytes(str);
                    bytes = new byte[strBytes.Length + 3];
                    bytes[0] = 1;
                    bytes[1] = 0xFF;
                    bytes[2] = 0xFE;
                    Array.Copy(strBytes, 0, bytes, 3, strBytes.Length);
                    break;
                case ID3V2Encoding.UTF8:
                    strBytes = Encoding.UTF8.GetBytes(str);
                    bytes = new byte[strBytes.Length + 1];
                    bytes[0] = 2;
                    Array.Copy(strBytes, 0, bytes, 1, strBytes.Length);
                    break;
                case ID3V2Encoding.Latin1:
                    strBytes = Encoding.Latin1.GetBytes(str);
                    bytes = new byte[strBytes.Length + 1];
                    bytes[0] = 0;
                    Array.Copy(strBytes, 0, bytes, 1, strBytes.Length);
                    break;
            }

            return bytes;
        }
        /// <summary>
        /// 编码图像信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetEncodedImages(byte[] imageData) {
            var bytes = new List<byte>();
            // 第一组一个字节作为数据描述段信息编码格式
            bytes.Add(0x00);
            // 第二组可变长度字节的MIME信息，根据图片头写入image/jpeg或image/png
            switch (Util.GetImageType(imageData)) {
                default:
                case AttatchedPictureType.Unknow:
                case AttatchedPictureType.Jpeg:
                    bytes.AddRange(new byte[] { 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x6A, 0x70, 0x65, 0x67, 0x00 });
                    break;
                case AttatchedPictureType.Png:
                    bytes.AddRange(new byte[] { 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x70, 0x6E, 0x67, 0x00 });
                    break;
            }
            // 第三组一个字节作为图片类型描述
            bytes.Add(0x03);
            // 第四组可变长度字节的数据描述段，这里只使用0x00填充
            bytes.Add(0x00);

            // 之后是正式的图片数据
            bytes.AddRange(imageData);
            return bytes.ToArray();
        }
    }
}

/* 
 * 附录：APIC图片类型描述
 * $00     Other
 * $01     32x32 pixels 'file icon' (PNG only)
 * $02     Other file icon
 * $03     Cover (front)
 * $04     Cover (back)
 * $05     Leaflet page
 * $06     Media (e.g. lable side of CD)
 * $07     Lead artist/lead performer/soloist
 * $08     Artist/performer
 * $09     Conductor
 * $0A     Band/Orchestra
 * $0B     Composer
 * $0C     Lyricist/text writer
 * $0D     Recording Location
 * $0E     During recording
 * $0F     During performance
 * $10     Movie/video screen capture
 * $11     A bright coloured fish
 * $12     Illustration
 * $13     Band/artist logotype
 * $14     Publisher/Studio logotype
 */