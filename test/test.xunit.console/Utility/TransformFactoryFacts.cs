//using System;
//using System.Collections.Generic;
//using System.IO;
//using Xunit;
//using Xunit.ConsoleClient;

//public class TransformFactoryFacts
//{
//    [Fact]
//    public void KnownTransformWithValidXslFile()
//    {
//        var config = new XunitConsoleConfigurationSection();
//        config.Transforms = new TestableTransformConfigItem("known", "xslFile.xsl");
//        var assembly = new XunitProjectAssembly();
//        assembly.Output["known"] = "foo.xml";
//        var factory = new TestableTransformFactory(config);

//        List<IResultXmlTransform> transforms = factory.GetTransforms(assembly);

//        IResultXmlTransform transform = Assert.Single(transforms);
//        var xslTransform = Assert.IsType<XslStreamTransformer>(transform);
//        Assert.Equal("foo.xml", xslTransform.OutputFilename);
//        Assert.Equal("xslFile.xsl", Path.GetFileName(xslTransform.XslFilename));
//    }

//    [Fact]
//    public void KnownTransformWithMissingXslFile()
//    {
//        var config = new XunitConsoleConfigurationSection();
//        config.Transforms = new TestableTransformConfigItem("known", "xslFile.xsl");
//        var assembly = new XunitProjectAssembly();
//        assembly.Output["known"] = "foo.xml";
//        var factory = new TestableTransformFactory(config);
//        factory.FileExistsResult = false;

//        Exception ex = Record.Exception(() => factory.GetTransforms(assembly));

//        Assert.IsType<ArgumentException>(ex);
//        Assert.Contains("cannot find transform XSL file", ex.Message);
//        Assert.Contains("xslFile.xsl", ex.Message);
//        Assert.Contains("for transform 'known'", ex.Message);
//    }

//    [Fact]
//    public void UnknownTransform()
//    {
//        var assembly = new XunitProjectAssembly();
//        assembly.Output["unknown"] = "foo.xml";
//        var factory = new TestableTransformFactory();

//        Exception ex = Record.Exception(() => factory.GetTransforms(assembly));

//        Assert.IsType<ArgumentException>(ex);
//        Assert.Equal("unknown output transform: unknown", ex.Message);
//    }

//    class TestableTransformConfigItem : TransformConfigurationElementCollection
//    {
//        public TestableTransformConfigItem(string commandLine, string xslFile)
//        {
//            BaseAdd(new TransformConfigurationElement { CommandLine = commandLine, XslFile = xslFile });
//        }
//    }

//    class TestableTransformFactory : TransformFactory
//    {
//        public bool FileExistsResult = true;

//        public TestableTransformFactory() { }

//        public TestableTransformFactory(XunitConsoleConfigurationSection config)
//        {
//            Config = config;
//        }

//        protected override bool FileExists(string filename)
//        {
//            return FileExistsResult;
//        }
//    }
//}
