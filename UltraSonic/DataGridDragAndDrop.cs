using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace UltraSonic
{
    public class DataGridDragAndDrop<T>
    {
        private bool IsEditing { get; set; }
        private bool IsDragging { get; set; }
        private ObservableCollection<T> Collection {get; set;}
        private DataGrid DataGrid { get; set; }
        private DependencyProperty DraggedItemProperty { get; set; }
        private Popup Popup { get; set; }
        private Window Window { get; set; }

        public static DataGridDragAndDrop<T> Create(ObservableCollection<T> collection, DataGrid dataGrid, Window window, Popup popup)
        {
            var test = new DataGridDragAndDrop<T>
                {
                    Collection = collection, 
                    DataGrid = dataGrid,
                    Popup = popup,
                    Window = window,
                    DraggedItemProperty = DependencyProperty.Register("DraggedItem", typeof(T), window.GetType())
                };

            return test;
        }

        private T DraggedItem
        {
            get { return (T)Window.GetValue(DraggedItemProperty); }
            set { Window.SetValue(DraggedItemProperty, value); }
        }

        public void DataGridOnBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            IsEditing = true;
            if (IsDragging) ResetDragDrop();
        }

        public void DataGridOnEndEdit(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsEditing = false;
        }

        public void DataGridOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEditing) return;

            var row = UiHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(DataGrid));
            if (row == null || row.IsEditing) return;

            IsDragging = true;
            DraggedItem = (T)row.Item;
        }

        public void DataGridOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDragging || IsEditing)
            {
                return;
            }

            T targetItem = DataGrid.SelectedItem is T ? (T) DataGrid.SelectedItem : default(T);

            if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem))
            {
                Collection.Remove(DraggedItem);
                var targetIndex = Collection.IndexOf(targetItem);
                Collection.Insert(targetIndex, DraggedItem);
                DataGrid.SelectedItem = DraggedItem;
            }

            ResetDragDrop();
        }

        public void DataGridOnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;

            if (!Popup.IsOpen)
            {
                DataGrid.IsReadOnly = true;
                Popup.IsOpen = true;
            }

            Size popupSize = new Size(Popup.ActualWidth, Popup.ActualHeight);
            Popup.PlacementRectangle = new Rect(e.GetPosition(Window), popupSize);

            Point position = e.GetPosition(DataGrid);
            var row = UiHelpers.TryFindFromPoint<DataGridRow>(DataGrid, position);
            if (row != null) DataGrid.SelectedItem = row.Item;
        }

        private void ResetDragDrop()
        {
            IsDragging = false;
            Popup.IsOpen = false;
            DataGrid.IsReadOnly = false;
        }
    }
}
