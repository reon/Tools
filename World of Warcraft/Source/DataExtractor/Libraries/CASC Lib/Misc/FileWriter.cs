using System.IO;

namespace CASC_Lib.Misc
{
    public class FileWriter
    {
        public static async void WriteFile(MemoryStream data, string path, FileMode fileMode = FileMode.Create)
        {
            using (var fs = new FileStream(path, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true))
                await fs.WriteAsync(data.ToArray(), 0, (int)data.Length);
        }
    }
}
