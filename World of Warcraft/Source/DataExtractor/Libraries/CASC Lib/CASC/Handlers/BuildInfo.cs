using System;
using System.Collections.Generic;
using System.IO;

namespace CASC_Lib.CASC.Handlers
{
    public class BuildInfo
    {
        public string this[string name]
        {
            get
            {
                string entry;

                if (entries.TryGetValue(name, out entry))
                    return entry;

                return null;
            }
        }

        Dictionary<string, string> entries = new Dictionary<string, string>();

        public BuildInfo(string file)
        {
            using (var sr = new StreamReader(file))
            {
                var header = sr.ReadLine().Split(new[] { '|', '!' });
                var data = sr.ReadLine().Split(new[] { '|' });

                if (data.Length != header.Length / 2)
                    throw new InvalidOperationException("bla...");

                for (var i = 0; i < data.Length; i++)
                    entries.Add(header[i << 1], data[i]);
            }
        }
    }
}
