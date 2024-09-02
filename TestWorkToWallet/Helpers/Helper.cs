using System.Text.RegularExpressions;

namespace TestWorkToWallet.Helpers
{
    public class Helper
    {
        public bool ContainsSqlInjection(string input)
        {
            string[] sqlInjectionPatterns = {
                "select", "insert", "update", "delete", "drop", "--", ";--", "/*", "*/", "@@", "char", "nchar",
                "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "exec",
                "execute", "fetch", "kill", "sys", "sysobjects", "syscolumns", "table", "update"
            };

            string lowerInput = input.ToLower();

            foreach (string pattern in sqlInjectionPatterns)
            {
                if (lowerInput.Contains(pattern))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
