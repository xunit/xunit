using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// TODO: Move to xunit.dll when Assert is ported
public class CollectionAssert
{
    public static void Collection<T>(IEnumerable<T> collection, params Action<T>[] elementInspectors)
    {
        T[] elements = collection.ToArray();
        Assert.Equal(elementInspectors.Length, elements.Length);

        for (int idx = 0; idx < elements.Length; idx++)
            elementInspectors[idx](elements[idx]);

        // REVIEW: Should this print out the contents of the collection when it fails?
    }
}
