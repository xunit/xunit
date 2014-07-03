using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        readonly IEnumerable<TElement> elements;

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            this.elements = elements;
        }

        public TKey Key { get; private set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }
}
