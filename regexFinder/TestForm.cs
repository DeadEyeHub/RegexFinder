using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace regexFinder
{
    public sealed class TestForm : Form
    {
        private readonly List<string> _fields;
        private readonly List<CheckDefinition> _checks = new();
        private readonly ComboBox _type = new();
        private readonly ComboBox _left = new();
        private readonly ComboBox _field = new();
        private readonly ComboBox _orderBy = new();
        private readonly ComboBox _previous = new();
        private readonly ComboBox _current = new();
        private readonly CheckedListBox _right = new();
        private readonly TextBox _name = new();
        private readonly TextBox _csvPath = new();
        private readonly TextBox _tolerance = new();
        private Label _leftLabel;
        private Label _rightLabel;
        private Label _orderLabel;
        private Label _previousLabel;
        private Label _currentLabel;
        private Label _toleranceLabel;
        private readonly ListBox _checkList = new();
        private readonly DataGridView _results = new();

        public TestForm(IEnumerable<string> fields)
        {
            _fields = fields.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToList();
            Text = "CSV Tests";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1100;
            Height = 700;
            _tolerance.Text = "0.01";
            BuildControls();
            FillFields();
        }

        private void BuildControls()
        {
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, ColumnCount = 4, RowCount = 6, Padding = new Padding(8) };
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
            _type.Items.AddRange(new object[] { "required", "comparison", "hashSequence", "sequence" });
            _type.SelectedIndex = 0;
            _type.SelectedIndexChanged += (_, _) => UpdateEditor();

            _leftLabel = Add(top, "Field / left", _left, 0, 2, 1);
            _orderLabel = Add(top, "Order by", _orderBy, 2, 2, 1);
            _previousLabel = Add(top, "Previous hash", _previous, 0, 3, 1);
            _currentLabel = Add(top, "Current hash", _current, 2, 3, 1);
            _toleranceLabel = Add(top, "Tolerance / step", _tolerance, 0, 4, 1);
            _rightLabel = Add(top, "Fields to sum", _right, 2, 4, 1);
            _right.Height = 55;

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
            top.Controls.Add(actions, 0, 5);
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

            var run = new Button { Text = "Run tests", Dock = DockStyle.Bottom, Height = 42 };
            run.Click += (_, _) => RunTests();
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 300 };
            split.Panel1.Controls.Add(_checkList);
            split.Panel2.Controls.Add(_results);
            Controls.Add(split);
            Controls.Add(run);
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
                _right.Items.Add(field);
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

            _leftLabel.Visible = _left.Visible = required || comparison || sequence;
            _rightLabel.Visible = _right.Visible = comparison;
            _orderLabel.Visible = _orderBy.Visible = hash || sequence;
            _previousLabel.Visible = _previous.Visible = hash;
            _currentLabel.Visible = _current.Visible = hash;
            _toleranceLabel.Visible = _tolerance.Visible = comparison || sequence;

            _leftLabel.Text = comparison ? "Left field" : "Field";
            _toleranceLabel.Text = sequence ? "Step" : "Tolerance";
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
                check.Right = _right.CheckedItems.Cast<string>().ToList();
                if (check.Right.Count == 0)
                {
                    MessageBox.Show("Select at least one field to sum.");
                    return;
                }
                check.Tolerance = ParseDouble(_tolerance.Text, 0.01);
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

            _checks.Add(check);
            _checkList.Items.Add(check.Name);
            _name.Clear();
        }

        private void BrowseCsv()
        {
            using var dialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) == DialogResult.OK) _csvPath.Text = dialog.FileName;
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
                var rows = CsvValidator.Load(_csvPath.Text);
                var failures = CsvValidator.Run(rows, _checks);
                _results.Rows.Clear();
                foreach (var failure in failures)
                {
                    var index = _results.Rows.Add(failure.CheckName, failure.Row, failure.Key, failure.Message);
                    _results.Rows[index].DefaultCellStyle.BackColor = Color.MistyRose;
                }
                if (failures.Count == 0)
                    MessageBox.Show($"All tests passed. Rows checked: {rows.Count}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSV test error: {ex.Message}");
            }
        }

        private static double ParseDouble(string text, double fallback) =>
            double.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }
}
