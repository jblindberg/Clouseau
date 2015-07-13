/*
 * Licensed according to the MIT License: 
 * 
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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Clouseau
{


    /// <summary>
    /// Pipeline class - an instance of a particular pipeline consisting of multiple stations.
    /// 
    /// These stations might represent sequential steps in the pipeline, 
    /// or they might be alternate stations representation parallel steps.
    /// An entity might visit any stations in any order; 
    /// no sequence or particular flow is enforced or assumed by the pipeline.
    /// The same entity can also reside at multiple stations concurrently.
    /// The entity doesn't need to still "reside" at a station; the station might also know 
    /// about the history of an entity as it passed through, e.g. log entries.
    /// 
    /// There might be multiple entity types (e.g. products, orders, documents) that travel
    /// between stations in a pipeline, or there might be just a single entity type that is
    /// tracked in a system.
    ///
    /// author: Jeff Lindberg
    /// </summary>

    public class Pipeline : PipelineBase, Resolver
    {
        // keep the stations in the same order as in the config file
        private List<ConfiguredStation> configuredStations = new List<ConfiguredStation>();

        // keep the stationCommands in the same order as in the config file
        private List<ConfiguredStationCommand> configuredCommands = new List<ConfiguredStationCommand>();

        private ClassFactory classFactory;

        private InstanceMemory memory;

        /// <summary>
        /// Error messages from pipeline creation (summary)
        /// </summary>
        public string Errors
        {
            get { return errors; }
        }
        private string errors;

        /// <summary>
        /// Error messages from pipeline creation (detail stack trace)
        /// </summary>
        public string ErrorsDetail
        {
            get { return errorsDetail; }
        }
        private string errorsDetail;

        /// <summary>
        /// Was there an error creating the pipeline and its stations?
        /// </summary>
        public bool HasError
        {
            get
            {
                return Errors.Length > 0;
            }
        }

        /// <summary>
        /// Aggregate set of roles supported by the stations in this pipeline.
        /// </summary>
        public List<string> Roles
        {
            get { return Stations.SelectMany(s => s.Roles).Distinct().OrderBy(s => s.ToUpper()).ToList(); }
        }


        /// <summary>
        /// constructor -- performs the initialization, including reading the config
        /// file, instantiating and initializing the configured stations.
        /// </summary>
        /// <param name="configName">name of configuration file which describes this specific pipeline</param>
        /// <param name="memory">instance memory to be associated with this pipeline</param>
        public Pipeline(string configName, InstanceMemory memory)
            : this(configName, memory, new DefaultClassFactory())
        {
        }

        /// <summary>
        /// alternate constructor -- performs the initialization, including reading the config
        /// file, instantiating and initializing the configured stations, 
        /// specifying an alternate class factory to instantiate stations.
        /// </summary>
        /// <param name="configName">name of configuration file which describes this specific pipeline</param>
        /// <param name="memory">instance memory to be associated with this pipeline</param>
        /// <param name="stationCreator">used when we had problems finding classes in a different assembly</param>
        public Pipeline(string configName, InstanceMemory memory, ClassFactory stationCreator)
        {
            this.classFactory = stationCreator;

            this.memory = memory;

            // read config file
            Stream configStream = new FileStream(configName, FileMode.Open, FileAccess.Read);
            // TODO new way to get the resource file -- actually should use IOC / Dependency Injection
            //         also accept a stream instead of a file

            ConfigData config = new ConfigData(configStream);

            errors = "";
            errorsDetail = "";

            ConfigureCommands(config);

            ConfigureStations(memory, config);

            ConfigFile = (configName);
            Description = (config.RequiredValue("description"));

            if (errors.Length > 0)
            {
                //Console.Error.WriteLine("WARNING: " + _errors); // leave it to the app to decide how to present error messages
                //throw new StationFailedException(_errors); // allow the constructor to succeed, caller can check HasError
            }

        }

        private void ConfigureStations(InstanceMemory mem, ConfigData config)
        {
            // for each station element in the configStream file,
            // create a station and add it to the stations list
            List<ConfigData> stationConfigs = config.GetConfigSections(Constants.StationConfig);
            int index = 0;
            foreach (ConfigData sConfig in stationConfigs)
            {
                ConfiguredStation cs = new ConfiguredStation(index++, sConfig);

                cs.Description = sConfig.Value(Constants.StationDescription);
                if (string.IsNullOrWhiteSpace(cs.Description))
                    cs.Description = sConfig.Value(Constants.StationClassName);

                configuredStations.Add(cs);

                // if the Station is configured to be inactive, don't initialize
                bool active = sConfig.BoolValue(Constants.ActiveStation, true);
                if (active)
                {
                    InitializeStation(mem, cs);
                }
            }
        }

        private void InitializeStation(InstanceMemory mem, ConfiguredStation cs)
        {
            Station s = null;
            try
            {
                s = NewStation(cs.ConfigData, mem);
                if (s != null)
                {
                    s.Connect();
                    cs.Station = s;
                    cs.Description = s.StationDescription; // allow station to provide an updated description
                    cs.Enabled = true;
                }
            }
            catch (Exception e)
            {
                cs.Error = e.Message;

                errors += "Station not available: ";
                errorsDetail += "Station not available: ";
                if (s != null)
                {
                    errors += s.ToString();
                    errorsDetail += s.ToString();
                }
                errors += "\n";
                errors += e.Message + "\n";

                errorsDetail += "\n";
                errorsDetail += e + "\n";
            }
        }


        /// <summary>
        /// Ensure any newly enabled stations are initialized.
        /// If any fail during initialization, the ConfiguredStation will contain the error.
        /// </summary>
        public void InitializeEnabledStations()
        {
            foreach (var cs in ConfiguredStations.Where(s => s.Enabled && s.Station == null))
            {
                InitializeStation(memory, cs);
            }
        }


        /// <summary>
        /// create a new Station, based on the station entityName specified in the portion of the config file.
        /// </summary>
        /// <param name="configData">portion of a config file describing this particular station</param>
        /// <param name="mem"></param>
        /// <returns>a new Station of object, of the desired flavor; returns null if the station is inactive</returns>
        private Station NewStation(ConfigData configData, InstanceMemory mem)
        {
            Station s;
            string stationClassName = configData.RequiredValue(Constants.StationClassName);

            //Console.Error.WriteLine("Creating new station: " + stationClassName);
            try
            {
                s = classFactory.InstantiateStationClass(stationClassName);
                if (s != null)
                {
                    s.Initialize(configData, mem, this);
                }
                else
                {
                    throw new Exception("Unable to create Station Class " + stationClassName);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create Station Class " + stationClassName, e);
            }
            //log.WriteLine("Station type "+s.getStationDescription()+" successfully initialized");
            return s;
        }


        private void ConfigureCommands(ConfigData config)
        {
            // for each command element in the configStream file,
            // create a StationCommand and add it to the Commands list
            List<ConfigData> commandConfigs = config.GetConfigSections(Constants.CommandConfig);
            int index = 0;
            foreach (ConfigData cmdConfig in commandConfigs)
            {
                ConfiguredStationCommand cc = new ConfiguredStationCommand(index++);

                cc.Description = cmdConfig.Value(Constants.CommandDescription);
                if (string.IsNullOrWhiteSpace(cc.Description))
                    cc.Description = cmdConfig.Value(Constants.CommandClassName);

                configuredCommands.Add(cc);

                InitializeCommand(cmdConfig, cc);
                //// if the Command is configured to be inactive, don't initialize
                //String active = cmdConfig.value(Constants.ACTIVE_STATION);
                //if (active == null || !active.Equals(Constants.FALSE, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    InitializeCommand(cmdConfig, cc);
                //}

            }
        }

        private void InitializeCommand(ConfigData cmdConfig, ConfiguredStationCommand cc)
        {
            StationCommand cmd = null;
            try
            {
                cmd = NewStationCommand(cmdConfig);
                if (cmd != null)
                {
                    cc.StationCommand = cmd;
                    cc.Description = cmd.Description; // allow command to provide an updated description
                    //cc.enabled = true;
                }
            }
            catch (Exception e)
            {
                cc.Error = e.Message;

                errors += "Command not available: ";
                errorsDetail += "Command not available: ";
                if (cmd != null)
                {
                    errors += cmd.ToString();
                    errorsDetail += cmd.ToString();
                }
                errors += "\n";
                errors += e.Message + "\n";

                errorsDetail += "\n";
                errorsDetail += e + "\n";
            }
        }


        /// <summary>
        /// create a new StationCommand, based on the StationCommand class name specified in the
        /// portion of the config file.
        /// </summary>
        /// <param name="configData">portion of a config file describing this particular stationCommand</param>
        /// <returns> a new StationCommand of object, of the desired flavor;
        /// returns null if the stationCommand is inactive</returns>
        private StationCommand NewStationCommand(ConfigData configData)
        {
            StationCommand cmd;
            string stationCommandClassName = configData.RequiredValue(Constants.CommandClassName);

            //// if the command is configured to be inactive, just return null
            //String active = configData.value(Constants.ACTIVE_COMMAND);
            //if (!String.IsNullOrEmpty(active) && active.Equals(Constants.FALSE, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    //Console.Error.WriteLine("ignoring inactive command: " + stationCommandClassName);
            //    return null;
            //}

            //Console.Error.WriteLine("Creating new stationCommand: " + stationCommandClassName);
            try
            {
                cmd = classFactory.InstantiateStationCommandClass(stationCommandClassName);
                if (cmd != null)
                {
                    cmd.Initialize(configData);
                }
                else
                {
                    throw new Exception("Unable to create StationCommand Class " + stationCommandClassName);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create StationCommand Class " + stationCommandClassName, e);
            }
            //log.WriteLine("Command type "+s.getStationCommandDescription()+" successfully initialized");
            return cmd;
        }


        /// <summary>
        /// Shut down a pipeline and disconnect all Stations.
        /// </summary>
        public void Shutdown()
        {
            foreach (Station s in Stations)
            {
                try
                {
                    s.Disconnect();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Error in disconnecting station: " + s + " - " + e);
                }
            }
        }


        /// <summary>
        /// Full description of the pipeline, including a description of all active stations
        /// </summary>
        public string GetFullString()
        {
            string s = ToString() + "\n";
            s += "\nActive Stations: \n";
            foreach (Station station in Stations)
            {
                s += station + "\n";
            }
            return s;
        }

        /// <summary>
        /// Full description of the pipeline, including a description of all active stations
        /// </summary>
        public string GetFullStringHtml()
        {
            return Util.ConvertTextToHtml(GetFullString());
        }

        /// <summary>
        /// A readable description of this Pipeline.
        /// </summary>
        public override string ToString()
        {
            string s = "";
            if (Description != null) s += Description + ": ";
            s += ConfigFile;
            return s;
        }

        public List<Station> Stations
        {
            get
            {
                var query = from cs in ConfiguredStations where cs.Enabled && cs.Station != null select cs.Station;
                return query.ToList();
            }
        }

        public List<ConfiguredStation> ConfiguredStations
        {
            get
            {
                return configuredStations;
            }
        }

        public List<StationCommand> Commands
        {
            get
            {
                var query = from cs in ConfiguredStationCommands where 
                                //cs.enabled &&
                                cs.StationCommand != null 
                            select cs.StationCommand;
                return query.ToList();
            }
        }

        /// <summary>
        /// Get the command object with the specified official name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>null if not found</returns>
        public StationCommand GetCommandByName(string name)
        {
            var query = from c in Commands where c.Name == name select c;
            return query.FirstOrDefault();
        }

        public List<ConfiguredStationCommand> ConfiguredStationCommands
        {
            get
            {
                return configuredCommands;
            }
        }



        /// <summary>
        /// Union of all Supported Search Operations for all Stations - distinct set
        /// </summary>
        public List<OperationDefinition> ConfiguredOperations
        {
            get
            {
                var query = (from s in Stations select s).SelectMany(s => s.Operations);
                return query.Distinct().ToList();
            }
        }


        /// <summary>
        /// Union of all Searchable Custom Fields for all Stations - distinct set
        /// </summary>
        public List<CustomFieldDefinition> ConfiguredSearchableCustomFields
        {
            get
            {
                var query = (from s in Stations select s).SelectMany(s => s.SearchableFields);
                return query.Distinct().ToList();
            }
        }


    }


    /// <summary>
    /// The same logical station might exist in multiple configurations at the same time.
    /// (e.g. located on multiple servers)
    /// This combines the station class with the particular configuration.
    /// </summary>
    public class ConfiguredStation
    {
        public Station Station;
        public string Description;
        public bool Enabled;
        public string Error;
        public ConfigData ConfigData;

        private int id;
        public int Id
        {
            get { return id; }
        }

        public ConfiguredStation(int id, ConfigData configData)
        {
            this.id = id;
            this.ConfigData = configData;
        }

    }

    public class ConfiguredStationCommand
    {
        public StationCommand StationCommand;
        public string Description;
        //public bool enabled; // not currently used
        public string Error;

        private int id;
        public int Id
        {
            get { return id; }
        }

        public ConfiguredStationCommand(int id)
        {
            this.id = id;
        }

    }



    public class DefaultClassFactory : ClassFactory
    {
        public Station InstantiateStationClass(string stationClassName)
        {
            Station s = null;
            Type stationType = Type.GetType(stationClassName);
            if (stationType != null)
            {
                s = (Station)Activator.CreateInstance(stationType);
            }
            else
            {
                // try going through all assemblies
                Assembly[] assems = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly a in assems)
                {
                    try
                    {
                        s = (Station)a.CreateInstance(stationClassName);
                        if (s != null)
                            break;
                    }
                    catch
                    {
                        // continue and try the next Assembly
                    }
                }

            }
            return s;
        }

        public StationCommand InstantiateStationCommandClass(string stationCommandClassName)
        {
            StationCommand s = null;
            Type stationCommandType = Type.GetType(stationCommandClassName);
            if (stationCommandType != null)
            {
                s = (StationCommand)Activator.CreateInstance(stationCommandType);
            }
            else
            {
                // try going through all assemblies
                Assembly[] assems = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly a in assems)
                {
                    try
                    {
                        s = (StationCommand)a.CreateInstance(stationCommandClassName);
                        if (s != null)
                            break;
                    }
                    catch
                    {
                        // continue and try the next Assembly
                    }
                }

            }
            return s;
        }

    }

    public interface ClassFactory
    {
        Station InstantiateStationClass(string stationClassName);
        StationCommand InstantiateStationCommandClass(string stationCommandClassName);
    }


}