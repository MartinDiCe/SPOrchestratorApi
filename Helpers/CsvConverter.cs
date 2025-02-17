using System.Text;

namespace SPOrchestratorAPI.Helpers
{
    public static class CsvConverter
    {
        public static string ConvertToCsv(List<Dictionary<string, object>>? rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return string.Empty;
            }

            var csvBuilder = new StringBuilder();

            var headers = new List<string>(rows[0].Keys);
            csvBuilder.AppendLine(string.Join(",", headers));

            foreach (var row in rows)
            {
                var values = new List<string>();
                foreach (var header in headers)
                {
                    var value = row[header]?.ToString() ?? "";
                    
                    value = value.Replace("\"", "\"\"");
                    if (value.Contains(","))
                    {
                        value = $"\"{value}\"";
                    }

                    values.Add(value);
                }

                csvBuilder.AppendLine(string.Join(",", values));
            }

            return csvBuilder.ToString();
        }
    }
}