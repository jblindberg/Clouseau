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
using System.Linq;

namespace Clouseau
{

	public class Criterion
	{

		private string fieldName;
		private string operation;
		private string value;

		// Operation Codes 
		// NOTE: when add to this list, also update InitializeDefinitions() below. 
		// 2 operands
		public static string Equal = "EQ";
		public static string GreaterThan = "GT";
		public static string GreaterThanOrEqual = "GE";
		public static string LessThan = "LT";
		public static string LessThanOrEqual = "LE";
		public static string Like = "LIKE";
		public static string NotEqual = "NE";
		// 1 operand
		public static string Empty = "EMPTY";   // null, empty string, or all blanks
		public static string NotEmpty = "NOT_EMPTY";
		// 0 operands
		public static string Stuck = "IS_STUCK";   // Entity is stuck in its workflow, needs attention
		public static string Error = "IS_ERROR";   // Entity is in some failure state
		public static string Complete = "IS_COMPLETE";  // Entity is finished being processed, now in a final state

		// TODO perhaps add "IN" operator --
		//      field matches any one of a set of multiple values.
		//      In this case, need a way to specify multiple values --
		//      either a collection, or comma separated list in a String.
		//      In the last option (CSL), provide utility routines in this class
		//      to deal with the multiples.  Including some method which indicates
		//      whether it's a single value or multi-value.


		// NOTE: this initialization must *follow* the operation definitions above.
		private static List<OperationDefinition> _definitions = InitializeDefinitions();
		private static List<OperationDefinition> InitializeDefinitions()
		{
			List<OperationDefinition> defs = new List<OperationDefinition>();

			defs.Add(new OperationDefinition() { OpCode = Equal, OperandCount = 2, Label = "=" });
			defs.Add(new OperationDefinition() { OpCode = GreaterThan, OperandCount = 2, Label = ">" });
			defs.Add(new OperationDefinition() { OpCode = GreaterThanOrEqual, OperandCount = 2, Label = ">=" });
			defs.Add(new OperationDefinition() { OpCode = LessThan, OperandCount = 2, Label = "<" });
			defs.Add(new OperationDefinition() { OpCode = LessThanOrEqual, OperandCount = 2, Label = "<=" });
			defs.Add(new OperationDefinition() { OpCode = Like, OperandCount = 2, Label = "like" });
			defs.Add(new OperationDefinition() { OpCode = NotEqual, OperandCount = 2, Label = "not equal" });

			// TODO implement these in stations, put back in the list
			//defs.Add(new OperationDefinition() { OpCode = EMPTY, OperandCount = 1, Label = "is empty" });
			//defs.Add(new OperationDefinition() { OpCode = NOT_EMPTY, OperandCount = 1, Label = "is not empty" });
			//defs.Add(new OperationDefinition() { OpCode = ERROR, OperandCount = 0, Label = "is Error" });
			//defs.Add(new OperationDefinition() { OpCode = COMPLETE, OperandCount = 0, Label = "is Complete" });

			defs.Add(new OperationDefinition() { OpCode = Stuck, OperandCount = 0, Label = "is Stuck" });

			return defs;
		}


	    /// <summary>
	    /// In the specified Criteria (collection), find the criteria with the specified fieldName name.
	    /// </summary>
	    /// <param name="crit">Collection of Criterion</param>
        /// <param name="field">desired fieldName name</param>
	    /// <returns>List of matching Criterion</returns>
	    public static List<Criterion> GetCriteriaByField(ICollection<Criterion> crit, string field)
		{
			var query = from c in crit where c.FieldName == field select c;
			return query.ToList();
		}

		public static bool HasStuckCriterion(ICollection<Criterion> crit)
		{
			return crit.Any(c => c.Operation == Stuck);
		}

		public Criterion(string field, string operation, string value)
		{
			this.fieldName = field;
			this.operation = operation;
			this.value = value;
		}

		public bool IsUnaryOperator()
		{
			return (operation == (Empty) || operation == (NotEmpty));
		}

		public OperationDefinition OpDefinition
		{
			get
			{
				return GetOperationDefinitionByCode(this.Operation);
			}
		}

		public static OperationDefinition GetOperationDefinitionByCode(string op)
		{
			var defQuery = from d in Definitions where d.OpCode == op select d;
			return defQuery.FirstOrDefault();
		}

		public static List<OperationDefinition> Definitions
		{
			get
			{
				return _definitions;
			}
		}

		public string FieldName
		{
			get { return fieldName; }
			set { this.fieldName = value; }
		}

		public string Operation
		{
			get { return operation; }
			set { this.operation = value; }
		}

		public string Value
		{
			get { return value; }
			set { this.value = value; }
		}


		public override string ToString()
		{
			string s = fieldName + " " + operation;
			if (!IsUnaryOperator()) s += " " + value;
			return s;
		}
	}

	public class OperationDefinition
	{
		public string OpCode { get; set; }
		public int OperandCount { get; set; }
		public string Label { get; set; }

		public OperationDefinition() { }

		public OperationDefinition(string opCode, int operandCount, string opLabel)
		{
			OpCode = opCode;
			OperandCount = operandCount;
			Label = opLabel;
		}
	}

}