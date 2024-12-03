namespace Server.Models
{
    public class ApiResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool SystemReport { get; set; }
        public List<string> Columns { get; set; }
        public List<string> LocalizedColumns { get; set; }
        public List<List<object>> Rows { get; set; } 
        public int TotalRowCount { get; set; }
        public List<string> ColumnTypes { get; set; }
        public string ReportPreviewSql { get; set; }
    }

}
