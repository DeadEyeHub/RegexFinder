using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace regexFinder
{
    public sealed class TestForm : Form
    {
        private readonly List<string> _fields;
        private readonly IReadOnlyList<PatternDefinition> _patterns;
        private readonly IReadOnlyList<string> _sourceLines;
        private readonly List<CheckDefinition> _checks = new();
        private readonly ComboBox _type = new();
        private readonly ComboBox _left = new();
        private readonly ComboBox _field = new();
        private readonly ComboBox _orderBy = new();
        private readonly ComboBox _previous = new();
        private readonly ComboBox _current = new();
        private readonly CheckedListBox _availableFields = new();
        private readonly CheckedListBox _ignoredTypes = new();
        private readonly ListBox _terms = new();
        private readonly TextBox _name = new();
        private readonly TextBox _csvPath = new();
        private readonly TextBox _tolerance = new();
        private Label _leftLabel;
        private Label _orderLabel;
        private Label _previousLabel;
        private Label _currentLabel;
        private Label _toleranceLabel;
        private Label _rightLabel;
        private Panel _rightPanel;
        private Label _termsLabel;
        private Label _ignoredTypesLabel;
        private readonly ListBox _checkList = new();
        private readonly DataGridView _results = new();
        private readonly Button _exportFailures = new() { Text = "Export failed", Width = 130, Enabled = false };
        private CsvDocument _lastDocument;
        private List<CheckResult> _lastFailures = new();
        private SourceCheckIndex _sourceIndex;

        public TestForm(
            IEnumerable<string> fields,
            string initialCsvPath = null,
            IReadOnlyList<PatternDefinition> patterns = null,
            IReadOnlyList<string> sourceLines = null)
        {
            _fields = fields.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToList();
            _patterns = patterns ?? Array.Empty<PatternDefinition>();
            _sourceLines = sourceLines ?? Array.Empty<string>();
            _sourceIndex = new SourceCheckIndex(_sourceLines, _patterns);
            Text = "CSV Tests";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1300;
            Height = 780;
            _tolerance.Text = "0.01";
            _csvPath.Text = initialCsvPath ?? FindLatestCsv();
            BuildControls();
            FillFields();
            LoadReceiptTypes(_csvPath.Text);
        }

        private void BuildControls()
        {
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 480, ColumnCount = 4, RowCount = 8, Padding = new Padding(8) };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            Add(top, "CSV file", _csvPath, 0, 0, 2);
            var browse = new Button { Text = "Browse...", Dock = DockStyle.Fill };
            browse.Click += (_, _) => BrowseCsv();
            top.Controls.Add(browse, 3, 0);

            Add(top, "Test name", _name, 0, 1, 1);
            Add(top, "Test type", _type, 2, 1, 1);
            _type.DropDownStyle = ComboBoxStyle.DropDownList;
            _type.Items.AddRange(new object[] { "required", "comparison", "hashSequence", "sequence", "grandTotalReconciliation" });
            _type.SelectedIndex = 0;
            _type.SelectedIndexChanged += (_, _) => UpdateEditor();

            _leftLabel = Add(top, "Field / left", _left, 0, 2, 1);
            _orderLabel = Add(top, "Order by", _orderBy, 2, 2, 1);
            _previousLabel = Add(top, "Previous hash", _previous, 0, 3, 1);
            _currentLabel = Add(top, "Current hash", _current, 2, 3, 1);
            _toleranceLabel = Add(top, "Tolerance / step", _tolerance, 0, 4, 1);
            _rightLabel = new Label { Text = "Available fields", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            top.Controls.Add(_rightLabel, 2, 4);
            _rightPanel = new Panel { Dock = DockStyle.Fill };
            var addSource = new Button { Text = "+ Add field", Width = 110 };
            addSource.Click += (_, _) => AddSourceField(false);
            var removeSource = new Button { Text = "- Remove field", Width = 120 };
            removeSource.Click += (_, _) => AddSourceField(true);
            var sourceToolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32, WrapContents = false };
            sourceToolbar.Controls.Add(new Label { Text = "Select fields below, then:", AutoSize = true, Padding = new Padding(0, 7, 8, 0) });
            sourceToolbar.Controls.Add(addSource);
            sourceToolbar.Controls.Add(removeSource);
            _availableFields.Dock = DockStyle.Fill;
            _availableFields.Height = 145;
            _rightPanel.Controls.Add(_availableFields);
            _rightPanel.Controls.Add(sourceToolbar);
            top.Controls.Add(_rightPanel, 3, 4);
            _termsLabel = new Label { Text = "Formula terms", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            top.Controls.Add(_termsLabel, 2, 5);
            _terms.Dock = DockStyle.Fill;
            _terms.Height = 75;
            top.Controls.Add(_terms, 3, 5);
            _ignoredTypesLabel = new Label { Text = "Ignore receipt types", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            top.Controls.Add(_ignoredTypesLabel, 0, 5);
            _ignoredTypes.Dock = DockStyle.Fill;
            _ignoredTypes.Height = 75;
            top.Controls.Add(_ignoredTypes, 1, 5);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            var add = new Button { Text = "Add test", Width = 110 };
            add.Click += (_, _) => AddTest();
            var remove = new Button { Text = "Remove", Width = 90 };
            remove.Click += (_, _) => RemoveTest();
            var save = new Button { Text = "Save tests", Width = 100 };
            save.Click += (_, _) =>
            {
                try { SaveTests(); }
                catch (Exception ex) { MessageBox.Show($"Cannot save tests: {ex.Message}"); }
            };
            var load = new Button { Text = "Load tests", Width = 100 };
            load.Click += (_, _) =>
            {
                try { LoadTests(); }
                catch (Exception ex) { MessageBox.Show($"Cannot load tests: {ex.Message}"); }
            };
            actions.Controls.Add(add);
            actions.Controls.Add(remove);
            actions.Controls.Add(save);
            actions.Controls.Add(load);
            top.Controls.Add(actions, 0, 7);
            top.SetColumnSpan(actions, 4);

            _checkList.Dock = DockStyle.Fill;
            _results.Dock = DockStyle.Fill;
            _results.ReadOnly = true;
            _results.AllowUserToAddRows = false;
            _results.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _results.Columns.Add("Check", "Check");
            _results.Columns.Add("Row", "Row");
            _results.Columns.Add("Key", "Key");
            _results.Columns.Add("Message", "Message");

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, WrapContents = false };
            var run = new Button { Text = "Run tests", Width = 120, Height = 42 };
            run.Click += (_, _) => RunTests();
            _exportFailures.Click += (_, _) => ExportFailures();
            bottom.Controls.Add(run);
            bottom.Controls.Add(_exportFailures);
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 300 };
            split.Panel1.Controls.Add(_checkList);
            split.Panel2.Controls.Add(_results);
            Controls.Add(split);
            Controls.Add(bottom);
            Controls.Add(top);
            UpdateEditor();
        }

        private void FillFields()
        {
            foreach (var field in _fields)
            {
                _left.Items.Add(field);
                _field.Items.Add(field);
                _orderBy.Items.Add(field);
                _previous.Items.Add(field);
                _current.Items.Add(field);
                _availableFields.Items.Add(field);
            }
            if (_left.Items.Count > 0)
            {
                _left.SelectedIndex = 0;
                _field.SelectedIndex = 0;
                _orderBy.SelectedIndex = 0;
                _previous.SelectedIndex = 0;
                _current.SelectedIndex = 0;
            }
        }

        private static Label Add(TableLayoutPanel panel, string label, Control control, int column, int row, int span)
        {
            var labelControl = new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            panel.Controls.Add(labelControl, column, row);
            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, column + 1, row);
            if (span > 1) panel.SetColumnSpan(control, span);
            return labelControl;
        }

        private void UpdateEditor()
        {
            var type = _type.Text;
            var required = type == "required";
            var comparison = type == "comparison";
            var hash = type == "hashSequence";
            var sequence = type == "sequence";
            var grandTotal = type == "grandTotalReconciliation";

            _leftLabel.Visible = _left.Visible = required || comparison || sequence || grandTotal;
            _rightLabel.Visible = _rightPanel.Visible = _termsLabel.Visible = _terms.Visible = comparison;
            _ignoredTypesLabel.Visible = _ignoredTypes.Visible = comparison || grandTotal;
            _orderLabel.Visible = _orderBy.Visible = hash || sequence || grandTotal;
            _previousLabel.Visible = _previous.Visible = hash || grandTotal;
            _currentLabel.Visible = _current.Visible = hash || grandTotal;
            _toleranceLabel.Visible = _tolerance.Visible = comparison || sequence || grandTotal;

            _leftLabel.Text = comparison ? "Field to compare" : grandTotal ? "Amount field" : "Field";
            _orderLabel.Text = grandTotal ? "Total field" : "Order by";
            _previousLabel.Text = grandTotal ? "Receipt type field" : "Previous hash";
            _currentLabel.Text = grandTotal ? "Exclude if nonzero" : "Current hash";
            _ignoredTypesLabel.Text = grandTotal ? "Included receipt types" : "Ignore receipt types";
            _toleranceLabel.Text = sequence ? "Step" : "Tolerance";
        }

        private void AddSourceField(bool subtract)
        {
            var fields = _availableFields.CheckedItems.Cast<string>().ToList();
            foreach (var field in fields)
            {
                var prefix = subtract ? "-" : "+";
                _terms.Items.Add($"{prefix} {field}");
                _availableFields.SetItemChecked(_availableFields.Items.IndexOf(field), false);
            }
        }

        private void AddTest()
        {
            if (string.IsNullOrWhiteSpace(_name.Text))
            {
                MessageBox.Show("Enter a test name.");
                return;
            }

            var check = new CheckDefinition { Name = _name.Text.Trim(), Type = _type.Text };
            if (_type.Text == "required" || _type.Text == "sequence") check.Field = _left.Text;
            if (_type.Text == "comparison")
            {
                check.Left = _left.Text;
                foreach (var term in _terms.Items.Cast<string>())
                {
                    if (term.StartsWith("+ ", StringComparison.Ordinal)) check.Right.Add(term[2..]);
                    if (term.StartsWith("- ", StringComparison.Ordinal)) check.Subtract.Add(term[2..]);
                }
                if (check.Right.Count == 0 && check.Subtract.Count == 0)
                {
                    MessageBox.Show("Add at least one field to the formula.");
                    return;
                }
                check.Tolerance = ParseDouble(_tolerance.Text, 0.01);
                check.IgnoreReceiptTypes = _ignoredTypes.CheckedItems.Cast<string>().ToList();
            }
            if (_type.Text == "hashSequence")
            {
                check.OrderBy = _orderBy.Text;
                check.PreviousField = _previous.Text;
                check.CurrentField = _current.Text;
            }
            if (_type.Text == "sequence")
            {
                check.OrderBy = _orderBy.Text;
                check.Step = ParseDouble(_tolerance.Text, 1);
                check.Tolerance = 0.01;
            }
            if (_type.Text == "grandTotalReconciliation")
            {
                check.AmountField = _left.Text;
                check.TotalField = _orderBy.Text;
                check.ReceiptTypeField = _previous.Text;
                check.IncludedReceiptTypes = _ignoredTypes.CheckedItems.Cast<string>().ToList();
                check.ExcludeIfField = _current.Text;
                check.ExcludeIfNonZero = !string.IsNullOrWhiteSpace(check.ExcludeIfField);
                check.Tolerance = ParseDouble(_tolerance.Text, 0.01);
            }

            _checks.Add(check);
            _checkList.Items.Add(check.Name);
            _name.Clear();
        }

        private void BrowseCsv()
        {
            using var dialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _csvPath.Text = dialog.FileName;
                LoadReceiptTypes(_csvPath.Text);
            }
        }

        private void LoadReceiptTypes(string path)
        {
            if (!File.Exists(path)) return;
            var rows = CsvValidator.Load(path);
            var types = rows.Select(row => row.TryGetValue("Ceka tips", out var value) ? value : string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var selected = _ignoredTypes.CheckedItems.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
            _ignoredTypes.Items.Clear();
            foreach (var type in types) _ignoredTypes.Items.Add(type, selected.Contains(type));
        }

        private void RemoveTest()
        {
            if (_checkList.SelectedIndex < 0) return;
            var index = _checkList.SelectedIndex;
            _checks.RemoveAt(index);
            _checkList.Items.RemoveAt(index);
        }

        private void SaveTests()
        {
            using var dialog = new SaveFileDialog { Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*", DefaultExt = "yaml", AddExtension = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(new Dictionary<string, List<CheckDefinition>> { ["checks"] = _checks });
            File.WriteAllText(dialog.FileName, yaml);
        }

        private void LoadTests()
        {
            using var dialog = new OpenFileDialog { Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var yaml = File.ReadAllText(dialog.FileName);
            var data = deserializer.Deserialize<Dictionary<string, List<CheckDefinition>>>(yaml);
            if (data == null || !data.TryGetValue("checks", out var checks))
                throw new InvalidDataException("The YAML file does not contain a checks section.");

            _checks.Clear();
            _checks.AddRange(checks);
            _checkList.Items.Clear();
            foreach (var check in _checks) _checkList.Items.Add(check.Name);
        }

        private static string FindLatestCsv()
        {
            var directories = new List<string>();
            var current = new DirectoryInfo(Application.StartupPath);
            for (var i = 0; i < 5 && current != null; i++, current = current.Parent)
                directories.Add(Path.Combine(current.FullName, "results"));

            return directories.SelectMany(directory =>
                    Directory.Exists(directory)
                        ? Directory.EnumerateFiles(directory, "*.csv")
                        : Enumerable.Empty<string>())
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault() ?? string.Empty;
        }

        private void RunTests()
        {
            if (_checks.Count == 0)
            {
                MessageBox.Show("Add at least one test.");
                return;
            }
            if (!File.Exists(_csvPath.Text))
            {
                BrowseCsv();
                if (!File.Exists(_csvPath.Text)) return;
            }

            try
            {
                _lastDocument = CsvValidator.LoadDocument(_csvPath.Text);
                _lastFailures = CsvValidator.Run(_lastDocument.Rows, _checks);
                _results.Rows.Clear();
                foreach (var failure in _lastFailures)
                {
                    var index = _results.Rows.Add(failure.CheckName, failure.Row, failure.Key, failure.Message);
                    _results.Rows[index].DefaultCellStyle.BackColor = Color.MistyRose;
                }
                _exportFailures.Enabled = _lastFailures.Count > 0;
                if (_lastFailures.Count == 0)
                    MessageBox.Show($"All tests passed. Rows checked: {_lastDocument.Rows.Count}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSV test error: {ex.Message}");
            }
        }

        private void ExportFailures()
        {
            if (_lastDocument == null || _lastFailures.Count == 0) return;
            using var dialog = new FolderBrowserDialog { Description = "Select a folder for failed test files" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            foreach (var group in _lastFailures.GroupBy(failure => failure.CheckName))
            {
                var keys = group.SelectMany(failure => failure.RelatedKeys ?? new())
                    .Concat(group.Select(failure => failure.Key))
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var rows = _lastDocument.Rows
                    .Where(row => keys.Contains(GetRowKey(row), StringComparer.OrdinalIgnoreCase))
                    .ToList();
                var safeName = string.Join("_", group.Key.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                if (string.IsNullOrWhiteSpace(safeName)) safeName = "failed_test";

                var csvRows = new List<List<string>> { _lastDocument.Headers };
                csvRows.AddRange(rows.Select(row => _lastDocument.Headers.Select(header =>
                    row.TryGetValue(header, out var value) ? value : string.Empty).ToList()));
                new CsvExporter().ExportResultToCsv(csvRows, Path.Combine(dialog.SelectedPath, safeName + ".csv"));

                var textLines = _sourceIndex.GetBlocks(keys).SelectMany(block => block.Concat(new[] { string.Empty })).ToArray();
                File.WriteAllLines(Path.Combine(dialog.SelectedPath, safeName + ".txt"), textLines, Encoding.UTF8);
            }

            MessageBox.Show($"Exported {_lastFailures.GroupBy(failure => failure.CheckName).Count()} failed test file sets.");
        }

        private static string GetRowKey(Dictionary<string, string> row) =>
            row.TryGetValue("Ceka numurs", out var key) ? key : string.Empty;

        private static double ParseDouble(string text, double fallback) =>
            double.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }
}
