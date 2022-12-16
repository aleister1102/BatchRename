using System.Text;

namespace BatchRename.Rules
{
    public class ReplaceSpecialCharsRule : IRule
    {
        public string SpecialChars { get; set; } = "";
        public string Replacement { get; set; } = "";
        public string Name => "ReplaceSpecialChars";

        public string Rename(string origin)
        {
            var builder = new StringBuilder();

            foreach (var c in origin)
            {
                if (SpecialChars.Contains(c))
                {
                    builder.Append(Replacement);
                }
                else
                {
                    builder.Append(c);
                }
            }
            string result = builder.ToString();
            return result;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Apply(string[] configPairs)
        {
            // Get config values
            var specialCharsConfig = configPairs[0];
            var specialChars = specialCharsConfig.Split("=")[1];

            var replacementConfig = configPairs[1];
            var replacement = replacementConfig.Split("=")[1];

            // Apply the configs
            SpecialChars = specialChars;
            Replacement = replacement;
        }
    }
}