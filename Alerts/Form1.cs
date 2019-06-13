using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alerts
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Alerts";
            txtOthers.Enabled = false;
        }

        string[] allfiles = new string[]
            {
                "AlertData.xlsx"
            };

        public string AlertConnection
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["AlertDBConnection"]
                    .ToString();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //Close the Expense form
            bool saveittogoogle = Utilities.SaveItToGoogleDrive(allfiles);
            if (saveittogoogle)
            {
                //MessageBox.Show("Updated in Google Drive");
            }
            else { MessageBox.Show("Error in saving"); }
            bool saveittoDbBackup = Utilities.SaveItToDbBackup(allfiles);
            if (saveittoDbBackup)
            {
                //MessageBox.Show("Updated in Local Backup");
            }
            else { MessageBox.Show("Error in saving"); }
            this.Close();
        }
        private int id;
        private string rawname;
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable data = GetLastIdInt();
            {
                id = int.Parse(data.Rows[0]["Expr1000"].ToString()) + 1;
            }
            if (cmbName.SelectedItem.ToString() =="Others")
            {
                rawname = txtOthers.Text;
            }
            else
            {
                rawname = cmbName.SelectedItem.ToString();
            }
            string name = rawname.Replace(' ', '_');
            string alertTime = dtpTime.Value.AddDays(-1).ToShortDateString();
            string alertMonth = Utilities.ConvertIntToMonth(dtpTime.Value.Month);
            string alertYear = DateTime.Now.Year.ToString();
            string created = DateTime.Now.ToShortDateString();
            int rowsAffected = 0;
            using (OleDbCommand oleDbCommand = new OleDbCommand())
            {
                // Set the command object properties
                oleDbCommand.Connection = new OleDbConnection(this.AlertConnection);
                oleDbCommand.CommandType = CommandType.Text;
                oleDbCommand.CommandText = "Insert Into" +
                " [AlertData$] (Id, Name, AlertTime, AlertMonth, AlertYear, Created)" +
                " Values (@id, @name, @alertTime, @alertMonth, @alertYear, @created)";// Scripts.sqlInsertExpense;

                // Add the input parameters to the parameter collection
                oleDbCommand.Parameters.AddWithValue("@id", id);
                oleDbCommand.Parameters.AddWithValue("@name", name);
                oleDbCommand.Parameters.AddWithValue("@alertTime", alertTime);
                oleDbCommand.Parameters.AddWithValue("@alertMonth", alertMonth);
                oleDbCommand.Parameters.AddWithValue("@alertYear", alertYear);
                oleDbCommand.Parameters.AddWithValue("@created", created);

                // Open the connection, execute the query and close the connection
                oleDbCommand.Connection.Open();
                rowsAffected = oleDbCommand.ExecuteNonQuery();
                oleDbCommand.Connection.Close();

            }
            if (rowsAffected > 0)
            {
                //MessageBox.Show("Alert is inserted");
                LoadData();
            }
            else { MessageBox.Show("Error inserting"); }

        }

        public DataTable GetLastIdInt()
        {
            DataTable dataTable = new DataTable();
            using (OleDbDataAdapter OleDbDbDataAdapter = new OleDbDataAdapter())
            {
                // Create the command and set its properties
                OleDbDbDataAdapter.SelectCommand = new OleDbCommand();
                OleDbDbDataAdapter.SelectCommand.Connection = new OleDbConnection(AlertConnection);
                OleDbDbDataAdapter.SelectCommand.CommandType = CommandType.Text;
                // Assign the SQL to the command object
                OleDbDbDataAdapter.SelectCommand.CommandText = " SELECT COUNT(*) From [AlertData$]";
                // Fill the datatable from adapter
                OleDbDbDataAdapter.Fill(dataTable);
            }
            return dataTable;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbName.Items.Add("Recharge Internet");
            cmbName.Items.Add("Deposit Savings");
            cmbName.Items.Add("Booking Tickets");
            cmbName.Items.Add("Room Rent");
            cmbName.Items.Add("Interview Call");
            cmbName.Items.Add("Others");

            //Load the Xls sheet from google drive
            bool fetchitfromgoogle = Utilities.RestoreItFromGoogleDrive(allfiles);
            if (fetchitfromgoogle)
            {
                //MessageBox.Show("Updated in Google Drive");
            }
            else { MessageBox.Show("Error in fetching the data from google"); }

            LoadData();
        }

        private void LoadData()
        {

            //fetch all the data
            DataTable dataTable = new DataTable();
			

            using (OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter())
            {
                // Create the command and set its properties
                oleDbDataAdapter.SelectCommand = new OleDbCommand();
                oleDbDataAdapter.SelectCommand.Connection = new OleDbConnection(this.AlertConnection);
                oleDbDataAdapter.SelectCommand.CommandType = CommandType.Text;

                oleDbDataAdapter.SelectCommand.CommandText = "Select Name, AlertTime, Created" +
                " From [AlertData$] order by YEAR(AlertTime) DESC, MONTH(AlertTime) DESC, DAY(AlertTime)"; // Scripts.sqlGetAllExpense;

                // Fill the datatable from adapter
                oleDbDataAdapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string name = cmbName.SelectedItem.ToString();
            string updatedname = name.Replace(' ', '_');
            LoadResults(updatedname);
        }

        private void LoadResults(string name)
        {
            DataTable dataTable = new DataTable();
            using (OleDbDataAdapter OleDbDbDataAdapter = new OleDbDataAdapter())
            {
                // Create the command and set its properties
                OleDbDbDataAdapter.SelectCommand = new OleDbCommand();
                OleDbDbDataAdapter.SelectCommand.Connection = new OleDbConnection(AlertConnection);
                OleDbDbDataAdapter.SelectCommand.CommandType = CommandType.Text;
                // Assign the SQL to the command object
                OleDbDbDataAdapter.SelectCommand.CommandText = "SELECT * From [AlertData$] where Name='" + name + "' order by Id";
                // Fill the datatable from adapter
                OleDbDbDataAdapter.Fill(dataTable);
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dataTable;
            }
        }

        private void cmbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string result = cmbName.SelectedItem.ToString();
            if (result=="Others")
            {
                txtOthers.Enabled = true;
            }
            else { txtOthers.Enabled = false; }
        }
    }
}
