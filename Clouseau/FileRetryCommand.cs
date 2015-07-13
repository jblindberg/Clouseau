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
using System.IO;

namespace Clouseau
{
    /// <summary>
    /// Retry a file that failed
    /// </summary>
    public class FileRetryCommand : AbstractStationCommand
    {
        public override bool IsStationScope { get { return false; } }
        public override bool IsInstanceScope { get { return true; } }
        public override bool IsBatchInstanceScope { get { return true; } }


        private string retryPath;
        private string retryUser;
        private string retryPassword;

        private string possibleError; 

        public override void Initialize(ConfigData configData)
        {
            base.Initialize(configData);

            retryPath = configData.RequiredValue("retryPath");
            retryUser = configData.RequiredValue("retryUser");
            retryPassword = configData.RequiredValue("retryPassword");
        }

        /// <summary>
        /// Is this command enabled for the specified Instance at the specified Station? 
        /// </summary>
        /// <param name="station"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public override bool IsEnabled(Station station, Instance instance)
        {
            FileInstanceInfo i = (FileInstanceInfo)instance;
            FileRetryHandler handler = (FileRetryHandler)station;
            return handler.CanRetry(i);
        }

        /// <summary>
        /// Perform the retry.  Copies the file to a designated retry folder,
        /// using the ID as the file basename,
        /// and cleans up the failed file and associated files/records.
        /// </summary>
        /// <param name="station"></param>
        /// <param name="instance">Expect IFileInstanceInfo type, such as FileFolderInstance</param>
        /// <returns></returns>
        public override CommandResult Execute(Station station, Instance instance)
        {
            FileInstanceInfo i = (FileInstanceInfo)instance;
            FileRetryHandler handler = (FileRetryHandler)station;

            CommandResultWritable result = new CommandResultWritable
            {
                Command = this,
                Station = station,
                Instance = instance,
                Success = true,
            };

            try
            {
                using (FileSystemConnectionManager.Instance.GetConnection(i.User, i.Password, i.Path))
                {
                    CopyToRetry(i);

                    handler.CleanUpFailed(i);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = Util.ExceptionMessageIncludingInners(ex);
                if (possibleError != null) result.Message = result.Message + "; " + possibleError;
            }
            return result;
        }

        /// <summary>
        /// Copy this file to the configured retry folder, using the ID as the file basename.
        /// Throws exception if destination file already exists.
        /// </summary>
        /// <param name="instance"></param>
        public virtual void CopyToRetry(FileInstanceInfo instance)
        {
            FileInfo sourceFile = new FileInfo(instance.Path);
            string targetFileName = instance.ID + sourceFile.Extension;
            string targetPath = retryPath + "\\" + targetFileName;

            //var unc = Util.ConnectUNC(retryUser, retryPassword, retryPath, out possibleError, out useUNC);
            var unc = FileSystemConnectionManager.Instance.GetConnection(retryUser, retryPassword, retryPath);
            possibleError = (unc != null) ? unc.ErrorMessage : null;

            if (File.Exists(targetPath))
            {
                possibleError = null;
                throw new Exception("File already exists in Retry folder: " + targetFileName);
            }

            try
            {
                sourceFile.CopyTo(targetPath, false); // won't overwrite
            }
            finally
            {
                if (unc != null) unc.Dispose();
            }
        }

    }

    /// <summary>
    /// Interface for station that provides the methods/properties needed by the FileRetryCommand
    /// </summary>
    public interface FileRetryHandler
    {
        /// <summary>
        /// This method is called after the file is copied to its Retry location
        /// </summary>
        /// <param name="instance"></param>
        void CleanUpFailed(FileInstanceInfo instance);

        /// <summary>
        /// Is the File Retry command possible for this instance?
        /// </summary>
        /// <param name="instance"></param>
        bool CanRetry(FileInstanceInfo instance);
    
    }
}
