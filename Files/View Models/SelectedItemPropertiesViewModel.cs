using Files.Filesystem;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace Files.View_Models
{
    public class SelectedItemPropertiesViewModel : INotifyPropertyChanged
    {
        private ListedItem scopedItem;
        public ListedItem ScopedItem 
        { 
            get
            {
                return scopedItem;
            }
            set
            {
                scopedItem = value;
                NotifyPropertyChanged("ScopedItem");
                // Set properties for item in scope accordingly
                ItemName = value?.ItemName;
                NotifyPropertyChanged("ItemName");
                ItemType = value?.ItemType;
                NotifyPropertyChanged("ItemType");
                ItemPath = value?.ItemPath;
                NotifyPropertyChanged("ItemPath");
                ItemSize = value?.FileSize;
                NotifyPropertyChanged("FileSize");
                ItemModifiedTimestamp = value?.ItemDateModified;
                NotifyPropertyChanged("ItemDateModified");
                FileIconSource = value?.FileImage;
                NotifyPropertyChanged("FileImage");
                if(value != null)
                {
                    LoadFolderGlyph = value.LoadFolderGlyph;
                    NotifyPropertyChanged("LoadFolderGlyph");
                    LoadUnknownTypeGlyph = value.LoadUnknownTypeGlyph;
                    NotifyPropertyChanged("LoadUnknownTypeGlyph");
                    LoadFileIcon = value.LoadFileIcon;
                    NotifyPropertyChanged("LoadFileIcon");
                }
                else
                {
                    LoadFolderGlyph = false;
                    NotifyPropertyChanged("LoadFolderGlyph");
                    LoadUnknownTypeGlyph = false;
                    NotifyPropertyChanged("LoadUnknownTypeGlyph");
                    LoadFileIcon = false;
                    NotifyPropertyChanged("LoadFileIcon");
                }
                
            }
        }

        public string ItemName { get;  internal set; }
        public string ItemType { get; internal set; }
        public string ItemPath { get; internal set; }
        public string ItemSize { get; internal set; }
        public string ItemModifiedTimestamp { get; internal set; }
        public ImageSource FileIconSource { get; internal set; }
        public bool LoadFolderGlyph { get; internal set; }
        public bool LoadUnknownTypeGlyph { get; internal set; }
        public bool LoadFileIcon { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
