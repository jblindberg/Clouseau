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
    /// Abstract class representing a command which can be performed on a station 
    /// or on an Instance inside the station.  
    /// Examples:  Delete a file, Retry a processing step, etc.
    /// 
    /// This abstract class provides default implementation of methods and properties defined in the interface.
    /// </summary>
    abstract public class AbstractStationCommand : StationCommand
    {
        /// <summary>
        /// Can this command operate on an overall Station (instead of an individual Instance)
        /// </summary>
        public abstract bool IsStationScope { get; }

        /// <summary>
        /// Can this command operate on an individual Instance?
        /// </summary>
        public abstract bool IsInstanceScope { get; }

        /// <summary>
        /// Can this command operate on multiple individual Instances at once?
        /// </summary>
        public abstract bool IsBatchInstanceScope { get; }


        private string name;
        /// <summary>
        /// Identifying name (key) for this command
        /// </summary>
        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        private string label;
        /// <summary>
        /// Short user readable label for use in UI, e.g. menu item
        /// </summary>
        public virtual string Label
        {
            get
            {
                return label;
            }
        }

        private string description;
        /// <summary>
        /// Longer description of this command, e.g. in ToolTip
        /// </summary>
        public virtual string Description
        {
            get
            {
                return description;
            }
        }

		private bool doConfirm;
		/// <summary>
		/// Should the user confirm this command before it is executed?
		/// Default is false.
		/// </summary>
		public virtual bool DoConfirm
		{
			get
			{
				return doConfirm;
			}
		}

		private string confirmPrompt;
		/// <summary>
		/// prompt presented to the user when confirming the command
		/// </summary>
		public virtual string ConfirmPrompt { 
			get
			{
				return confirmPrompt;
			}
		}

        /// <summary>
        /// Is an interactive dialog required to perform this command?
        /// If so, the dialog name is returned.
        /// Otherwise this returns null (the default).
        /// </summary>
        public virtual string Dialog
        {
            get { return null; }
        }


        /// <summary>
        /// Optional message from latest Execute(), e.g. error message
        /// </summary>
        public virtual string Message { get { return ""; } }

        /// <summary>
        /// Is this command enabled for the specified Instance at the specified Station?
        /// Can be used to determine if the command should be displayed for a specific instance in the UI.
        /// </summary>
        /// <param name="station"></param>
        /// <param name="instance"></param>
        /// <returns>Default implementation returns true</returns>
        public virtual bool IsEnabled(Station station, Instance instance)
        {
            return true;
        }

        /// <summary>
        /// Execute the command on the specified Instance at the specified Station
        /// </summary>
        /// <param name="station"></param>
        /// <param name="instance"></param>
        /// <returns>CommandResult</returns>
        public virtual CommandResult Execute(Station station, Instance instance)
        {
            CommandResultWritable result = new CommandResultWritable
            {
                Command = this,
                Station = station,
                Instance = instance,
                Success = false,
                Message = "Not implemented"
            };
            return result;
        }

        /// <summary>
        /// Execute the command on the specified Station
        /// </summary>
        /// <param name="station"></param>
        /// <returns>CommandResult</returns>
        public virtual CommandResult Execute(Station station)
        {
            CommandResultWritable result = new CommandResultWritable
            {
                Command = this,
                Station = station,
                Instance = null,
                Success = false,
                Message = "Not implemented"
            };
            return result;
        }

        /// <summary>
        /// initialization using data in ConfigData object.
        /// This should be called immediately after the constructor.
        /// Each subclass should call base.initialize(configData) from its own initialize() method.
        /// </summary>
        /// <param name="configData"></param>
        public virtual void Initialize(ConfigData configData)
        {
            this.name = configData.RequiredValue(Constants.CommandName);
            this.label = configData.Value(Constants.CommandLabel);
            this.description = configData.Value(Constants.CommandDescription);

            //string confirm = configData.value(Constants.COMMAND_DO_CONFIRM);
            //this._doConfirm = (!String.IsNullOrEmpty(confirm) && confirm.Equals(Constants.TRUE, StringComparison.InvariantCultureIgnoreCase));
            this.doConfirm = configData.BoolValue(Constants.CommandDoConfirm, false);

			this.confirmPrompt = configData.Value(Constants.CommandConfirmPrompt);
        }

    }


    public class CommandResultWritable : CommandResult
    {
        public StationCommand Command { get; set; }
        public Station Station { get; set; }
        public Instance Instance { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

    }

}
