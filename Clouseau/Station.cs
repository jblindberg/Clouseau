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

using System.Collections.Generic;

namespace Clouseau
{

    /// <summary>
    /// Interface representing a Station on a multi-step processing Pipeline.
    /// Implementing classes will provide methods for accessing
    /// the specific document repositories, folders, log files etc.
    /// to monitor items that reside or pass through this "station".
    ///
    /// author:  Jeff Lindberg - 2003
    /// </summary>

    public interface Station
    {

        /// <summary>
        /// initialization using data in ConfigData object.
        /// This should be called immediately after the constructor.
        /// Each Station subclass should call base.initialize(configData,memory)
        /// from its own initialize() method.
        /// </summary>
        void Initialize(ConfigData configData, InstanceMemory memory, Resolver resolver);

        /// <summary>
        /// Connect to the desired system so that the Station can obtain
        /// real-time document information.
        /// For some stations, this will be a no-op since they will dynamically
        /// connect in DoSearch.
        ///
        /// TODO need reconnect()? or just use this method again?
        /// </summary>
        void Connect();

        /// <summary>
        /// Shut down this Station, and release any connections or other resources.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Retrieve any instances of objects at this Station with the specified
        /// criteria.
        /// </summary>
        /// <returns>Set of instances found at this Station</returns>
        InstanceRefList DoSearch(ICollection<Criterion> crit);

        string StationDescription { get; set; }

        /// <summary>
        /// Entities supported by this station
        /// </summary>
        List<StationEntity> Entities { get; set; }

        /// <summary>
        /// Map an entity name to the actual StationEntity object
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns>null if not found</returns>
        StationEntity GetEntity(string entityName);

        List<OperationDefinition> Operations { get; set; }

        List<string> Roles { get; set; }

        /// <summary>
        /// Does this station play the desired role?
        /// Used for specialized roles / purposes to be served by special stations.
        /// </summary>
        bool HasRole(string role);

        /// <summary>
        /// Throws StationUnsupportedCriterionException if any criteria are not supported by this station.
        /// </summary>
        /// <param name="crit"></param>
        void CheckSupportedCriteria(ICollection<Criterion> crit);


        /// <summary>
        /// Get the instance content, e.g. from a file.
        /// </summary>
        /// <param name="i">Instance of type that matches the station</param>
        /// <returns>null if no content is available</returns>
        byte[] Content(Instance i);

        /// <summary>
        /// Content format, e.g. PDF, TIFF, XML, etc.
        /// </summary>
        ContentType ContentType(Instance i);

        /// <summary>
        /// URL to the Content 
        /// </summary>
        string ContentUrl(Instance i);

        /// <summary>
        /// Can this station provide content for an instance? (e.g. file contents)
        /// </summary>
        bool HasContent { get; }

        /// <summary>
        /// Register a command for this station
        /// </summary>
        /// <param name="command"></param>
        void Register(StationCommand command);

        /// <summary>
        /// Commands which have been registered for this station
        /// </summary>
        List<StationCommand> Commands { get; }

        /// <summary>
        /// Subset of commands which operate on the overall station.
        /// </summary>
        List<StationCommand> StationScopeCommands { get; }

        /// <summary>
        /// Subset of commands which operate on specific instances.
        /// </summary>
        List<StationCommand> InstanceScopeCommands { get; }

        /// <summary>
        /// Subset of commands which operate on specific instances.
        /// </summary>
        List<StationCommand> BatchInstanceScopeCommands { get; }

        /// <summary>
        /// Public Fields defined at this station
        /// </summary>
        List<CustomFieldDefinition> PublicFields { get; }

        /// <summary>
        /// Custom Searchable Fields defined at this station
        /// </summary>
        List<CustomFieldDefinition> SearchableFields { get; }

        /// <summary>
        /// Populate the public field values (managed by this station) on all the instances in the targets list.
        /// </summary>
        /// <param name="fields">Public fields to be set (should only be ones that this station knows how to set)</param>
        /// <param name="targets">Search result instances which need the public fields set; will be updated in place</param>
        void SetPublicFieldValues(List<CustomFieldDefinition> fields, List<InstanceRefList> targets);

    }
}