using System;
using System.Collections;
using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// FOR INTERNAL USE ONLY.
    /// </summary>
    public class XunitProject : IEnumerable<XunitProjectAssembly>
    {
        readonly IEnumerable<XunitProjectAssembly> _assemblies;

        /// <summary/>
        public XunitProject(IEnumerable<XunitProjectAssembly> assemblies)
        {
            _assemblies = assemblies;
            Filters = new XunitFilters();
            Output = new Dictionary<string, string>();
        }

        /// <summary/>
        public IEnumerable<XunitProjectAssembly> Assemblies
        {
            get { return _assemblies; }
        }

        /// <summary/>
        public XunitFilters Filters { get; private set; }

        /// <summary/>
        public Dictionary<string, string> Output { get; set; }

        /// <summary/>
        public IEnumerator<XunitProjectAssembly> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
