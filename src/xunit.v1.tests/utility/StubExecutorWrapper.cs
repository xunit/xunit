using System;
using System.Collections.Generic;
using System.Xml;
using Xunit;

public class StubExecutorWrapper : IExecutorWrapper
{
    public bool Dispose__Called;

    public XmlNode EnumerateTests__Result;

    public int GetAssemblyTestCount__Result;

    public readonly List<XmlNode> RunAssembly__CallbackNodes = new List<XmlNode>();
    public event EventHandler RunAssembly__CallbackEvent;
    public XmlNode RunAssembly__Result;

    public string RunClass_Type;
    public List<XmlNode> RunClass__CallbackNodes = new List<XmlNode>();
    public event EventHandler RunClass__CallbackEvent;
    public XmlNode RunClass__Result;

    public string RunTest_Type;
    public string RunTest_Method;
    public List<XmlNode> RunTest__CallbackNodes = new List<XmlNode>();
    public event EventHandler RunTest__CallbackEvent;
    public XmlNode RunTest__Result;

    public string RunTests_Type;
    public List<string> RunTests_Methods;
    public List<XmlNode> RunTests__CallbackNodes = new List<XmlNode>();
    public event EventHandler RunTests__CallbackEvent;
    public XmlNode RunTests__Result;

    public string AssemblyFilename { get; set; }
    public string ConfigFilename { get; set; }
    public string XunitVersion { get; set; }

    public XmlNode EnumerateTests()
    {
        return EnumerateTests__Result;
    }

    public void Dispose()
    {
        Dispose__Called = true;
    }

    public int GetAssemblyTestCount()
    {
        return GetAssemblyTestCount__Result;
    }

    public XmlNode RunAssembly(Predicate<XmlNode> callback)
    {
        if (callback != null)
            foreach (XmlNode node in RunAssembly__CallbackNodes)
                callback(node);

        if (RunAssembly__CallbackEvent != null)
            RunAssembly__CallbackEvent(this, EventArgs.Empty);

        return RunAssembly__Result;
    }

    public XmlNode RunClass(string type, Predicate<XmlNode> callback)
    {
        RunClass_Type = type;

        if (callback != null)
            foreach (XmlNode node in RunClass__CallbackNodes)
                callback(node);

        if (RunClass__CallbackEvent != null)
            RunClass__CallbackEvent(this, EventArgs.Empty);

        return RunClass__Result;
    }

    public XmlNode RunTest(string type, string method, Predicate<XmlNode> callback)
    {
        RunTest_Type = type;
        RunTest_Method = method;

        if (callback != null)
            foreach (XmlNode node in RunTest__CallbackNodes)
                callback(node);

        if (RunTest__CallbackEvent != null)
            RunTest__CallbackEvent(this, EventArgs.Empty);

        return RunTest__Result;
    }

    public XmlNode RunTests(string type, List<string> methods, Predicate<XmlNode> callback)
    {
        RunTests_Type = type;
        RunTests_Methods = methods;

        if (callback != null)
            foreach (XmlNode node in RunTests__CallbackNodes)
                callback(node);

        if (RunTests__CallbackEvent != null)
            RunTests__CallbackEvent(this, EventArgs.Empty);

        return RunTests__Result;
    }

    internal delegate void Handler();
}
