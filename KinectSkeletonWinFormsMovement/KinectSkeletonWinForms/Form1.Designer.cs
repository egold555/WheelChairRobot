namespace KinectSkeletonWinForms
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
            if (disposing && (components != null)) {
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
            this.skelImage = new System.Windows.Forms.PictureBox();
            this.statusBarText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.skelImage)).BeginInit();
            this.SuspendLayout();
            // 
            // skelImage
            // 
            this.skelImage.BackColor = System.Drawing.Color.Black;
            this.skelImage.Location = new System.Drawing.Point(77, 12);
            this.skelImage.Name = "skelImage";
            this.skelImage.Size = new System.Drawing.Size(640, 480);
            this.skelImage.TabIndex = 0;
            this.skelImage.TabStop = false;
            // 
            // statusBarText
            // 
            this.statusBarText.AutoSize = true;
            this.statusBarText.Location = new System.Drawing.Point(12, 567);
            this.statusBarText.Name = "statusBarText";
            this.statusBarText.Size = new System.Drawing.Size(35, 13);
            this.statusBarText.TabIndex = 1;
            this.statusBarText.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 613);
            this.Controls.Add(this.statusBarText);
            this.Controls.Add(this.skelImage);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.skelImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox skelImage;
        private System.Windows.Forms.Label statusBarText;
    }
}

