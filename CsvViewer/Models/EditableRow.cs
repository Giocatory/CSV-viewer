using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CsvViewer.Models
{
    /// <summary>
    /// Редактируемая строка с индексатором для привязки к DataGrid
    /// Полностью исправлено отслеживание изменений ячеек
    /// </summary>
    public class EditableRow : INotifyPropertyChanged
    {
        private readonly ObservableCollection<EditableCell> _cells = new();
        private bool _isModified;

        public EditableRow()
        {
            // Подписываемся на изменения коллекции ячеек
            _cells.CollectionChanged += (s, e) =>
            {
                // Обрабатываем добавление новых ячеек
                if (e.NewItems != null)
                {
                    foreach (EditableCell cell in e.NewItems)
                    {
                        cell.PropertyChanged += Cell_PropertyChanged;
                    }
                }

                // Обрабатываем удаление ячеек
                if (e.OldItems != null)
                {
                    foreach (EditableCell cell in e.OldItems)
                    {
                        cell.PropertyChanged -= Cell_PropertyChanged;
                    }
                }

                UpdateIsModified();
            };
        }

        public EditableRow(IEnumerable<string> values) : this()
        {
            foreach (var value in values)
                _cells.Add(new EditableCell(value));

            UpdateIsModified();
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditableCell.IsModified))
                UpdateIsModified();
        }

        private void UpdateIsModified()
        {
            bool wasModified = _isModified;
            _isModified = _cells.Any(c => c.IsModified);

            if (wasModified != _isModified)
            {
                OnPropertyChanged(nameof(IsModified));
                // Обновляем все привязки для строки
                OnPropertyChanged(string.Empty);
            }
        }

        // Индексатор для привязки в XAML: Binding="{Binding [0]}"
        public string this[int index]
        {
            get => index < _cells.Count ? _cells[index].Value : string.Empty;
            set
            {
                if (index < _cells.Count)
                {
                    _cells[index].Value = value;
                }
                else if (index == _cells.Count)
                {
                    _cells.Add(new EditableCell(value));
                }
            }
        }

        public ObservableCollection<EditableCell> Cells => _cells;

        public bool IsModified => _isModified;

        public void AcceptChanges()
        {
            foreach (var cell in _cells)
                cell.AcceptChanges();

            UpdateIsModified();
        }

        public void RejectChanges()
        {
            foreach (var cell in _cells)
                cell.RejectChanges();

            UpdateIsModified();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}