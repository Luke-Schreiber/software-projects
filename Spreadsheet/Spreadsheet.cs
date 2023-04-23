using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SS
{
    [JsonObject(MemberSerialization.OptIn)]
    /// <summary>
    /// An Spreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected).
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// We are not concerned with values in PS4, but to give context for the future of the project,
    /// the value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid). 
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        // Holds private values of cells and their names, a dependency graph, and a changed bool.
        [JsonProperty]
        private Dictionary<string, Cell> cells;
        private DependencyGraph graph;
        private bool changed;

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed {
            get { return changed; }
            protected set { changed = value; }
        }

        // Instantiates and creates an empty spreadsheet object with this constructor.
        public Spreadsheet() : base(s => true, s => s, "default")
        {
            cells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            changed = false;
        }
        // Instantiates and creates an empty spreadsheet object with this constructor and 3 params.
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            IsValid = isValid;
            Normalize = normalize;
            changed = false;
        }
        // Instantiates and creates a spreadsheet object from file with this constructor and 4 params.
        public Spreadsheet(string userFilename, Func<string, bool> isValid,
            Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();

            // Converts file to spreadsheet using user given filename and version.
            fileToSpreadsheet(userFilename, version);

            // Uses this constructors isValid, Normalize, and Version for this spreadsheet.
            IsValid = isValid;
            Normalize = normalize;
            Version = version;
            changed = false;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            // For every cell name, if the cell is empty the cell name is added to a hashset.
            HashSet<string> names = new();
            foreach (string name in cells.Keys)
            {
                if (!GetCellContents(name).Equals(""))
                    names.Add(name);
            }

            // Turns the hashset to a type Ienumerable and returns it.
            IEnumerable<string> nonEmptyCellNames = names;
            return nonEmptyCellNames;
        }


        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            string Name = Normalize(name);
            // If name is invalid, throws an InvalidNameException.
            if (!Regex.IsMatch(Name, "^[a-zA-Z]+[0-9]+$") || !IsValid(Name))
                throw new InvalidNameException();

            if (!cells.ContainsKey(Name))
                return "";
            // Returns the contents of that cell name.
            return cells[Name].Contents;
        }


        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {

            // If cell does not exist, set contents.
            if (!cells.ContainsKey(name))
                cells.Add(name, new Cell(number));

            else {
                cells[name].Contents = number;
                IEnumerable<string> noDependees = new HashSet<string>();
                graph.ReplaceDependees(name, noDependees);
            }

            // Gets cells to recalculate and returns to caller.
            IList<string> list = GetCellsToRecalculate(name).ToList();
            IEnumerator<string> listEnum = list.GetEnumerator();

            ChangeValues(listEnum);
            return list;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, string text)
        {

            // If cell does exist, set contents.
            if (cells.ContainsKey(name))
            {
                cells[name].Contents = text;
                IEnumerable<string> noDependees = new HashSet<string>();
                graph.ReplaceDependees(name, noDependees);
            }
            // If cell does not exist create cell and add contents.
            else cells.Add(name, new Cell(text));


            // Gets cells to recalculate and returns to caller.
            IList<string> list = GetCellsToRecalculate(name).ToList();
            IEnumerator<string> listEnum = list.GetEnumerator();

            ChangeValues(listEnum);

            return list;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            // Replace the dependees of this cell.
            graph.ReplaceDependees(name, formula.GetVariables());


            // If cell does exist, set contents.
            if (cells.ContainsKey(name))
                cells[name].Contents = formula;

            // If cell does not exist, add cell.
            else cells.Add(name, new Cell(formula));

            // Gets cells to recalculate and returns to caller.
            IList<string> list = GetCellsToRecalculate(name).ToList();
            IEnumerator<string> listEnum = list.GetEnumerator();

            // Goes and changes the values for the cells.
            ChangeValues(listEnum);

            return list;
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
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
            // Returns the dependents of this cell name.
            return graph.GetDependents(name);
        }
        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "cells" - an object containing 0 or more cell objects
        ///           Each cell object has a field named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "stringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of stringForm is that string
        ///               - If the contents is a double d, the value of stringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "cells": {
        ///     "A1": {
        ///       "stringForm": "5"
        ///     },
        ///     "B3": {
        ///       "stringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            // Serializes this current object.
            string json = JsonConvert.SerializeObject(this);

            try { File.WriteAllText(filename, json); }
            catch(Exception)
            {
                throw new SpreadsheetReadWriteException("File not found");
            }
            // Writes to a file using user given filename.
            

            // Marks changed to false.
            changed = false;

        }
        // ADDED FOR PS5
        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            // Normalizes given name.
            string Name = Normalize(name);

            // Throws exception if name is invalid.
            if (!Regex.IsMatch(Name, "^[a-zA-Z]+[0-9]+$") || !IsValid(Name))
                throw new InvalidNameException();

            // If there is no cell, return empty string, otherwise return its value.
            if (!cells.ContainsKey(Name))
                return "";
            else
                return cells[Name].Value;
        }
        // ADDED FOR PS5
        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            // Normalize string and marked changed to true.
            Changed = true;
            string Name = Normalize(name);

            // If name is invalid, throws an InvalidNameException.
            if (!Regex.IsMatch(Name, "^[a-zA-Z]+[0-9]+$") || !IsValid(Name))
                throw new InvalidNameException();

            // If string is a formula, call set cell contents with that formula.
            if (content.StartsWith("="))
                return SetCellContents(Name, new Formula(content.Substring(1), Normalize, IsValid));

            // If string is a double, call set cell contents with that double.
            else if (double.TryParse(content, out double result))
                return SetCellContents(Name, result);

            // If string is a string, call set cell contents with that string.
            else
                return SetCellContents(Name, content);
        }

        /// <summary>
        /// With a filename and a version, converts a Json file to be this current spreadsheet.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="version"></param>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        private void fileToSpreadsheet(string filename, string version)
        {
            // File converted to string 'filestring' and deserialized spreadsheet object.
            string fileString;
            Spreadsheet? spreadsheet;

            // If the filename cannot be read and then serialized into spreadsheet, throw exception.
            try
            {
                fileString = File.ReadAllText(filename);
                spreadsheet = JsonConvert.DeserializeObject<Spreadsheet>(fileString);
            }
            catch (Exception) 
            { 
                throw new SpreadsheetReadWriteException("Error opening, reading, or closing the file"); 
            }

            // If the spreadsheet is not null, attempts to set the cell contents/value of every cell from the json
            // file, catching exceptions as they occur.
            if (spreadsheet != null)
            {
                // If the spreadsheet version is not the same as the constructor version for this object, throw exc.
                if (spreadsheet.Version != version)
                    throw new SpreadsheetReadWriteException("Version entered doesnt match file version");
                try
                {
                    foreach (string c in spreadsheet.GetNamesOfAllNonemptyCells())
                        SetContentsOfCell(c, (string)spreadsheet.GetCellContents(c));
                }
                catch (InvalidNameException)
                {
                    throw new SpreadsheetReadWriteException("Invalid cell name(s)");
                }
                catch (CircularException)
                {
                    throw new SpreadsheetReadWriteException("Circular Dependency");
                }
                catch (FormulaFormatException)
                {
                    throw new SpreadsheetReadWriteException("Invalid Formula");
                }
            }
        }

        /// <summary>
        /// Changes every value in the list of cells to change.
        /// </summary>
        /// <param name="cellsToChange"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ChangeValues(IEnumerator<string> cellsToChange)
        {
            // A lookup function for the formula class to lookup cell values.
            double lookup(string s)
            {
                // If the cell value is not a valid double string, throw exception.
                if (!double.TryParse(GetCellValue(s).ToString(), out double value))
                    throw new ArgumentException();
                return value;
            }

            // For every cell to be changed, check is cell's contents is a formula or other, and set value.
            while (cellsToChange.MoveNext())
            {
                // If cells contents is formula, get cast the contents to formula and evaluate using our lookup.
                if (cells[cellsToChange.Current].Contents is Formula)
                {
                    Formula current = (Formula)GetCellContents(cellsToChange.Current);
                    cells[cellsToChange.Current].Value = current.Evaluate(lookup);
                }

                // Otherwise, set the value as the current contents.
                else cells[cellsToChange.Current].Value = GetCellContents(cellsToChange.Current);
            }
        }

        /// <summary>
        /// Cell object that holds contents, values, and a JSON "stringForm".
        /// </summary>
        private class Cell
        {
            private Object contents;
            private Object value;

            // Cell constructor.
            public Cell(Object content)
            {
                this.contents = content;
                this.value = content;
            }
            // Gets and sets cell contents.
            [JsonIgnore]
            public object Contents
            {
                get { return contents; }
                set { contents = value; }
            }
            // Gets and sets cell values.
            [JsonIgnore]
            public object Value
            {
                get { return value; }
                set { this.value = value; }
            }
            // Gets and sets cell stringForm.
            [JsonProperty]
            public string? stringForm
            {
                get 
                { 
                    // Checks if contents is formula, double, or string.
                    // Returns proper .toString accordingly.
                    if (contents is Formula)
                        return "=" + ((Formula)contents).ToString();
                    else if (contents is double)
                        return ((double)contents).ToString();
                    else return contents.ToString();
                }
                set
                { 
                  if (value != null) 
                    this.contents = value; 
                }
            }
        }
    }


}
