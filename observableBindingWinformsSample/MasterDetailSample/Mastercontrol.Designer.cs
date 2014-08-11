namespace observableBindingWinformsSample.MasterDetailSample
{
    partial class MasterControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.attendingCheckBox = new System.Windows.Forms.CheckBox();
            this.foodTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.invitedDateCalendar = new System.Windows.Forms.MonthCalendar();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(6, 16);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(100, 20);
            this.nameTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name";
            // 
            // attendingCheckBox
            // 
            this.attendingCheckBox.AutoSize = true;
            this.attendingCheckBox.Location = new System.Drawing.Point(6, 42);
            this.attendingCheckBox.Name = "attendingCheckBox";
            this.attendingCheckBox.Size = new System.Drawing.Size(71, 17);
            this.attendingCheckBox.TabIndex = 2;
            this.attendingCheckBox.Text = "Attending";
            this.attendingCheckBox.UseVisualStyleBackColor = true;
            // 
            // foodTypeComboBox
            // 
            this.foodTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.foodTypeComboBox.FormattingEnabled = true;
            this.foodTypeComboBox.Location = new System.Drawing.Point(6, 78);
            this.foodTypeComboBox.Name = "foodTypeComboBox";
            this.foodTypeComboBox.Size = new System.Drawing.Size(121, 21);
            this.foodTypeComboBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Food Choice";
            // 
            // invitedDateCalendar
            // 
            this.invitedDateCalendar.Location = new System.Drawing.Point(166, 22);
            this.invitedDateCalendar.Name = "invitedDateCalendar";
            this.invitedDateCalendar.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(163, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "InvitedDate";
            // 
            // MasterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.invitedDateCalendar);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.foodTypeComboBox);
            this.Controls.Add(this.attendingCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nameTextBox);
            this.Name = "MasterControl";
            this.Size = new System.Drawing.Size(467, 358);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox attendingCheckBox;
        private System.Windows.Forms.ComboBox foodTypeComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MonthCalendar invitedDateCalendar;
        private System.Windows.Forms.Label label3;
    }
}
