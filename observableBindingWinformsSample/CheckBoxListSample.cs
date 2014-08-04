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
using observableBindingWinformsSample.Bindings;

namespace observableBindingWinformsSample
{
    public partial class CheckBoxListSample : Form
    {
        private CheckBoxSampleViewModel ViewModel;

        public CheckBoxListSample()
        {
            InitializeComponent();
            ViewModel = new CheckBoxSampleViewModel();
            ViewModel.InvitedGuests.Bind(checkedListBox1, model => model.Name, model => model.Attending);
            ViewModel.AttendingGuests.BindText(checkedItemsLabel);
        }

        private void inviteButton_Click(object sender, EventArgs e)
        {
            ViewModel.Invite(newInviteTextBox.Text);
        }
    }
}