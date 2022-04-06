using Playground.MP3.ID3V2;

namespace Playground.MP3 {
    public class MP3Info {
        public ID3V2Data ID3V2 { get; } = new ID3V2Data();
        public byte[] MusicData { get; } = Array.Empty<byte>();

        public static MP3Info Load(string file) {
            return new MP3Info(file);
        }

        private MP3Info(string file) {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                // 前10个字节作为标头读取
                byte[] buffer = new byte[10];
                fs.Read(buffer, 0, buffer.Length);
                var header = new ID3V2Header(buffer);
                fs.Seek(0, SeekOrigin.Begin);
                byte[] dataBuffer = new byte[header.GetSize() + 10];
                fs.Read(dataBuffer, 0, dataBuffer.Length);
                ID3V2 = ID3V2Reader.Read(dataBuffer);


                // 读取音频数据
                var musicData = new List<byte>();
                byte[] musicDataBuffer = new byte[32];
                while (fs.Read(musicDataBuffer, 0, 32) != 0) {
                    musicData.AddRange(musicDataBuffer);
                }
                MusicData = musicData.ToArray();
            }
        }
    }
}