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

using System.Collections.Generic;

namespace Clouseau
{
    /// <summary>
    /// Holds values for Public Fields, with field name as key.
    /// Note: can have multiple values for the same field name.
    /// Values can be string, number, boolean, or DateTime (all values are the same type for one field name).
    /// All values are Nullable.
    /// 
    /// NOTE:  Design Issue: for multiple values, should probably have an empty list if there are none.
    /// But for single values, if the value is null, should store a null in the first list element.
    /// 
    /// </summary>
    public class PublicFieldCollection : Dictionary<string, List<object>>
    {

        /// <summary>
        /// Retrieve a scalar value stored in a Public Field.
        /// For convenience since all public fields have a List of values.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="instance"></param>
        /// <returns>null if the field is not defined in this instance, or if no values exist, or if the first value is null</returns>
        public static string GetPublicFieldSingleStringValue(string fieldName, Instance instance)
        {
            if (instance.PublicFields.ContainsKey(fieldName))
            {
                var field = instance.PublicFields[fieldName];
                if (field.Count > 0 && field[0] != null)
                    return field[0].ToString();
                else
                    return null;
            }
            else
            {
                return null;
            }
        }



    }
}
