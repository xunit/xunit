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
        /// <param name="xslFilename">The XSL filename.</param>
        /// <param name="outputFilename">The output filename.</param>
        public XslStreamTransformer(string xslFilename, string outputFilename)
        {
            XslFilename = xslFilename;
            OutputFilename = outputFilename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XslStreamTransformer"/> class.
        /// </summary>
        /// <param name="xslStream">The stream with the XSL stylesheet.</param>
        /// <param name="outputFilename">The output filename.</param>
        public XslStreamTransformer(Stream xslStream, string outputFilename)
        {
            XslStream = xslStream;
            OutputFilename = outputFilename;
        }

        /// <inheritdoc/>
        public string OutputFilename { get; private set; }

        /// <summary>
        /// Gets or sets the XSL filename.
        /// </summary>
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
            using (StreamReader reader = new StreamReader(XslStream))
            {
                XPathDocument doc = new XPathDocument(new StringReader(xml));
                XslCompiledTransform xslTransform = new XslCompiledTransform();
                XmlTextReader transformReader = new XmlTextReader(reader);
                xslTransform.Load(transformReader);

                using (FileStream outStream = new FileStream(OutputFilename, FileMode.Create))
                    xslTransform.Transform(doc, null, outStream);
            }
        }
    }
}