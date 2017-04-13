using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedDesktopApp
{
    public partial class Invoices : Form
    {
        public Invoices()
        {
            InitializeComponent();
        }

        public IppRealmOAuthProfile IppRealmOAuthProfile { get; set; }
        private ServiceContext serviceContext = null;
        private IList<KeyValuePair<string, string>> CustomersNameId;

        private void btnInvoices_Click(object sender, EventArgs e)
        {
            tssInfo.Text = "Loading invoices ...";
            displayInvoices(cmbCustomers.SelectedValue.ToString());
            tssInfo.Text = "Loading invoices finished";
        }

        private void displayInvoices(string id = "")
        {
            var query = new StringBuilder("Select * From Invoice");
            if (!string.IsNullOrEmpty(id))
                query.AppendFormat(" where CustomerRef = '{0}'", id);

            QueryService<Invoice> customerQueryService = new QueryService<Invoice>(serviceContext);
            var cc = cmbCustomers.DataSource;
            var ii = customerQueryService.ExecuteIdsQuery(query.ToString()).Select(s => new { s.Id, CustomerRef = GetValue(s.CustomerRef), CustomerName = CustomersNameId.First(f => f.Key.Equals(s.CustomerRef.Value)).Value, CustomerMemo = GetValue(s.CustomerMemo), s.Balance, s.Deposit, s.DueDate, s.Overview, s.PaymentType, s.TotalAmt }).ToList();
            dataGridView1.DataSource = ii;
        }

        private static string GetValue(ReferenceType x)
        {
            if (x != null)
                return x.Value;
            else
                return "";
        }

        private static string GetValue(MemoRef x)
        {
            if (x != null)
                return x.Value;
            else
                return "";
        }

        private void Invoices_Load(object sender, EventArgs e)
        {
            string consumerKey = ConfigurationSettings.AppSettings["consumerKey"];
            string consumerSecret = ConfigurationSettings.AppSettings["consumerSecret"];
            OAuthRequestValidator oauthRequestValidator = new OAuthRequestValidator(IppRealmOAuthProfile.accessToken, IppRealmOAuthProfile.accessSecret, consumerKey, consumerSecret);
            serviceContext = new ServiceContext(IppRealmOAuthProfile.realmId, IntuitServicesType.QBO, oauthRequestValidator);
            DataService dataService = new DataService(serviceContext);

            QueryService<Customer> customerQueryService = new QueryService<Customer>(serviceContext);
            CustomersNameId = customerQueryService.ExecuteIdsQuery("Select * From Customer MaxResults 50").Select(s => new KeyValuePair<string, string>(s.Id, s.DisplayName)).ToList();
            var source = new List<KeyValuePair<string, string>>();
            source.Add(new KeyValuePair<string, string>("", "--select customer--"));
            source.AddRange(CustomersNameId);
            cmbCustomers.DataSource = source;
            cmbCustomers.DisplayMember = "Value";
            cmbCustomers.ValueMember = "Key";

        }
    }
}
