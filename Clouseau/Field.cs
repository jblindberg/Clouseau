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

    public class Field
    {
        // NOTE: the actual Field object is not currently being used. 
        // It was intended for instantiating well known fields based on configuration data
        // (standard fields that exist at multiple stations).
        // See also StationField.


        // Standard generic data types of fields
        public static string TextType = "TEXT";
        public static string DateType = "DATE";
        public static string NumberType = "NUMBER";
        public static string BooleanType = "BOOLEAN";

        // special well-known field names

        // this string would be used mainly in search criteria, not necessarily in the actual Instance
        public static string EntityType = "TYPE"; // e.g. CUSTOMER or ORDER

        // unique ID of this stationEntity (not instance), e.g. customer number
        public static string EntityId = "ID";

        // unique ID of this INSTANCE of the stationEntity;
        // may be multiple instance of the same stationEntity,
        // e.g. the same order may be updated multiple times.
        public static string InstanceUniqueId = "UNIQUE_ID";

        public static string InstanceUpdateDate = "UPDATE_DATE";
        public static string InstanceArrivalDate = "ARRIVAL_DATE";

        // Status of an instance in the Station.  E.g. "waiting"
        public static string InstanceStatus = "STATUS";

        //public Field() {
        //}

        //public void setName(String name) {
        //    this.name = name;
        //}

        //public String getName() {
        //    return name;
        //}

        //public void setType(String type) {
        //    this.type = type;
        //}

        //public String getType() {
        //    return type;
        //}

    }
}
