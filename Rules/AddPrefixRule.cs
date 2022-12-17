using System.Text;

namespace BatchRename.Rules
{
    public class AddPrefixRule : IRule
    {
        public string Prefix { get; set; } = "";
        public string Name => "AddPrefix";

        public string Rename(string origin)
        {
            var builder = new StringBuilder();

            builder.Append(Prefix);
            builder.Append(origin);
            builder.Append(' ');

            string result = builder.ToString();
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Apply(string[] presetPairs)
        {
            // Get preset values
            var prefixPreset = presetPairs[0];
            var prefix = prefixPreset.Split("=")[1];

            // Apply the presets
            Prefix = prefix;
        }
    }
}