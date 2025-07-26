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
        private RegexFinder regexFinder;
        private List<PatternDefinition> _patterns;
        public bool _isUTF8 = true; // По умолчанию UTF-8
        public Form1()
        {
            InitializeComponent();
        }

        private void bTransform_Click(object sender, EventArgs e)
        {
            RegexFinder regexFinder = new RegexFinder(_cancellationTokenSource.Token);
            regexFinder.Lines = _lines.ToList();
            regexFinder.Patterns = _patterns;
            var progress = new NotificationProgress(tbProgress, pbConverter);
            var results = regexFinder.FindAllMatches(progress);
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
                        fileLoader.LoadTextFile(filePath, UTF8.Checked);
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
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "YAML files (*.yaml;*.yml)|*.yaml;*.yml|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;

                        PatternLoader patternLoader = new PatternLoader();
                        _patterns = patternLoader.LoadPatterns(filePath); // сохранить в поле

                        // Если RegexFinder уже существует
                        if (regexFinder != null)
                        {
                            regexFinder.Patterns = _patterns;
                        }

                        // Для совместимости, если используется старый _regexList
                        _regexList = _patterns
                            .Where(p => !string.IsNullOrWhiteSpace(p.RegexCommand))
                            .Select(p => new Regex(p.RegexCommand))
                            .ToList();

                        textBox7.Text = $"Loaded YAML: {Path.GetFileName(filePath)} patterns: {_regexList.Count}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading YAML file: {ex.Message}");
                    }
                }
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private void UTF8_CheckedChanged(object sender, EventArgs e)
        {
            if (!UTF8.Checked)
            {
                _isUTF8 = false;
            }
        }
    }
}
