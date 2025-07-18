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
                TextContent = File.ReadAllText(path, Encoding.UTF8);
            }
            else
            {
                throw new FileNotFoundException($"Text file not found: {path}");
            }
        }

     }
}
