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
using System.Linq;

namespace Clouseau
{
    public class CustomFieldDefinition
    {
        public string Name { get; private set; }

        public string Type { get; private set; }

        public string Label { get; set; }

        public bool Public { get; set; } // default false

        public bool Searchable { get; set; } // default false

        private static string[] _validTypes = { Field.TextType, Field.DateType, Field.NumberType, Field.BooleanType };

        /// <summary>
        /// Create a CustomField with this name and type.
        /// </summary>
        /// <param name="name">well known name of this CustomField</param>
        /// <param name="type">type (defined in Field class); if null or empty, Field.TEXT_TYPE is the default type</param>
        public CustomFieldDefinition(string name, string type)
        {
            this.Name = name;

            if (string.IsNullOrWhiteSpace(type))
            {
                this.Type = Field.TextType;
            }
            if (_validTypes.Contains(type))
            {
                this.Type = type;
            }
            else
            {
                string msg = string.Format("Invalid type {0} declared for custom field {1}", type, name);
                throw new Exception(msg);
            }

        }

        public override bool Equals(object obj)
        {
            var item = obj as CustomFieldDefinition;

            if (item == null)
            {
                return false;
            }

            return this.Name.Equals(item.Name) && this.Type.Equals(item.Type);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }


    }
}
