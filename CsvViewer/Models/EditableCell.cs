using System.ComponentModel;

namespace CsvViewer.Models
{
    /// <summary>
    /// Редактируемая ячейка с поддержкой отслеживания изменений
    /// </summary>
    public class EditableCell : INotifyPropertyChanged
    {
        private string _value;
        private string _originalValue;

        public EditableCell(string value)
        {
            _value = value ?? string.Empty;
            _originalValue = _value;
        }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value ?? string.Empty;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        public string OriginalValue => _originalValue;
        public bool IsModified => _value != _originalValue;

        public void AcceptChanges()
        {
            _originalValue = _value;
            OnPropertyChanged(nameof(IsModified)); // Обновляем флаг после принятия изменений
        }

        public void RejectChanges()
        {
            // Сбрасываем на оригинальное значение
            Value = _originalValue;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}