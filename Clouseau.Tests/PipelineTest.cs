using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for PipelineTest and is intended
    ///to contain all PipelineTest Unit Tests
    ///</summary>
	[TestClass()]
	public class PipelineTest
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



		private InstanceMemory memory = new InstanceMemory();
		string configName = ConfigDataTest.TESTFILEDIR + "TestPipeline_config.xml";

		/// <summary>
		///A test for Pipeline Constructor
		///</summary>
		[TestMethod()]
		public void PipelineConstructorTest()
		{
            Pipeline p = getPipeline();

			Assert.AreEqual(2, p.Stations.Count);

            Assert.AreEqual(2, p.Commands.Count);

			Station s = p.Stations[0];
			StationTest_1.AssertStationEntities(s);  // use other unit tests to validate TestStation1

			// test the 2nd station too
			Station s2 = p.Stations[1];
			Assert.AreEqual("Test Station 1 version B", s2.StationDescription);
			StationEntity se2 = s2.GetEntity("PRODUCT");
			StationField sf2 = se2.GetField("UPDATE_DATE");
			Assert.AreEqual(Field.DateType, sf2.Type);

		}

        private Pipeline getPipeline()
        {
            Pipeline p = new Pipeline(configName, memory);

            if (p.HasError)
            {
                Assert.Fail(p.Errors);
            }
            return p;
        }

        [TestMethod()]
        public void CommandTest_Station()
        {
            Pipeline p = getPipeline();
            Station s = p.Stations[0];

            StationCommand cmd = p.GetCommandByName("TEST_STATION_COMMAND");

            Assert.IsTrue(cmd.IsStationScope);

            CommandResult result = cmd.Execute(s);
            Assert.IsTrue(result.Success, "command failed to execute");

            StationCommand cmd2 = s.Commands[0];
            result = cmd2.Execute(s);
            Assert.IsTrue(result.Success, "command failed to execute");

            Assert.IsFalse(cmd.IsInstanceScope);
            Instance i =  new TestInstance1("id1","uid2","COMPLETE");
            result = cmd.Execute(s, i);
            Assert.IsFalse(result.Success);
        }


	}
}
