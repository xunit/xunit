using System.Collections.Generic;

namespace Xunit.Sdk
{
    // https://github.com/xunit/xunit/issues/3031
    // When the user provides TheoryData<SomeType[]>, the covariance of IEnumerable<T> will silently
    // convert SomeType[] into object[], and thus try to present the data as multiple parameter values
    // of SomeType instead of a single parameter value of SomeType[]. Bypassing tests for IEnumerable<object[]>
    // here allows us to sidestep the covariance problem.
    internal interface ITheoryData
    {
        IReadOnlyCollection<object[]> GetData();
    }
}
