using Xunit;

public class StubTransformer : IResultXmlTransform
{
    public bool Transform__Called;
    public string Transform_Xml;

    public string OutputFilename
    {
        get { return "filename"; }
    }

    public void Transform(string xml)
    {
        Transform__Called = true;
        Transform_Xml = xml;
    }
}
