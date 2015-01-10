using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomerQueueManagement
{
    public partial class CustomerQueueManagementUI : Form
    {

        enum Progress_Status
        {
            Not_Served,
            On_Service,
            Served
        }

        public CustomerQueueManagementUI()
        {
            InitializeComponent();
        }

        string connString = @"Data source = BRISTY-PC\SQLEXPRESS; Database = CustomerQueueDB; Integrated Security = true";

        private void enqueueButton_Click(object sender, EventArgs e)
        {
            SaveComplain();
            ShowCustomerComplain();
        }

        private void ShowCustomerComplain()
        {
            SqlConnection aSqlConnection = new SqlConnection(connString);
            aSqlConnection.Open();
            string commandText = "SELECT * FROM t_customer_complain WHERE progress_status = '" + Progress_Status.Not_Served + "' OR progress_status = '" + Progress_Status.On_Service + "'";
            SqlCommand aSqlCommand = new SqlCommand(commandText, aSqlConnection);

            SqlDataReader aSqlDataReader = aSqlCommand.ExecuteReader();

            List<Customer> customers = new List<Customer>();

            while (aSqlDataReader.Read() == true)
            {
                Customer aCustomer = new Customer();
                aCustomer.serial = Convert.ToInt32(aSqlDataReader["Id"]);
                aCustomer.name = aSqlDataReader["name"].ToString();
                aCustomer.complain = aSqlDataReader["complain"].ToString();
                aCustomer.status = aSqlDataReader["progress_status"].ToString();
                customers.Add(aCustomer);
            }
            aSqlConnection.Close();

            remainingCustomerListView.Items.Clear();

            foreach (Customer aCustomer in customers)
            {
                ListViewItem aListViewItem = new ListViewItem();
                aListViewItem.Text = aCustomer.serial.ToString();
                aListViewItem.SubItems.Add(aCustomer.name);
                aListViewItem.SubItems.Add(aCustomer.complain);
                aListViewItem.SubItems.Add(aCustomer.status);
                remainingCustomerListView.Items.Add(aListViewItem);
            }

            remainingLabel.Text = customers.Count.ToString();
            ShowNoOfCustomerServedToday();
        }

        public void ShowNoOfCustomerServedToday()
        {
            SqlConnection aSqlConnection = new SqlConnection(connString);
            aSqlConnection.Open();
            string commandText = "SELECT COUNT(Id) FROM t_customer_complain WHERE progress_status = '" + Progress_Status.Served +"'";
            SqlCommand aSqlCommand = new SqlCommand(commandText, aSqlConnection);
            SqlDataReader aSqlDataReader = aSqlCommand.ExecuteReader();
            aSqlDataReader.Read();
            int noOfServedCustomer = Convert.ToInt32(aSqlDataReader[0]);
            aSqlConnection.Close();
            servedLabel.Text = noOfServedCustomer.ToString();

        }

        public void SaveComplain()
        {
            SqlConnection aSqlConnection = new SqlConnection(connString);
            aSqlConnection.Open();
            string commandText = "INSERT INTO t_customer_complain VALUES('" + nameTextBox.Text + "','" + complainTextbox.Text + "','" + Progress_Status.Not_Served +"')";
            SqlCommand aSqlCommand = new SqlCommand(commandText, aSqlConnection);
            aSqlCommand.ExecuteNonQuery();
            aSqlConnection.Close();
            ClearEnqueue();
        }

        private void ClearEnqueue()
        {
            nameTextBox.Text = string.Empty;
            complainTextbox.Text = string.Empty;
        }

        private void CustomerQueueManagementUI_Load(object sender, EventArgs e)
        {
            ShowCustomerComplain();
        }
       

        private void dequeueButton_Click(object sender, EventArgs e)
        {
            DequeueProcessing();
            if (remainingLabel.Text == (0).ToString())
            {
                ClearDequeue();
            }
          
            
            
        }

        private void DequeueProcessing()
        {
            SqlConnection SqlConnection = new SqlConnection(connString);
            SqlConnection.Open();
            string command = "SELECT * FROM t_customer_complain WHERE progress_status = '" + Progress_Status.Not_Served + "'OR progress_status = '" + Progress_Status.On_Service + "' ";
            SqlCommand SqlCommand = new SqlCommand(command, SqlConnection);

            SqlDataReader SqlDataReader = SqlCommand.ExecuteReader();
            List<Customer> customers = new List<Customer>();
            Customer aCustomer = new Customer();

            while (SqlDataReader.Read() == true)
            {

                aCustomer.serial = Convert.ToInt32(SqlDataReader["Id"]);
                aCustomer.name = SqlDataReader["name"].ToString();
                aCustomer.complain = SqlDataReader["complain"].ToString();
                aCustomer.status = SqlDataReader["progress_status"].ToString();
                if (aCustomer.status == "Not_Served")
                {
                    int minId = aCustomer.serial;
                    RetrieveCustomerComplain(minId, aCustomer);
                    break;
                }
                else if (aCustomer.status == "On_Service")
                {
                    int minId = aCustomer.serial;
                    UpdateForService(minId, aCustomer);
                    break;
                }
                customers.Add(aCustomer);

            }
            ShowCustomerComplain();
            SqlConnection.Close();
        }

        private void UpdateForService(int minId,Customer aCustomer)
        {
            if (aCustomer.status == "On_Service")
            {
                SqlConnection con = new SqlConnection(connString);

                con.Open();
                string status = "Served";
                string query = "UPDATE t_customer_complain SET progress_status='" + status + "' WHERE progress_status='" + Progress_Status.On_Service + "' AND Id = '" + minId + "'";
                SqlCommand command = new SqlCommand(query, con);

                command.ExecuteNonQuery();
                DequeueProcessing();
                    
                
                con.Close();
            }
            else if (aCustomer.status == "Not_Served")
            {
                SqlConnection con = new SqlConnection(connString);

                con.Open();
                string status = "On_Service";
                string query = "UPDATE t_customer_complain SET progress_status='" + status + "' WHERE progress_status='" + Progress_Status.Not_Served + "' AND Id = '" + minId + "'";
                SqlCommand command = new SqlCommand(query, con);

                command.ExecuteNonQuery();
                con.Close();
            }
            
        }
     
        private void RetrieveCustomerComplain(int minId, Customer aCustomer)
        {
            SqlConnection aSqlConnection = new SqlConnection(connString);
            aSqlConnection.Open();
            string commandText = "SELECT * FROM t_customer_complain where Id = '" + minId + "'  ";
            SqlCommand aSqlCommand = new SqlCommand(commandText, aSqlConnection);
            SqlDataReader aSqlDataReader = aSqlCommand.ExecuteReader();

            if (aSqlDataReader.Read())
            {
                aCustomer.serial = Convert.ToInt32(aSqlDataReader["Id"]);
                aCustomer.name = aSqlDataReader["name"].ToString();
                aCustomer.complain = aSqlDataReader["complain"].ToString();
                aCustomer.status = aSqlDataReader["progress_status"].ToString();

                 serialNoTextBox.Text = aCustomer.serial.ToString();
                 customerNameTextBox.Text = aCustomer.name;
                 customerComplainTextBox.Text = aCustomer.complain;
            }
            UpdateForService(minId,aCustomer);
            aSqlConnection.Close();
        }
        void ClearDequeue()
        {
            serialNoTextBox.Text = string.Empty;
            customerNameTextBox.Text = string.Empty;
            customerComplainTextBox.Text = string.Empty;
        }
        
          
        }            
          
 }
          

        
    



