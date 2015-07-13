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

namespace Clouseau
{
    /// <summary>
    /// An Entity that is supported by a particular station.
    /// In a pipeline involving multiple entity types, some stations might know about certain entities but not others.
    /// E.g. a product data sheet update approval station would know about products, but not customers or orders.
    /// 
    /// In a system that has only entity type, this class is not really needed; the entity can be hard coded instead.
    /// </summary>
	public class StationEntity
	{

		private string name;
		private List<StationField> fields;
		private string location; // for table name, directory path, etc.
		private List<StationField> summaryFields = new List<StationField>();
		private string sortField;
		private bool descendingSort = false;


		public StationEntity(string name)
		{
			this.name = name;
			fields = new List<StationField>();
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

        /// <summary>
        /// Map a well known field name to the particular StationField object.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>null if not found</returns>
		public StationField GetField(string fieldName)
		{
			// TODO faster implementation using private Map<fieldName, StationField>?
			//    would need to eliminate getFields() -- otherwise couldn't guarantee
			//    to keep Map in sync with List.

			foreach (StationField sf in fields)
			{
				if (sf.Name == (fieldName)) return sf;
			}
			return null;
		}

		public void AddField(StationField field)
		{
			fields.Add(field);

			int? lev = field.Level;
			if (lev != null && lev == 1)
			{
				summaryFields.Add(field);
			}
		}

        /// <summary>
        /// Set:  Reset the list of fields for this entity (e.g. an Order) at this station.
        /// Also resets the list of Summary fields to those with Level == 1.
        /// </summary>
        public List<StationField> Fields
		{
			get
			{
				return fields;
			}
			set
			{
				this.fields = value;

				// initialize list of summary fields too
				summaryFields = new List<StationField>();
				foreach (StationField f in fields)
				{
					int? lev = f.Level;
					if (lev != null && lev == 1)
					{
						summaryFields.Add(f);
					}
				}

			}
		}

        /// <summary>
        /// Can be used to specify table name, directory path, etc.
        /// </summary>
		public string Location
		{
			get { return location; }
			set { location = value; }
		}

        /// <summary>
        /// Fields to be shown in a summary of this station/entity.
        /// </summary>
		public List<StationField> SummaryFields
		{
			get
			{
				return summaryFields;
			}
			set
			{
				this.summaryFields = value;
			}
		}

		public string SortField
		{
			get
			{
				return sortField;
			}
			set
			{
			this.sortField = value;
			}
		}

		public bool DescendingSort
		{
			get
			{
				return descendingSort;
			}
			set
			{
				descendingSort = value;
			}
		}


	}
}