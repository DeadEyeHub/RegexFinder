using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace regexFinder
{
    public partial class Form1 : Form
    {
        public string[] _lines;
        public List<Regex> _regexList = new List<Regex>();
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();
        }

        private void bTransform_Click(object sender, EventArgs e)
        {
            RegexFinder regexFinder = new RegexFinder(_cancellationTokenSource.Token);
            regexFinder.Lines = _lines.ToList();
            var progress = new NotificationProgress(tbProgress, pbConverter);
            var results = regexFinder.FindAllMatches(_regexList, progress);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    CsvExporter csvExporter = new CsvExporter();                    
                    csvExporter.ExportResultToCsv(results, filePath);
                    MessageBox.Show($"Results exported to {filePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting results: {ex.Message}");
                }
            }
        }

        private void bBills_Click(object sender, EventArgs e)
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
                        fileLoader.LoadTextFile(filePath, false);
                        _lines = fileLoader.Lines;
                        textBox8.Text = $"Loaded Bills: {Path.GetFileName(filePath)} lines{_lines.Count()}"; // Set text in textBox2
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}");
                    }
                }
            }
        }

        private void bRegex_Click(object sender, EventArgs e)
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
                        fileLoader.LoadTextFile(filePath, true);
                        var regexList = new List<Regex>();
                        foreach (var pattern in fileLoader.Lines)
                        {
                            try
                            {
                                regexList.Add(new Regex(pattern));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error compiling regex '{pattern}': {ex.Message}");
                                MessageBox.Show($"Error compiling regex '{pattern}': {ex.Message}");
                                return;
                            }
                        }
                        _regexList = regexList;


                        textBox7.Text = $"Loaded Regex: {Path.GetFileName(filePath)} lines{_regexList.Count()}"; // Set text in textBox2
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}");
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
