using System;
using System.Collections;
using System.Collections.Generic;
using Android.Views;

namespace MonoDroid.Dialog
{
    internal class ViewElement : IEnumerable<View>
    {
        public ViewElement(object o, View view, bool b)
        {
            
        }

        public IEnumerator<View> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}