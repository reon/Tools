using System.IO;

namespace CASC_Lib.Misc
{
    public class FileWriter
    {
        public static async void WriteFile(MemoryStream data, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true))
                await fs.WriteAsync(data.ToArray(), 0, (int)data.Length);
        }
    }
}
