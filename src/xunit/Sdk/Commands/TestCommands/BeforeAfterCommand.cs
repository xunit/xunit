using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Sdk
{
	/// <summary>
	/// Implementation of <see cref="ITestCommand"/> which executes the
	/// <see cref="BeforeAfterTestAttribute"/> instances attached to a test method.
	/// </summary>
	public class BeforeAfterCommand : DelegatingTestCommand
	{
		MethodInfo testMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="BeforeAfterCommand"/> class.
		/// </summary>
		/// <param name="innerCommand">The inner command.</param>
		/// <param name="testMethod">The method.</param>
		public BeforeAfterCommand(ITestCommand innerCommand, MethodInfo testMethod)
			: base(innerCommand)
		{
			this.testMethod = testMethod;
		}

		/// <summary>
		/// Executes the test method.
		/// </summary>
		/// <param name="testClass">The instance of the test class</param>
		/// <returns>Returns information about the test run</returns>
		[SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses", Justification = "We do actually verify this. Promise!")]
		public override MethodResult Execute(object testClass)
		{
			List<BeforeAfterTestAttribute> beforeCalled = new List<BeforeAfterTestAttribute>();
			bool testExceptionThrown = false;

			try
			{
				foreach (var attr in GetBeforeAfterTestAttributes())
				{
					attr.Before(testMethod);
					beforeCalled.Add(attr);
				}

				return InnerCommand.Execute(testClass);
			}
			catch
			{
				testExceptionThrown = true;
				throw;
			}
			finally
			{
				List<Exception> afterExceptions = new List<Exception>();
				beforeCalled.Reverse();

				foreach (BeforeAfterTestAttribute attr in beforeCalled)
					try
					{
						attr.After(testMethod);
					}
					catch (Exception ex)
					{
						afterExceptions.Add(ex);
					}

				if (!testExceptionThrown && afterExceptions.Count > 0)
					throw new AfterTestException(afterExceptions);
			}
		}

		private IEnumerable<BeforeAfterTestAttribute> GetBeforeAfterTestAttributes()
		{
			foreach (BeforeAfterTestAttribute beforeAfterTestAttribute in
					testMethod.GetCustomAttributes(typeof(BeforeAfterTestAttribute), true))
			{
				yield return beforeAfterTestAttribute;
			}
			if (testMethod.DeclaringType == null)
				yield break;
			foreach (BeforeAfterTestAttribute beforeAfterTestAttribute in
					testMethod.DeclaringType.GetCustomAttributes(typeof(BeforeAfterTestAttribute), true))
			{
				yield return beforeAfterTestAttribute;
			}
			foreach (BeforeAfterTestAttribute beforeAfterTestAttribute in
					testMethod.DeclaringType.Assembly.GetCustomAttributes(typeof(BeforeAfterTestAttribute), true))
			{
				yield return beforeAfterTestAttribute;
			}
		}
	}
}