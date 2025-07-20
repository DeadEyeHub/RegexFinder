using System;
using System.IO;
using System.Text;

namespace regexFinder
{
    internal class FileLoader
    {
        public string TextContent { get; private set; }
        public string RegexContent { get; private set; }

        public void LoadTextFile(string path)
        {
            if (File.Exists(path))
            {
                Encoding win1257 = Encoding.GetEncoding("windows-1257");
                TextContent = File.ReadAllText(path);
            }
            else
            {
                throw new FileNotFoundException($"Text file not found: {path}");
            }
        }

     }
}
