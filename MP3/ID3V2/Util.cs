using System.Text;

namespace Playground.MP3.ID3V2 {

    public static class Util {
        public static readonly string[] TypeIDs = new string[] {
            "AENC",
            "APIC",
            "COMM",
            "COMR",
            "ENCR",
            "ETC0",
            "GEOB",
            "GRID",
            "IPLS",
            "MCDI",
            "MLLT",
            "OWNE",
            "PRIV",
            "PCNT",
            "POPM",
            "POSS",
            "RBUF",
            "RVAD",
            "RVRB",
            "SYLT",
            "SYTC",
            "TALB",
            "TBPM",
            "TCOM",
            "TCON",
            "TCOP",
            "TDAT",
            "TDLY",
            "TENC",
            "TEXT",
            "TFLT",
            "TIME",
            "TIT1",
            "TIT2",
            "TIT3",
            "TKEY",
            "TLAN",
            "TLEN",
            "TMED",
            "TOAL",
            "TOFN",
            "TOLY",
            "TOPE",
            "TORY",
            "TOWM",
            "TPE1",
            "TPE2",
            "TPE3",
            "TPE4",
            "TPOS",
            "TPUB",
            "TRCK",
            "TRDA",
            "TRSN",
            "TRSO",
            "TSIZ",
            "TSRC",
            "TSSE",
            "TYER",
            "TXXX",
            "UFID",
            "USER",
            "USLT",
            "WCOM",
            "WCOP",
            "WOAF",
            "WOAR",
            "WOAS",
            "WORS",
            "WPAY",
            "WPUB",
            "WXXX",
        };

        public static AttatchedPictureType GetImageType(Span<byte> imageData) {
            // 探测图片类型
            // 0xFF, 0xD8
            if (CheckFileHeader(imageData, new byte[] { 0xFF, 0xD8 })) {
                return AttatchedPictureType.Jpeg;
            }
            // 0x89, 0x50, 0x4E
            else if (CheckFileHeader(imageData, new byte[] { 0x89, 0x50, 0x4E })) {
                return AttatchedPictureType.Png;
            }
            else {
                return AttatchedPictureType.Unknow;
            }
        }
        public static string GetIDType(string id, Language language) {
            try {
                return language switch {
                    Language.SChinese => _idTypesCH[id],
                    Language.English => _idTypesEN[id],
                    _ => _idTypesEN[id]
                };
            }
            catch (Exception) {
                return id;
            }

        }
        public static string GetStringFromBytes(ID3V2Frame frame) {
            if (frame.Data.Length != frame.Size) {
                throw new FormatException("Invalid data length");
            }
            if (frame.ID == "APIC") {
                return frame.ToString();
            }

            switch (frame.Data[0]) {
                case 0:
                    return Encoding.Latin1.GetString(frame.Data.AsSpan().Slice(1));
                case 1:
                    _ = new int[2] { frame.Data[1], frame.Data[2] };
                    return Encoding.Unicode.GetString(frame.Data.AsSpan().Slice(3, frame.Size - 3));
                case 2:
                    return Encoding.UTF8.GetString(frame.Data.AsSpan().Slice(1));
                default:
                    throw new NotSupportedException("Not supported encoding");
            }
        }
        public static void ExtractCoverImage(byte[] data, string output) {
            int start = 0;
            // 第一个字节作为信息编码格式
            start += 1;
            // 可变长度，使用strlen求出MIME Type长度，考虑到0，需要+1
            start += GetCStrlen(data.AsSpan().Slice(1)) + 1;
            // 图像类型描述占1字节
            start += 1;
            // 数据描述段长度
            start += GetCStrlen(data.AsSpan().Slice(start)) + 1;

            string correctedOutput = Path.Combine(Path.GetDirectoryName(output) ?? "", Path.GetFileNameWithoutExtension(output));
            // 探测图片类型
            switch (GetImageType(data.AsSpan().Slice(start))) {
                default:
                case AttatchedPictureType.Unknow:
                case AttatchedPictureType.Jpeg:
                    correctedOutput += ".jpeg";
                    break;
                case AttatchedPictureType.Png:
                    correctedOutput += ".png";
                    break;
            }

            // 写入图像
            using (var fs = new FileStream(correctedOutput, FileMode.Create, FileAccess.Write)) {
                for (int i = start; i < data.Length; i++) {
                    fs.WriteByte(data[i]);
                }
            }
        }

