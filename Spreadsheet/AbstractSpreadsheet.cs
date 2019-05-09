
using System;
using System.Collections.Generic;
using Formulas;
using System.IO;

namespace SS
{
    /// <summary>
    /// Thrown to indicate that a change to a cell will cause a circular dependency.
    /// </summary>
    public class CircularException : Exception
    {
    }

    /// <summary>
    /// Thrown to indicate that a name parameter was either null or invalid.
    /// </summary>
    public class InvalidNameException : Exception
    {
    }

    /// <summary>
    /// Thrown to indicate that a saved spreadsheet could not be read
    /// because of a formatting problem.
    /// </summary>
    public class SpreadsheetReadException : Exception
    {
        /// <summary>
        /// Creates the exception with a message
        /// </summary>
        public SpreadsheetReadException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// Thrown to indicate that a saved spreadsheet could not be read
    /// because of a versioning problem.
    /// </summary>
    public class SpreadsheetVersionException : Exception
    {
        /// <summary>
        /// Creates the exception with a message
        /// </summary>
        public SpreadsheetVersionException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// A possible value of a cell.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }


    public abstract class AbstractSpreadsheet
    {
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public abstract bool Changed { get; protected set; }


        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        /// </summary>
        public abstract void Save(TextWriter dest);

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public abstract object GetCellValue(String name);

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public abstract IEnumerable<String> GetNamesOfAllNonemptyCells();

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public abstract object GetCellContents(String name);


        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        ///
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor with s => s.ToUpper() as the normalizer and a validator that
        /// checks that s is a valid cell name as defined in the AbstractSpreadsheet
        /// class comment.  
        /// </summary>
        public abstract ISet<String> SetContentsOfCell(String name, String content);

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// </summary>
        protected abstract ISet<String> SetCellContents(String name, double number);

   
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// </summary>
        protected abstract ISet<String> SetCellContents(String name, String text);

    
        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        /// 
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// </summary>
        protected abstract ISet<String> SetCellContents(String name, Formula formula);

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.

        /// </summary>
        protected abstract IEnumerable<String> GetDirectDependents(String name);

        /// <summary>
        /// Requires that names be non-null.  Also requires that if names contains s,
        /// then s must be a valid non-null cell name.
        /// 
        /// If any of the named cells are involved in a circular dependency,
        /// throws a CircularException.
        /// 
        /// Otherwise, returns an enumeration of the names of all cells whose values must
        /// be recalculated, assuming that the contents of each cell named in names has changed.
        /// The names are enumerated in the order in which the calculations should be done.  
        /// </summary>
        protected IEnumerable<String> GetCellsToRecalculate(ISet<String> names)
        {
            LinkedList<String> changed = new LinkedList<String>();
            HashSet<String> visited = new HashSet<String>();
            foreach (String name in names)
            {
                if (!visited.Contains(name))
                {
                    Visit(name, name, visited, changed);
                }
            }
            return changed;
        }

        /// <summary>
        /// A convenience method for invoking the other version of GetCellsToRecalculate
        /// with a singleton set of names.  See the other version for details.
        /// </summary>
        protected IEnumerable<String> GetCellsToRecalculate(String name)
        {
            return GetCellsToRecalculate(new HashSet<String>() { name });
        }

        /// <summary>
        /// A helper for the GetCellsToRecalculate method.
        /// </summary>
        private void Visit(String start, String name, ISet<String> visited, LinkedList<String> changed)
        {
            visited.Add(name);
            foreach (String n in GetDirectDependents(name))
            {
                if (n.Equals(start))
                {
                    throw new CircularException();
                }
                else if (!visited.Contains(n))
                {
                    Visit(start, n, visited, changed);
                }
            }
            changed.AddFirst(name);
        }

    }
}

