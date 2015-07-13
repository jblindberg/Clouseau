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

namespace Clouseau
{

    public static class SqlTools
    {
        /// <summary>
        /// Helper methods to build SQL queries.
        /// Not used recently since using Entity Framework instead.
        /// </summary>

        /// <summary>
        /// Build database SQL query clause for multiple criteria.
        /// TODO could add a "joiner" operator to use between multiple clauses (OR, AND)
        /// </summary>
        /// <param name="prefix">operator to precede clause, e.g. "AND"</param>
        /// <param name="crit">set of Criterion (in a Collection)</param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string BuildQueryClause(string prefix, ICollection<Criterion> crit, StationEntity entity)
        {
            string clause = "";
            foreach (Criterion c in crit)
            {
                // ignore "TYPE" criteria when building query
                if (c.FieldName == (Field.EntityType)) continue;

                // if we have already added a clause, use "AND" to join the next clause
                if (clause.Length > 0) prefix = "AND";
                // get the implementation details for this field in this station
                StationField sf = entity.GetField(c.FieldName);

                // TODO should we just return an empty string instead?
                //     risk is returning too many results
                //     this could be configurable if we make this a separate instantiable class
                if (sf == null) throw new StationUnsupportedCriterionException(
                    "Station doesn't support field " + c.FieldName);

                string type = sf.Type;
                // TODO for now, we assume types are generic types.
                //    when we want to extend with custom types, we will need another
                //    mechanism to handle building queries for custom types.
                if (type == (Field.TextType))
                    clause += AddSqlTextClause(prefix, sf, c);
                else if (type == (Field.NumberType))
                    clause += AddSqlNumberClause(prefix, sf, c);
                else if (type == (Field.DateType))
                    clause += AddSqlDateClause(prefix, sf, c);
                // TODO implement clause builders for other data types : BOOLEAN
                else throw new Exception("Field type not yet implemented: " + type);
            }
            return clause;
        }

        /// <summary>
        /// Build "order by" using the specified generic field name.
        /// </summary>
        /// <param name="sortField">official field name (not the column name)</param>
        /// <param name="entity"></param>
        /// <param name="descending">true for descending sort, false for ascending sort</param>
        /// <returns>empty string if sortField not defined for this station entity.</returns>
        public static string AddSortOrder(string sortField, StationEntity entity,
        bool descending)
        {
            string orderBy = "";
            StationField sf = entity.GetField(sortField);
            if (sf != null)
            {
                orderBy += " order by " + sf.Location + " ";
                if (descending) orderBy += "desc ";
            }
            return orderBy;
        }

        /// <summary>
        /// Build database SQL query clause for one criterion.
        /// Assumes field is string or string-compatible.
        /// </summary>
        /// <param name="prefix">operator to precede clause, e.g. "AND"</param>
        /// <param name="sf">StationField which describes the desired field</param>
        /// <param name="c"></param>
        /// <returns>empty string if c is null</returns>
        public static string AddSqlTextClause
        (string prefix, StationField sf, Criterion c)
        {
            string clause = "";
            if (c != null)
            {
                clause = " " + prefix + " ";
                if (c.IsUnaryOperator())
                {
                    clause += SqlUnaryTextOperation(c, sf);
                }
                else
                { // binary operator
                    clause += sf.Location + " ";
                    clause += SqlBinaryOperator(c.Operation) +
                               " '" + c.Value + "' ";
                }
            }
            return clause;
        }

        /// <summary>
        /// Build database SQL query clause for one criterion.
        /// Assumes field is numeric.
        /// </summary>
        /// <param name="prefix">operator to precede clause, e.g. "AND"</param>
        /// <param name="sf">StationField which describes the desired field</param>
        /// <param name="c"></param>
        /// <returns>empty string if c is null</returns>
        public static string AddSqlNumberClause
        (string prefix, StationField sf, Criterion c)
        {
            string clause = "";
            if (c != null)
            {
                clause = " " + prefix + " ";
                if (c.IsUnaryOperator())
                {
                    clause += SqlUnaryOperation(c, sf);
                }
                else
                { // binary operator
                    clause += sf.Location + " ";
                    clause += SqlBinaryOperator(c.Operation) +
                        // still use quotes in case it's not really a number field
                        // or the operator is "LIKE"
                            " '" + c.Value + "' ";
                }
            }
            return clause;
        }

