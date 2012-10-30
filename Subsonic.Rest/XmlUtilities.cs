using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Subsonic.Rest.Api
{
    public static class XmlUtilities
    {
        /// <summary>
        /// Deserialize XML string into object type specified.
        /// </summary>
        /// <typeparam name="T">Object type to deserialize the XML into.</typeparam>
        /// <param name="xml">XML string to deserialize.</param>
        /// <returns>T</returns>
        public static T DeserializeFromXml<T>(string xml)
        {
            T result;

            try
            {
                var xmlSerializer = new XmlSerializer(typeof (T));

                using (TextReader textReader = new StringReader(xml))
                    result = (T) xmlSerializer.Deserialize(textReader);
            }
            catch (Exception ex)
            {
                throw new XmlException(ex.Message, ex);
            }

            return result;
        }
    }
}