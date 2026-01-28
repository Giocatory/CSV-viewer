using System.Collections.ObjectModel;

namespace CsvViewer.Models
{
    public class CsvModel
    {
        public ObservableCollection<string> Headers { get; set; } = new();
        public ObservableCollection<EditableRow> Rows { get; set; } = new();
        public char Delimiter { get; set; } = ',';
        public string FilePath { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;

        public bool HasUnsavedChanges =>
            Rows.Any(r => r.IsModified) || string.IsNullOrEmpty(FilePath);
    }
}