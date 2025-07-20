using System;
using System.IO;
using System.Text;

namespace regexFinder
{
    internal class FileLoader
    {
        public string[] Lines { get; private set; }

        public void LoadTextFile(string path, bool isUTF)
        {
            if (File.Exists(path))
            {
                Encoding win1257 = Encoding.GetEncoding("windows-1257");
                Lines = File.ReadAllLines(path,  isUTF ?  Encoding.UTF8: win1257);
            }
            else
            {
                throw new FileNotFoundException($"Text file not found: {path}");
            }
        }

     }
}
