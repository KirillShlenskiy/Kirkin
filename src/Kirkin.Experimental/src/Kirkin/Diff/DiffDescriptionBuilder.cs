using System;
using System.Text;

namespace Kirkin.Diff
{
    public static class DiffDescriptionBuilder
    {
        public static string BuildDiffMessage(IDiffResult diffResult)
        {
            if (diffResult == null) throw new ArgumentNullException(nameof(diffResult));

            StringBuilder sb = new StringBuilder();

            BuildMessage(sb, 0, diffResult);

            return sb.ToString();
        }

        private static void BuildMessage(StringBuilder sb, int indenting, IDiffResult diffResult)
        {
            if (!diffResult.AreSame)
            {
                if (indenting != 0) {
                    sb.Append(new string(' ', indenting * 3));
                }

                sb.Append(diffResult.Name);
                sb.Append(": ");
                sb.Append(diffResult.Message);
                sb.AppendLine();

                foreach (IDiffResult childEntry in diffResult.Entries) {
                    BuildMessage(sb, indenting + 1, childEntry);
                }
            }
        }
    }
}