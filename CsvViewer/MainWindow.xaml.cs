using CsvViewer.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CsvViewer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.Headers))
                    GenerateColumns();
            };
        }

        private void GenerateColumns()
        {
            // Очищаем старые столбцы (кроме #)
            while (dataGrid.Columns.Count > 1)
                dataGrid.Columns.RemoveAt(1);

            // Создаём новые столбцы
            for (int i = 0; i < _viewModel.Headers.Count; i++)
            {
                var col = new DataGridTextColumn
                {
                    Header = _viewModel.Headers[i],
                    Binding = new System.Windows.Data.Binding($"[{i}]")
                    {
                        Mode = System.Windows.Data.BindingMode.TwoWay,
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                    },
                    SortMemberPath = $"[{i}]",
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };
                dataGrid.Columns.Add(col);
            }
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // Можно добавить логику для валидации перед редактированием
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Обновляем статус при изменении
                if (_viewModel.Rows.Any(r => r.IsModified))
                {
                    _viewModel.StatusText = $"Есть несохранённые изменения ({_viewModel.Rows.Count(r => r.IsModified)} изменённых строк)";
                }
            }
        }
    }
}