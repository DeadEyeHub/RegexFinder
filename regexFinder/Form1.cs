using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace regexFinder
{
    public partial class Form1 : Form
    {
        public string bills = "";
        public string regex = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TextSplitter textSplitter = new TextSplitter();
            RegexFinder regexFinder = new RegexFinder();
            List<string> lines = regexFinder.SplitText(bills);
            List<string> regexPatterns = regexFinder.SplitText(regex);
            regexPatterns = regexFinder.Patterns;
            lines = regexFinder.Lines;
            Dictionary<string, List<string>> results = regexFinder.FindAllMatches();
            richTextBox1.Clear();
            foreach (var pattern in results.Keys)
            {
                richTextBox1.AppendText($"Pattern: {pattern}\n");
                int totalLines = results[pattern].Count;
                for (int lineNumber = 0; lineNumber < totalLines; lineNumber++)
                {
                    richTextBox1.AppendText($"Line {lineNumber + 1} from {totalLines} amount is now searched through\n");
                    richTextBox1.AppendText($"Result: {results[pattern][lineNumber]}\n");
                    Application.DoEvents(); // Keeps UI responsive for large files
                }
                richTextBox1.AppendText("\n");
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // This button is intended to load a text file
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        FileLoader fileLoader = new FileLoader();
                        fileLoader.LoadTextFile(filePath);
                        string bills = fileLoader.TextContent;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}");
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // This button is intended to load a text file
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        FileLoader fileLoader = new FileLoader();
                        fileLoader.LoadRegexFile(filePath);
                        string regex = fileLoader.TextContent;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}");
                    }
                }
            }
        }
    }
}
