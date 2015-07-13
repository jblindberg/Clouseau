using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for StationEntityTest and is intended
    ///to contain all StationEntityTest Unit Tests
    ///</summary>
	[TestClass()]
	public class StationEntityTest
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
		///A test for getField
		///</summary>
		[TestMethod()]
		public void getFieldTest()
		{
			string name = "TEST_STATION_ENTITY"; 
			StationEntity se = new StationEntity(name);
			
			StationField sf1 = new StationField("Title");
			sf1.Level = (1);
			StationField sf2 = new StationField("CreateDate");
			sf2.Level = (1);
			StationField sf3 = new StationField("Detail");
			sf3.Level = (2);
			StationField sf4 = new StationField("UniqueId");

			se.AddField(sf1);
			se.AddField(sf2);
			se.AddField(sf3);
			se.AddField(sf4);

			// getField(String fieldName)
			Assert.AreEqual(2, se.GetField("Detail").Level);

			// getSummaryFields // level 1
			List<StationField> sflist = se.SummaryFields;
			Assert.AreEqual(2, sflist.Count);
			Assert.AreEqual("Title", sflist[0].Name);

			// getFields / Add / setFields then verify
			sflist = se.Fields;
			Assert.AreEqual(4, sflist.Count);
			sflist.Add(new StationField("Name"));
			se.Fields = (sflist);
			sflist = se.Fields;
			Assert.AreEqual(5, sflist.Count);
			Assert.AreEqual("Title", sflist[0].Name);
			Assert.AreEqual("Name", sflist[4].Name);
			sflist = se.SummaryFields;
			Assert.AreEqual(2, sflist.Count);
			Assert.AreEqual("Title", sflist[0].Name);
			Assert.AreEqual(1, sflist[1].Level);

		}


	}
}
