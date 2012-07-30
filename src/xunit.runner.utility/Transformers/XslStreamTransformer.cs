using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IResultXmlTransform"/> which writes the
    /// XML to a file after applying the XSL stylesheet in the given stream.
    /// </summary>
    public class XslStreamTransformer : IResultXmlTransform
    {
        Stream xslStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="XslStreamTransformer"/> class.
        /// </summary>
        /// <param name="xslFileName">The XSL filename.</param>
        /// <param name="outputFileName">The output filename.</param>
        public XslStreamTransformer(string xslFileName, string outputFileName)
        {
            XslFilename = xslFileName;
            OutputFilename = outputFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XslStreamTransformer"/> class.
        /// </summary>
        /// <param name="xslStream">The stream with the XSL stylesheet.</param>
        /// <param name="outputFileName">The output filename.</param>
        public XslStreamTransformer(Stream xslStream, string outputFileName)
        {
            XslStream = xslStream;
            OutputFilename = outputFileName;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Filename", Justification = "This would be a breaking change.")]
        public string OutputFilename { get; private set; }

        /// <summary>
        /// Gets or sets the XSL filename.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Filename", Justification = "This would be a breaking change.")]
        public string XslFilename { get; private set; }

        /// <summary>
        /// Gets or sets the XSL stream.
        /// </summary>
        protected Stream XslStream
        {
            get
            {
                if (xslStream == null)
                    xslStream = File.OpenRead(XslFilename);
                return xslStream;
            }
            set
            {
                xslStream = value;
            }
        }

        /// <inheritdoc/>
        public void Transform(string xml)
        {
            using (StringReader xmlReader = new StringReader(xml))
            using (StreamReader streamReader = new StreamReader(XslStream))
            {
                XPathDocument doc = new XPathDocument(xmlReader);
                XslCompiledTransform xslTransform = new XslCompiledTransform();
                XmlTextReader transformReader = new XmlTextReader(streamReader);
                xslTransform.Load(transformReader);

                using (FileStream outStream = new FileStream(OutputFilename, FileMode.Create))
                    xslTransform.Transform(doc, null, outStream);
            }
        }
    }
}