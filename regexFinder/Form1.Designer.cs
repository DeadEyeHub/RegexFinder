namespace regexFinder
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new System.Windows.Forms.Label();
            bTransform = new System.Windows.Forms.Button();
            bBills = new System.Windows.Forms.Button();
            bRegex = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            textBox3 = new System.Windows.Forms.TextBox();
            textBox4 = new System.Windows.Forms.TextBox();
            textBox5 = new System.Windows.Forms.TextBox();
            textBox6 = new System.Windows.Forms.TextBox();
            textBox7 = new System.Windows.Forms.TextBox();
            textBox8 = new System.Windows.Forms.TextBox();
            tbProgress = new System.Windows.Forms.TextBox();
            pbConverter = new System.Windows.Forms.ProgressBar();
            UTF8 = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("MingLiU_HKSCS-ExtB", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            label1.Location = new System.Drawing.Point(91, 160);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(1031, 53);
            label1.TabIndex = 0;
            label1.Text = "Cash register bills to CSV converter";
            // 
            // bTransform
            // 
            bTransform.Location = new System.Drawing.Point(973, 748);
            bTransform.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            bTransform.Name = "bTransform";
            bTransform.Size = new System.Drawing.Size(124, 44);
            bTransform.TabIndex = 1;
            bTransform.Text = "Transform";
            bTransform.UseVisualStyleBackColor = true;
            bTransform.Click += bTransform_Click;
            // 
            // bBills
            // 
            bBills.Location = new System.Drawing.Point(70, 748);
            bBills.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            bBills.Name = "bBills";
            bBills.Size = new System.Drawing.Size(233, 44);
            bBills.TabIndex = 2;
            bBills.Text = "Upload cash register bills";
            bBills.UseVisualStyleBackColor = true;
            bBills.Click += bBills_Click;
            // 
            // bRegex
            // 
            bRegex.Location = new System.Drawing.Point(483, 748);
            bRegex.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            bRegex.Name = "bRegex";
            bRegex.Size = new System.Drawing.Size(243, 44);
            bRegex.TabIndex = 3;
            bRegex.Text = "Upload Regex commands";
            bRegex.UseVisualStyleBackColor = true;
            bRegex.Click += bRegex_Click;
            // 
            // textBox1
            // 
            textBox1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            textBox1.Location = new System.Drawing.Point(573, 232);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(213, 44);
            textBox1.TabIndex = 4;
            textBox1.Text = "How to use:";
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(153, 342);
            textBox2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(535, 31);
            textBox2.TabIndex = 5;
            textBox2.Text = "1. Transform each file, that each bill or command occupies one line";
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(153, 394);
            textBox3.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(535, 31);
            textBox3.TabIndex = 6;
            textBox3.Text = "2. Upload each file by pressing supposed button";
            // 
            // textBox4
            // 
            textBox4.Location = new System.Drawing.Point(153, 446);
            textBox4.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox4.Name = "textBox4";
            textBox4.Size = new System.Drawing.Size(164, 31);
            textBox4.TabIndex = 7;
            textBox4.Text = "3. Press transform";
            // 
            // textBox5
            // 
            textBox5.Location = new System.Drawing.Point(153, 496);
            textBox5.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox5.Name = "textBox5";
            textBox5.Size = new System.Drawing.Size(164, 31);
            textBox5.TabIndex = 8;
            textBox5.Text = "4. Wait";
            // 
            // textBox6
            // 
            textBox6.Location = new System.Drawing.Point(153, 546);
            textBox6.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox6.Name = "textBox6";
            textBox6.Size = new System.Drawing.Size(194, 31);
            textBox6.TabIndex = 9;
            textBox6.Text = "5. Save your CSV file";
            // 
            // textBox7
            // 
            textBox7.Location = new System.Drawing.Point(524, 698);
            textBox7.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox7.Name = "textBox7";
            textBox7.Size = new System.Drawing.Size(164, 31);
            textBox7.TabIndex = 11;
            // 
            // textBox8
            // 
            textBox8.Location = new System.Drawing.Point(102, 698);
            textBox8.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            textBox8.Name = "textBox8";
            textBox8.Size = new System.Drawing.Size(164, 31);
            textBox8.TabIndex = 12;
            // 
            // tbProgress
            // 
            tbProgress.Location = new System.Drawing.Point(954, 270);
            tbProgress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            tbProgress.Name = "tbProgress";
            tbProgress.ReadOnly = true;
            tbProgress.Size = new System.Drawing.Size(251, 31);
            tbProgress.TabIndex = 13;
            tbProgress.TabStop = false;
            // 
            // pbConverter
            // 
            pbConverter.Location = new System.Drawing.Point(213, 630);
            pbConverter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pbConverter.Maximum = 10000;
            pbConverter.Name = "pbConverter";
            pbConverter.Size = new System.Drawing.Size(926, 36);
            pbConverter.TabIndex = 14;
            // 
            // UTF8
            // 
            UTF8.AutoSize = true;
            UTF8.Location = new System.Drawing.Point(70, 801);
            UTF8.Name = "UTF8";
            UTF8.Size = new System.Drawing.Size(78, 29);
            UTF8.TabIndex = 15;
            UTF8.Text = "UTF8";
            UTF8.UseVisualStyleBackColor = true;
            UTF8.CheckedChanged += UTF8_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1333, 865);
            Controls.Add(UTF8);
            Controls.Add(pbConverter);
            Controls.Add(tbProgress);
            Controls.Add(textBox8);
            Controls.Add(textBox7);
            Controls.Add(textBox6);
            Controls.Add(textBox5);
            Controls.Add(textBox4);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(bRegex);
            Controls.Add(bBills);
            Controls.Add(bTransform);
            Controls.Add(label1);
            Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            Name = "Form1";
            Text = "Window";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bTransform;
        private System.Windows.Forms.Button bBills;
        private System.Windows.Forms.Button bRegex;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.TextBox tbProgress;
        private System.Windows.Forms.ProgressBar pbConverter;
        private System.Windows.Forms.CheckBox UTF8;
    }
}

