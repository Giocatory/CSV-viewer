using CsvViewer.Models;
using CsvViewer.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CsvViewer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly CsvModel _csvData = new();
        private ICollectionView _view;
        private string _filterText = string.Empty;
        private string _statusText = "Файл не загружен";
        private char _selectedDelimiter = ',';

        public ObservableCollection<string> Headers => _csvData.Headers;
        public ObservableCollection<EditableRow> Rows => _csvData.Rows;
        public ICollectionView DataView
        {
            get => _view;
            private set { _view = value; OnPropertyChanged(nameof(DataView)); }
        }

        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(nameof(FilterText)); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        public char SelectedDelimiter
        {
            get => _selectedDelimiter;
            set { _selectedDelimiter = value; _csvData.Delimiter = value; OnPropertyChanged(nameof(SelectedDelimiter)); }
        }

        public List<char> AvailableDelimiters { get; } = new() { ',', ';', '\t', '|' };

        public ICommand OpenCommand { get; }
        public ICommand ExportJsonCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand RejectChangesCommand { get; }
        public ICommand FilterCommand { get; }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(OpenFile);
            ExportJsonCommand = new RelayCommand(ExportToJson);
            ExportCsvCommand = new RelayCommand(ExportToCsv);
            SaveChangesCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            RejectChangesCommand = new RelayCommand(RejectChanges, CanRejectChanges);
            FilterCommand = new RelayCommand(ApplyFilter);
        }

        private void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV/XLSX Files (*.csv;*.xlsx)|*.csv;*.xlsx|CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Выберите файл данных"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                    List<string> headers;
                    List<EditableRow> rows;

                    using (var stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        if (extension == ".xlsx" || ExcelParser.IsXlsxFile(stream))
                        {
                            stream.Position = 0;
                            (headers, rows) = ExcelParser.Parse(stream);
                            _csvData.FileExtension = ".xlsx";
                            StatusText = "Загрузка XLSX файла...";
                        }
                        else
                        {
                            stream.Position = 0;
                            (headers, rows) = CsvParser.Parse(stream, SelectedDelimiter);
                            _csvData.FileExtension = ".csv";
                            StatusText = "Загрузка CSV файла...";
                        }
                    }

                    // Очистка данных
                    _csvData.Headers.Clear();
                    _csvData.Rows.Clear();
                    _csvData.FilePath = dialog.FileName;

                    foreach (var header in headers)
                        _csvData.Headers.Add(header);

                    foreach (var row in rows)
                        _csvData.Rows.Add(row);

                    // Настройка представления
                    DataView = CollectionViewSource.GetDefaultView(_csvData.Rows);
                    DataView.Filter = RowFilter;

                    StatusText = $"Загружено {_csvData.Rows.Count} строк, {_csvData.Headers.Count} столбцов ({Path.GetFileName(_csvData.FilePath)})";
                    OnPropertyChanged(nameof(Headers));
                    OnPropertyChanged(nameof(Rows));
                    OnPropertyChanged(nameof(SaveChangesCommand));
                    OnPropertyChanged(nameof(RejectChangesCommand));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText = $"Ошибка: {ex.Message}";
                }
            }
        }

        private bool RowFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
                return true;

            if (item is EditableRow row)
            {
                var searchText = FilterText.ToLowerInvariant();
                return row.Cells.Any(cell => cell.Value?.ToLowerInvariant().Contains(searchText) == true);
            }
            return false;
        }

        private void ApplyFilter()
        {
            DataView?.Refresh();
            int visibleCount = DataView?.Cast<object>().Count() ?? 0;
            StatusText = $"Отображается {visibleCount} из {_csvData.Rows.Count} строк";
        }

        private void ExportToJson()
        {
            if (_csvData.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Сохранить как JSON",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    FileExporter.ExportToJson(_csvData, dialog.FileName);
                    MessageBox.Show($"Данные экспортированы:\n{dialog.FileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToCsv()
        {
            if (_csvData.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Сохранить как CSV",
                DefaultExt = "csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    FileExporter.ExportToCsv(_csvData, dialog.FileName, SelectedDelimiter);
                    MessageBox.Show($"Данные экспортированы:\n{dialog.FileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveChanges()
        {
            if (string.IsNullOrEmpty(_csvData.FilePath))
            {
                // Сохранить как...
                var dialog = new SaveFileDialog
                {
                    Filter = _csvData.FileExtension == ".xlsx"
                        ? "Excel Files (*.xlsx)|*.xlsx"
                        : "CSV Files (*.csv)|*.csv",
                    Title = "Сохранить изменения",
                    DefaultExt = _csvData.FileExtension.TrimStart('.')
                };

                if (dialog.ShowDialog() != true)
                    return;

                _csvData.FilePath = dialog.FileName;
            }

            try
            {
                if (_csvData.FileExtension == ".xlsx")
                {
                    FileExporter.ExportToXlsx(_csvData, _csvData.FilePath);
                }
                else
                {
                    FileExporter.ExportToCsv(_csvData, _csvData.FilePath, _csvData.Delimiter);
                }

                // Сброс флага изменений
                foreach (var row in _csvData.Rows)
                {
                    row.AcceptChanges();
                }

                StatusText = $"Изменения сохранены в {_csvData.FilePath}";
                OnPropertyChanged(nameof(SaveChangesCommand));
                OnPropertyChanged(nameof(RejectChangesCommand));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveChanges() => _csvData.HasUnsavedChanges;

        private void RejectChanges()
        {
            if (MessageBox.Show("Отменить все несохранённые изменения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var row in _csvData.Rows)
                    row.RejectChanges();

                StatusText = "Изменения отменены";
                OnPropertyChanged(nameof(SaveChangesCommand));
                OnPropertyChanged(nameof(RejectChangesCommand));
            }
        }

        private bool CanRejectChanges() => _csvData.HasUnsavedChanges;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            // Обновляем команды при изменении данных
            if (name == nameof(Rows) || name == nameof(Headers))
            {
                (SaveChangesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectChangesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    // Расширенная реализация ICommand с поддержкой CanExecute
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}