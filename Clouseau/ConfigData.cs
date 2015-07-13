/*
 * Copyright (c) 2015 Jeff Lindberg
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Clouseau
{

    /// <summary>
    /// Title:        Configuration Data tool
    ///
    /// Description:  Access methods for retrieving configuration values for specific
    ///               fields from an XML configuration file (or an XML chunk).
    ///               Uses System.Xml.Linq to get the element values.
    ///               
    /// ASSUMPTION:  text values are only in leaf nodes of the XML.
    /// Currently does not make any use of attributes. Values are stored in element text instead.
    ///
    /// Jeff Lindberg
    /// </summary>
	public class ConfigData
	{

		private XElement config;

		public ConfigData(XElement configElement)
		{
			config = configElement;
		}

        /// <summary>
        /// Constructs a ConfigData object containing the entire XML file
        /// provided in the InputStream, i.e. the root element.
        /// </summary>
        public ConfigData(Stream xmlStream)
		{
			config = XElement.Load(xmlStream);

			// Java DOM4J technique:
			//SAXReader reader = new SAXReader();
			//Document doc = reader.read(xmlStream);
			//config = doc.getRootElement();
		}

        /// <summary>
        /// get cumulative text value of this ConfigData object, including all its children.
        /// Note: unless this is a leaf node, it may return more text than you are expecting.
        /// </summary>
        public string Value()
		{
			string v = null;
			XElement x = config;
			if (x != null)
				v = x.Value;
			if (v != null)
				v = v.Trim();
			return v;
		}

        /// <summary>
        /// get value of a particular property.
        /// </summary>
        /// <param name="property">name of the property in the config file</param>
        /// <returns>value configured for the specified property (may be null)</returns>
        public string Value(string property)
		{
			string v = null;
			XElement x = config.Element(property);
			if (x != null)
				v = x.Value;
			if (v != null)
				v = v.Trim();
			//elementTextTrim(property);
			return v;
		}

        /// <summary>
        /// Get a boolean config parameter.
        /// True is designated as "true" (case insensitive), everything else is false.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="defaultValue">default value to return if property not found</param>
        /// <returns></returns>
	    public bool BoolValue(string property, bool defaultValue)
	    {
            string v = Value(property);
            bool result = defaultValue;
            if (v != null)
            {
                result = (v.Equals(Constants.True, StringComparison.InvariantCultureIgnoreCase));
            }
	        return result;
	    }

        /// <summary>
        /// get value of a particular property.
        /// </summary>
        /// <param name="property">name of the property in the config file</param>
        /// <returns>value configured for the specified property (may be null)</returns>
        public int? IntValue(string property)
		{
			string v = Value(property);
			int result;
			if (v != null && int.TryParse(v, out result))
				return result;
			else
				return null;
		}

        /// <summary>
        /// get value of a particular property.
        /// </summary>
        /// <param name="property">name of the property in the config file</param>
        /// <returns>value configured for the specified property</returns>
        public string RequiredValue(string property)
		{
			string v = Value(property);
			if (string.IsNullOrEmpty(v))
			{
				throw new Exception("Required property is missing: " + property);
			}
			return v;
		}

        /// <summary>
        /// get multiple values for a particular property.
        /// </summary>
        /// <param name="property">name of the property in the config file</param>
        /// <returns>array of all values configured for the specified property (zero or more)</returns>
        public string[] Values(string property)
		{
			var elems = config.Elements(property);
			var values = from e in elems select e.Value;
			return values.ToArray();
		}

        /// <summary>
        /// Return all the segments (lower level elements) inside this ConfigData which
        /// have the matching name.  Return each one as separate ConfigData object.
        /// Preserves the order of the sections as listed in the original config file.
        /// </summary>
        /// <returns>empty List if none found</returns>
        public List<ConfigData> GetConfigSections(string sectionName)
		{
			var elems = config.Elements(sectionName);
			return (from e in elems select new ConfigData(e)).ToList();
		}

	}

}