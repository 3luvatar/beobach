namespace observableBindingWinformsSample
{
    partial class SelectedListSample
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.selectedItemTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.newItemTextBox = new System.Windows.Forms.TextBox();
            this.addNewItemButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(156, 342);
            this.listBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(174, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected Item";
            // 
            // selectedItemTextBox
            // 
            this.selectedItemTextBox.Location = new System.Drawing.Point(177, 28);
            this.selectedItemTextBox.Name = "selectedItemTextBox";
            this.selectedItemTextBox.Size = new System.Drawing.Size(172, 20);
            this.selectedItemTextBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(174, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "New Item";
            // 
            // newItemTextBox
            // 
            this.newItemTextBox.Location = new System.Drawing.Point(177, 67);
            this.newItemTextBox.Name = "newItemTextBox";
            this.newItemTextBox.Size = new System.Drawing.Size(172, 20);
            this.newItemTextBox.TabIndex = 4;
            // 
            // addNewItemButton
            // 
            this.addNewItemButton.Location = new System.Drawing.Point(355, 65);
            this.addNewItemButton.Name = "addNewItemButton";
            this.addNewItemButton.Size = new System.Drawing.Size(52, 23);
            this.addNewItemButton.TabIndex = 5;
            this.addNewItemButton.Text = "Add";
            this.addNewItemButton.UseVisualStyleBackColor = true;
            this.addNewItemButton.Click += new System.EventHandler(this.addNewItemButton_Click);
            // 
            // SelectedListSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 364);
            this.Controls.Add(this.addNewItemButton);
            this.Controls.Add(this.newItemTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.selectedItemTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox1);
            this.Name = "SelectedListSample";
            this.Text = "SelectedListSample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox selectedItemTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox newItemTextBox;
        private System.Windows.Forms.Button addNewItemButton;
    }
}