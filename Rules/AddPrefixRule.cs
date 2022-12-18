using System.Diagnostics;
using System.IO;
using System.Text;
using BatchRenamePlugins;

namespace BatchRename.Rules
{
    internal class AddPrefixRule : IRule
    {
        public string Prefix { get; set; } = string.Empty;
        public string Name => "AddPrefix";

        public void Apply(string[] presetPairs)
        {
            var prefixPreset = presetPairs[0];
            var prefix = prefixPreset.Split("=")[1];

            Prefix = prefix;
        }

        public object Clone()
        {
            return MemberwiseClone();
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