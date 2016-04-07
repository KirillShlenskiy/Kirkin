using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
            if (ExecuteButton.Tag != null)
            {
                ((CancellationTokenSource)ExecuteButton.Tag).Cancel();

                ExecuteButton.Enabled = false;
            }
            else
            {
                ExecuteDiff();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) {
                ExecuteDiff();
            }
        }

        private async void ExecuteDiff()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            ExecuteButton.Tag = cts;
            ExecuteButton.Text = "Cancel";

            try
            {
                Text = DefaultText + ": executing left ...";
                KeyValuePair<LightDataSet, TimeSpan> result1 = await ProduceDataSetAsync(ConnectionStringTextBox1.Text, CommandTextTextBox1.Text, cts.Token);
                Text = DefaultText + ": executing right ...";
                KeyValuePair<LightDataSet, TimeSpan> result2 = await ProduceDataSetAsync(ConnectionStringTextBox2.Text, CommandTextTextBox2.Text, cts.Token);
                cts.Token.ThrowIfCancellationRequested();

                LightDataSet ds1 = result1.Key;
                LightDataSet ds2 = result2.Key;
                TimeSpan timeTaken1 = result1.Value;
                TimeSpan timeTaken2 = result2.Value;

                Text = DefaultText + ": comparing ...";
                Stopwatch diffStopwatch = Stopwatch.StartNew();
                DiffResult diff = await Task.Run(() => DataSetDiff.Compare(ds1, ds2));

                cts.Token.ThrowIfCancellationRequested();
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
            catch (OperationCanceledException)
            {
                // Expected.
            }
            finally
            {
                ExecuteButton.Tag = null;
                ExecuteButton.Text = "Execute";
                ExecuteButton.Enabled = true;
                Text = DefaultText;
            }
        }

        private static async Task<KeyValuePair<LightDataSet, TimeSpan>> ProduceDataSetAsync(string connectionString, string commandText, CancellationToken ct)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    command.CommandTimeout = 0;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        LightDataSet ds = new LightDataSet();

                        while (true)
                        {
                            ds.Tables.Add(await TableFromReaderAsync(reader, ct).ConfigureAwait(false));

                            if (!await reader.NextResultAsync(ct).ConfigureAwait(false)) {
                                break;
                            }
                        }

                        return new KeyValuePair<LightDataSet, TimeSpan>(ds, sw.Elapsed);
                    }
                }
            }
        }

        private static async Task<LightDataTable> TableFromReaderAsync(SqlDataReader reader, CancellationToken ct)
        {
            LightDataTable table = new LightDataTable();

            for (int i = 0; i < reader.FieldCount; i++) {
                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                object[] itemArray = new object[reader.FieldCount];

                for (int i = 0; i < itemArray.Length; i++) {
                    itemArray[i] = reader[i];
                }

                table.Rows.Add(itemArray);
            }

            table.Rows.TrimExcess(); // Manage GC pressure.

            return table;
        }
    }
}