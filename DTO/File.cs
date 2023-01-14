using System;

namespace BatchRename
{
    public class File : ICloneable
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}