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

namespace Clouseau
{
    /// <summary>
    ///     Abstract class representing a Station on the Pipeline.
    ///     Subclasses will implement methods for accessing
    ///     the specific document repositories, folders, log files etc.
    ///     to monitor documents that reside or pass through this "station".
    ///     Jeff Lindberg - 2003
    /// </summary>
    public abstract class AbstractStation : Station
    {
        private readonly List<StationCommand> commands = new List<StationCommand>();
        private readonly List<CustomFieldDefinition> customFields = new List<CustomFieldDefinition>();
        private List<StationEntity> entities;
        private List<OperationDefinition> operations = new List<OperationDefinition>();
        private List<string> roles = new List<string>();
        private string stationDescription;
        public InstanceMemory Memory; // public for classes in other packages

        public virtual int MaxSearchResults { get; set; }

        /// <summary>
        ///     "IS_STUCK" threshold for the station, in seconds
        /// </summary>
        public virtual int AgeLimit { get; set; }

        public List<OperationDefinition> Operations
        {
            get { return operations; }
            set { operations = value; }
        }

        public List<string> Roles
        {
            get { return roles; }
            set { roles = value; }
        }

        public List<CustomFieldDefinition> PublicFields
        {
            get { return customFields.Where(f => f.Public).ToList(); }
        }

        public List<CustomFieldDefinition> SearchableFields
        {
            get { return customFields.Where(f => f.Searchable).ToList(); }
        }

        /// <summary>
        /// initialization using data in ConfigData object.
        /// This should be called immediately after the constructor.
        /// Each Station subclass should call base.initialize(configData, memory) from its own initialize() method.
        /// </summary>
        public virtual void Initialize(ConfigData configData, InstanceMemory memory, Resolver resolver)
        {
            // do any other global Station initialization here
            this.Memory = memory;

            stationDescription = configData.Value(Constants.StationDescription);
            entities = LoadStationEntities(configData);

            var max = configData.IntValue(Constants.StationMaxSearchResults);
            MaxSearchResults = max ?? int.MaxValue;

            var ageLimit = configData.IntValue(Constants.StationAgeLimit);
            AgeLimit = ageLimit ?? int.MaxValue;

            PopulateRoles(configData);

            PopulateOperations(configData);

            PopulateCommands(configData, resolver);

            PopulatePublicFields(configData);
        }

        /// <summary>
        /// Throws StationUnsupportedCriterionException if any criterion is not supported by this station
        /// </summary>
        /// <param name="crit"></param>
        public virtual void CheckSupportedCriteria(ICollection<Criterion> crit)
        {
            foreach (var c in crit)
            {
                var query = from op in Operations where c.Operation == op.OpCode select op;
                if (!query.Any())
                    throw new StationUnsupportedCriterionException(string.Format("Unsupported operation: {0}",
                        c.Operation));

                // TODO  check supported fields here too?
                //      probably just check the custom fields, not the standard fields
            }
        }

        /// <summary>
        /// Returns true if specified role is supported by this station
        /// </summary>
        /// <param name="role">Desired role to be played by a station</param>
        public virtual bool HasRole(string role)
        {
            var query = from r in Roles where r == role select r;
            return query.Any();
        }

        /// <summary>
        /// Connect to the desired system so that the Station can obtain real-time document information.
        /// </summary>
        public virtual void Connect()
        {
        }

        /// <summary>
        /// Shut down this Station, and release any connections or other resources.
        /// </summary>
        public virtual void Disconnect()
        {
        }

        /// <summary>
        /// Retrieve any instances of objects at this Station with the specified criteria.
        /// </summary>
        /// <returns>Set of instances found at this Station</returns>
        public abstract InstanceRefList DoSearch(ICollection<Criterion> crit);

        public virtual List<StationEntity> Entities
        {
            get { return entities; }
            set { entities = value; }
        }

        public virtual StationEntity GetEntity(string entityName)
        {
            foreach (var e in entities)
            {
                if (e.Name == (entityName)) return e;
            }
            return null;
        }

        public virtual string StationDescription
        {
            get { return stationDescription; }
            set { stationDescription = value; }
        }

        /// <summary>
        ///     Can this station provide content for an instance? (e.g. file contents)
        ///     Default is false.
        ///     This will be overridden by specific stations that have actual content.
        /// </summary>
        public virtual bool HasContent
        {
            get { return false; }
        }

        /// <summary>
        ///     Default behavior is to return null.
        ///     This will be overridden by specific stations that have actual content.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual byte[] Content(Instance i)
        {
            return null;
        }

        /// <summary>
        ///     Default is to return NONE.
        ///     This will be overridden by specific stations that have actual content.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual ContentType ContentType(Instance i)
        {
            return Clouseau.ContentType.None;
        }

        /// <summary>
        ///     Default behavior is to return null.
        ///     This will be overridden by specific stations that have actual content.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual string ContentUrl(Instance i)
        {
            return null;
        }

        /// <summary>
        ///     Commands which have been registered for this station
        /// </summary>
        public virtual List<StationCommand> Commands
        {
            get { return commands; }
        }

        /// <summary>
        ///     Does this station have Station-scope commands?
        ///     Those that operate on the overall station.
        /// </summary>
        public virtual List<StationCommand> StationScopeCommands
        {
            get
            {
                var query = from c in commands where c.IsStationScope select c;
                return query.ToList();
            }
        }

        /// <summary>
        ///     Does this station have Instance-scope commands?
        ///     Those that operate on specific instances.
        /// </summary>
        public virtual List<StationCommand> InstanceScopeCommands
        {
            get
            {
                var query = from c in commands where c.IsInstanceScope select c;
                return query.ToList();
            }
        }

