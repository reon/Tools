using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ClientDBExtractor
{
    internal class CDNHandler
    {
        static readonly ByteArrayComparer comparer = new ByteArrayComparer();
        Dictionary<byte[], IndexEntry> CDNIndexData = new Dictionary<byte[], IndexEntry>(comparer);

        private CASCConfig CASCConfig;

        private CDNHandler(CASCConfig cascConfig)
        {
            CASCConfig = cascConfig;
        }

        public static CDNHandler Initialize(CASCConfig config)
        {
            var handler = new CDNHandler(config);

            for (int i = 0; i < config.Archives.Count; i++)
            { 
                string index = config.Archives[i];

                handler.OpenFile(index, i);
            }

            Logger.WriteLine("CDNHandler: loaded {0} indexes", handler.CDNIndexData.Count);
            return handler;
        }

        private void ParseIndex(Stream stream, int i)
        {
            using (var br = new BinaryReader(stream))
            {
                stream.Seek(-12, SeekOrigin.End);
                int count = br.ReadInt32();
                stream.Seek(0, SeekOrigin.Begin);

                for (int j = 0; j < count; ++j)
                {
                    byte[] key = br.ReadBytes(16);

                    if (key.IsZeroed()) // wtf?
                        key = br.ReadBytes(16);

                    if (key.IsZeroed()) // wtf?
                        throw new Exception("key.IsZeroed()");

                    IndexEntry entry = new IndexEntry();
                    entry.Index = i;
                    entry.Size = br.ReadInt32BE();
                    entry.Offset = br.ReadInt32BE();

                    CDNIndexData.Add(key, entry);
                }
            }
        }

        private void DownloadFile(string index, int i)
        {
            if (!Directory.Exists("data\\indices\\"))
                Directory.CreateDirectory("data\\indices\\");

            var path = "data\\indices\\" + index + ".index";

            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    ParseIndex(fs, i);
                }
                return;
            }

            try
            {
                var url = CASCConfig.CDNUrl + "/data/" + index.Substring(0, 2) + "/" + index.Substring(2, 2) + "/" + index + ".index";

                using (WebClient webClient = new WebClient())
                using (Stream s = webClient.OpenRead(url))
                using (MemoryStream ms = new MemoryStream())
                using (FileStream fs = File.Create(path))
                {
                    s.CopyTo(ms);
                    ms.Position = 0;
                    ms.CopyTo(fs);

                    ParseIndex(ms, i);
                }
            }
            catch
            {
                throw new Exception("DownloadFile failed!");
            }
        }

        private void OpenFile(string index, int i)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory + "\\", "Data\\indices\\", index + ".index");

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    ParseIndex(fs, i);
                }
            }
            catch
            {
                throw new Exception("OpenFile failed!");
            }
        }

        public IndexEntry GetCDNIndexInfo(byte[] key)
        {
            IndexEntry result;
            if (!CDNIndexData.TryGetValue(key, out result))
                Logger.WriteLine("CDNHandler: missing index: {0}", key.ToHexString());

            return result;
        }
    }
}
