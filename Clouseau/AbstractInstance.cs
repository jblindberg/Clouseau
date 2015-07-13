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

namespace Clouseau
{
    /// <summary>
    /// 
    /// </summary>
    abstract public class AbstractInstance : Instance
    {

        /// <summary>
        /// identifying number of item - e.g. customer #, order #
        /// </summary>
        public virtual string ID { 
            get; 
            set; 
        }

        /// <summary>
        /// unique identifier for THIS instance of the item (can be a combination of multiple fields)
        /// </summary>
        public virtual string UniqueId { get; set; }

        /// <summary>
        /// e.g. ORDER, QUOTE, CUSTOMER, REQUEST
        /// </summary>
        public virtual string EntityName { get; set; }

        /// <summary>
        /// Date/time this instance arrived at this station
        /// </summary>
        public virtual DateTime ArrivalDate { get; set; }

        /// <summary>
        /// Date/time this instance was last updated. 
        /// Expected (but not required) to be equal to or later than ArrivalDate.
        /// </summary>
        public virtual DateTime UpdateDate { get; set; }

        /// <summary>
        /// Status with regard to the Station.
        /// e.g. COMPLETE, ERROR, WAITING
        /// </summary>
        public virtual string Status { get; set; }

        /// <summary>
        /// one line summary of item (description, title)
        /// </summary>
        public virtual string Summary {
            get
            {
                return string.Format(@"{0} {1} : {2}", EntityName, ID, Status);
            }
        }

        /// <summary>
        /// one line additional details of item, not including the standard EntityName/Id/Status/UpdateDate fields
        /// </summary>
        public abstract string AdditionalSummary { get; }

        /// <summary>
        /// Simple multi-line listing of item content, with newlines separating lines
        /// </summary>
        public abstract string Details { get; }

        /// <summary>
        /// Returns Details
        /// </summary>
        public override string ToString()
        {
            return Details;
        }

        /// <summary>
        /// File name or other name of content - for display purposes only.
        /// Might have folder name, full path, or just file name.
        /// </summary>
        public virtual string ContentName { get { return null; } }


        private PublicFieldCollection publicFields = new PublicFieldCollection();
        /// <summary>
        /// Fields which are accessible across stations (by name).
        /// Intended to be used for global (master) properties of instances.
        /// e.g. originating customer, product number, borrowing site
        /// </summary>
        public virtual PublicFieldCollection PublicFields
        {
            get
            {
                return publicFields;
            }
        }

    }

}
