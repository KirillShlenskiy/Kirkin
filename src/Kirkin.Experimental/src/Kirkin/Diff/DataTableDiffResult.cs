using System.Data;

namespace Kirkin.Diff
{
    public sealed class DataTableDiffResult : DiffResult
    {
        public DataTable DataTable1 { get; }
        public DataTable DataTable2 { get; }

        internal DataTableDiffResult(bool areSame, string message, DataTable dataTable1, DataTable dataTable2)
            : base(areSame, message)
        {
            DataTable1 = dataTable1;
            DataTable2 = dataTable2;
        }
    }
}