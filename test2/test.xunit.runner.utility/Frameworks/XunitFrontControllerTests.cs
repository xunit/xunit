//using System;
//using System.IO;
//using Moq;
//using Xunit;

//public class XunitFrontControllerTests
//{
//    public class Construction
//    {
//        [Fact]
//        public void GuardClauses()
//        {
//            var anex = Assert.Throws<ArgumentNullException>(() => new XunitFrontController(assemblyFileName: null, configFileName: null, shadowCopy: false));
//            Assert.Equal("assemblyFileName", anex.ParamName);

//            var aex = Assert.Throws<ArgumentException>(() => new XunitFrontController("?", null, false));
//            Assert.Equal("Illegal characters in path.", aex.Message);

//            var aex2 = Assert.Throws<ArgumentException>(() => new XunitFrontController("{C69ECA90-7487-4740-B1CE-BC6032053D5B}.dll", null, false));
//            Assert.Contains("Could not find file: " + Path.GetFullPath("{C69ECA90-7487-4740-B1CE-BC6032053D5B}.dll"), aex2.Message);
//            Assert.Equal("assemblyFileName", aex2.ParamName);
//        }

//        [Fact]
//        public void UsesDefaultConfigFileIfItExists()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            using (TempFile configFile = new TempFile("foo.dll.config"))
//            {
//                var factory = new Mock<IXunitControllerFactory>();
//                factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                       .Returns(new Mock<IXunitController>().Object);

//                var frontController = new XunitFrontController(assembly.FileName, null, true, factory.Object);

//                factory.Verify(f => f.Create(assembly.FileName, configFile.FileName, true));
//            }
//        }

//        [Fact]
//        public void UsesProvidedConfigFile()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            using (TempFile configFile = new TempFile("foo.dll.config"))
//            {
//                var factory = new Mock<IXunitControllerFactory>();
//                factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                       .Returns(new Mock<IXunitController>().Object);

//                var frontController = new XunitFrontController(assembly.FileName, "Dummy.config", false, factory.Object);

//                factory.Verify(f => f.Create(assembly.FileName, Path.GetFullPath("Dummy.config"), false));
//            }
//        }

//        [Fact]
//        public void DoesNotUseConfigFileIfNotPassedAndDefaultIsNotPresent()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            {
//                var factory = new Mock<IXunitControllerFactory>();
//                factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                       .Returns(new Mock<IXunitController>().Object);

//                var frontController = new XunitFrontController(assembly.FileName, null, false, factory.Object);

//                factory.Verify(f => f.Create(assembly.FileName, null, false));
//            }
//        }

//        [Fact]
//        public void UsesControllerFromFirstFactoryThatOffersResponse()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            {
//                var factory1 = new Mock<IXunitControllerFactory>();
//                factory1.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                        .Returns(new Mock<IXunitController>().Object);

//                var factory2 = new Mock<IXunitControllerFactory>();
//                factory2.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                        .Returns(new Mock<IXunitController>().Object);

//                var frontController = new XunitFrontController(assembly.FileName, null, false, factory1.Object, factory2.Object);

//                factory1.Verify(f => f.Create(assembly.FileName, null, false));
//                factory2.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
//            }
//        }

//        [Fact]
//        public void ContinuesPastFactoriesThatReturnNull()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            {
//                var factory1 = new Mock<IXunitControllerFactory>();
//                factory1.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                        .Returns<IXunitController>(null);

//                var factory2 = new Mock<IXunitControllerFactory>();
//                factory2.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                        .Returns(new Mock<IXunitController>().Object);

//                var frontController = new XunitFrontController(assembly.FileName, null, false, factory1.Object, factory2.Object);

//                factory1.Verify(f => f.Create(assembly.FileName, null, false));
//                factory2.Verify(f => f.Create(assembly.FileName, null, false));
//            }
//        }

//        [Fact]
//        public void WhenAllFactoriesReturnNullThrows()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            {
//                var factory1 = new Mock<IXunitControllerFactory>();
//                factory1.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                        .Returns<IXunitController>(null);

//                var factory2 = new Mock<IXunitControllerFactory>();
//                factory2.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                        .Returns<IXunitController>(null);

//                var ex = Record.Exception(() => new XunitFrontController(assembly.FileName, null, false, factory1.Object, factory2.Object));

//                Assert.IsType<InvalidOperationException>(ex);
//                Assert.Equal("Could not locate a controller for your unit tests. Are you missing xunit.dll or xunit2.dll?", ex.Message);
//            }
//        }
//    }

//    public class Dispose
//    {
//        [Fact]
//        public void DisposingFrontControllerDisposesController()
//        {
//            using (TempFile assembly = new TempFile("foo.dll"))
//            {
//                var factory = new Mock<IXunitControllerFactory>();
//                var controller = new Mock<IXunitController>();
//                factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
//                       .Returns(controller.Object);
//                var frontController = new XunitFrontController(assembly.FileName, null, false, factory.Object);

//                frontController.Dispose();

//                controller.Verify(c => c.Dispose());
//            }
//        }
//    }
//}