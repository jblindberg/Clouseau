using System;
using System.Diagnostics;

namespace Clouseau.Tests
{

    public class TestInstance : AbstractInstance
    {

        private string _number = "12345";
        private string _uniqueID = "97531";

        public TestInstance()
        {
            Debug.WriteLine("**** default constructor of TestInstance called ***");
        }

        public TestInstance(string number, string uniqueID)
        {
            this._number = number;
            this._uniqueID = uniqueID;
        }

        public override string ID
        {
            get
            {
                return _number;
            }
        }

        public override string UniqueId
        {
            get
            {
                return _uniqueID;
            }
            set
            {
                _uniqueID = value;
            }
        }

        public override string EntityName
        {
            get
            {
                return "TEST";
            }
        }

        public override DateTime ArrivalDate
        {
            get
            {
                return new DateTime();
            }
        }


        public override DateTime UpdateDate
        {
            get
            {
                return new DateTime();
            }
        }

        public override string Status
        {
            get
            {
                return "R";
            }
        }

        public override string Summary
        {
            get
            {
                return "Test Item Instance by JbL";
            }
        }

        public override string AdditionalSummary
        {
            get
            {
                return "Special fields for this instance type";
            }
        }


        public override string ToString()
        {
            return "Test Item " + ID + "\n" +
            "Type " + EntityName + "\n" +
            "Updated " + UpdateDate + "\n" +
            "Status " + Status + "\n" +
            "Summary: " + Summary + "\n";
        }

        public void setNumber(string number)
        {
            this._number = number;
        }

        public override string Details
        {
            get
            {
                return ToString();
            }
        }



    }

}