        /// <summary>
        ///     Does this station have Instance-scope commands?
        ///     Those that operate on specific instances.
        /// </summary>
        public virtual List<StationCommand> BatchInstanceScopeCommands
        {
            get
            {
                var query = from c in commands where c.IsBatchInstanceScope select c;
                return query.ToList();
            }
        }

        /// <summary>
        /// Register a command for this station
        /// </summary>
        /// <param name="command">Unique name of the command</param>
        public virtual void Register(StationCommand command)
        {
            // TODO throw exception if a command with the same name is already registered at this station
            //         or ignore, avoiding duplicates?

            commands.Add(command);
        }

        /// <summary>
        ///     Populate the public field values (managed by this station) on all the instances in the targets list.
        /// </summary>
        /// <param name="fields">Public fields to be set (should only be ones that this station knows how to set)</param>
        /// <param name="targets">Search result instances which need the public fields set; will be updated in place</param>
        public virtual void SetPublicFieldValues(List<CustomFieldDefinition> fields, List<InstanceRefList> targets)
        {
            // Default implementation - do nothing
        }

        private void PopulatePublicFields(ConfigData configData)
        {
            var publicFieldConfigs = configData.GetConfigSections(Constants.CustomFieldConfig);
            foreach (var pubConfig in publicFieldConfigs)
            {
                var name = pubConfig.RequiredValue(Constants.CustomFieldName);
                var label = pubConfig.Value(Constants.CustomFieldLabel);
                var type = pubConfig.Value(Constants.CustomFieldType);
                var searchable = pubConfig.BoolValue(Constants.CustomFieldSearchable, false);
                var pub = pubConfig.BoolValue(Constants.CustomFieldPublic, false);

                var field = new CustomFieldDefinition(name, type)
                {
                    Label = label,
                    Searchable = searchable,
                    Public = pub
                };
                customFields.Add(field);
            }
        }

        /// <summary>
        ///     Populate the set of roles which this station supports.  May be a zero-length list.
        /// </summary>
        /// <param name="configData">ConfigData for this station</param>
        protected virtual void PopulateRoles(ConfigData configData)
        {
            roles = configData.Values(Constants.StationRole).ToList();
        }

        /// <summary>
        ///     Populate the set of operations which this station supports.
        ///     Required: opCode
        ///     Optional: opLabel (defaults to opCode)
        ///     Optional: operandCount (defaults to zero)
        /// </summary>
        /// <param name="configData">ConfigData for this station</param>
        protected virtual void PopulateOperations(ConfigData configData)
        {
            operations = new List<OperationDefinition>(Criterion.Definitions);
                // start with copy of the standard operations

            // add the custom operations 
            var opConfig = configData.GetConfigSections(Constants.Operation);
            foreach (var cd in opConfig)
            {
                var code = cd.RequiredValue(Constants.OperationCode);

                var operandCount = cd.IntValue(Constants.OperationOperandCount);
                if (!operandCount.HasValue) operandCount = 0;

                var label = cd.Value(Constants.OperationLabel);
                if (label == null) label = code;

                operations.Add(new OperationDefinition(code, operandCount.Value, label));
            }
        }

        public override string ToString()
        {
            var s = "";
            if (!string.IsNullOrEmpty(stationDescription)) s += stationDescription + ": ";
            s += "unknown station configuration";
            return s;
        }

        protected virtual List<StationEntity> LoadStationEntities(ConfigData configData)
        {
            var entitiesList = new List<StationEntity>();
            var entityConfigs = configData.GetConfigSections(Constants.StationEntity);
            foreach (var entityConfig in entityConfigs)
            {
                var entityName = entityConfig.RequiredValue(Constants.StationEntityName);
                var entity = new StationEntity(entityName);
                var entityLocation = entityConfig.Value(Constants.StationEntityLocation);
                entity.Location = (entityLocation);
                var fieldConfigs = entityConfig.GetConfigSections(Constants.StationField);
                foreach (var fieldConfig in fieldConfigs)
                {
                    var field = new StationField(fieldConfig.RequiredValue(Constants.StationFieldName));
                    field.Type = (fieldConfig.RequiredValue(Constants.StationFieldType));

                    // if location is empty, just use field name as default location
                    var location = fieldConfig.Value(Constants.StationFieldLocation);
                    if (location == null) location = field.Name;
                    field.Location = (location);

                    field.Level = (fieldConfig.IntValue(Constants.StationFieldLevel));
                    // TODO set other field properties -  referredEntity, referredEntityField
                    entity.AddField(field);
                }
                var sortField = entityConfig.Value(Constants.StationEntitySortField);
                if (sortField != null)
                {
                    entity.SortField = (sortField);
                    var sortOrder = entityConfig.Value(Constants.StationEntitySortOrder);
                    if (sortOrder != null && sortOrder.ToUpper().StartsWith("DESC"))
                    {
                        entity.DescendingSort = (true);
                    }
                }
                entitiesList.Add(entity);
            }
            return entitiesList;
        }

        /// <summary>
        /// Populate the set of commands which this station supports.  May be a zero-length list.
        /// </summary>
        /// <param name="configData">ConfigData for this station</param>
        /// <param name="commandResolver"></param>
        protected virtual void PopulateCommands(ConfigData configData, Resolver commandResolver)
        {
            var commandNames = configData.Values(Constants.StationCommand).ToList();

            foreach (var name in commandNames)
            {
                if (commandResolver == null)
                {
                    throw new Exception(string.Format("No CommandResolver available for command {0}, station {1}",
                        name, StationDescription));
                }
                var cmd = commandResolver.GetCommandByName(name);
                if (cmd == null)
                {
                    throw new Exception(string.Format("No Command found for commandName {0}, station {1}",
                        name, StationDescription));
                }
                Register(cmd);
            }
        }
    } // AbstractStation
}