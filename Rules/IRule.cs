using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Rules
{
    public interface IRule : ICloneable
    {
        string Name { get; }

        public string Rename(string origin);

        public IRule Apply(string configs);
    }
}