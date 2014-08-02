using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Beobach.BindingProviders;

namespace observableBindingWinformsSample
{
    public partial class Form1 : Form
    {
        private SampleViewModel ViewModel;

        public Form1()
        {
            InitializeComponent();
            ViewModel = new SampleViewModel();
            ViewModel.FirstName.BindText(firstNameTextBox);
            ViewModel.LastName.BindText(lastNameTextBox);
            ViewModel.FullName.BindText(fullNameLabel);
            ViewModel.MiddleName.BindText(middleNameTextBox, middleNameLabel);
            ViewModel.HasMiddleName.BindCheckBox(enableMiddleNameCheckBox);
        }
    }
}
