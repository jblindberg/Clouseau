using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for InstanceMemoryTest and is intended
    ///to contain all InstanceMemoryTest Unit Tests
    ///</summary>
	[TestClass()]
	public class InstanceMemoryTest
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
		///A test for getRef
		///</summary>
		[TestMethod()]
		public void getRefTest()
		{
			InstanceMemory mem = new InstanceMemory();
            Station testStation = new TestStation();

			TestInstance1 t1 = new TestInstance1("123", "123-abc", "FILLED");
			TestInstance1 t2 = new TestInstance1("123", "123-def", "UNFILLED");
			TestInstance1 t3 = new TestInstance1("456", "456-abc", "NEW");
			TestInstance1 t4 = new TestInstance1("789", "789-abc", "FILLED");

            InstanceRef i1 = mem.AddRef(t1, testStation);
            InstanceRef i2 = mem.AddRef(t2, testStation);
            InstanceRef i3 = mem.AddRef(t3, testStation);
            InstanceRef i4 = mem.AddRef(t4, testStation);
            InstanceRef i2b = mem.AddRef(t2, testStation);
            InstanceRef i1c = mem.AddRef(t1, testStation);

			InstanceRef expected = i2b; 
			InstanceRef actual;
			actual = mem.GetRef(i2b.Id);
			Assert.AreEqual(expected, actual);

			Assert.AreEqual("UNFILLED", actual.Instance.Status);
			Assert.AreEqual("123", actual.Instance.ID);
			Assert.AreEqual(t2, actual.Instance);

		}


	}
}
