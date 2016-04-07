using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
            TimeSpan timeTaken1;
            TimeSpan timeTaken2;

            using (DataSet ds1 = ProduceDataSet(ConnectionStringTextBox1.Text, CommandTextTextBox1.Text, out timeTaken1))
            using (DataSet ds2 = ProduceDataSet(ConnectionStringTextBox2.Text, CommandTextTextBox2.Text, out timeTaken2))
            {
                DiffResult diff = DataSetDiff.Compare(ds1, ds2);
                string timeTakenString = $"Time taken (left): {(double)timeTaken1.Milliseconds / 1000:0.###}s, right: {(double)timeTaken2.Milliseconds / 1000:0.###}s";

                if (diff.AreSame)
                {
                    MessageBox.Show(
                        timeTakenString +
                        Environment.NewLine +
                        $"Result sets identical. Tables: {ds1.Tables.Count}, Rows: {ds1.Tables.Cast<DataTable>().Sum(dt => dt.Rows.Count)}."
                    );
                }
                else
                {
                    MessageBox.Show(
                        timeTakenString + Environment.NewLine + diff.ToString(DiffTextFormat.Indented)
                    );
                }
            }
        }

        private DataSet ProduceDataSet(string connectionString, string commandText, out TimeSpan timeTaken)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(commandText, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataSet ds = new DataSet();
                Stopwatch sw = Stopwatch.StartNew();

                adapter.Fill(ds);

                timeTaken = sw.Elapsed;

                return ds;
            }
        }      
    }
}