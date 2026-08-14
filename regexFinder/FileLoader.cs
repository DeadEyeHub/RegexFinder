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
            if (!File.Exists(path))
                throw new FileNotFoundException($"Text file not found: {path}");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Encoding encoding = isUTF ? Encoding.UTF8 : Encoding.GetEncoding("windows-1257");
            Lines = File.ReadAllLines(path, encoding);
        }

    }
}
