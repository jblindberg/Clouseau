using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for CriterionTest and is intended
    ///to contain all CriterionTest Unit Tests
    ///</summary>
	[TestClass()]
	public class CriterionTest
	{


		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for getCriterionByField
		///</summary>
		[TestMethod()]
		public void getCriterionByFieldTest()
		{
			Criterion c1 = new Criterion("F1", "EQ", "V1");
			Criterion c2 = new Criterion("F2", "EQ", "V2");
			Criterion c3 = new Criterion("F1", "GT", "V3");

			ICollection<Criterion> crit = new List<Criterion>();
			crit.Add(c1);
			crit.Add(c2);
			crit.Add(c3);

			string field = "F1";
			List<Criterion> crit2 = Criterion.GetCriteriaByField(crit, field);
			Criterion actual = crit2[0];
			Assert.AreEqual("V1", actual.Value);
			Assert.IsFalse(actual.IsUnaryOperator());
			Assert.AreEqual(2, crit2.Count);
		}

		/// <summary>
		///A test for Definitions
		///</summary>
		[TestMethod()]
		public void DefinitionsTest()
		{
			List<OperationDefinition> defs = Criterion.Definitions;

			foreach (OperationDefinition cd in defs)
			{
				Console.WriteLine("Operation {0} : {1} operands", cd.OpCode, cd.OperandCount);
			}

			Assert.IsTrue(defs.Count >= 8); // assume at least 8 definitions

			var query = from d in defs where d.OpCode == "EQ" select d;
			Assert.AreEqual(2, query.First().OperandCount); // check that we have EQ operation, with 2 operands

		}


	}
}
