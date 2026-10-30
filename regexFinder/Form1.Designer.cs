using System.Drawing;
using System.Windows.Forms;

namespace regexFinder
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Font = new Font("Segoe UI", 10F);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = AppTheme.Canvas;
            ForeColor = AppTheme.Ink;
            ClientSize = new Size(920, 650);
            MinimumSize = new Size(740, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Name = "Form1";
            Text = "regexFinder";

            var page = new TableLayoutPanel {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7,
                Padding = new Padding(32, 24, 32, 16), AutoScroll = true
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var heading = AppTheme.Label("Cash register bills to CSV converter", 22, true);
            heading.Margin = new Padding(0, 0, 0, 10);
            page.Controls.Add(heading, 0, 0);
            var instructions = AppTheme.Label("Load your TXT file and YAML rules, then transform and save the CSV.");
            instructions.Margin = new Padding(0, 0, 0, 24);
            page.Controls.Add(instructions, 0, 1);

            bBills = new Button { Name = "bBills", Text = "&Upload cash register bills", TabIndex = 0 };
            bRegex = new Button { Name = "bRegex", Text = "Upload &Regex commands", TabIndex = 0 };
            textBox8 = FileStatus("textBox8", "No TXT file loaded", "Loaded cash register file");
            textBox7 = FileStatus("textBox7", "No YAML file loaded", "Loaded extraction rules");
            UTF8 = new CheckBox {
                Name = "UTF8", Text = "UTF&8", Checked = true, AutoSize = true,
                Margin = new Padding(0, 12, 0, 0), TabIndex = 1,
                AccessibleDescription = "Use UTF-8 with Windows-1257 fallback when loading the text file."
            };
            page.Controls.Add(InputSection("1. Source text", bBills, textBox8, UTF8), 0, 2);
            page.Controls.Add(InputSection("2. Extraction rules", bRegex, textBox7, null), 0, 3);

            var output = new TableLayoutPanel {
                AutoSize = true, Dock = DockStyle.Top, ColumnCount = 1, RowCount = 4,
                Margin = new Padding(0, 8, 0, 0)
            };
            output.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            output.Controls.Add(AppTheme.Label("3. Transform and save", 12, true), 0, 0);
            tbProgress = new TextBox {
                Name = "tbProgress", ReadOnly = true, BorderStyle = BorderStyle.None,
                BackColor = AppTheme.Canvas, ForeColor = AppTheme.Muted, Dock = DockStyle.Fill,
                Text = "Ready when both files are loaded.", TabStop = false,
                AccessibleName = "Conversion progress", Margin = new Padding(0, 2, 0, 10)
            };
            output.Controls.Add(tbProgress, 0, 1);
            pbConverter = new ProgressBar {
                Name = "pbConverter", Dock = DockStyle.Fill, Height = 12, Maximum = 10000,
                Margin = new Padding(0, 0, 0, 16), AccessibleName = "Conversion progress"
            };
            output.Controls.Add(pbConverter, 0, 2);
            var actions = new FlowLayoutPanel {
                Dock = DockStyle.Fill, AutoSize = true, Margin = Padding.Empty,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true
            };
            bTransform = new Button { Name = "bTransform", Text = "&Transform", Margin = new Padding(0, 0, 12, 0), TabIndex = 0 };
            bTests = new Button { Name = "bTests", Text = "&Tests", Margin = Padding.Empty, TabIndex = 1 };
            AppTheme.StyleButton(bTransform, true);
            AppTheme.StyleButton(bTests);
            actions.Controls.Add(bTransform);
            actions.Controls.Add(bTests);
            output.Controls.Add(actions, 0, 3);
            page.Controls.Add(output, 0, 4);
            buildInfoLabel = AppTheme.Label("");
            buildInfoLabel.Name = "buildInfoLabel";
            buildInfoLabel.Font = new Font("Segoe UI", 9F);
            buildInfoLabel.Margin = new Padding(0, 20, 0, 0);
            page.Controls.Add(buildInfoLabel, 0, 6);
            Controls.Add(page);

            bBills.Click += bBills_Click;
            bRegex.Click += bRegex_Click;
            bTransform.Click += bTransform_Click;
            bTests.Click += bTests_Click;
            FormClosing += Form1_FormClosing;
            ResumeLayout(true);
        }

        private static TextBox FileStatus(string name, string text, string accessibleName)
        {
            return new TextBox {
                Name = name, Text = text, ReadOnly = true, TabStop = false,
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White, ForeColor = AppTheme.Muted,
                AccessibleName = accessibleName, Margin = new Padding(0, 4, 0, 4)
            };
        }

        private static TableLayoutPanel InputSection(string title, Button browse, TextBox status, Control option)
        {
            var section = new TableLayoutPanel {
                Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 3,
                Margin = new Padding(0, 0, 0, 24)
            };
            section.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            section.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var label = AppTheme.Label(title, 12, true);
            section.Controls.Add(label, 0, 0);
            section.SetColumnSpan(label, 2);
            section.Controls.Add(status, 0, 1);
            AppTheme.StyleButton(browse);
            browse.MinimumSize = new Size(196, 32);
            browse.Margin = new Padding(16, 0, 0, 0);
            section.Controls.Add(browse, 1, 1);
            if (option != null) { section.Controls.Add(option, 0, 2); section.SetColumnSpan(option, 2); }
            return section;
        }

        private Button bTransform;
        private Button bBills;
        private Button bRegex;
        private Button bTests;
        private TextBox textBox7;
        private TextBox textBox8;
        private TextBox tbProgress;
        private ProgressBar pbConverter;
        private CheckBox UTF8;
        private Label buildInfoLabel;
    }
}
