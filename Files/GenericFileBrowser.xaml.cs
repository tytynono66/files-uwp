using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Files.Enums;
using Files.Filesystem;
using Windows.System;
using Windows.UI.Xaml.Input;
using System.Linq;

namespace Files
{
    public sealed partial class GenericFileBrowser : BaseLayout
    {
        
        public string previousFileName;
        private DataGridColumn _sortedColumn;
        public DataGridColumn SortedColumn
        {
            get
            {
                return _sortedColumn;
            }
            set
            {
                if (value == nameColumn)
                    App.occupiedInstance.DirectorySortOption = SortOption.Name;
                else if (value == dateColumn)
                    App.occupiedInstance.DirectorySortOption = SortOption.DateModified;
                else if (value == typeColumn)
                    App.occupiedInstance.DirectorySortOption = SortOption.FileType;
                else if (value == sizeColumn)
                    App.occupiedInstance.DirectorySortOption = SortOption.Size;
                else
                    App.occupiedInstance.DirectorySortOption = SortOption.Name;

                if (value != _sortedColumn)
                {
                    // Remove arrow on previous sorted column
                    if (_sortedColumn != null)
                        _sortedColumn.SortDirection = null;
                }
                value.SortDirection = App.occupiedInstance.DirectorySortDirection == SortDirection.Ascending ? DataGridSortDirection.Ascending : DataGridSortDirection.Descending;
                _sortedColumn = value;
            }
        }

        public GenericFileBrowser()
        {
            this.InitializeComponent();

            switch (App.occupiedInstance.DirectorySortOption)
            {
                case SortOption.Name:
                    SortedColumn = nameColumn;
                    break;
                case SortOption.DateModified:
                    SortedColumn = dateColumn;
                    break;
                case SortOption.FileType:
                    SortedColumn = typeColumn;
                    break;
                case SortOption.Size:
                    SortedColumn = sizeColumn;
                    break;
            }

            App.occupiedInstance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DirectorySortOption")
            {
                switch (App.occupiedInstance.DirectorySortOption)
                {
                    case SortOption.Name:
                        SortedColumn = nameColumn;
                        break;
                    case SortOption.DateModified:
                        SortedColumn = dateColumn;
                        break;
                    case SortOption.FileType:
                        SortedColumn = typeColumn;
                        break;
                    case SortOption.Size:
                        SortedColumn = sizeColumn;
                        break;
                }
            }
            else if (e.PropertyName == "DirectorySortDirection")
            {
                // Swap arrows
                SortedColumn = _sortedColumn;
            }
        }

        private void AllView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

        }

        private async void AllView_DropAsync(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                App.occupiedInstance.InteractionOperations.itemsPasted = 0;
                App.occupiedInstance.InteractionOperations.ItemsToPaste = await e.DataView.GetStorageItemsAsync();
                foreach (IStorageItem item in await e.DataView.GetStorageItemsAsync())
                {
                    if (item.IsOfType(StorageItemTypes.Folder))
                    {
                        App.occupiedInstance.InteractionOperations.CloneDirectoryAsync((item as StorageFolder).Path, App.occupiedInstance.instanceViewModel.Universal.path, (item as StorageFolder).DisplayName, false);
                    }
                    else
                    {
                        App.occupiedInstance.UpdateProgressFlyout(InteractionOperationType.PasteItems, ++App.occupiedInstance.InteractionOperations.itemsPasted, App.occupiedInstance.InteractionOperations.ItemsToPaste.Count);
                        await (item as StorageFile).CopyAsync(await StorageFolder.GetFolderFromPathAsync(App.occupiedInstance.instanceViewModel.Universal.path));
                    }
                }
            }
        }

        private void AllView_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var textBox = e.EditingElement as TextBox;
            var selectedItem = AllView.SelectedItem as ListedItem;
            int extensionLength = selectedItem.DotFileExtension?.Length ?? 0;

            previousFileName = selectedItem.FileName;
            textBox.Focus(FocusState.Programmatic); // Without this, cannot edit text box when renaming via right-click
            textBox.Select(0, selectedItem.FileName.Length - extensionLength);
        }

        private async void AllView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
                return;

            var selectedItem = AllView.SelectedItem as ListedItem;
            string currentName = previousFileName;
            string newName = (e.EditingElement as TextBox).Text;

            bool successful = await App.occupiedInstance.InteractionOperations.RenameFileItem(selectedItem, currentName, newName);
            if (!successful)
            {
                selectedItem.FileName = currentName;
                ((sender as DataGrid).Columns[1].GetCellContent(e.Row) as TextBlock).Text = currentName;
            }
        }

        private void GenericItemView_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            AllView.SelectedItem = null;
            App.occupiedInstance.HomeItems.isEnabled = false;
            App.occupiedInstance.ShareItems.isEnabled = false;
        }

        private void AllView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllView.CommitEdit();
            base.SelectedStorageItems = AllView.SelectedItems?.Cast<ListedItem>().ToList();
            if (e.AddedItems.Count > 0)
            {
                App.occupiedInstance.HomeItems.isEnabled = true;
                App.occupiedInstance.ShareItems.isEnabled = true;
            }
            else if (AllView.SelectedItems.Count == 0)
            {
                App.occupiedInstance.HomeItems.isEnabled = false;
                App.occupiedInstance.ShareItems.isEnabled = false;
            }
        }

        private void AllView_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
        }
        
        private void AllView_Sorting(object sender, DataGridColumnEventArgs e)
        {
            if (e.Column == SortedColumn)
                App.occupiedInstance.IsSortedAscending = !App.occupiedInstance.IsSortedAscending;
            else if (e.Column != iconColumn)
                SortedColumn = e.Column;
        }

        private void AllView_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                App.occupiedInstance.InteractionOperations.List_ItemClick(null, null);
                e.Handled = true;
            }
        }
    }
}
