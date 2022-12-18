using System.Text;
using RenamingRulePlugins;

namespace BatchRename.Rules
{
    public class AddPrefixRule : IRule
    {
        public string Prefix { get; set; } = "";
        public string Name => "AddPrefix";

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Apply(string[] presetPairs)
        {
            var prefixPreset = presetPairs[0];
            var prefix = prefixPreset.Split("=")[1];

            Prefix = prefix;
        }

        public string Rename(string origin)
        {
            var builder = new StringBuilder();

            builder.Append(Prefix);
            builder.Append(' ');
            builder.Append(origin);

            string result = builder.ToString();
            return result;
        }
    }
}