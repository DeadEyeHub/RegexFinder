using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace regexFinder
{
    public partial class Form1 : Form
    {
        public string[] _lines;
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private List<PatternDefinition> _patternList;
        private List<BlockDefinition> _blockList;
        public Form1()
        {
            InitializeComponent();
            var buildTime = File.GetLastWriteTime(Application.ExecutablePath);
            Text = $"regexFinder {Application.ProductVersion}";
            buildInfoLabel.Text = $"Version {Application.ProductVersion} | build {buildTime:yyyy-MM-dd HH:mm:ss}";
        }

        private void bTransform_Click(object sender, EventArgs e)
        {
            if (_lines == null || _lines.Length == 0)
            {
                MessageBox.Show("Load a text file before transforming.");
                return;
            }

            if (_patternList == null || !_patternList.Any(p =>
                    string.Equals(p?.Name, "Splitter", StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Load a YAML file containing a Splitter pattern before transforming.");
                return;
            }

            try
            {
                var finder = new RegexFinder(_cancellationTokenSource.Token)
                {
                    Lines = _lines.ToList(),
                    Patterns = _patternList,
                    Blocks = _blockList ?? new List<BlockDefinition>()
                };

                var progress = new NotificationProgress(tbProgress, pbConverter);
                var results = finder.FindAlChecks(progress);
                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    new CsvExporter().ExportResultToCsv(results, saveFileDialog.FileName);
                    MessageBox.Show($"Results exported to {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file: {ex.Message}");
            }
        }

        private void bBills_Click(object sender, EventArgs e)
        {
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
                        textBox8.Text = $"Loaded bills: {Path.GetFileName(filePath)}; lines: {_lines.Length}";
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
                        var results = YamlRoot.LoadParts(filePath);
                        _blockList = results.Blocks;
                        _patternList = results.Patterns;

                        textBox7.Text = $"Loaded YAML: {Path.GetFileName(filePath)} patterns: {_patternList.Count}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading YAML file: {ex.Message}");
                    }
                }
            }
        }

        private void bTests_Click(object sender, EventArgs e)
        {
            var fields = _patternList?.Select(p => p.Name).Where(n => !string.Equals(n, "Splitter", StringComparison.OrdinalIgnoreCase))
                ?? Enumerable.Empty<string>();
            using var testForm = new TestForm(fields, patterns: _patternList, sourceLines: _lines);
            testForm.ShowDialog(this);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

    }
}
