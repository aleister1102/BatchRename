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

        public IRule Apply(string configsLine)
        {
            var configs = configsLine.Split(':')[1];

            var rule = new ReplaceSpecialCharsRule();
            return rule;
        }
    }
}