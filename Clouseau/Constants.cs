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

    public class Constants
    {
        // TODO move these to ConfigData class?
        // Standard Config file element names and default values
        public const string StationConfig = "station";
        public const string StationClassName = "class";
        public const string StationDescription = "description";
        public const string StationMaxSearchResults = "maxSearchResults";
        public const string StationAgeLimit = "ageLimit";
        public const string StationCommand = "stationCommandName";
        public const string ActiveStation = "active";

		public const string False = "false";
		public const string True = "true";

        public const string Operation = "operation";
        public const string OperationCode = "opCode";
        public const string OperationOperandCount = "operandCount";
        public const string OperationLabel = "opLabel";

        public const string StationRole = "role";

        public const string CommandConfig = "command";
        public const string CommandClassName = "class";
        public const string CommandName = "name";
        public const string CommandLabel = "label";
		public const string CommandDescription = "description";
		public const string CommandDoConfirm = "confirm";
		public const string CommandConfirmPrompt = "confirmPrompt";

        public const string CustomFieldConfig = "field";
        public const string CustomFieldName = "name";
        public const string CustomFieldType = "type";
        public const string CustomFieldLabel = "label";
        public const string CustomFieldPublic = "public";
        public const string CustomFieldSearchable = "searchable";

        // for defining multiple result object types (entities)
        public const string StationEntity = "stationEntity";
        public const string StationEntityName = "name";
        public const string StationEntityLocation = "location"; // e.g. DB column name
        public const string StationEntitySortField = "sortField";
        public const string StationEntitySortOrder = "sortOrder";

        // for fields defined inside Entities
        public const string StationField = "stationField";
        public const string StationFieldName = "name";
        public const string StationFieldLocation = "location";
        public const string StationFieldType = "type";
        public const string StationFieldLevel = "level";

        public const string ReferredEntity = "referredEntity";
        public const string ReferredEntityField = "referredEntityField";

        public const int DefaultStationFieldLevel = 2; // TODO make it configurable in config file
        
        public const string UnknownValue = "UNKNOWN";

        // Well defined status values
        public const string StatusComplete = "COMPLETE";
        public const string StatusError = "ERROR";
        public const string StatusArchive = "ARCHIVE";

    }

    public enum ContentType
    {
        Pdf,
        Tiff,
        Html,
        Xml,
        Txt,
        None
    }

}