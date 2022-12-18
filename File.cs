using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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