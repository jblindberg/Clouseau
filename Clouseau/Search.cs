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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clouseau
{

    public class Search
    {

        private List<InstanceRefList> resultsList;
        private ICollection<Criterion> crit = new HashSet<Criterion>();
        private Pipeline pipeline;

        public Search(Pipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        public void DoSearch()
        {
            DoSearch(null);
        }

        /// <summary>
        /// Search restricted to stations with the specified role.
        /// </summary>
        /// <param name="role">if null, not restricted to any role</param>
        public void DoSearch(string role)
        {

            if (pipeline == null)
            {
                throw new Exception("Pipeline not set");
            }

            // clear results list from previous search
            //_resultsList = new List<InstanceRefList>(); // Unnecessary now that we're populating a tempResultsList

            this.Error = null;

            // DONE 5/23/2013 JbL: Use multiple threads for parallel processing and faster response.
            // with a thread-safe _resultsList.Add(irl);  --  using a thread safe collection type
            //
            // Originally, when I tried this simply, I got file system access errors (network path),
            // meaning it probably had UNC connections closed randomly while other stations were still using them.
            // added code to manage a pool of UNC connections and make sure they're kept open during the search process.

            var tempResultsList = new ConcurrentBag<InstanceRefList>(); // Thread safe
            Parallel.ForEach(pipeline.Stations, s => SearchStation(role, s, tempResultsList));

            // put results lists in same original order as stations
            var orderedByStations = from s in pipeline.Stations
                                    join irl in tempResultsList on s equals irl.Station
                                    select irl;
            resultsList = orderedByStations.ToList();

            // populate Public Fields
            try
            {
                foreach (Station s in pipeline.Stations)
                {
                    // TODO Public Fields - call this on stations not included in search, but still configured with public fields
                    s.SetPublicFieldValues(s.PublicFields, resultsList);
                }

            }
            catch (Exception ex)
            {
                this.Error = "Error populating public fields: " + Util.ExceptionMessageIncludingInners(ex);
            }

            // TODO PublicFields searching - filter all results by public field criteria
            //     for any criteria field names == public field names
            


        }

        private void SearchStation(string role, Station s, ConcurrentBag<InstanceRefList> results)
        {
            // TODO  first check if the station supports the object type ???
            //       or is this part of criteria? e.g. TYPE = 'ORDER'

            InstanceRefList irl = null;
            try
            {
                s.CheckSupportedCriteria(crit); // will throw exception if not supported

                if (!UnsupportedCustomFields(s) && (string.IsNullOrEmpty(role) || s.HasRole(role)))
                    irl = DoSearchWithRetry(s);
            }
            catch (StationUnsupportedCriterionException ex)
            {
                irl = new InstanceRefList(s);
                irl.Error = "Station skipped: " + Util.ExceptionMessageIncludingInners(ex);
            }
            catch (Exception ex)
            {
                irl = new InstanceRefList(s);
                irl.Error = Util.ExceptionMessageIncludingInners(ex);
            }
            finally
            {
                if (irl != null)
                    results.Add(irl);
            }
        }

        
        private bool UnsupportedCustomFields(Station s)
        {
            var stationCustomFieldNames = s.SearchableFields.Select(sf => sf.Name).ToList();
            var searchFieldNames = crit.Select(sf => sf.FieldName);
            var usedCustomFields = pipeline.ConfiguredSearchableCustomFields
                .Where(c => searchFieldNames.Contains(c.Name));
            foreach (var ucf in usedCustomFields)
            {
                if (!stationCustomFieldNames.Contains(ucf.Name))
                    // silently ignore instead of throwing exception
                    //throw new StationUnsupportedCriterionException(
                    //string.Format("Unsupported custom field: {0} ({1})", ucf.Label, ucf.Name));
                    return true;
            }
            return false; // all fields are OK
        }

        private InstanceRefList DoSearchWithRetry(Station s)
        {
            InstanceRefList irl;
            int maxTries = 4; // TODO get from station config file, or default?

            for (int tries = 1; ; tries++)
            {
                try
                {
                    irl = s.DoSearch(crit);
                    break;
                }
                catch (Exception ex)
                {
                    if (tries >= maxTries || !IsRetriedException(ex, s))
                        throw;
                    // else try again
                }
            }

            return irl;
        }

        private bool IsRetriedException(Exception ex, Station station)
        {
            // TODO get list of exception texts from station 
            string[] retryExceptions =
                {
                    "unexpected network error", // any file folders, maybe any station
                    "file sharing violation",  
                    "error occurred while reading from the store provider's data reader" 
                };
            var query = from r in retryExceptions where ex.Message.Contains(r) select r;
            if (query.Any())
                return true;

            // TODO could check actual exception types too

            return false;
        }

        public List<InstanceRefList> ResultsList
        {
            get
            {
                return resultsList;
            }
        }

        public void ClearResults()
        {
            resultsList = null;
        }

        public ICollection<Criterion> Crit
        {
            get
            {
                return crit;
            }
            set
            {
                this.crit = value;
            }
        }

        public Pipeline Pipeline
        {
            get
            {
                return pipeline;
            }
        }

        /// <summary>
        /// Return number of instance results found by search
        /// </summary>
        public int Count
        {
            get
            {
                if (ResultsList != null)
                {
                    var q = from irl in this.ResultsList select irl.List.Count;
                    return q.Sum();
                }
                else
                    return 0;
            }
        }

        private string error;
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


        /// <summary>
        /// Return all instance references in one flat list.
        /// Note:  Loses the notion of which station the instance came from.
        /// </summary>
        public List<InstanceRef> AllInstanceRefs
        {
            get
            {
                List<InstanceRef> list = new List<InstanceRef>();
                foreach (InstanceRefList irl in ResultsList)
                {
                    list.AddRange(irl.List);
                }
                return list;
            }
        }


        /// <summary>
        /// Return all instances in one flat list.
        /// Note:  Loses the notion of which station the instance came from.
        /// </summary>
        public List<Instance> AllInstances
        {
            get
            {
                List<Instance> list = new List<Instance>();
                foreach (InstanceRefList irl in ResultsList)
                {
                    list.AddRange(irl.InstanceList);
                }
                return list;
            }
        }


        /// <summary>
        /// Perform a search using the specified criteria.  
        /// But don't change the criteria stored in the Search object.
        /// </summary>
        /// <param name="newCrit"></param>
        /// <param name="role">if null, not restricted to any role</param>
        /// <returns>List of Instances</returns>
        public List<Instance> DoSearchWithSpecifiedCriteria(ICollection<Criterion> newCrit, string role)
        {
            ICollection<Criterion> origCrit = this.Crit; // remember the original criteria
            this.Crit = newCrit;
            DoSearch(role);
            this.Crit = origCrit; // restore to original
            return this.AllInstances;
        }

        /// <summary>
        /// Perform search using the existing criteria plus an additional temporary criterion.
        /// </summary>
        /// <param name="c">Additional criterion for this search only</param>
        /// <param name="role">if null, not restricted to any role</param>
        public List<Instance> DoSearchWithAdditionalCriterion(Criterion c, string role)
        {
            ICollection<Criterion> newCrit = new List<Criterion>(this.Crit);  // make copy of the current criteria
            newCrit.Add(c);

            return DoSearchWithSpecifiedCriteria(newCrit, role);
        }

        /// <summary>
        /// Perform search using the specified temporary criterion.
        /// But put the Search object's Criteria back to its original set.
        /// </summary>
        /// <param name="c">Only criterion to be used for this search</param>
        /// <param name="role">if null, not restricted to any role</param>
        public List<Instance> DoSearchWithSpecifiedCriterion(Criterion c, string role)
        {
            ICollection<Criterion> newCrit = new List<Criterion>();
            newCrit.Add(c);

            return DoSearchWithSpecifiedCriteria(newCrit, role);
        }



    }
}