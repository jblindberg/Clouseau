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

using System.Collections.Generic;
using System.Linq;

namespace Clouseau
{


    /// <summary>
    /// List of InstanceRef objects -
    /// these are for Instances in active search results;
    /// one InstanceRefList per Station.
    /// 
    /// refreshed at each new search (simplest model). (initial implementation)
    /// 
    /// Could also later retain previous search result instances,
    /// up to a limit.  Would need to allow a way to identify searches
    /// which are still active, and those which are obsolete.
    /// </summary>
    public class InstanceRefList
    {

        // An alternative would be to extend List, adding custom methods:
        // public class InstanceRefList : List<InstanceRef>

        private List<InstanceRef> list = new List<InstanceRef>();
        private List<Instance> instanceList;
        private string error;
        private Station station; // reference back to station that found these results


        public InstanceRefList(Station station)
        {
            this.station = station;
        }

        public List<InstanceRef> List
        {
            get
            {
                return list;
            }
            set
            {
                instanceList = null;
                this.list = value;
            }
        }



        /// <summary>
        /// Get the instances themselves, instead of the InstanceRefs.
        /// for convenience
        /// </summary>
        public List<Instance> InstanceList
        {
            get
            {
                if (instanceList == null)
                {
                    instanceList = (from ir in List select ir.Instance).ToList();
                }
                return instanceList;
            }
        }


        /// <summary>
        /// Get all the non-empty ID values from the Instances in this list
        /// </summary>
        ///<returns>empty List if no ID values or no instances in this list</returns>
        public List<string> InstanceIDs
        {
            get
            {
                var query = from i in InstanceList where (!string.IsNullOrEmpty(i.ID)) select i.ID;
                return query.ToList();
            }
        }



        public static List<Instance> MergeInstances(List<InstanceRefList> lists)
        {
            var query = lists.SelectMany(l => l.InstanceList);
            return query.ToList();
        }

        /// <summary>
        /// Get all the distinct Instance.ID values across all the InstanceRefList lists.
        /// </summary>
        /// <param name="lists"></param>
        /// <returns>empty List if no ID values or no instances in the lists</returns>
        public static List<string> UniqueInstanceIDs(List<InstanceRefList> lists)
        {
            var query = from i in MergeInstances(lists) select i.ID;
            return query.Distinct().ToList();
        
        }


        /// <summary>
        /// To clear the error, set to null.
        /// </summary>
        public string Error
        {
            get
            {
                return error;
            }
            set
            {
                this.error = value;
            }
        }

        public bool HasError
        {
            get
            {
                return error != null;
            }
        }

        public Station Station
        {
            get
            {
                return station;
            }
        }


        public void AddInstanceRef(InstanceRef iRef)
        {
            instanceList = null;
            list.Add(iRef);
        }

    }
}