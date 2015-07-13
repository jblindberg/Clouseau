using System;
using System.Runtime.InteropServices;

namespace Clouseau
{
    /// <summary>
    /// From work by Adrian Hayes 10/2009
    /// http://www.codeproject.com/Articles/43091/Connect-to-a-UNC-Path-with-Credentials
    /// </summary>
    public class UncAccessWithCredentials : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct UseInfo2
        {
            internal string ui2_local;
            internal string ui2_remote;
            internal string ui2_password;
            internal uint ui2_status;
            internal uint ui2_asg_type;
            internal uint ui2_refcount;
            internal uint ui2_usecount;
            internal string ui2_username;
            internal string ui2_domainname;
        }

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint NetUseAdd(
            string uncServerName,
            uint level,
            ref UseInfo2 buf,
            out uint parmError);

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint NetUseDel(
            string uncServerName,
            string useName,
            uint forceCond);

        private bool disposed;

        private string sUncPath;
        private string sUser;
        private string sPassword;
        private string sDomain;
        private int iLastError;

        /// <summary>
        /// The last system error code returned from NetUseAdd or NetUseDel.  Success = 0
        /// </summary>
        public int LastError
        {
            get { return iLastError; }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                disposed = true;
                NetUseDelete();
            }
            disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Connects to a UNC path using the credentials supplied.
        /// </summary>
        /// <param name="uncPath">Fully qualified domain name UNC path</param>
        /// <param name="user">A user with sufficient rights to access the path.</param>
        /// <param name="domain">Domain of User.</param>
        /// <param name="password">Password of User</param>
        /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
        public bool NetUseWithCredentials(string uncPath, string user, string domain, string password)
        {
            sUncPath = uncPath;
            sUser = user;
            sPassword = password;
            sDomain = domain;
            return NetUseWithCredentials();
        }

        private bool NetUseWithCredentials()
        {
            uint returncode;
            try
            {
                UseInfo2 useinfo = new UseInfo2();

                useinfo.ui2_remote = sUncPath;
                useinfo.ui2_username = sUser;
                useinfo.ui2_domainname = sDomain;
                useinfo.ui2_password = sPassword;
                useinfo.ui2_asg_type = 0;
                useinfo.ui2_usecount = 1;
                uint paramErrorIndex;
                returncode = NetUseAdd(null, 2, ref useinfo, out paramErrorIndex);
                iLastError = (int)returncode;
                return returncode == 0;
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        /// <summary>
        /// Ends the connection to the remote resource 
        /// </summary>
        /// <returns>True if it succeeds.  Use LastError to get the system error code</returns>
        public bool NetUseDelete()
        {
            uint returncode;
            try
            {
                returncode = NetUseDel(null, sUncPath, 2);
                iLastError = (int)returncode;
                return (returncode == 0);
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        ~UncAccessWithCredentials()
        {
            Dispose();
        }

    }
}
