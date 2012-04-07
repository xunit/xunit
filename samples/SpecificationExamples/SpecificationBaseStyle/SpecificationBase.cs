using Xunit;

namespace SpecificationBaseStyle
{
    [RunWith(typeof(SpecificationBaseRunner))]
    public abstract class SpecificationBase
    {
        protected virtual void Because() { }

        protected virtual void DestroyContext() { }

        protected virtual void EstablishContext() { }

        internal void OnFinish()
        {
            DestroyContext();
        }

        internal void OnStart()
        {
            EstablishContext();
            Because();
        }
    }
}