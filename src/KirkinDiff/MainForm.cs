using System;
using System.Data.SqlClient;
using System.Windows.Forms;

using Kirkin.Diff;

namespace KirkinDiff
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            DiffResult diff = ExecuteDiff();

            if (diff.AreSame)
            {
                MessageBox.Show("Result sets identical.");
            }
            else
            {
                MessageBox.Show(diff.ToString(DiffTextFormat.Indented));
            }
        }

        private DiffResult ExecuteDiff()
        {
            using (SqlConnection connection1 = new SqlConnection(ConnectionStringTextBox1.Text))
            {
                connection1.Open();

                using (SqlConnection connection2 = new SqlConnection(ConnectionStringTextBox2.Text))
                {
                    connection2.Open();

                    using (SqlCommand command1 = new SqlCommand(CommandTextTextBox1.Text, connection1))
                    using (SqlCommand command2 = new SqlCommand(CommandTextTextBox2.Text, connection2))
                    {
                        return SqlCommandDiff.CompareResultSets(command1, command2);
                    }
                }
            }
        }
    }
}