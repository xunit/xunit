using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that the provided object raised <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// as a result of executing the given test code.
        /// </summary>
        /// <param name="object">The object which should raise the notification</param>
        /// <param name="propertyName">The property name for which the notification should be raised</param>
        /// <param name="testCode">The test code which should cause the notification to be raised</param>
        /// <param name="expectedValue">The expected value of the property as the notification is raised</param>
        /// <exception cref="PropertyChangedException">Thrown when the notification is not raised, or if 
        /// given property was not updated with expected value when notification was raised.</exception>
        public static void PropertyChanged(INotifyPropertyChanged @object, string propertyName, Action testCode, object expectedValue = null)
        {
            Assert.GuardArgumentNotNull("object", @object);
            Assert.GuardArgumentNotNull("testCode", testCode);

            bool propertyChangeHappened = false;
            object actualValue = null;

            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (propertyName.Equals(args.PropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    propertyChangeHappened = true;
                    if (expectedValue != null)
                    {
                        actualValue = @object.GetType().GetRuntimeProperties().Single(property => property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).GetValue(@object);
                    }
                }
            };

            @object.PropertyChanged += handler;

            try
            {
                testCode();
                if (!propertyChangeHappened)
                    throw new PropertyChangedException(propertyName);

                if (expectedValue != null && !expectedValue.Equals(actualValue))
                    throw new PropertyChangedException(propertyName, expectedValue);
            }
            finally
            {
                @object.PropertyChanged -= handler;
            }
        }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("You must call Assert.PropertyChangedAsync (and await the result) when testing async code.", true)]
        [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This is a purposeful use of NotImplementedException")]
        public static void PropertyChanged(INotifyPropertyChanged @object, string propertyName, Func<Task> testCode, object expectedValue = null) { throw new NotImplementedException(); }

        /// <summary>
        /// Verifies that the provided object raised <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// as a result of executing the given test code.
        /// </summary>
        /// <param name="object">The object which should raise the notification</param>
        /// <param name="propertyName">The property name for which the notification should be raised</param>
        /// <param name="testCode">The test code which should cause the notification to be raised</param>
        /// <param name="expectedValue"></param>
        /// <exception cref="PropertyChangedException">Thrown when the notification is not raised</exception>
        public static async Task PropertyChangedAsync(INotifyPropertyChanged @object, string propertyName, Func<Task> testCode, string expectedValue = null)
        {
            Assert.GuardArgumentNotNull("object", @object);
            Assert.GuardArgumentNotNull("testCode", testCode);

            bool propertyChangeHappened = false;
            object actualValue = null;

            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (propertyName.Equals(args.PropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    propertyChangeHappened = true;
                    if (expectedValue != null)
                    {
                        actualValue = @object.GetType().GetRuntimeProperties().Single(property => property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).GetValue(@object);
                    }
                }
            };

            @object.PropertyChanged += handler;

            try
            {
                await testCode();
                if (!propertyChangeHappened)
                    throw new PropertyChangedException(propertyName);

                if (expectedValue != null && !expectedValue.Equals(actualValue))
                    throw new PropertyChangedException(propertyName, expectedValue);
            }
            finally
            {
                @object.PropertyChanged -= handler;
            }
        }
    }
}