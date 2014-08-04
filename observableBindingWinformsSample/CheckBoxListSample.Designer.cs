namespace observableBindingWinformsSample
{
    partial class CheckBoxListSample
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
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkedItemsLabel = new System.Windows.Forms.Label();
            this.newInviteTextBox = new System.Windows.Forms.TextBox();
            this.inviteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.CheckOnClick = true;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 57);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(232, 379);
            this.checkedListBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Checked Items:";
            // 
            // checkedItemsLabel
            // 
            this.checkedItemsLabel.AutoSize = true;
            this.checkedItemsLabel.Location = new System.Drawing.Point(99, 9);
            this.checkedItemsLabel.Name = "checkedItemsLabel";
            this.checkedItemsLabel.Size = new System.Drawing.Size(33, 13);
            this.checkedItemsLabel.TabIndex = 2;
            this.checkedItemsLabel.Text = "None";
            // 
            // newInviteTextBox
            // 
            this.newInviteTextBox.Location = new System.Drawing.Point(15, 25);
            this.newInviteTextBox.Name = "newInviteTextBox";
            this.newInviteTextBox.Size = new System.Drawing.Size(148, 20);
            this.newInviteTextBox.TabIndex = 3;
            // 
            // inviteButton
            // 
            this.inviteButton.Location = new System.Drawing.Point(169, 23);
            this.inviteButton.Name = "inviteButton";
            this.inviteButton.Size = new System.Drawing.Size(75, 23);
            this.inviteButton.TabIndex = 4;
            this.inviteButton.Text = "Invite";
            this.inviteButton.UseVisualStyleBackColor = true;
            this.inviteButton.Click += new System.EventHandler(this.inviteButton_Click);
            // 
            // CheckBoxListSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 441);
            this.Controls.Add(this.inviteButton);
            this.Controls.Add(this.newInviteTextBox);
            this.Controls.Add(this.checkedItemsLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkedListBox1);
            this.Name = "CheckBoxListSample";
            this.Text = "CheckBoxListSample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label checkedItemsLabel;
        private System.Windows.Forms.TextBox newInviteTextBox;
        private System.Windows.Forms.Button inviteButton;
    }
}