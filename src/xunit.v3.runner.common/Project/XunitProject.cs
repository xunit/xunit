using System.Collections;
using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// FOR INTERNAL USE ONLY.
    /// </summary>
    public class XunitProject : IEnumerable<XunitProjectAssembly>
    {
        readonly List<XunitProjectAssembly> assemblies;

        /// <summary/>
        public XunitProject()
        {
            assemblies = new List<XunitProjectAssembly>();
            Filters = new XunitFilters();
            Output = new Dictionary<string, string>();
        }

        /// <summary/>
        public ICollection<XunitProjectAssembly> Assemblies
        {
            get { return assemblies; }
        }

        /// <summary/>
        public XunitFilters Filters { get; private set; }

        /// <summary/>
        public Dictionary<string, string> Output { get; set; }

        /// <summary/>
        public void Add(XunitProjectAssembly assembly)
        {
            Guard.ArgumentNotNull("assembly", assembly);

            assemblies.Add(assembly);
        }

        /// <summary/>
        public IEnumerator<XunitProjectAssembly> GetEnumerator()
        {
            return assemblies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
