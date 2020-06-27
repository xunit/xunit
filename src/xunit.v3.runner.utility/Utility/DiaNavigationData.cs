// TODO: Should all the DIA code just move to the VSTest adapter?

#nullable disable

namespace Xunit
{
    class DiaNavigationData
    {
        public string FileName { get; set; }

        public int LineNumber { get; set; }
    }
}
