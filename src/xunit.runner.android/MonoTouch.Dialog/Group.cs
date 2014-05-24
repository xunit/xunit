namespace MonoDroid.Dialog
{
    /// <summary>
    /// Used by root elements to fetch information when they need to
    /// render a summary (Checkbox count or selected radio group).
    /// </summary>
    internal class Group
    {
        public string Key;

        public Group(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Captures the information about mutually exclusive elements in a RootElement
    /// </summary>
    internal class RadioGroup : Group
    {
        public int Selected;

        public RadioGroup(string key, int selected)
            : base(key)
        {
            Selected = selected;
        }

        public RadioGroup(int selected)
            : base(null)
        {
            Selected = selected;
        }
    }
}