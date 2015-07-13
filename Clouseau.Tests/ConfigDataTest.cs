using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{


	/// <summary>
	///This is a test class for ConfigDataTest and is intended
	///to contain all ConfigDataTest Unit Tests
	///</summary>
	[TestClass()]
	public class ConfigDataTest
	{
        // TODO put these files in a resource, or else dynamically find the right path
		//public static string TESTFILEDIR = @"..\..\..\..\..\UnitTests\Clouseau.Tests\Files\";
        public static string TESTFILEDIR = @"C:\projects\Open\ClouseauOpen\Clouseau.Tests\Files\";
		
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

		XElement x1 =
				new XElement("config",
					new XElement("child", "Child Text 1"),
					new XElement("child", "Child Text 2")
					);

		XElement x2attrNoText =
				new XElement("config",
					new XElement("child", new XAttribute("attrib", "abc")),
					new XElement("child", "Child Text 2")
					);

		XElement x2number =
				new XElement("config",
					new XElement("child", "11"),
					new XElement("child", "Child Text 2")
					);

		XElement x3root1config =
			new XElement("root",
				new XElement("config",
					new XElement("child", "Child Text 1"),
					new XElement("child", "Child Text 2")
					)
				);

		XElement x3root2config =
			new XElement("root",
				new XElement("config",
					new XElement("child", "Child Text 1"),
					new XElement("child", "Child Text 2")
					),
				new XElement("xxx",
					new XElement("child", "Child Text x1"),
					new XElement("child", "Child Text x2")
					),
				new XElement("config",
					new XElement("child", "3"),
					new XElement("child", "4")
					)
				);

		XElement xBase =
				new XElement("config", "Child Text 1");

		[TestMethod()]
		public void valueTestBase()
		{
			XElement configElement = xBase;
			ConfigData target = new ConfigData(configElement);

			string expected = "Child Text 1";
			string actual;
			actual = target.Value();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		public void valueTest()
		{
			XElement configElement = x1;
			ConfigData target = new ConfigData(configElement);

			string property = "child";
			string expected = "Child Text 1";
			string actual;
			actual = target.Value(property);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		public void valueTest_PropertyNotFound()
		{
			XElement configElement = x1;
			ConfigData target = new ConfigData(configElement);

			string property = "xxx";
			string expected = null;
			string actual;
			actual = target.Value(property);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		public void valueTest_Empty()
		{
			XElement configElement = x2attrNoText;
			ConfigData target = new ConfigData(configElement);

			string property = "child";
			string expected = "";
			string actual;
			actual = target.Value(property);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		public void valuesTest()
		{
			XElement configElement = x1;
			ConfigData target = new ConfigData(configElement);
			string property = "child";

			string[] expected = { "Child Text 1", "Child Text 2" };
			string[] actual;
			actual = target.Values(property);

			Assert.IsTrue(ArrayEquals(expected, actual));
		}

		[TestMethod()]
		public void intValueTest()
		{
			XElement configElement = x2number;
			ConfigData target = new ConfigData(configElement);
			string property = "child";
			Nullable<int> expected = 11;
			Nullable<int> actual;
			actual = target.IntValue(property);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod()]
		public void requiredValueTest()
		{
			XElement configElement = x1;
			ConfigData target = new ConfigData(configElement);
			string property = "child";
			string expected = "Child Text 1";
			string actual;
			actual = target.RequiredValue(property);
			Assert.AreEqual(expected, actual);

			configElement = x2attrNoText;
			target = new ConfigData(configElement);
			property = "child";
			expected = "";
			try
			{
				actual = target.RequiredValue(property);
				Assert.Fail("should throw exception");
			}
			catch
			{
				// should throw exception	
			}

		}

		[TestMethod()]
		public void getConfigSectionsTest()
		{
			ConfigData target = new ConfigData(x3root1config);

			string sectionName = "config";
			List<ConfigData> actual = target.GetConfigSections(sectionName);

			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual("Child Text 1", actual[0].Value("child"));

			ConfigData target2 = new ConfigData(x3root2config);
			testx3root2config(target2);

		}

		// do some tests on x3root2config whether from file or built in memory
		private void testx3root2config(ConfigData target2)
		{
			List<ConfigData> actual2 = target2.GetConfigSections("config");

			Assert.AreEqual(2, actual2.Count);
			Assert.AreEqual("3", actual2[1].Value("child"));

			List<ConfigData> children = actual2[1].GetConfigSections("child");
			Assert.AreEqual(2, children.Count);
		}



		// utility method to compare two arrays for equality
		// copied from Kathy Kam blog
		public static bool ArrayEquals<T>(T[] a, T[] b)
		{
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; i++)
			{
				if (!a[i].Equals(b[i]))
					return false;
			}
			return true;
		}

	}
}
