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
    /// Command can be used to delete a file which is no longer needed.
    /// </summary>
    public class FileDeleteCommand : AbstractStationCommand
    {
        public override bool IsStationScope { get { return false; } }
        public override bool IsInstanceScope { get { return true; } }
        public override bool IsBatchInstanceScope { get { return true; } }

        public override CommandResult Execute(Station station, Instance instance)
        {
            FileInstanceInfo i = (FileInstanceInfo)instance;
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
                    File.Delete(i.Path);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = Util.ExceptionMessageIncludingInners(ex);
            }
            return result;
        }

    }
}
