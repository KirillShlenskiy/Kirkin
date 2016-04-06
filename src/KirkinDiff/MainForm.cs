using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

using Kirkin.Diff;
using Kirkin.Diff.Data;

using KirkinDiff.Properties;

namespace KirkinDiff
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ConnectionStringTextBox1.Text = Settings.Default.ConnectionString1;
            ConnectionStringTextBox2.Text = Settings.Default.ConnectionString2;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.ConnectionString1 = ConnectionStringTextBox1.Text;
            Settings.Default.ConnectionString2 = ConnectionStringTextBox2.Text;

            Settings.Default.Save();
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            ExecuteDiff();
        }

        private void ExecuteDiff()
        {
            using (DataSet ds1 = ProduceDataSet(ConnectionStringTextBox1.Text, CommandTextTextBox1.Text))
            using (DataSet ds2 = ProduceDataSet(ConnectionStringTextBox2.Text, CommandTextTextBox2.Text))
            {
                DiffResult diff = DataSetDiff.Compare(ds1, ds2);

                if (diff.AreSame)
                {
                    MessageBox.Show(
                        $"Result sets identical. Tables: {ds1.Tables.Count}, Rows: {ds1.Tables.Cast<DataTable>().Sum(dt => dt.Rows.Count)}."
                    );
                }
                else
                {
                    MessageBox.Show(diff.ToString(DiffTextFormat.Indented));
                }
            }
        }

        private DataSet ProduceDataSet(string connectionString, string commandText)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(commandText, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataSet ds = new DataSet();

                adapter.Fill(ds);

                return ds;
            }
        }      
    }
}