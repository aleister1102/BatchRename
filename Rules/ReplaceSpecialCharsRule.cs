﻿using System.Text;
using RenamingRulePlugins;

namespace BatchRename.Rules
{
    public class ReplaceSpecialCharsRule : IRule
    {
        public string SpecialChars { get; set; } = "";
        public string Replacement { get; set; } = "";
        public string Name => "ReplaceSpecialChars";

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Apply(string[] presetPairs)
        {
            var specialCharsPreset = presetPairs[0];
            var specialChars = specialCharsPreset.Split("=")[1];

            var replacementPreset = presetPairs[1];
            var replacement = replacementPreset.Split("=")[1];

            SpecialChars = specialChars;
            Replacement = replacement;
        }

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
    }
}