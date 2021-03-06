using System.Text.RegularExpressions;

namespace ElasticElmah.Core.Infrastructure
{
    #region Imports

    

    #endregion

    /// <summary>
    /// XML 1.0 services.
    /// </summary>
    public sealed class XmlText
    {
        private XmlText()
        {
        }

        /// <summary>
        /// Replaces illegal XML characters with a question mark (?).
        /// </summary>
        /// <remarks>
        /// Only strips illegal characters as per XML 1.0, not 1.1. 
        /// See section <a href="http://www.w3.org/TR/2006/REC-xml-20060816/#charsets">2.2 Characters</a>
        /// of <a href="http://www.w3.org/TR/2006/REC-xml-20060816">Extensible Markup Language (XML) 1.0 (Fourth Edition)</a>.
        /// </remarks>
        public static string StripIllegalXmlCharacters(string xml)
        {
            return StripIllegalXmlCharacters(xml, null);
        }

        /// <summary>
        /// Replaces illegal XML characters with a replacement string,
        /// with the default being a question mark (?) if the replacement
        /// is null reference.
        /// </summary>
        /// <remarks>
        /// Only strips illegal characters as per XML 1.0, not 1.1. 
        /// See section <a href="http://www.w3.org/TR/2006/REC-xml-20060816/#charsets">2.2 Characters</a>
        /// of <a href="http://www.w3.org/TR/2006/REC-xml-20060816">Extensible Markup Language (XML) 1.0 (Fourth Edition)</a>.
        /// </remarks>
        public static string StripIllegalXmlCharacters(string xml, string replacement)
        {
            // TODO Consider expanding illegal character set to XML 1.1

            const string pattern = @"&#x(0{0,3}[0-8BCEF]|0{0,2}1[0-F]|D[89A-F][0-9A-F]{2}|FFF[EF]);";
            const RegexOptions options = RegexOptions.IgnoreCase
                                         | RegexOptions.CultureInvariant;
            return Regex.Replace(xml, pattern, replacement != null ? replacement : "?", options);
        }
    }
}