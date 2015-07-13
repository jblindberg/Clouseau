using System;

namespace Clouseau.Tests
{
    public class TestInstance1 : AbstractInstance
    {
        private string _id;
        private string _uniqueID;
        private string _status;
        private DateTime _updated = DateTime.Now;

        public TestInstance1(string id, string uniqueId, string status)
        {
            this._id = id;
            this._uniqueID = uniqueId;
            this._status = status;
        }

        /**
    * identifying number of item - e.g. customer #, order #
    */
        public override string ID
        {
            get
            {
                return _id;
            }
        }

        /**
         * unique identifier for THIS instance of the item
         * (can be a combination of multiple fields)
         */
        public override string UniqueId
        {
            get
            {
                return _uniqueID;
            }
        }

        /**
         * e.g. ORDER, QUOTE, CUSTOMER
         */
        public override string EntityName
        {
            get
            {
                return "TEST_ENTITY";
            }
        }

        /**
         *
         */
        public override DateTime ArrivalDate
        {
            get
            {
                return _updated;
            }
        }

        /**
         *
         */
        public override DateTime UpdateDate
        {
            get
            {
                return _updated;
            }
        }

        /**
         * Status with regard to the Station
         * e.g. REC_STATE value
         */
        public override string Status
        {
            get
            {
                return _status;
            }
        }

        /**
         * one line summary of item (description, title)
         */
        public override string Summary
        {
            get
            {
                return string.Format(@"{0} # {1}", EntityName, ID);
            }
        }

        public override string AdditionalSummary
        {
            get
            {
                return "Special fields for this instance type";
            }
        }

        /**
         * Simple multi-line listing of item content, with newlines separating lines
         */
        public override string ToString()
        {
            return string.Format(@"{0} # {1}\n  Status: {2}, Updated {3}",
                EntityName, ID, _status, _updated);
        }

        /**
         * Simple multi-line listing of item content, with newlines separating lines
         */
        public override string Details
        {
            get { return ToString(); }
        }

        public override string ContentName { get { return string.Format(@"{0}-{1}", EntityName, ID); } }



    }
}