        private static readonly Dictionary<string, string> _idTypesCH = new Dictionary<string, string>() {
            ["AENC"] = "音频加密技术",
            ["APIC"] = "内嵌的图片",
            ["COMM"] = "注释",
            ["COMR"] = "广告",
            ["ENCR"] = "加密方法注册",
            ["EQUA"] = "均衡",
            ["ETC0"] = "事件时间编码",
            ["GEOB"] = "常规压缩对象",
            ["GRID"] = "组识别注册",
            ["IPLS"] = "复杂类别列表",
            ["LINK"] = "链接的信息",
            ["MCDI"] = "音乐CD标识符",
            ["MLLT"] = "MPEG位置查找表格",
            ["OWNE"] = "所有权",
            ["PRIV"] = "私有",
            ["PCNT"] = "播放计数",
            ["POPM"] = "普通仪表",
            ["POSS"] = "位置同步",
            ["RBUF"] = "推荐缓冲区大小",
            ["RVAD"] = "音量调节器",
            ["RVRB"] = "混响",
            ["SYLT"] = "同步歌词或文本",
            ["SYTC"] = "同步节拍编码",
            ["TALB"] = "专辑",
            ["TBPM"] = "每分钟节拍数",
            ["TCOM"] = "作曲家",
            ["TCON"] = "流派",
            ["TCOP"] = "版权",
            ["TDAT"] = "日期",
            ["TDLY"] = "播放列表返录",
            ["TENC"] = "编码",
            ["TEXT"] = "歌词作者",
            ["TFLT"] = "文件类型",
            ["TIME"] = "时间",
            ["TIT1"] = "内容组描述",
            ["TIT2"] = "标题",
            ["TIT3"] = "副标题",
            ["TKEY"] = "最初关键字",
            ["TLAN"] = "语言",
            ["TLEN"] = "长度",
            ["TMED"] = "媒体类型",
            ["TOAL"] = "原唱片集",
            ["TOFN"] = "原文件名",
            ["TOLY"] = "原歌词作者",
            ["TOPE"] = "原艺术家",
            ["TORY"] = "最初发行年份",
            ["TOWM"] = "文件所有者（许可证者）",
            ["TPE1"] = "艺术家",
            ["TPE2"] = "乐队",
            ["TPE3"] = "指挥者",
            ["TPE4"] = "翻译（记录员、修改员）",
            ["TPOS"] = "作品集部分",
            ["TPUB"] = "发行人",
            ["TRCK"] = "音轨",
            ["TRDA"] = "录制日期",
            ["TRSN"] = "Intenet电台名称",
            ["TRSO"] = "Intenet电台所有者",
            ["TSIZ"] = "大小",
            ["TSRC"] = "ISRC",
            ["TSSE"] = "编码使用的软件",
            ["TYER"] = "年代",
            ["TXXX"] = "用户定义文本信息",
            ["UFID"] = "唯一文件标识符",
            ["USER"] = "使用条款",
            ["USLT"] = "非同步歌词转录",
            ["WCOM"] = "广告信息",
            ["WCOP"] = "版权信息",
            ["WOAF"] = "官方音频文件网页",
            ["WOAR"] = "官方艺术家网页",
            ["WOAS"] = "官方音频原始资料网页",
            ["WORS"] = "官方互联网无线配置首页",
            ["WPAY"] = "付款",
            ["WPUB"] = "出版商官方网页",
            ["WXXX"] = "用户定义的URL链接",
        };
        private static readonly Dictionary<string, string> _idTypesEN = new Dictionary<string, string>() {
            ["AENC"] = "Audio encryption",
            ["APIC"] = "Attached picture",
            ["COMM"] = "Comments",
            ["COMR"] = "Commercial frame",
            ["ENCR"] = "Encryption method registration",
            ["EQUA"] = "Equalization",
            ["ETCO"] = "Event timing codes",
            ["GEOB"] = "General encapsulated object",
            ["GRID"] = "Group identification registration",
            ["IPLS"] = "Involved people list",
            ["LINK"] = "Linked information",
            ["MCDI"] = "Music CD identifier",
            ["MLLT"] = "MPEG location lookup table",
            ["OWNE"] = "Ownership frame",
            ["PRIV"] = "Private frame",
            ["PCNT"] = "Play counter",
            ["POPM"] = "Popularimeter",
            ["POSS"] = "Position synchronisation frame",
            ["RBUF"] = "Recommended buffer size",
            ["RVAD"] = "Relative volume adjustment",
            ["RVRB"] = "Reverb",
            ["SYLT"] = "Synchronized lyric/text",
            ["SYTC"] = "Synchronized tempo codes",
            ["TALB"] = "Album/Movie/Show title",
            ["TBPM"] = "BPM (beats per minute)",
            ["TCOM"] = "Composer",
            ["TCON"] = "Content type",
            ["TCOP"] = "Copyright message",
            ["TDAT"] = "Date",
            ["TDLY"] = "Playlist delay",
            ["TENC"] = "Encoded by",
            ["TEXT"] = "Lyricist/Text writer",
            ["TFLT"] = "File type",
            ["TIME"] = "Time",
            ["TIT1"] = "Content group description",
            ["TIT2"] = "Title/songname/content description",
            ["TIT3"] = "Subtitle/Description refinement",
            ["TKEY"] = "Initial key",
            ["TLAN"] = "Language(s)",
            ["TLEN"] = "Length",
            ["TMED"] = "Media type",
            ["TOAL"] = "Original album/movie/show title",
            ["TOFN"] = "Original filename",
            ["TOLY"] = "Original lyricist(s)/text writer(s)",
            ["TOPE"] = "Original artist(s)/performer(s)",
            ["TORY"] = "Original release year",
            ["TOWN"] = "File owner/licensee",
            ["TPE1"] = "Lead performer(s)/Soloist(s)",
            ["TPE2"] = "Band/orchestra/accompaniment",
            ["TPE3"] = "Conductor/performer refinement",
            ["TPE4"] = "Interpreted, remixed, or otherwise modified by",
            ["TPOS"] = "Part of a set",
            ["TPUB"] = "Publisher",
            ["TRCK"] = "Track number/Position in set",
            ["TRDA"] = "Recording dates",
            ["TRSN"] = "Internet radio station name",
            ["TRSO"] = "Internet radio station owner",
            ["TSIZ"] = "Size",
            ["TSRC"] = "ISRC (international standard recording code)",
            ["TSSE"] = "Software/Hardware and settings used for encoding",
            ["TYER"] = "Year",
            ["TXXX"] = "User defined text information frame",
            ["UFID"] = "Unique file identifier",
            ["USER"] = "Terms of use",
            ["USLT"] = "Unsychronized lyric/text transcription",
            ["WCOM"] = "Commercial information",
            ["WCOP"] = "Copyright/Legal information",
            ["WOAF"] = "Official audio file webpage",
            ["WOAR"] = "Official artist/performer webpage",
            ["WOAS"] = "Official audio source webpage",
            ["WORS"] = "Official internet radio station homepage",
            ["WPAY"] = "Payment",
            ["WPUB"] = "Publishers official webpage",
            ["WXXX"] = "User defined URL link frame"
        };
        private static int GetCStrlen(Span<byte> chars) {
            int length = 0;
            for (int i = 0; i < chars.Length; i++) {
                if (chars[i] == '\0') {
                    break;
                }
                length++;
            }
            return length;
        }
        private static bool CheckFileHeader(Span<byte> bytes, byte[] header) {
            if (bytes.Length < header.Length) {
                return false;
            }
            for (int i = 0; i < header.Length; i++) {
                if (bytes[i] != header[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}