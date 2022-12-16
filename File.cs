using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename
{
    public class File : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }

        public File()
        {
            Path = "";
            Name = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}