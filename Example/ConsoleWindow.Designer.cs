namespace xr
{
    partial class ConsoleWindow
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
            this.xrConsole = new xr.Console();
            this.SuspendLayout();
            // 
            // xrConsole
            // 
            this.xrConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xrConsole.Font = new System.Drawing.Font("Arial", 9F);
            this.xrConsole.Location = new System.Drawing.Point(0, 0);
            this.xrConsole.Name = "xrConsole";
            this.xrConsole.Size = new System.Drawing.Size(624, 282);
            this.xrConsole.TabIndex = 0;
            // 
            // ConsoleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 282);
            this.Controls.Add(this.xrConsole);
            this.Name = "ConsoleWindow";
            this.Text = "xrConsole";
            this.ResumeLayout(false);

        }

        #endregion

        private Console xrConsole;
    }
}
