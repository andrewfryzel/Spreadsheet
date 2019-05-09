// Andrew Fryzel 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Dependencies;
using Formulas;

namespace SS
{
    /// <summary>
    /// A Spreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// A spreadsheet contains a unique cell corresponding to each possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private DependencyGraph dg;
        private Dictionary<String, Cell> cellList;
        private Regex IsValid { get; set; }

        /// <summary>
        /// If a cell has been changed
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// Spreadsheet constructor
        /// A DependencyGraph dg keeps track of cell dependencies
        /// A cellList keep track of all the non-empty cells present in the spreadsheet.
        /// Creates an empty Spreadsheet whose IsValid regular expression accepts every string.
        /// </summary>
        public Spreadsheet()
        {
            this.cellList = new Dictionary<string, Cell>();
            this.dg = new DependencyGraph();
            this.Changed = false;
            IsValid = new Regex(@".*");
        }



        /// Creates an empty Spreadsheet whose IsValid regular expression is provided as the parameter
        public Spreadsheet(Regex isValid) : this()
        {
            this.IsValid = isValid;
        }

        /// Creates a Spreadsheet that is a duplicate of the spreadsheet saved in source.

        public Spreadsheet(TextReader source, Regex newIsValid)
        {
            this.dg = new DependencyGraph();
            this.cellList = new Dictionary<string, Cell>();

            IsValid = newIsValid;
            this.Changed = false;

            Regex r = newIsValid;
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(null, "Spreadsheet.xsd");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schema;
            settings.ValidationEventHandler += ThrowExceptionValidation;

            using (XmlReader reader = XmlReader.Create(source, settings))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "spreadsheet":
                                try
                                {
                                    r = new Regex(reader["IsValid"]);
                                }
                                catch
                                {
                                    throw new SpreadsheetReadException("Invalid Expression");
                                }
                                break;

                            case "cell":
                                if (cellList.ContainsKey(reader["name"]))
                                {
                                    throw new SpreadsheetReadException("");
                                }

                                IsValid = r;
                                try
                                {
                                    this.SetContentsOfCell(reader["name"], reader["contents"]);
                                }
                                catch (Exception e) when (e is FormulaFormatException || e is CircularException || e is InvalidNameException)
                                {
                                    if (e is FormulaFormatException)
                                    {
                                        throw new SpreadsheetVersionException("");
                                    }

                                    if (e is FormulaFormatException)
                                    {
                                        throw new SpreadsheetVersionException("");
                                    }
                                    if (e is InvalidNameException)
                                    {
                                        throw new SpreadsheetVersionException("");
                                    }

                                }

                                IsValid = newIsValid;
                                try
                                {
                                    this.SetContentsOfCell(reader["name"], reader["contents"]);

                                }
                                catch (Exception ee) when (ee is FormulaFormatException || ee is CircularException || ee is InvalidNameException)
                                {
                                    if (ee is FormulaFormatException)
                                    {
                                        throw new SpreadsheetVersionException("");
                                    }

                                    if (ee is FormulaFormatException)
                                    {
                                        throw new SpreadsheetVersionException("");
                                    }
                                    if (ee is InvalidNameException)
                                    {
                                        throw new SpreadsheetVersionException("");
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static void ThrowExceptionValidation(object obj, ValidationEventArgs e)
        {
            throw new SpreadsheetReadException("error");
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            Cell cell;
            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }

            if (cellList.TryGetValue(name.ToUpper(), out cell))
            {
                return cell.contents;
            }

            return "";
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            //done
            Cell cell;

            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }

            name = name.ToUpper();
            if (cellList.TryGetValue(name.ToUpper(), out cell))
            {
                return cell.value;
            }

            else
            {
                return "";
            }

        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cellList.Keys;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {


            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }

            Cell cell = new Cell(number);
            cell.value = number;



            if (cellList.ContainsKey(name))
            {

                cellList[name] = cell;
            }

            else
            {
                cellList.Add(name, cell);
            }


            dg.ReplaceDependees(name, new HashSet<string>());


            HashSet<String> ans = new HashSet<String>(GetCellsToRecalculate(name));

            foreach (String item in ans)
            {
                if (cellList[item].contents is Formula)
                {

                    Formula formula = (Formula)cellList[item].contents;
                    try
                    {
                        double num = formula.Evaluate((Lookup)FindVal);
                        cellList[item].value = item;

                    }
                    catch (Exception e)
                    {
                        cellList[item].value = new FormulaError();
                    }
                }
            }

            return ans;

        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }

            Cell cell = new Cell(text);
            cell.value = text;

            if (cellList.ContainsKey(name))
            {
                cellList[name] = cell;
            }


            else
            {
                cellList.Add(name, cell);
            }

            if (cellList[name].contents.Equals(""))
            {

                cellList.Remove(name);
            }

            dg.ReplaceDependees(name, new HashSet<string>());

            return new HashSet<string>(GetCellsToRecalculate(name));
        }

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
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {

            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }

            IEnumerable<String> old_dependees = dg.GetDependees(name);

            dg.ReplaceDependees(name, formula.GetVariables());

