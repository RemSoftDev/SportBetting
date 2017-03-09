using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;

namespace Nbt.Services.Spf
{
    public class XmlPreprocess
    {

        public string xmlData;
        private string filePath;
        private bool xmlValid;

        public static string xmlFragment = SPFPage.SPFSchema;


        public XmlPreprocess() : this(null, false) { }

        public XmlPreprocess(string xmlContent, bool isFile, String duplicateText = "")
        {
            DuplicateText = duplicateText;
            if (isFile)
            {
                xmlData = null;
                filePath = xmlContent;
            }
            else
            {
                xmlData = xmlContent;
                filePath = null;
            }
        }

        /*~XmlPreprocess() {			
        }*/

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            xmlValid = false;
        }


        /// <summary>
        /// validates XML (specified with constructor) with the schema SPFPage.SPFSchema
        /// </summary>
        /// <returns>true if XML is valid</returns>
        public bool Validate()
        {
            try
            {
                //XmlTextReader xmlReader = new XmlTextReader(xmlFragment, XmlNodeType.Document, null);		
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas.Add(null, new XmlTextReader(xmlFragment, XmlNodeType.Document, null));
                settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

                Stream xmlContentReader = null;
                if (this.filePath == null)
                    xmlContentReader = new MemoryStream(Encoding.Default.GetBytes(this.xmlData));
                else
                    xmlContentReader = File.OpenRead(this.filePath);
                //xmlContentReader = new FileStream(this.filePath, FileMode.Open);

                XmlReader reader = XmlTextReader.Create(xmlContentReader, settings);
                this.xmlValid = true;

                while (reader.Read()) ;			//xmlValid is set to false in case of error

                //if (this.xmlValid)
                //    Console.WriteLine(" xml walid!");

                return this.xmlValid;

            }
            catch (ArgumentNullException e)
            {
                return false;
            }
            catch (XmlException e)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }




        /// <summary>
        /// converts XML to a SPF object
        /// </summary>
        /// <returns>null on erro</returns>
        public SPFPage Marshaling()
        {
            try
            {

                SPFPage page = null;
                XmlSerializer pageSerializer = new XmlSerializer(typeof(SPFPage));

                if (this.filePath == null)
                {
                    /*using (XmlReader xmlContentReader = XmlReader.Create(new StringReader(xmlData)))
                        page = pageSerializer.Deserialize(xmlContentReader, Encoding.Default.EncodingName) as SPFPage;*/
                    using (StringReader xmlContent = new StringReader(XmlData))
                        page = pageSerializer.Deserialize(xmlContent) as SPFPage;

                    /*using (MemoryStream xmlContent = new MemoryStream(Encoding.Default.GetBytes(this.xmlData))) {
					
                        page = pageSerializer.Deserialize(xmlContent) as SPFPage;

                    }*/
                }
                else
                {
                    using (StreamReader file = new StreamReader(this.filePath))
                        page = pageSerializer.Deserialize(file) as SPFPage;
                }

                return page;

            }
            catch (Exception e)
            {
                return null;
            }

        }

        public String DuplicateText { get; set; }

        private string XmlData
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DuplicateText))
                    return xmlData;

                XDocument doc = XDocument.Parse(xmlData);
                doc.Descendants("Line").First().AddAfterSelf(XElement.Parse(DuplicateText));
                return String.Concat(doc.Declaration.ToString(), doc.ToString());

            }
        }

    }
}
