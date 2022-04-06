using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground.MP3.ID3V2 {
    public static class ID3V2Writer {
        public static void Write(ID3V2Data data, byte[] musicData, string output) {
            using (var fs = new FileStream(output, FileMode.Create, FileAccess.Write)) {
                // 写入ID3V2信息
                foreach (var b in data.ToBytes()) {
                    fs.WriteByte(b);
                }
                // 写入音频数据
                foreach (var b in musicData) {
                    fs.WriteByte(b);
                }
            }
        }
    }
}
