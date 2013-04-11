using System;
using NSubstitute.Core;

public static class NSubExtensions
{
    public static WhenCalledAny<T> WhenAny<T>(this T substitute, Action<T> substituteCall) where T : class
    {
        var context = SubstitutionContext.Current;
        return new WhenCalledAny<T>(context, substitute, substituteCall, MatchArgs.Any);
    }

    public class WhenCalledAny<T> : WhenCalled<T>
        where T : class
    {
        public WhenCalledAny(ISubstitutionContext context, T substitute, Action<T> call, MatchArgs matchArgs)
            : base(context, substitute, call, matchArgs) { }

        public void Do<T>(Action<T> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T)callInfo[0]));
        }

        public void Do<T1, T2>(Action<T1, T2> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1]));
        }

        public void Do<T1, T2, T3>(Action<T1, T2, T3> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2]));
        }

        public void Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3]));
        }

        public void Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3], (T5)callInfo[4]));
        }

        public void Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callbackWithArguments)
        {
            this.Do(callInfo => callbackWithArguments((T1)callInfo[0], (T2)callInfo[1], (T3)callInfo[2], (T4)callInfo[3], (T5)callInfo[4], (T6)callInfo[5]));
        }
    }
}