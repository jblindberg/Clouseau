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

namespace Clouseau
{
    /// <summary>
    /// Interface representing a command which can be performed on a station,
    /// or on an Instance inside a station.  
    /// Initially this system was a "read only" inspection tool, but commands
    /// were added later, to allow interventions in certain exception cases. 
    /// 
    /// Examples:  Delete a stuck file, Retry a processing step that failed, 
    /// retry the delivery of a document that was stuck in a delivery station.
    /// 
    /// JbL 5/19/2011
    /// </summary>
    public interface StationCommand
    {

        /// <summary>
        /// Can this command operate on an overall Station (instead of an individual Instance)
        /// </summary>
        bool IsStationScope { get; }

        /// <summary>
        /// Can this command operate on an individual Instance?
        /// </summary>
        bool IsInstanceScope { get; }

        /// <summary>
        /// Can this command operate on multiple individual Instances at once?
        /// </summary>
        bool IsBatchInstanceScope { get; }

        /// <summary>
        /// Identifying name (key) for this command
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Short user readable label for use in UI, e.g. menu item
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Longer description of this command, e.g. in ToolTip
        /// </summary>
        string Description { get; }

		/// <summary>
		/// Should the user confirm this command before it is executed?
		/// </summary>
		bool DoConfirm { get; }

		/// <summary>
		/// What prompt should be presented to the user when confirming the command?
		/// </summary>
		string ConfirmPrompt { get; }

        /// <summary>
        /// Is an interactive dialog required to perform this command?
        /// If so, the user interface will use this property to determine what dialog code to invoke.
        /// If not, this returns null.
        /// </summary>
        string Dialog { get; }


        /// <summary>
        /// Is this command enabled for the specified Instance at the specified Station?
        /// Can be used to determine if the command should be displayed for a specific instance in the UI.
        /// </summary>
        /// <param name="station"></param>
        /// <param name="instance"></param>
        /// <returns>True if enabled</returns>
        bool IsEnabled(Station station, Instance instance);

        /// <summary>
        /// Execute the command on the specified Instance at the specified Station
        /// </summary>
        /// <param name="station"></param>
        /// <param name="instance"></param>
        /// <returns>True if successful; may throw exception</returns>
        CommandResult Execute(Station station, Instance instance);

        /// <summary>
        /// Execute the command on the specified Station
        /// </summary>
        /// <param name="station"></param>
        /// <returns>True if successful; may throw exception</returns>
        CommandResult Execute(Station station);

        /// <summary>
        /// initialization using data in ConfigData object.
        /// This should be called immediately after the constructor.
        /// Each subclass should call base.initialize(configData) from its own initialize() method.
        /// </summary>
        /// <param name="configData"></param>
        void Initialize(ConfigData configData);


        /*
         *  Possible future methods:
         *  
         *  void HasBeenRegistered(Station) 
         *      called after a command has been registered at a particular station
         *      i.e. call from end of station.Register(cmd) method
         * 
         *  void StationsReady(List<Station>)
         *      called after all the stations have been initialized, 
         *      in case this command wants to register itself for some or all stations
         * 
         */

    }


    /// <summary>
    /// Composite object to return the results of the Execute call, 
    /// as well as information about the Command and the Execute parameters.
    /// Note: Execute may also throw exception, in which case this CommandResult will not be returned.
    /// </summary>
    public interface CommandResult
    {
        StationCommand Command { get; }
        
        Station Station { get; }
        
        Instance Instance { get; }

        /// <summary>
        /// Did command successfully complete?  
        /// Note: Execute may also throw exception, in which case this CommandResult will not be returned.
        /// </summary>
        bool Success { get; }
        
        /// <summary>
        /// If command was not successful, error message may be returned here.
        /// Note: Execute may also throw exception.
        /// </summary>
        string Message { get; }
    }

    /// <summary>
    /// Structure to pass command arguments to a UI dialog
    /// </summary>
    public class DialogArguments
    {
        public long InstanceRefId { get; set; }
        public string CommandName { get; set; }
    }

}
