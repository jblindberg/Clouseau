/*
 * Licensed according to the MIT License: 
 * 
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
    ///  a particular instance of an item (e.g. Order, Customer, Product)
    /// </summary>
    public interface Instance
    {

        /// <summary>
        /// identifying number of item - e.g. customer #, order #
        /// </summary>
        string ID { get; }

        /// <summary>
        /// unique identifier for THIS instance of the item (can be a combination of multiple fields)
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// e.g. ORDER, QUOTE, CUSTOMER, DOCUMENT_ID
        /// </summary>
        string EntityName { get; }

        /// <summary>
        /// Date/time this instance arrived at this station
        /// </summary>
        DateTime ArrivalDate { get; }

        /// <summary>
        /// Date/time this instance was last updated. 
        /// Expected (but not required) to be equal to or later than ArrivalDate.
        /// </summary>
        DateTime UpdateDate { get; }

        /// <summary>
        /// Status with regard to the Station.
        /// e.g. COMPLETE, ERROR, WAITING
        /// </summary>
        string Status { get; }

        /// <summary>
        /// one line summary of item (description, title)
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// one line additional details of item, not including the standard EntityName/Id/Status/UpdateDate fields
        /// </summary>
        string AdditionalSummary { get; }

        /// <summary>
        /// Simple multi-line listing of item content, with newlines separating lines
        /// </summary>
        string Details { get; }

        /// <summary>
        /// File name or other name of content - for display purposes only.
        /// Might have folder name, full path, or just file name.
        /// </summary>
        string ContentName { get; }


        /// <summary>
        /// HTML view of item content.
        /// This might include clickable links to "CrossRef" items, maybe even using custom tags?  ADF components?
        /// Instances might use helper methods to build the "links" to other items.
        /// </summary>
        // TODO is this needed?  
        //String ContentHTML { get; }


        /// <summary>
        /// Fields which are accessible across stations (by name).
        /// Intended to be used for global (master) properties of instances.
        /// e.g. originating customer, product number, borrowing site.
        /// 
        /// Design Guideline: for multiple values, should probably have an empty list if there are none.
        /// But for single values, if the value is null, should store a null in the first list element.
        /// </summary>
        PublicFieldCollection PublicFields { get; }


        /// <summary>
        /// Simple multi-line listing of item content, with newlines separating lines. 
        /// This is not really part of the interface, since it is defined in Object.
        /// </summary>
        /// <returns></returns>
        string ToString();


    }
}