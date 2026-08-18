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
                Lines = SplitLines(utf8.GetString(bytes));
            }
            catch (DecoderFallbackException)
            {
                Lines = Encoding.GetEncoding("windows-1257").GetString(bytes)
                    .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            }
        }

        private static string[] SplitLines(string text)
        {
            if (text.Length > 0 && text[0] == '\uFEFF')
                text = text[1..];

            return text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

    }
}
