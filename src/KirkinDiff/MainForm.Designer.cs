namespace KirkinDiff
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Panel = new System.Windows.Forms.Panel();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.CommandTextTextBox1 = new System.Windows.Forms.RichTextBox();
            this.ConnectionStringTextBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CommandTextTextBox2 = new System.Windows.Forms.RichTextBox();
            this.ConnectionStringTextBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ExecuteButton = new System.Windows.Forms.Button();
            this.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel
            // 
            this.Panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel.Controls.Add(this.SplitContainer);
            this.Panel.Location = new System.Drawing.Point(12, 12);
            this.Panel.Name = "Panel";
            this.Panel.Size = new System.Drawing.Size(1046, 681);
            this.Panel.TabIndex = 0;
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.IsSplitterFixed = true;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.CommandTextTextBox1);
            this.SplitContainer.Panel1.Controls.Add(this.ConnectionStringTextBox1);
            this.SplitContainer.Panel1.Controls.Add(this.label1);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.CommandTextTextBox2);
            this.SplitContainer.Panel2.Controls.Add(this.ConnectionStringTextBox2);
            this.SplitContainer.Panel2.Controls.Add(this.label2);
            this.SplitContainer.Size = new System.Drawing.Size(1046, 681);
            this.SplitContainer.SplitterDistance = 523;
            this.SplitContainer.TabIndex = 0;
            // 
            // CommandTextTextBox1
            // 
            this.CommandTextTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CommandTextTextBox1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommandTextTextBox1.Location = new System.Drawing.Point(2, 44);
            this.CommandTextTextBox1.Name = "CommandTextTextBox1";
            this.CommandTextTextBox1.Size = new System.Drawing.Size(514, 634);
            this.CommandTextTextBox1.TabIndex = 2;
            this.CommandTextTextBox1.Text = "";
            // 
            // ConnectionStringTextBox1
            // 
            this.ConnectionStringTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionStringTextBox1.Location = new System.Drawing.Point(2, 17);
            this.ConnectionStringTextBox1.Name = "ConnectionStringTextBox1";
            this.ConnectionStringTextBox1.Size = new System.Drawing.Size(514, 20);
            this.ConnectionStringTextBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connection String 1";
            // 
            // CommandTextTextBox2
            // 
            this.CommandTextTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CommandTextTextBox2.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommandTextTextBox2.Location = new System.Drawing.Point(2, 44);
            this.CommandTextTextBox2.Name = "CommandTextTextBox2";
            this.CommandTextTextBox2.Size = new System.Drawing.Size(514, 634);
            this.CommandTextTextBox2.TabIndex = 2;
            this.CommandTextTextBox2.Text = "";
            // 
            // ConnectionStringTextBox2
            // 
            this.ConnectionStringTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionStringTextBox2.Location = new System.Drawing.Point(2, 17);
            this.ConnectionStringTextBox2.Name = "ConnectionStringTextBox2";
            this.ConnectionStringTextBox2.Size = new System.Drawing.Size(514, 20);
            this.ConnectionStringTextBox2.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Connection String 2";
            // 
            // ExecuteButton
            // 
            this.ExecuteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExecuteButton.Location = new System.Drawing.Point(12, 699);
            this.ExecuteButton.Name = "ExecuteButton";
            this.ExecuteButton.Size = new System.Drawing.Size(75, 23);
            this.ExecuteButton.TabIndex = 1;
            this.ExecuteButton.Text = "Execute";
            this.ExecuteButton.UseVisualStyleBackColor = true;
            this.ExecuteButton.Click += new System.EventHandler(this.ExecuteButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1070, 734);
            this.Controls.Add(this.ExecuteButton);
            this.Controls.Add(this.Panel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KirkinDiff";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Panel.ResumeLayout(false);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel1.PerformLayout();
            this.SplitContainer.Panel2.ResumeLayout(false);
            this.SplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel;
        private System.Windows.Forms.Button ExecuteButton;
        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.TextBox ConnectionStringTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ConnectionStringTextBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox CommandTextTextBox1;
        private System.Windows.Forms.RichTextBox CommandTextTextBox2;
    }
}