        /// <summary>
        /// Build database SQL query clause for one criterion.
        /// Assumes field is date.
        /// </summary>
        /// <param name="prefix">operator to precede clause, e.g. "AND"</param>
        /// <param name="sf">StationField which describes the desired field</param>
        /// <param name="c"></param>
        /// <returns>empty string if c is null</returns>
        public static string AddSqlDateClause
        (string prefix, StationField sf, Criterion c)
        {
            string clause = "";
            if (c != null)
            {
                clause = " " + prefix + " ";
                if (c.IsUnaryOperator())
                {
                    clause += SqlUnaryOperation(c, sf);
                }
                else
                { // binary operator
                    // TODO Idea: recognize keyword like "TODAY", would require a special clause

                    // TODO "equals" for date doesn't work -- need to compare only the date,
                    //     not the time (or adjust the end time to 11:59:59)

                    clause += sf.Location + " ";
                    clause += SqlBinaryOperator(c.Operation) +
                        " TO_DATE('" + c.Value + "','" + SqlDateFormat(c.Value) + "') ";
                }
            }
            return clause;
        }

        /// <summary>
        /// Find the Criterion with the specified fieldName.
        /// </summary>
        /// <returns>null if desired fieldName not found in the Collection.</returns>
        public static Criterion GetCriterion(string field, ICollection<Criterion> crit)
        {
            foreach (Criterion c in crit)
            {
                if (c.FieldName == (field)) return c;
            }
            return null;
        }

        private static string SqlBinaryOperator(string operation)
        {
            if (operation == (Criterion.Equal)) return "=";
            if (operation == (Criterion.NotEqual)) return "<>";
            if (operation == (Criterion.GreaterThan)) return ">";
            if (operation == (Criterion.GreaterThanOrEqual)) return ">=";
            if (operation == (Criterion.LessThan)) return "<";
            if (operation == (Criterion.LessThanOrEqual)) return "<=";
            if (operation == (Criterion.Like)) return "LIKE";
            throw new Exception("unrecognized binary operation: " + operation);
        }

        /// <summary>
        /// Build SQL clause for unary operation, for text fields.
        /// </summary>
        /// <returns>clause like " (COLUMN is null OR TRIM(COLUMN) = '') "</returns>
        private static string SqlUnaryTextOperation(Criterion c, StationField sf)
        {
            if (c == null || sf == null) return "";
            string clause;
            string column = sf.Location;
            if (c.Operation == (Criterion.Empty))
            {
                clause = " (" + column + " is null OR TRIM(" + column + ") = '') ";
            }
            else if (c.Operation == (Criterion.NotEmpty))
            {
                clause = " (" + column + " is not null AND TRIM(" + column + ") <> '') ";
            }
            else
            {
                throw new Exception("unrecognized unary operation: " + c.Operation);
            }
            return clause;
        }

        /// <summary>
        /// Build SQL clause for unary operation, for non-text fields.
        /// </summary>
        /// <returns>clause like " COLUMN is null "</returns>
        private static string SqlUnaryOperation(Criterion c, StationField sf)
        {
            if (c == null || sf == null) return "";
            string clause;
            string column = sf.Location;
            if (c.Operation == (Criterion.Empty))
            {
                clause = " " + column + " is null ";
            }
            else if (c.Operation == (Criterion.NotEmpty))
            {
                clause = " " + column + " is not null ";
            }
            else
            {
                throw new Exception("unrecognized unary operation: " + c.Operation);
            }
            return clause;
        }

        /// <summary>
        /// determine the date format being used in the criterion value
        /// </summary>
        /// <returns>SQL date format, e.g. "MM/dd/yyyy"</returns>
        public static string SqlDateFormat(string dateString)
        {
            int slashIndex = dateString.IndexOf('/');
            int dashIndex = dateString.IndexOf('-');
            int length = dateString.Length;

            // TODO recognize MM/dd and MM-dd also -- use current year

            if (slashIndex == 1 || slashIndex == 2) return "MM/dd/yyyy";
            if (slashIndex == 4) return "yyyy/MM/dd";
            if (dashIndex == 1 || slashIndex == 2) return "MM-dd-yyyy";
            if (dashIndex == 4) return "yyyy-MM-dd";
            if (slashIndex < 0 && dashIndex < 0 && length == 8) return "yyyyMMdd";

            throw new Exception("unrecognized date format: '" + dateString + "'");
        }

    }
}