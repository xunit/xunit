using System;
using System.ComponentModel;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that the provided object raised INotifyPropertyChanged.PropertyChanged
        /// as a result of executing the given test code.
        /// </summary>
        /// <param name="object">The object which should raise the notification</param>
        /// <param name="propertyName">The property name for which the notification should be raised</param>
        /// <param name="testCode">The test code which should cause the notification to be raised</param>
        /// <exception cref="PropertyChangedException">Thrown when the notification is not raised</exception>
        public static void PropertyChanged(INotifyPropertyChanged @object, string propertyName, Action testCode)
        {
            Assert.GuardArgumentNotNull("object", @object);
            Assert.GuardArgumentNotNull("testCode", testCode);

            Object oldPropertyValue = null;
            try
            {
                oldPropertyValue = @object.GetType().GetRuntimeProperty(propertyName).GetValue(@object);

            }
            catch (Exception)
            {
                throw new InaccessiblePropertyException(propertyName);
            }

            Object newPropertyValue = null;
            bool propertyChangeHappened = false;

            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (propertyName.Equals(args.PropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    propertyChangeHappened = true;
                    newPropertyValue = @object.GetType().GetRuntimeProperty(propertyName).GetValue(@object);
                }
            };

            @object.PropertyChanged += handler;

            try
            {

                testCode();
                if (!propertyChangeHappened)
                    throw new PropertyChangedException(propertyName);
                if (Object.Equals(oldPropertyValue, newPropertyValue))
                {
                    throw new PropertyChangedPrematurelyException(propertyName);
                }

            }
            finally
            {
                @object.PropertyChanged -= handler;
            }
        }
    }
}