            try
            {

                HashSet<String> all_dependees = new HashSet<String>(GetCellsToRecalculate(name));

                Cell cell = new Cell(formula, FindVal);

                cell.value = formula;

                if (cellList.ContainsKey(name))
                {
                    cellList[name] = cell;
                }
                else
                {
                    cellList.Add(name, cell);
                }


                foreach (string values in all_dependees)
                {
                    if (cellList[values].contents is Formula)
                    {
                        Formula f = (Formula)(cellList[values].contents);
                        try
                        {
                            double value = f.Evaluate((Lookup)FindVal);

                            cellList[values].value = value;
                        }
                        catch (Exception e)
                        {
                            cellList[values].value = new FormulaError();
                        }
                    }
                    else
                    {
                        cell.value = formula.Evaluate((Lookup)FindVal);
                    }


                }

                return all_dependees;
            }
            catch
            {
                dg.ReplaceDependees(name, old_dependees);
                throw new CircularException();
            }

        }


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
        /// class comment.  There are then three possibilities:
        ///
        ///   (1) If the remainder of content cannot be parsed into a Formula, a
        ///       Formulas.FormulaFormatException is thrown.
        ///
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///
        ///   (3) Otherwise, the contents of the named cell becomes f.
        ///
        /// Otherwise, the contents of the named cell becomes content.
        ///
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>

        public override ISet<string> SetContentsOfCell(string name, string content)
        {


            if (content == null)
            {
                throw new ArgumentNullException();
            }
            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }

            name = name.ToUpper();

            if (Regex.IsMatch(content, @"^(?:\d+\.\d*|\d*\.\d+|\d+)(?:e[\+-]?\d+)?$"))
            {
                Changed = true;
                return SetCellContents(name, Double.Parse(content));
            }
            else if (content.Length > 0 && content[0] == '=')
            {
                Formula f = new Formula(content.Substring(1, content.Length - 1), s => s.ToUpper(), s => Regex.IsMatch(s, IsValid.ToString()));


                Changed = true;
                return SetCellContents(name, f);
            }
            else
            {
                Changed = true;
                return SetCellContents(name, content);
            }
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            // If name is null, throws an ArgumentNullException.
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            // Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
            if (!Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
            {
                throw new InvalidNameException();
            }


            return dg.GetDependents(name.ToUpper());
        }

        /// <summary>
        /// Helper Method to determin if a cell name is valid
        /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names.  On the other hand, 
        /// "Z", "X07", and "hello" are not valid cell names.
        /// </summary>

        public static bool Valid(String item)
        {

            if (item == null)
            {
                throw new InvalidNameException();
            }

            Regex r = new Regex(@" ^[a - zA - Z_](?:[a-zA-Z_]|\d)*$");

            String temp = item.ToUpper();
            char[] charArr = temp.ToCharArray();

            //Check the first and last characters for validity
            if (!(charArr[0] >= 'A' && charArr[0] <= 'Z') || !(charArr[charArr.Length - 1] >= '0' && charArr[charArr.Length - 1] <= '9'))
            {
                throw new InvalidNameException();
            }
            for (int i = 1; i < item.Length; i++)
            {
                if (char.IsLetter(item[i]) || Char.IsNumber(item[i]))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        /// </summary>
        public override void Save(TextWriter dest)
        {
            Regex r = new Regex(@"^[a-zA-Z]+[1-9][0-9]*$");
            try
            {
                XmlWriterSettings setting = new XmlWriterSettings();

                using (XmlWriter writer = XmlWriter.Create(dest))
                {
                    Cell cell = new Cell();
                    IEnumerable<String> cells = this.GetNamesOfAllNonemptyCells();

                    writer.WriteStartDocument();
                    writer.WriteStartElement("", "spreadsheet", "");
                    writer.WriteAttributeString("IsValid", this.IsValid.ToString());

                    foreach (String item in GetNamesOfAllNonemptyCells())
                    {

                        writer.WriteStartElement("cell");
                        writer.WriteAttributeString("name", item);

                        cellList.TryGetValue(item, out cell);

                        if (cell.contents is Formula)
                        {
                            writer.WriteAttributeString("contents", "=" + cellList[item].contents.ToString());
                        }

                        else if (cell.contents is Double)
                        {
                            writer.WriteAttributeString("contents", cellList[item].contents.ToString());
                        }

                        else
                        {
                            String temp = (string)cell.contents;
                            writer.WriteAttributeString("contents", temp);

                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();

                    this.Changed = false;
                }
            }
            catch (XmlException e)
            {
                throw new IOException(e.Message);

            }
            catch (IOException e)
            {
                throw new IOException(e.Message);
            }

            Changed = false;
        }

        private double FindVal(String str)
        {
            Cell cell;

            if (cellList.TryGetValue(str, out cell))
            {
                if (cell.value is Double)
                {
                    return (double)cell.value;
                }
                else
                {
                    throw new UndefinedVariableException("");
                }
            }

            else
            {
                throw new UndefinedVariableException("");
            }

        }



        /// <summary>
        /// A Cell struct with contents and values
        /// The contents of a cell can be(1) a string, (2) a double, or(3) a Formula. 
        /// </summary>
        private class Cell
        {

            public Object contents { get; private set; }
            public Object value { get; set; }


            /// <summary>
            /// String Constructor
            /// </summary>
            public Cell(string name)
            {
                contents = name;
                value = name.ToUpper();

            }

            /// <summary>
            /// Double Constructor
            /// </summary>
            /// <param name="name"></param>
            public Cell(double name)
            {
                contents = name;
                value = name;

            }

            /// <summary>
            /// Formula Constructor
            /// </summary>
            /// <param name="name"></param>
            public Cell(Formula contents, Lookup lookup)
            {
                this.contents = contents;

                try
                {
                    this.value = contents.Evaluate(lookup);
                }

                catch (Exception ex) when (ex is ArgumentException || ex is FormulaEvaluationException)
                {
                    if (ex is ArgumentException)
                    {
                        this.value = new FormulaError("Invalid");
                    }
                    else if (ex is FormulaEvaluationException)
                    {
                        if (ex.Message.Equals("Division by zero"))
                        {
                            this.value = new FormulaError("Division by zero");
                        }
                    }
                }
            }
            /// <summary>
            /// Empty Constructor
            /// </summary>
            public Cell()
            {

            }
        }
    }
}
