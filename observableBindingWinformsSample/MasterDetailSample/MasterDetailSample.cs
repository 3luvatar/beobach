using System.Linq;
using System.Windows.Forms;
using Beobach;
using Beobach.BindingProviders;
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
            masterControl2.View(Observe.Value(ViewModel.Guests.First()));
            ViewModel.AttendingGuests.BindText(label2);
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            ViewModel.AddNew();
        }
    }
}
