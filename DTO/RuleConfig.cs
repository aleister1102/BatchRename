using System.ComponentModel;

namespace BatchRename
{
    public class RuleConfig : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public RuleConfig(string name, string value, string message = "")
        {
            Name = name;
            Value = value;
            Message = message;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}