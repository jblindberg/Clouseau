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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for UtilTest and is intended
    ///to contain all UtilTest Unit Tests
    ///</summary>
	[TestClass()]
	public class UtilTest
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
		///A test for DateOnly
		///</summary>
		[TestMethod()]
		public void DateOnlyTest()
		{
			string value = "3/5/10 3:05";
			bool expected = false; 
			bool actual = Util.DateOnly(value);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for DateOnly
		///</summary>
		[TestMethod()]
		public void DateOnlyTest2()
		{
			string value = "3/5/10";
			bool expected = true;
			bool actual = Util.DateOnly(value);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for DateOnly
		///</summary>
		[TestMethod()]
		public void DateOnlyTest3()
		{
			string value = "3/5/10/20";
			bool expected = false;
			bool actual = Util.DateOnly(value);
			Assert.AreEqual(expected, actual);
		}



        /// <summary>
        ///A test for ConvertStringsToInts
        ///</summary>
        [TestMethod()]
        public void ConvertStringsToIntsTest_empty()
        {
            string[] aStrings = { };
            int[] aInts = { };
            Check_ConvertStringsToInts(aStrings, aInts);
        }

        [TestMethod()]
        public void ConvertStringsToIntsTest_1()
        {
            string[] aStrings = { "12" };
            int[] aInts = { 12 };
            Check_ConvertStringsToInts(aStrings, aInts);
        }

        [TestMethod()]
        public void ConvertStringsToIntsTest_2()
        {
            string[] aStrings = { "12", "23" };
            int[] aInts = { 12, 23 };
            Check_ConvertStringsToInts(aStrings, aInts);
        }

        [TestMethod()]
        public void ConvertStringsToIntsTest_3()
        {
            string[] aStrings = { "12", "", null, "abc", "12abc", "23" };
            int[] aInts = { 12, 23 };
            Check_ConvertStringsToInts(aStrings, aInts);
        }

        private static void Check_ConvertStringsToInts(string[] aStrings, int[] aInts)
        {
            List<string> strings = new List<string>(aStrings);

            List<int> expected = new List<int>(aInts);

            List<int> actual;
            actual = Util.ConvertStringsToInts(strings);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }



	}
}
