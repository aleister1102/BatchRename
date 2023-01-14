using BaseRule;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BatchRename
{
    public class RuleInfo : INotifyPropertyChanged, ICloneable
    {
        public IRule Rule { get; set; }
        public bool State { get; set; } = false;
        public int Order { get; set; } = 0;
        public ObservableCollection<RuleConfig> Configs { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Activate() => State = RuleStatus.Active;

        public void Deactivate() => State = RuleStatus.Inactive;

        public bool IsActive() => State == RuleStatus.Active;

        public void ChangeOrder(int value) => Order = value;

        public void ReduceOrder() => Order -= 1;

        public bool Validate()
        {
            bool result = true;

            foreach (var config in Configs)
            {
                config.Message = Rule.Validate(config.Name, config.Value);
                result = result && config.Message == "";
            }

            return result;
        }
    }
}