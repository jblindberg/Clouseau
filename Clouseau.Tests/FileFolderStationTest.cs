using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for FileFolderStationTest and is intended
    ///to contain all FileFolderStationTest Unit Tests
    ///</summary>
	[TestClass()]
	public class FileFolderStationTest
	{

		private static string STATION_CONFIG_PATH = ConfigDataTest.TESTFILEDIR + "FileFolderTest_config.xml";
		private Stream configStream = new FileStream(STATION_CONFIG_PATH, FileMode.Open, FileAccess.Read);

		private static string STATION_REMOTE_CONFIG_PATH = ConfigDataTest.TESTFILEDIR + "FileFolderRemoteTest_config.xml";
		private Stream remoteConfigStream = new FileStream(STATION_REMOTE_CONFIG_PATH, FileMode.Open, FileAccess.Read);

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

		private FileFolderStation CreateTestStation()
		{
			ConfigData stationConfig = new ConfigData(configStream);
			FileFolderStation target = new FileFolderStation();
			target.Initialize(stationConfig, memory, null);
			return target;
		}

		private FileFolderStation CreateRemoteTestStation()
		{
			ConfigData stationConfig = new ConfigData(remoteConfigStream);
			FileFolderStation target = new FileFolderStation();
			target.Initialize(stationConfig, memory, null);
			return target;
		}


		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doRemoteSearchMaxTest()
		{
			FileFolderStation s = CreateRemoteTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
			InstanceRefList actual = s.DoSearch(crit);

			Assert.IsTrue(actual.List.Count > 0);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
			}

			Assert.AreEqual(s.MaxSearchResults, instances.Count);

		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doRemoteSearchAllTest()
		{
			FileFolderStation s = CreateRemoteTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
			Criterion c1 = new Criterion("STATUS", Criterion.NotEqual, "Archive");  
			crit.Add(c1);

			InstanceRefList actual = s.DoSearch(crit);

			Assert.IsTrue(actual.List.Count > 0);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
			}

			Assert.IsTrue(FoundResult(instances, "3292216", "Holding", "3292216.tiff"));
			Assert.AreEqual(6, instances.Count);

		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doSearchAllTest()
		{
			FileFolderStation s = CreateTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
			InstanceRefList actual = s.DoSearch(crit);

			Assert.AreEqual(5, actual.List.Count);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
			}

			Assert.IsTrue(FoundResult(instances, "H1", "HOLDING", "H1.log"));
			Assert.IsTrue(FoundResult(instances, "H2", "HOLDING", "H2.LOG"));
			Assert.IsTrue(FoundResult(instances, "H3", "HOLDING", "H3.txt"));
			Assert.IsTrue(FoundResult(instances, "R1234", "Ready", "R1234.log"));
			Assert.IsTrue(FoundResult(instances, "SUB999", "SUBHOLDING", "SUB999.log"));

		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doSearchUniqueIDTest()
		{
			FileFolderStation s = CreateTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
			InstanceRefList actual = s.DoSearch(crit);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
			}

			Instance inst = getResultById(instances, "SUB999");
			// Assumes Unique ID is based on folder + filename; subject to change in future
			Assert.AreEqual("Holding/Sub_Holding/SUB999.log", inst.UniqueId);

		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doSearchDateTest()
		{
			FileFolderStation s = CreateRemoteTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
			Criterion c1 = new Criterion(Field.InstanceUpdateDate, Criterion.GreaterThanOrEqual, "4/1/2010");
			crit.Add(c1);
			Criterion c3 = new Criterion(Field.InstanceUpdateDate, Criterion.LessThan, "5/1/2010");
			crit.Add(c3);
			Criterion c2 = new Criterion(Field.InstanceStatus, Criterion.Equal, "Archive");
			crit.Add(c2);

			InstanceRefList actual = s.DoSearch(crit);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
			}

			Assert.AreEqual(10, actual.List.Count);
			Assert.IsTrue(FoundResult(instances, "3307322", "Archive", "3307322.pdf"));

		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doSearchArrivalDateTest()
		{
			FileFolderStation s = CreateRemoteTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
            Criterion c1 = new Criterion(Field.InstanceArrivalDate, Criterion.GreaterThanOrEqual, "2/12/2015 16:05:07");
			crit.Add(c1);
            Criterion c3 = new Criterion(Field.InstanceArrivalDate, Criterion.LessThan, "2/12/2015 16:05:09");
			crit.Add(c3);

            DateTime start = new DateTime(2015, 2, 12, 16, 5, 6);
            DateTime end = new DateTime(2015, 2, 12, 16, 5, 9);

			InstanceRefList actual = s.DoSearch(crit);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
				Assert.IsTrue(i.ArrivalDate > start);
				Assert.IsTrue(i.ArrivalDate < end);
			}

			Assert.IsTrue(actual.List.Count > 0, "Will fail if file dates have changed"); // NOTE:  Depends on the last time the test files were moved/copied
            // Thursday, February 12, 2015, 4:05:07 PM
            // Thursday, February 12, 2015, 4:05:08 PM

		}

		/// <summary>
		///A test for doSearch
		///</summary>
		[TestMethod()]
		public void doSearchIDTest()
		{
			FileFolderStation s = CreateRemoteTestStation();

			ICollection<Criterion> crit = new List<Criterion>();
			Criterion c1 = new Criterion(Field.InstanceUpdateDate, Criterion.GreaterThanOrEqual, "5/1/2010");
			crit.Add(c1);
			Criterion c2 = new Criterion(Field.EntityId, Criterion.Equal, "3230596");
			crit.Add(c2);

			InstanceRefList actual = s.DoSearch(crit);

			List<Instance> instances = actual.InstanceList;

			foreach (Instance i in instances)
			{
				Console.WriteLine(i.Details);
			}

			Assert.AreEqual(2, actual.List.Count);
			Assert.IsTrue(FoundResult(instances, "3230596", "Archive", "3230596.pdf"));

		}





		private bool FoundResult(List<Instance> instances, string id, string status, string fname)
		{
			var query = from i in instances select i;
			query = query.Where(i => i.ID == id && i.Status == status && i.Details.Contains(fname));

			return (query.Count() == 1);
		}

		private Instance getResultById(List<Instance> instances, string id)
		{
			var query = from i in instances select i;
			query = query.Where(i => i.ID == id);

			return query.FirstOrDefault();
		}




		/// <summary>
		///A test for CompareFileId
		///</summary>
		[TestMethod()]
		public void CompareFileIdTest()
		{
			FileFolderStation target = new FileFolderStation();
			FileInfo f = new FileInfo("12345.pdf");
			string id = "12345";
			int expected = 0; 
			int compare = target.CompareFileId(f, id);
			Assert.AreEqual(expected, compare);

			compare = target.CompareFileId(f, "1235");
			Assert.IsTrue(compare > 0);

			compare = target.CompareFileId(f, "1235X");
			Assert.IsTrue(compare < 0);

			FileInfo f2 = new FileInfo(@"C:\DIR\12345");
			compare = target.CompareFileId(f2, "12345");
			Assert.IsTrue(compare == 0);

			f2 = new FileInfo(@"3478336");
			compare = target.CompareFileId(f2, "12345678");
			Assert.IsTrue(compare < 0);

			f2 = new FileInfo(@"ABC336.txt");
			compare = target.CompareFileId(f2, "DEF335");
			Assert.IsTrue(compare < 0);
			

		}



        /// <summary>
        ///A test for ContentType
        ///</summary>
        [TestMethod()]
        public void ContentTypeTest2()
        {
            string fname = ("abc.pdf");
            Assert.AreEqual(ContentType.Pdf, Util.FileContentType(fname));

            string path = (@"C:\TEST\abc.xml");
            Assert.AreEqual(ContentType.Xml, Util.FileContentType(path));

            path = (@"test/folder/abc.tif");
            Assert.AreEqual(ContentType.Tiff, Util.FileContentType(path));

            Assert.AreEqual(ContentType.Pdf, Util.ExtensionType(".PDF"));

            Assert.AreEqual(ContentType.Html, Util.ExtensionType(".htm"));

        }


        /// <summary>
        ///A test for Content
        ///</summary>
        [TestMethod()]
        public void ContentTest()
        {
            FileFolderStation s = CreateTestStation();

            ICollection<Criterion> crit = new List<Criterion>();
            InstanceRefList actual = s.DoSearch(crit);

            List<Instance> instances = actual.InstanceList;

            foreach (Instance i in instances)
            {
                Console.WriteLine(i.Details);
            }

            Instance inst = getResultById(instances, "SUB999");
            // Assumes Unique ID is based on folder + filename; subject to change in future
            Assert.AreEqual(ContentType.Txt, s.ContentType(inst));

            byte[] content = s.Content(inst);

            Assert.AreEqual(46113, content.Length);

        }

        /// <summary>
        ///A test for Content
        ///</summary>
        [TestMethod()]
        public void RemoteContentTest()
        {
            FileFolderStation s = CreateRemoteTestStation();

            ICollection<Criterion> crit = new List<Criterion>();
            InstanceRefList actual = s.DoSearch(crit);

            List<Instance> instances = actual.InstanceList;

            Instance inst = getResultById(instances, "2510989");
            Assert.AreEqual(ContentType.Pdf, s.ContentType(inst));

            byte[] content = s.Content(inst);
            Assert.AreEqual(2535804, content.Length);

        }




	}
}
