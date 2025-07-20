using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            regexFinder.Lines = regexFinder.SplitText(bills);
            regexFinder.Patterns = regexFinder.SplitText(regex);
            var progress = new NotificationProgress(tbProgress, pbConverter);
            Dictionary<string, List<string>> results = regexFinder.FindAllMatches(progress);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    CsvExporter csvExporter = new CsvExporter();
                    csvExporter.ExportDictionaryToCsv(results, filePath);
                    MessageBox.Show($"Results exported to {filePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting results: {ex.Message}");
                }
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
                        bills = fileLoader.TextContent;
                        textBox8.Text = $"Loaded Bills: {Path.GetFileName(filePath)}"; // Set text in textBox2
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
                        fileLoader.LoadTextFile(filePath);
                        regex = fileLoader.TextContent;
                        textBox7.Text = $"Loaded Regex: {Path.GetFileName(filePath)}"; // Set text in textBox2
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
