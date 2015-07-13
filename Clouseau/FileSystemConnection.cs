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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Clouseau
{
    /// <summary>
    /// Represents one usage of a connection to a possibly remote file system path.
    /// </summary>
    public class FileSystemConnection : IDisposable
    {

        internal FileSystemConnection(FileSystemSharedConnection sharedConnection, FileSystemConnectionManager manager)
        {
            this.sharedConnection = sharedConnection;
            this.manager = manager;
        }

        private FileSystemSharedConnection sharedConnection;
        internal FileSystemSharedConnection SharedConnection { get { return sharedConnection; } }

        private FileSystemConnectionManager manager;

        public string ErrorMessage { get { return sharedConnection.ErrorMessage; } }

        public void Dispose()
        {
            manager.Release(this);
        }

    }


    /// <summary>
    /// Represents a connection to a possibly remote file system path.
    /// Same instance can be shared across multiple "users" (e.g. stations)
    /// </summary>
    internal class FileSystemSharedConnection
    {
        public FileSystemSharedConnection(FileSystemConnectionManager manager, string sharePath, UncAccessWithCredentials unc, string error)
        {
            this.unc = unc;
            this.SharePath = sharePath;
            this.ErrorMessage = error;
            this.manager = manager;
        }

        private FileSystemConnectionManager manager;
        private UncAccessWithCredentials unc;
        private int useCount;

        internal string SharePath { get; set; }
        internal string ErrorMessage { get; set; }

        internal void IncrementUse()
        {
            useCount++;
        }

        internal void DecrementUse()
        {
            if (useCount == 0)
                throw new InvalidOperationException("Use count already zero");

            if (--useCount == 0)
            {
                manager.ReleaseShared(this);

                // if unc not null, dispose unc
                // NOTE: tried without disposing unc, it gave intermittent unexpected network error
                //if (unc != null)
                //    unc.Dispose(); // unmanaged resource
            }
        }
    }




    public class FileSystemConnectionManager
    {
        private static readonly object SingletonLock = new object();
        private static FileSystemConnectionManager _singletonInstance;
        public static FileSystemConnectionManager Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    if (_singletonInstance == null)
                        _singletonInstance = new FileSystemConnectionManager();
                    return _singletonInstance;
                }
            }
        }

        private FileSystemConnectionManager() // private to prevent others from calling constructor
        {
        }


        private const string LocalSharePath = "localhost";

        private object threadLock = new object();

        private readonly List<FileSystemSharedConnection> pool = new List<FileSystemSharedConnection>();

        public FileSystemConnection GetConnection(string user, string password, string path)
        {
            string sharePath = (path.StartsWith(@"\\")) ? GetBaseSharePath(path) : LocalSharePath;
            FileSystemSharedConnection sharedConnection;
            lock (threadLock)
            {
                sharedConnection = GetSharedConnection(sharePath, user, password);
                sharedConnection.IncrementUse();
            }
            return new FileSystemConnection(sharedConnection, this);
        }

        private FileSystemSharedConnection GetSharedConnection(string sharePath, string user, string password)
        {
            var sharedConn = pool.FirstOrDefault(c => c.SharePath == sharePath);
            if (sharedConn == null)
            {
                // not yet in pool; create shared connection and add to pool

                string error = null;
                UncAccessWithCredentials unc = null;

                if (sharePath != LocalSharePath)
                {
                    unc = Util.ConnectUnc(user, password, sharePath, out error);
                }
                sharedConn = new FileSystemSharedConnection(this, sharePath, unc, error);
                pool.Add(sharedConn);
            }
            return sharedConn;
        }

        private static Regex _regexUncSharePath =
            new Regex(@"^(\\\\[a-zA-Z0-9-\._]+\\[a-zA-Z0-9`~!@#$%^&(){}'._-]+)\\.*$");

        private string GetBaseSharePath(string path)
        {
            path = path.Replace("/", @"\");
            Match m = _regexUncSharePath.Match(path);
            if (!m.Success)
                throw new Exception("Invalid UNC share path");
            return m.Groups[1].Value;
        }

        public void Release(FileSystemConnection conn)
        {
            lock (threadLock)
            {
                conn.SharedConnection.DecrementUse();
            }
        }

        internal void ReleaseShared(FileSystemSharedConnection fileSystemSharedConnection)
        {
            // Visual Studio (or Resharper) shows a warning that I don't think is true:
            //     "﻿The field is sometimes used inside synchronized block and sometimes used without synchronization"
            // Doing a code read, I verified that the following line is only called inside a lock (threadlock).
            pool.Remove(fileSystemSharedConnection);
        }

    }

}
