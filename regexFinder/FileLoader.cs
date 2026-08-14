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

            var bytes = File.ReadAllBytes(path);
            if (!isUTF)
            {
                Lines = Encoding.GetEncoding("windows-1257").GetString(bytes)
                    .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                return;
            }

            try
            {
                var utf8 = new UTF8Encoding(false, true);
                Lines = utf8.GetString(bytes)
                    .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            }
            catch (DecoderFallbackException)
            {
                Lines = Encoding.GetEncoding("windows-1257").GetString(bytes)
                    .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            }
        }

    }
}
