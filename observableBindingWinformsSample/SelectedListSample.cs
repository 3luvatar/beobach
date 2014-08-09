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
    public partial class SelectedListSample : Form
    {
        SelectedListViewModel ViewModel = new SelectedListViewModel();
        public SelectedListSample()
        {
            InitializeComponent();
            ViewModel.Friends.Bind(listBox1, friend => friend.Name, ViewModel.SelectedFriend);
            ViewModel.SelectedFriendName.BindText(selectedItemTextBox);
        }

        private void addNewItemButton_Click(object sender, EventArgs e)
        {
            ViewModel.AddFriend(newItemTextBox.Text);
        }

    }
}
