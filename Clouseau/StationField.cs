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

namespace Clouseau
{

    /// <summary>
    /// Fields (attributes) that are configured in a particular Station for a particular entity (StationEntity).
    /// 
    /// Useful if different stations expose different entity attributes.
    /// Not really necessary if all stations in the pipeline are only dealing with one kind of entity
    /// and a standard set of fields.
    /// </summary>
    public class StationField
    {

        private string name;
        private string type;
        private string location;
        private int? level;

        public StationField(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        // TODO
        // Following could be useful if this field actually refers to a different type of Entity:
        // ReferredEntity (may be null) : if this Field refers to another Entity, the name of that Entity
        // ReferredEntityField (may be null) : Name of Field within ReferredEntity to which this StationField refers. 
        //      Default is ID (unique ID).


        /// <summary>
        /// At what level of detail (in an Entity) is this Field?
        /// level 0 = an ID (identifies a specific stationEntity)
        /// level 1 = summary fields
        /// level 2 = detail
        /// level 3 = all the gory details
        /// </summary>
        public int? Level
        {
            get { return level; }
            set { level = value; }
        }

    }
}