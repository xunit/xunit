using System;
using Android.Content;
using Android.Widget;

namespace MonoDroid.Dialog
{
    internal class DialogHelper
    {
        private Context context;
        private RootElement formLayer;

        //public event Action<Section, Element> ElementClick;
        //public event Action<Section, Element> ElementLongClick;

        public RootElement Root { get; set; }

        private DialogAdapter DialogAdapter { get; set; }

        public DialogHelper(Context context, ListView dialogView, RootElement root)
        {
            this.Root = root;
            this.Root.Context = context;

            dialogView.Adapter = this.DialogAdapter = new DialogAdapter(context, this.Root);
            dialogView.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(ListView_ItemClick);
            //dialogView.ItemLongClick += new EventHandler<ItemEventArgs>(ListView_ItemLongClick);
            dialogView.Tag = root;
        }

        void ListView_ItemLongClick(object sender, ItemEventArgs e)
        {
            var elem = this.DialogAdapter.ElementAtIndex(e.Position);
            if (elem != null && elem.LongClick != null)
                elem.LongClick(sender, e);
        }

        void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var elem = this.DialogAdapter.ElementAtIndex(e.Position);
            if (elem != null && elem.Click != null)
                elem.Click(sender, e);
        }
    }
}