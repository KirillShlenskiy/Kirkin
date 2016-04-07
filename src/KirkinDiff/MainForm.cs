using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kirkin.Diff;
using Kirkin.Diff.Data;

using KirkinDiff.Properties;

namespace KirkinDiff
{
    public partial class MainForm : Form
    {
        private string DefaultText;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs _)
        {
            DefaultText = Text;

            ConnectionStringTextBox1.Text = Settings.Default.ConnectionString1;
            ConnectionStringTextBox2.Text = Settings.Default.ConnectionString2;

            CommandTextTextBox1.AcceptsTab = false;
            CommandTextTextBox2.AcceptsTab = false;

            KeyPreview = true;

            KeyDown += OnKeyDown;

            await Task.Yield();

            CommandTextTextBox1.Focus();
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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) {
                ExecuteDiff();
            }
        }

        private async void ExecuteDiff()
        {
            ExecuteButton.Enabled = false;

            try
            {
                TimeSpan timeTaken1 = TimeSpan.Zero;
                TimeSpan timeTaken2 = TimeSpan.Zero;

                Text = DefaultText + ": executing left ...";

                LightDataSet ds1 = await Task.Run(() => ProduceDataSet(ConnectionStringTextBox1.Text, CommandTextTextBox1.Text, out timeTaken1));
                Text = DefaultText + ": executing right ...";

                LightDataSet ds2 = await Task.Run(() => ProduceDataSet(ConnectionStringTextBox2.Text, CommandTextTextBox2.Text, out timeTaken2));
                Text = DefaultText + ": comparing ...";

                Stopwatch diffStopwatch = Stopwatch.StartNew();
                DiffResult diff = await Task.Run(() => DataSetDiff.Compare(ds1, ds2));

                diffStopwatch.Stop();

                StringBuilder resultText = new StringBuilder();

                resultText.Append($"Time taken (left): {timeTaken1.TotalMilliseconds / 1000:0.###}s, ");
                resultText.Append($"right: {timeTaken2.TotalMilliseconds / 1000:0.###}s, ");
                resultText.AppendLine($"compare: {(double)diffStopwatch.ElapsedMilliseconds / 1000:0.###}s");
                resultText.AppendLine();

                if (diff.AreSame)
                {
                    resultText.AppendLine($"Result sets identical. Tables: {ds1.Tables.Count}, Rows: {ds1.Tables.Sum(dt => dt.Rows.Count)}.");
                }
                else
                {
                    resultText.AppendLine(diff.ToString(DiffTextFormat.Indented));
                }

                if (resultText.Length > 1500)
                {
                    resultText.Length = 1500;
                    resultText.Append(" ...");
                }

                ExecuteButton.Enabled = true;
                Text = DefaultText + ": done";

                MessageBox.Show(
                    resultText.ToString(),
                    diff.AreSame ? "No diff" : "Changes detected",
                    MessageBoxButtons.OK,
                    diff.AreSame ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation
                );
            }
            finally
            {
                ExecuteButton.Enabled = true;
                Text = DefaultText;
            }
        }

        private static LightDataSet ProduceDataSet(string connectionString, string commandText, out TimeSpan timeTaken)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    command.CommandTimeout = 0;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        LightDataSet ds = new LightDataSet();

                        while (true)
                        {
                            ds.Tables.Add(TableFromReader(reader));

                            if (!reader.NextResult()) {
                                break;
                            }
                        }

                        timeTaken = sw.Elapsed;

                        return ds;
                    }
                }
            }
        }

        private static LightDataTable TableFromReader(SqlDataReader reader)
        {
            LightDataTable table = new LightDataTable();

            for (int i = 0; i < reader.FieldCount; i++) {
                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (reader.Read())
            {
                object[] itemArray = new object[reader.FieldCount];

                for (int i = 0; i < itemArray.Length; i++) {
                    itemArray[i] = reader.GetValue(i);
                }

                table.Rows.Add(itemArray);
            }

            return table;
        }
    }
}