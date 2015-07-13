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

namespace Clouseau
{


    /// <summary>
    /// Contains set of Instance References.
    /// Provides association between Instances and Reference IDs.
    /// Should be one of these per session.
    /// </summary>
    public class InstanceMemory
	{

		// Note: we are letting InstanceMemory build up with no max
		// TODO could call memory and add a whole list at a time --
		//      this would allow memory to control its trimming (when get
		//      too many instances) but still keep all of the current search
		//      results list.

		private IDictionary<long, InstanceRef> refs;

        // ensure uniqueness over time, at least within the same app instance
		private long lastId = DateTime.Now.Ticks; 

		public InstanceMemory()
		{
			refs = new Dictionary<long, InstanceRef>();
		}

        /// <summary>
        /// Create and remember a reference to an Instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="station">station to which this instance belongs</param>
        /// <returns>new instance reference</returns>
        public InstanceRef AddRef(Instance instance, Station station)
		{
			long id = NextId();
			InstanceRef iRef = new InstanceRefImpl(id, instance, station);
			refs.Add(id, iRef);
			return iRef;
		}

        /// <summary>
        /// Return the instance reference corresponding to this unique reference ID.
        /// </summary>
        public InstanceRef GetRef(long refId)
		{
			InstanceRef iRef;
			if (refs.TryGetValue(refId, out iRef))
				return iRef;
			else
				return null;
		}

		private long NextId()
		{
			lock (this)  // synchronized
			{
				return ++lastId;
			}
		}


	}
}