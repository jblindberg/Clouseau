using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
	[TestClass]
	public class StationTest_1
	{

		private static string STATION_CONFIG_PATH = ConfigDataTest.TESTFILEDIR + "TestStation1_config.xml";
		private Stream configStream = new FileStream(STATION_CONFIG_PATH, FileMode.Open, FileAccess.Read);

		private InstanceMemory memory = new InstanceMemory();

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


		internal virtual Station CreateTestStation()
		{
			ConfigData stationConfig = new ConfigData(configStream);
			Station target = new TestStation1();
			target.Initialize(stationConfig, memory, null);
			return target;
		}

		/// <summary>
		///A test for getStationEntity
		///</summary>
		[TestMethod()]
		public void getEntityTest()
		{
			Station target = CreateTestStation();

			StationEntity se1 = target.GetEntity("PROFILE");
			Assert.AreEqual("PROFILE", se1.Name);
			StationField sf1 = se1.GetField("UNIQUE_ID");
			Assert.AreEqual(Field.NumberType, sf1.Type);

		}


		/// <summary>
		///A test for getEntities
		///</summary>
		[TestMethod()]
		public void getEntitiesTest()
		{
			Station s = CreateTestStation();
			AssertStationEntities(s);

		}

		public static void AssertStationEntities(Station s)
		{
			List<StationEntity> actual;
			actual = s.Entities;
			Assert.AreEqual(2, actual.Count);
			StationEntity se2 = actual[1];
			Assert.AreEqual("ORDER", se2.Name);
			StationField sf1 = se2.GetField("CUST_NAME");
			Assert.AreEqual(Field.TextType, sf1.Type);
		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doSearchTest()
		{
			Station target = CreateTestStation();

			Criterion c1 = new Criterion("F1", "EQ", "V1");
			Criterion c2 = new Criterion("F2", "EQ", "V2");
			Criterion c3 = new Criterion("F1", "GT", "V3");

			ICollection<Criterion> crit = new List<Criterion>();
			crit.Add(c1);
			crit.Add(c2);
			crit.Add(c3);

			InstanceRefList actual;
			actual = target.DoSearch(crit);
			Assert.IsTrue(actual.List.Count > 1);
		}


	
	}
}
