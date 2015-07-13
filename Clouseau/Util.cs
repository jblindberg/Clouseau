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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Clouseau
{
    public class Util
    {

        public static string ExceptionMessageIncludingInners(Exception ex)
        {
            string msg = "";

            while (ex != null)
            {
                msg += ex.Message + "; ";
                ex = ex.InnerException;
            }

            return msg;
        }

        public static string ConvertTextToHtml(string s)
        {
            if (s==null) return "";

            s = HttpUtility.HtmlEncode(s);
            s = s.Replace("\n", "<br />");
            return s;
        }

        /// <summary>
        /// Compares two strings.
        /// If both strings are integers, compare as integers, otherwise compare as strings.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>Same return codes as String.Compare method</returns>
        public static int CompareIntegerOrString(string s1, string s2)
        {
            long i1;
            long i2;
            if (long.TryParse(s1, out i1) && long.TryParse(s2, out i2))
                return i1.CompareTo(i2);
            else
                return string.Compare(s1, s2, StringComparison.InvariantCulture);
        }


        /// <summary>
        /// Is this a string representation of a date, with no time?
        /// e.g. "12/5/2010" or any other recognized format.
        /// </summary>
        public static bool DateOnly(string value)
        {
            bool dateOnly = false;

            DateTime d;
            if (DateTime.TryParse(value, out d))
            {
                if (d.TimeOfDay == new TimeSpan(0)) // evaluates to start of the day
                {
                    if (!value.Contains(':')) // and no hour:minute was in the string
                    {
                        dateOnly = true;
                    }
                }
            }

            return dateOnly;
        }


        /// <summary>
        /// Return basename of specified file
        /// Strips directory and file extension (like .xml)
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string Basename(FileInfo f)
        {
            string name = f.Name;
            string ext = f.Extension;
            if (!string.IsNullOrEmpty(ext))
            {
                name = name.Substring(0, name.Length - ext.Length);
            }
            return name;
        }


        /// <summary>
        /// Convert bytes to MB
        /// </summary>
        /// <returns>floating point</returns>
        public static double SizeInMb(long size)
        {
            return size / (double)(1024 * 1024);
        }


        /// <summary>
        /// Map a file extension (e.g. "pdf" or ".pdf") to a recognized ContentType
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static ContentType ExtensionType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return ContentType.None;

            if (extension.StartsWith("."))
                extension = extension.Substring(1);

            extension = extension.ToUpper();

            if (extension == "PDF")
                return ContentType.Pdf;
            if (extension == "TIFF" || extension == "TIF")
                return ContentType.Tiff;
            if (extension == "XML")
                return ContentType.Xml;
            if (extension == "HTML" || extension == "HTM" || extension == "ASPX")
                return ContentType.Html;
            else
                return ContentType.Txt; // default type if not recognized as something else
        }

        /// <summary>
        /// Get recognized ContentType for a file
        /// </summary>
        /// <param name="path">Could be either full path or partial path (I think)</param>
        /// <returns></returns>
        public static ContentType FileContentType(string path)
        {
            FileInfo f = new FileInfo(path);
            return ExtensionType(f.Extension);
        }


        public static string MimeType(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Pdf:
                    return "application/pdf";
                case ContentType.Tiff:
                    return "image/x-tiff"; // if problems, other options would be image/tiff or application/x-tiff 
                case ContentType.Html:
                    return "text/html";
                case ContentType.Xml:
                    return "text/xml";
                case ContentType.Txt:
                    return "text/plain";
                case ContentType.None:
                    return null;  // alternative is to return text/plain 
                default:
                    return "text/plain";
            }
        }


        /// <summary>
        /// If path is a Windows UNC path (starts with \\), then try to connect using the
        /// supplied credentials.  If it is not a UNC path, does nothing.
        /// Note that the path might still be accessible even if the connect is attempted and fails.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="path"></param>
        /// <param name="possibleError">Message if connect fails</param>
        /// <returns>Object that should be kept in scope during file access, and disposed of when finished</returns>
        public static UncAccessWithCredentials ConnectUnc(string user, string password, string path,
            out string possibleError)
        {
            UncAccessWithCredentials unc = null;
            possibleError = null;

            if (user != null && password != null && path.StartsWith(@"\\"))
            {
                // try connecting via UNC
                unc = new UncAccessWithCredentials();
                {
                    string uncPath = path;
                    if (uncPath.EndsWith("/") || uncPath.EndsWith(@"\"))
                    {
                        uncPath = uncPath.Substring(0, uncPath.Length - 1);
                    }
                    // TODO will we need domain sometimes? should put it in the config file
                    if (unc.NetUseWithCredentials(uncPath, user, "", password))
                    {
                        // connected successfully
                    }
                    else
                    {
                        possibleError = "Failed to connect to '" + uncPath + "' : Error = " + unc.LastError;
                        // if error, try proceeding without UNC
                    }
                }
            }
            return unc;
        }


        public static bool IsValidInt(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        public static bool IsValidLong(string s)
        {
            long i;
            return long.TryParse(s, out i);
        }

        /// <summary>
        /// Convert list of strings to corresponding integers.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns>List of integers in same order; except that invalid integers are omitted in results</returns>
        public static List<int> ConvertStringsToInts(List<string> strings)
        {
            var query = from s in strings where IsValidInt(s) select int.Parse(s);
            return query.ToList();
        }

        /// <summary>
        /// populate an instance's public field with the field name,
        /// and an object list containing the single value  
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldName"></param>
        /// <param name="value">might be null; will still be added to the public field values list</param>
        public static void LoadPublicFieldSingle(Instance i, string fieldName, object value)
        {
            List<object> values = new List<object>();
            values.Add(value);
            i.PublicFields.Add(fieldName, values);
        }


    }



    /* *************** PredicateBuilder Class ****************  */
    /// <summary>
    /// Used to dynamically construct LINQ queries.
    /// See http://www.albahari.com/nutshell/predicatebuilder.aspx
    /// </summary>
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                                                                                Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                        (Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                                                                                 Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                        (Expression.And(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }


    public static class SelectByParameterListLinqKit
    {
        // TODO figure out how to get this hooked in -- to use in solving the "more than 2100 parameters" error
        /*
        public static IEnumerable<T> SelectByParameterList<T, PropertyType>(this Table<T> items, IEnumerable<PropertyType> parameterList, Expression<Func<T, PropertyType>> propertySelector, int blockSize) where T : class
        {
            var groups = parameterList
                .Select((Parameter, index) =>
                        new
                        {
                            GroupID = index / blockSize, //# of parameters per request
                            Parameter
                        }
                )
                .GroupBy(x => x.GroupID)
                .AsEnumerable();

            var selector = LinqKit.Linq.Expr(propertySelector);

            var results = groups
            .Select(g => new { Group = g, Parameters = g.Select(x => x.Parameter) })
            .SelectMany(g =>
                // AsExpandable() extension method requires LinqKit DLL 
                items.AsExpandable().Where(item => g.Parameters.Contains(selector.Invoke(item)))
            );

            return results;
        }

    */


    }


}
