using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clouseau.Tests
{
    [TestClass]
    public class FileSystemConnectionTest
    {
        // TODO to test UNC functionality: 
        //    update the UNC paths below to actual file share locations
        //    put some files in each test folder
        //    update the related user and passwords

        private const string UNC_CONNECTED_1 = @"\\share10\C$\Test";
        private const string UNC_CONNECTED_2 = @"\\share11\C$\Test";
        private const string UNC_UNCONNECTED_3 = @"\\share12\C$\Test";
        private const string UNC_UNCONNECTED_4 = @"\\share13\C$\Users";

        private const string USER = "administrator"; // same user for all servers
        private const string PASS_1 = "password";
        private const string PASS_2 = "password";
        private const string PASS_3 = "password";
        private const string PASS_4 = "password";



        [TestMethod]
        public void ExistingConn1()
        {
            VerifyReadManualRelease(UNC_CONNECTED_1, PASS_1);
        }

        [TestMethod]
        public void ExistingConn2()
        {
            VerifyReadUsingDispose(UNC_CONNECTED_1, PASS_1);
        }

        [TestMethod]
        public void Unconnected1()
        {
            VerifyReadManualRelease(UNC_UNCONNECTED_3, PASS_3);
        }

        [TestMethod]
        public void Unconnected2()
        {
            VerifyReadUsingDispose(UNC_UNCONNECTED_3, PASS_3);
        }

        [TestMethod]
        public void NestedUsing()
        {
            var fsmgr = FileSystemConnectionManager.Instance;
            using (var conn1 = fsmgr.GetConnection(USER, PASS_4, UNC_UNCONNECTED_4))
            {
                using (var conn2 = fsmgr.GetConnection(USER, PASS_3, UNC_UNCONNECTED_3))
                {
                    VerifyDirRead(UNC_UNCONNECTED_4);
                    VerifyDirRead(UNC_UNCONNECTED_3);
                }
                VerifyDirRead(UNC_UNCONNECTED_4);
            }
        }


        [TestMethod]
        public void Unconnected2overlap()
        {
            string pass = PASS_4;
            string path = UNC_UNCONNECTED_4;

            var fsmgr = FileSystemConnectionManager.Instance;
            var conn1 = fsmgr.GetConnection(USER, pass, path);
            var conn2 = fsmgr.GetConnection(USER, pass, path);
            // overlap reading and closing
            VerifyDirRead(path);
            fsmgr.Release(conn1);
            VerifyDirRead(path);
            fsmgr.Release(conn2);
        }

        [TestMethod]
        public void CombinationOverlap6()
        {
            var fsmgr = FileSystemConnectionManager.Instance;
          
            var conn1 = fsmgr.GetConnection(USER, PASS_1, UNC_CONNECTED_1);
            var conn2 = fsmgr.GetConnection(USER, PASS_2, UNC_CONNECTED_2);
            var conn3 = fsmgr.GetConnection(USER, PASS_3, UNC_UNCONNECTED_3);
            var conn4 = fsmgr.GetConnection(USER, PASS_4, UNC_UNCONNECTED_4);
            var conn5 = fsmgr.GetConnection(USER, PASS_2, UNC_CONNECTED_2); // 5 = 2
            var conn6 = fsmgr.GetConnection(USER, PASS_4, UNC_UNCONNECTED_4); // 6 = 4
            
            // overlap reading and closing
            VerifyDirRead(UNC_CONNECTED_1);
            VerifyDirRead(UNC_CONNECTED_2);
            VerifyDirRead(UNC_UNCONNECTED_3);
            VerifyDirRead(UNC_UNCONNECTED_4);
            
            fsmgr.Release(conn6);
            VerifyDirRead(UNC_CONNECTED_1);
            VerifyDirRead(UNC_CONNECTED_2);
            VerifyDirRead(UNC_UNCONNECTED_3);
            VerifyDirRead(UNC_UNCONNECTED_4);

            fsmgr.Release(conn1);
            VerifyDirRead(UNC_CONNECTED_2);
            VerifyDirRead(UNC_UNCONNECTED_3);
            VerifyDirRead(UNC_UNCONNECTED_4);
            
            fsmgr.Release(conn2);
            VerifyDirRead(UNC_CONNECTED_2);
            VerifyDirRead(UNC_UNCONNECTED_3);
            VerifyDirRead(UNC_UNCONNECTED_4);
            
            fsmgr.Release(conn3);
            VerifyDirRead(UNC_CONNECTED_2);
            VerifyDirRead(UNC_UNCONNECTED_4);
            
            fsmgr.Release(conn4);
            VerifyDirRead(UNC_CONNECTED_2);
            
            fsmgr.Release(conn5);
        }

        private static void VerifyDirRead(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            int fileCount = 0;
            foreach (var f in dir.EnumerateFiles())
            {
                Console.WriteLine(f.Name);
                fileCount++;
            }
            Assert.IsTrue(fileCount > 0);
        }

        private static void VerifyReadManualRelease(string path, string password)
        {
            var fsmgr = FileSystemConnectionManager.Instance;
            var conn = fsmgr.GetConnection(USER, password, path);

            VerifyDirRead(path);

            fsmgr.Release(conn);
        }

        private static void VerifyReadUsingDispose(string path, string password)
        {
            var fsmgr = FileSystemConnectionManager.Instance;
            using (var conn = fsmgr.GetConnection(USER, password, path))
            {
                VerifyDirRead(path);
            }
        }
    }
}
