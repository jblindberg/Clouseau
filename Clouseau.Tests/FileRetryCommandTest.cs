using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    
    
    /// <summary>
    ///This is a test class for FileRetryCommandTest and is intended
    ///to contain all FileRetryCommandTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileRetryCommandTest
    {

        private static string COMMAND_CONFIG_PATH = ConfigDataTest.TESTFILEDIR + "FileRetryCommandTest_config.xml";
        private Stream configStream = new FileStream(COMMAND_CONFIG_PATH, FileMode.Open, FileAccess.Read);


        private FileRetryCommand CreateTestCommand()
        {
            ConfigData stationConfig = new ConfigData(configStream);
            FileRetryCommand target = new FileRetryCommand();
            target.Initialize(stationConfig);
            return target;
        }

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
        ///A test for CopyToRetry
        ///</summary>
        [TestMethod()]
        public void CopyToRetryRemoteTest()
        {
            FileRetryCommand cmd = new FileRetryCommand();
            ConfigData stationConfig = new ConfigData(configStream);
            cmd.Initialize(stationConfig);

            string newID = DateTime.Now.Ticks.ToString();

            // TODO to test remote folder (file share) functionality:
            //   Below, use the path of an accessible remote folder
            //   Copy the subfolders of the RemoteFileFolderTest folder to that remote folder
            //   And update the <retryPath> in FileRetryCommandTest_config.xml 

            //string remoteTargetDir = @"\\test1.company.com\C$\Inspector\Test\Delay\Ready\";
            string remoteTargetDir = @"C:\projects\Open\Clouseau\Clouseau.Tests\Files\FileFolderTest\Ready\";
            string remoteTargetPath = remoteTargetDir + newID + ".xml";

            // delete existing file
            // NOTE: This is commented out because it had problems getting error 1219 from the UNC connection
            // Workaround: new filename every time.
            //string possibleError;
            //bool useUNC;
            //UNCAccessWithCredentials unc =
            //    Util.ConnectUNC("administrator", "password", remoteTargetDir, out possibleError, out useUNC);
            //Assert.IsNull(possibleError, "Error trying to delete remote test file: " + possibleError);
            //if (File.Exists(remoteTargetPath))
            //{
            //    File.Delete(remoteTargetPath);
            //}
            //if (unc != null) unc.Dispose();

            FileFolderInstance instance = new FileFolderInstance();
            instance.ID = newID;
            instance.FileName = "13531.xml";
            instance.Location = "FileFolderTest/Failed/" + instance.FileName;
            instance.Path = ConfigDataTest.TESTFILEDIR + instance.Location;

            cmd.CopyToRetry(instance);

            Assert.IsTrue(File.Exists(remoteTargetPath));
        }
    }
}
