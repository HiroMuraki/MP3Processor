using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playground.MP3;
using Playground.MP3.ID3V2;
using ID3V2Util = Playground.MP3.ID3V2.Util;

namespace Test {
    public class Program {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                ShowHelpInfo();
                return;
            }

            try {
                switch (args[0].ToLower()) {
                    case "-h":
                    case "-help":
                    case "/?":
                        ShowHelpInfo();
                        break;
                    case "-extract":
                    case "-e":
                        ExtractID3V2(args[1..]);
                        break;
                    case "-merge":
                    case "-m":
                        MergeID3V2(args[1..]);
                        break;
                    default:
                        ShowHelpInfo();
                        break;
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
        }

        private static void ShowHelpInfo() {
            Console.WriteLine(">>> 提取ID3V2信息：");
            Console.WriteLine("        语法：program.exe -extract mp3File");
            Console.WriteLine("           [1] program.exe - 程序名");
            Console.WriteLine("           [2] -extract - 可缩写为-e，表明操作为抽取");
            Console.WriteLine("           [3] file - 要处理的音频文件路径，若为*则处理同目录下的所有文件");
            Console.WriteLine();
            Console.WriteLine(">>> 合并ID3V2信息：");
            Console.WriteLine("        语法：program.exe -merge mp3File infoFile");
            Console.WriteLine("            [1] program.exe - 程序名");
            Console.WriteLine("            [2] -merge - 可缩写为-m，表明操作为合并");
            Console.WriteLine("            [3] mp3File - mp3音频文件路径");
            Console.WriteLine("            [4] infoFile - 储存了ID3V2信息的json");
            Console.WriteLine("                (1)：json格式参考提取出来的json");
            Console.WriteLine("                (2)：对于APIC即内嵌封面数据，在json中提供图片路径即可，如 \"APIC\":\"C:/Pictures/image.png\"");
            Console.WriteLine();
            Console.WriteLine(">>> 附录：ID3V2的ID含义参考");
            foreach (var typeID in ID3V2Util.TypeIDs) {
                string chName = ID3V2Util.GetIDType(typeID, Language.SChinese);
                string enName = ID3V2Util.GetIDType(typeID, Language.English);
                Console.WriteLine($"      {typeID} = {chName}({enName})");
            }
            return;
        }
        private static void MergeID3V2(string[] args) {
            if (args.Length == 0) {
                return;
            }
            string mp3File = args[0];
            string infoFile = args[1];

            var info = MP3Info.Load(mp3File);

            var id3Data = new ID3V2Data();
            id3Data.Version = info.ID3V2.Version;
            id3Data.Revision = info.ID3V2.Revision;
            id3Data.Flag = info.ID3V2.Flag;
            using (var reader = new StreamReader(infoFile)) {
                var attDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
                if (attDict == null) {
                    throw new ArgumentException("Invalid info file");
                }
                foreach (var item in attDict) {
                    // 特殊处理APIC数据，读取其值指向的文件
                    if (item.Key == "APIC") {
                        var frame = new ID3V2Frame() {
                            ID = item.Key
                        };
                        try {
                            using (var fs = new FileStream(item.Value, FileMode.Open, FileAccess.Read)) {
                                byte[] buffer = new byte[fs.Length];
                                fs.Read(buffer, 0, buffer.Length);
                                frame.Data = ID3V2Frame.GetEncodedImages(buffer);
                            }
                            id3Data.Frames.Add(frame);
                        }
                        catch (Exception e) {
                            Console.WriteLine($"Unable to read image，{e.Message}");
                        }
                        continue;
                    }

                    id3Data.Frames.Add(new ID3V2Frame() {
                        ID = item.Key,
                        Data = ID3V2Frame.GetEncodedString(item.Value, ID3V2Encoding.UTF16)
                    });
                }

            }

            string outputFile = Path.Combine(Path.GetDirectoryName(mp3File) ?? "", $"[merged] {Path.GetFileName(mp3File)}");
            ID3V2Writer.Write(id3Data, info.MusicData, outputFile);
            Console.WriteLine($"文件已输出至：{outputFile}");
        }
        private static void ExtractID3V2(string[] args) {
            if (args[0] == "*") {
                foreach (var fileName in Directory.GetFiles(Environment.CurrentDirectory)) {
                    if (Path.GetExtension(fileName) == ".mp3") {
                        Console.WriteLine($"正在处理：{fileName}");
                        ProcessFile(fileName, Path.GetFileNameWithoutExtension(fileName));
                    }
                }
            }
            else {
                ProcessFile(args[0], args.Length >= 2 ? args[1] : "");
            }

            // 文件处理
            static void ProcessFile(string fileName, string infoFileName) {
                if (string.IsNullOrWhiteSpace(infoFileName)) {
                    infoFileName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName));
                }

                MP3Info mp3Info = MP3Info.Load(fileName);

                var infoJson = new JObject();
                foreach (var frame in mp3Info.ID3V2.Frames) {
                    try {
                        infoJson[frame.ID] = ID3V2Util.GetStringFromBytes(frame);
                        if (frame.ID == "APIC") {
                            ID3V2Util.ExtractCoverImage(frame.Data, infoFileName);
                        }
                    }
                    catch {
                        Console.WriteLine($"ERROR：Unable to process {frame}");
                    }

                }
                using (var writer = new StreamWriter(infoFileName + ".json")) {
                    writer.Write(JsonConvert.SerializeObject(infoJson, Formatting.Indented));
                }
            }
        }
    }
}