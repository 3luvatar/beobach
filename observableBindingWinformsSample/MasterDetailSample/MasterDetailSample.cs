using System.Windows.Forms;
using observableBindingWinformsSample.Bindings;

namespace observableBindingWinformsSample.MasterDetailSample
{
    public partial class MasterDetailSample : Form
    {
        ViewModel ViewModel = new ViewModel();
        public MasterDetailSample()
        {
            InitializeComponent();
            ViewModel.Guests.Bind(listBox1, guest => guest.Name, ViewModel.SelectedGuest);
            masterControl1.View(ViewModel.SelectedGuest);
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            ViewModel.AddNew();
        }
    }
}
