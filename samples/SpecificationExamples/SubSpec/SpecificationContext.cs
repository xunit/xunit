using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Xunit.Sdk;

namespace SubSpec
{
    static class SpecificationContext
    {
        static List<KeyValuePair<string, Action>> asserts;
        static KeyValuePair<string, Action>? context;
        static KeyValuePair<string, Action>? @do;
        static Exception exception;

        static SpecificationContext()
        {
            Reset();
        }

        public static void Context(string message, Action contextAction)
        {
            if (exception != null)
                return;

            if (!context.HasValue)
                context = new KeyValuePair<string, Action>(message, contextAction);
            else
                try
                {
                    throw new InvalidOperationException("Cannot have two Context statements in one specification");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
        }

        public static void Do(string message, Action doAction)
        {
            if (exception != null)
                return;

            if (!@do.HasValue)
                @do = new KeyValuePair<string, Action>(message, doAction);
            else
                try
                {
                    throw new InvalidOperationException("Cannot have two Do statements in one specification");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
        }

        public static void Assert(string message, Action assertAction)
        {
            asserts.Add(new KeyValuePair<string, Action>(message, assertAction));
        }

        public static void Reset()
        {
            exception = null;
            context = null;
            @do = null;
            asserts = new List<KeyValuePair<string, Action>>();
        }

        public static IEnumerable<ITestCommand> ToTestCommands(IMethodInfo method)
        {
            try
            {
                if (exception == null && asserts.Count == 0)
                    try
                    {
                        throw new InvalidOperationException("Must have at least one Assert in each specification");
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                if (exception == null && !context.HasValue)
                    try
                    {
                        throw new InvalidOperationException("Must have a Context in each specification");
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                if (exception != null)
                {
                    yield return new ExceptionTestCommand(method, exception);
                    yield break;
                }

                string name = context.Value.Key;

                if (@do.HasValue)
                    name += " " + @do.Value.Key;

                foreach (KeyValuePair<string, Action> kvp in asserts)
                {
                    List<Action> actions = new List<Action>();

                    actions.Add(context.Value.Value);
                    if (@do.HasValue) actions.Add(@do.Value.Value);
                    actions.Add(kvp.Value);

                    yield return new ActionTestCommand(method, name + ", " + kvp.Key, MethodUtility.GetTimeoutParameter(method), actions);
                }
            }
            finally
            {
                Reset();
            }
        }

        class ActionTestCommand : ITestCommand
        {
            readonly IEnumerable<Action> actions;
            readonly IMethodInfo method;

            public ActionTestCommand(IMethodInfo method, string name, int timeout, IEnumerable<Action> actions)
            {
                this.method = method;
                this.actions = actions;
                Name = name;
                Timeout = timeout;
            }

            public string Name { get; private set; }

            public bool ShouldCreateInstance
            {
                get { return false; }
            }

            public int Timeout { get; private set; }

            public MethodResult Execute(object testClass)
            {
                foreach (Action action in actions)
                    action();

                return new PassedResult(method, Name);
            }

            public string DisplayName
            {
                get { return Name; }
            }

            public virtual XmlNode ToStartXml()
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<dummy/>");
                XmlNode testNode = XmlUtility.AddElement(doc.ChildNodes[0], "start");

                XmlUtility.AddAttribute(testNode, "name", DisplayName);
                XmlUtility.AddAttribute(testNode, "type", method.TypeName);
                XmlUtility.AddAttribute(testNode, "method", method.Name);

                return testNode;
            }
        }
    }
}