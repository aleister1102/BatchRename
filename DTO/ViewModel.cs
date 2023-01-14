using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BatchRename
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Presets { get; set; } = new();

        public ObservableCollection<RuleInfo> RulesInfo { get; set; } = new();

        public ObservableCollection<File> Files { get; set; } = new();

        public RuleInfo SelectedRule { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}