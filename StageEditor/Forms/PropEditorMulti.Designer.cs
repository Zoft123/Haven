﻿namespace Haven.Render
{
    partial class PropEditorMulti
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
            btnSnapMulti = new Button();
            lbPropSelectCount = new Label();
            labelSpawnEditZ = new Label();
            labelSpawnEditY = new Label();
            labelSpawnEditX = new Label();
            tbSpawnEditY = new TextBox();
            tbSpawnEditZ = new TextBox();
            tbSpawnEditX = new TextBox();
            btnApply = new Button();
            SuspendLayout();
            // 
            // btnSnapMulti
            // 
            btnSnapMulti.Location = new Point(12, 119);
            btnSnapMulti.Name = "btnSnapMulti";
            btnSnapMulti.Size = new Size(579, 46);
            btnSnapMulti.TabIndex = 0;
            btnSnapMulti.Text = "Snap to Ground";
            btnSnapMulti.UseVisualStyleBackColor = true;
            btnSnapMulti.Click += btnSnapMulti_Click;
            // 
            // lbPropSelectCount
            // 
            lbPropSelectCount.AutoSize = true;
            lbPropSelectCount.Location = new Point(15, 190);
            lbPropSelectCount.Name = "lbPropSelectCount";
            lbPropSelectCount.Size = new Size(196, 32);
            lbPropSelectCount.TabIndex = 1;
            lbPropSelectCount.Text = "Selected Props: 0";
            // 
            // labelSpawnEditZ
            // 
            labelSpawnEditZ.AutoSize = true;
            labelSpawnEditZ.Location = new Point(409, 19);
            labelSpawnEditZ.Margin = new Padding(6, 0, 6, 0);
            labelSpawnEditZ.Name = "labelSpawnEditZ";
            labelSpawnEditZ.Size = new Size(113, 32);
            labelSpawnEditZ.TabIndex = 11;
            labelSpawnEditZ.Text = "Y (Offset)";
            // 
            // labelSpawnEditY
            // 
            labelSpawnEditY.AutoSize = true;
            labelSpawnEditY.Location = new Point(212, 19);
            labelSpawnEditY.Margin = new Padding(6, 0, 6, 0);
            labelSpawnEditY.Name = "labelSpawnEditY";
            labelSpawnEditY.Size = new Size(114, 32);
            labelSpawnEditY.TabIndex = 10;
            labelSpawnEditY.Text = "Z (Offset)";
            // 
            // labelSpawnEditX
            // 
            labelSpawnEditX.AutoSize = true;
            labelSpawnEditX.Location = new Point(15, 19);
            labelSpawnEditX.Margin = new Padding(6, 0, 6, 0);
            labelSpawnEditX.Name = "labelSpawnEditX";
            labelSpawnEditX.Size = new Size(114, 32);
            labelSpawnEditX.TabIndex = 9;
            labelSpawnEditX.Text = "X (Offset)";
            // 
            // tbSpawnEditY
            // 
            tbSpawnEditY.Location = new Point(409, 57);
            tbSpawnEditY.Margin = new Padding(6);
            tbSpawnEditY.Name = "tbSpawnEditY";
            tbSpawnEditY.Size = new Size(182, 39);
            tbSpawnEditY.TabIndex = 8;
            tbSpawnEditY.Text = "0";
            // 
            // tbSpawnEditZ
            // 
            tbSpawnEditZ.Location = new Point(212, 57);
            tbSpawnEditZ.Margin = new Padding(6);
            tbSpawnEditZ.Name = "tbSpawnEditZ";
            tbSpawnEditZ.Size = new Size(182, 39);
            tbSpawnEditZ.TabIndex = 7;
            tbSpawnEditZ.Text = "0";
            // 
            // tbSpawnEditX
            // 
            tbSpawnEditX.Location = new Point(15, 57);
            tbSpawnEditX.Margin = new Padding(6);
            tbSpawnEditX.Name = "tbSpawnEditX";
            tbSpawnEditX.Size = new Size(182, 39);
            tbSpawnEditX.TabIndex = 6;
            tbSpawnEditX.Text = "0";
            // 
            // btnApply
            // 
            btnApply.Location = new Point(441, 183);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(150, 46);
            btnApply.TabIndex = 12;
            btnApply.Text = "Done";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // PropEditorMulti
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(609, 248);
            Controls.Add(btnApply);
            Controls.Add(labelSpawnEditZ);
            Controls.Add(labelSpawnEditY);
            Controls.Add(labelSpawnEditX);
            Controls.Add(tbSpawnEditY);
            Controls.Add(tbSpawnEditZ);
            Controls.Add(tbSpawnEditX);
            Controls.Add(lbPropSelectCount);
            Controls.Add(btnSnapMulti);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "PropEditorMulti";
            Text = "PropEditorMulti";
            Load += PropEditorMulti_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSnapMulti;
        private Label lbPropSelectCount;
        private Label labelSpawnEditZ;
        private Label labelSpawnEditY;
        private Label labelSpawnEditX;
        private TextBox tbSpawnEditY;
        private TextBox tbSpawnEditZ;
        private TextBox tbSpawnEditX;
        private Button btnApply;
    }
}