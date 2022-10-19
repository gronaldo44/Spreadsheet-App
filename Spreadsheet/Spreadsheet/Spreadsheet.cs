// Functionality implemented by Ronald Foster for CS 3500, September 2022
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Xml.XPath;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Net.Mime;
using System.Data;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
using Microsoft.VisualBasic;
using JsonConstructorAttribute = Newtonsoft.Json.JsonConstructorAttribute;

namespace SS
{
    /// <summary>
    /// A cell is a member of a spreadsheet that contains its contents, value, and a list of 
    /// other cells directly dependent upon this cell
    /// </summary>
    internal class Cell
    {
        private object _content;
        public object Contents      // Either a string, double or Formula
        {
            get => _content;
            set
            {
                _content = value;
                Value = _content;   // Temporary value while the Formula is evaluated
            }
        }
        public object Value         // Either a string, double, or FormulaError
        {
            get; set;
        }

        /// <summary>
        /// List of cells whose value directly depends on the value of this cell
        /// </summary>
        public List<string> DirectDependents { get; }

        /// <summary>
        /// Constructs a cell with the argued contents to be evaluated and an empty list of
        /// empty direct dependents
        /// dired dependents
        /// </summary>
        /// <param name="value"></param>
        public Cell(Object contents)
        {
            this._content = contents;
            this.DirectDependents = new();
            this.Value = contents;
        }

    }

    /// <summary>
    /// Model for a spreadsheet of digital cells containing contents of either a string, double, or Formula
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private DependencyGraph cellDependency;     // Dependency status for cells with Formula contents
        private Dictionary<string, Cell> cells;     // Cell name -> cell data

        /// <summary>
        /// Has this spreadsheet been modified since it was created or last saved?
        /// </summary>
        public override bool Changed
        {
            get;
            set;
        }

        /// <summary>
        /// Constructs an empty spreadsheet with default validity and version.
        /// </summary>
        public Spreadsheet() : base(IsValidCellName, s => s, "default")
        {
            this.cellDependency = new();
            this.cells = new();
            this.Changed = false;
        }

        /// <summary>
        /// Constructs an empty spreadsheet with the specified cell name validity and normalization 
        /// and the specified version of spreadsheet.
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            this.cells = new();
            this.cellDependency = new();
            this.Changed = false;
        }

        /// <summary>
        /// Constructs a spreadsheet from a filepath.
        /// 
        /// Throws a SpreadsheetReadWriteException if the versions are incompatible or if the file structure 
        /// is incorrect
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        /// <exception cref="SpreadsheetReadWriteException">There was a problem reading from the file</exception>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize,
            string version) : base(isValid, normalize, version)
        {
            // Initialize spreadsheet attributes
            this.cells = new();
            this.cellDependency = new();
            this.Changed = false;

            // Read the file
            StreamReader file;
            try
            {
                file = new StreamReader(filepath);
            }
            catch
            {
                throw new SpreadsheetReadWriteException("There was an error reading the file");
            }
            string? line;
            string saveState = "";
            while ((line = file.ReadLine()) != null)
            {
                saveState += line;
            }

            // Load the save state
            bool invalidFileStructure = false;
            try
            {
                SaveState? loadedSave = JsonConvert.DeserializeObject<SaveState>(saveState);
                if (loadedSave != null)
                {
                    // Intialize this spreadsheet with the contents and version from the loaded save
                    foreach (string name in loadedSave.cells.Keys)
                    {
                        SetContentsOfCell(name, loadedSave.cells[name].stringForm);
                    }
                    this.Version = loadedSave.Version;
                }
            }
            catch
            {
                invalidFileStructure = true;
            }

            // Check if anything went wrong
            if (invalidFileStructure)
            {
                throw new SpreadsheetReadWriteException("Invalid file structure");
            }
            if (this.Version != version)
            {
                throw new SpreadsheetReadWriteException("Incompatible versions");
            }
        }

        /// <summary>
        /// Get the contents of the cell with this name
        /// 
        /// If name is invalid, throws an InvalidNameException
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the contents of the cell with this name</returns>
        /// <exception cref="InvalidNameException">Argued name is invalid</exception>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }

            if (cells.ContainsKey(name))
            {
                return cells[name].Contents;
            }
            else
            {   // Cell is empty
                return "";
            }
        }

        /// <summary>
        /// Gets the value of the named cell
        /// 
        /// If name is invalid, throws an InvalidNameException
        /// </summary>
        /// <param name="name"></param>
        /// <returns>value of the named cell</returns>
        /// <exception cref="InvalidNameException">Argued name is invalid</exception>
        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }

            if (!cells.ContainsKey(name))
            {   // Cell is empty
                return "";
            }
            else
            {
                return cells[name].Value;
            }
        }

        /// <summary>
        /// Gets the names of all nonempty cells
        /// </summary>
        /// <returns>names of all nonempty cells</returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cells.Keys;
        }

        /// <summary>
        /// Saves the current state of this spreadsheet to the argued filename
        /// 
        /// Save state is in JSON format:
        ///     {
        ///         "cells" : {
        ///             "a1": {
        ///                 "stringForm": value
        ///             },
        ///             "a2": {
        ///                 "stringForm": value
        ///             }
        ///         },
        ///         "Version": version
        ///     }
        ///     
        /// Constructing a spreadsheet with the given filename will initialize a 
        /// spreadsheet in this current save state.
        /// 
        /// If there is a problem writing to the file, throws a SpreadsheetReadWriteException
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        public override void Save(string filename)
        {
            // Compresss the cells data in this spreadsheet
            Dictionary<string, CompressedCell> savedCells = new();
            foreach (string name in cells.Keys)
            {
                savedCells.Add(name, new CompressedCell(cells[name].Contents));
            }

            // Write the compressed data to the save file
            string saveState = JsonConvert.SerializeObject(new SaveState(savedCells, Version));
            try
            {
                File.WriteAllText(filename, saveState);
                Changed = false;
            }
            catch
            {
                throw new SpreadsheetReadWriteException(
                    "There was a problem writing to the file: \"" + filename + "\"");
            }
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException
        /// 
        /// If content is a double, the contents of the named cell becomes that double.
        /// 
        /// If content begins with '=' and is followed up by a valid formula, the 
        /// contents of the named cell becomes that formula.
        /// 
        /// Otherwise, the contents of the named cell become content.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns>list consisting of name plus the mames of all other cells whos values,
        /// depends directly or indrectly, on the names of cell</returns>
        /// <exception cref="InvalidNameException">Invalid argued cell name</exception>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            name = Normalize(name);
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }
            Changed = true;

            if (Double.TryParse(content, out double num))
            {   // Adding a cell whose content is a double
                return SetCellContents(name, num);
            }
            else if (content.Length > 1 && content.First() == '=')
            {   // Adding a cell whose content is a Formula
                Formula f = new Formula(content.Substring(1), Normalize, IsValid);
                return SetCellContents(name, f);
            }
            else
            {   // Adding a cell whose content is a string
                return SetCellContents(name, content);
            }
        }

        /// <summary>
        /// Sets the contents of this cell to the argued number.
        /// 
        /// If name is invalid, throws an InvalidNameException.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns>list consisting of name plus the mames of all other cells whos values,
        /// depends directly or indrectly, on the names of cell</returns>
        /// <exception cref="InvalidNameException">Argued cell name is invalid</exception>
        protected override IList<string> SetCellContents(string name, double number)
        {
            IList<string> result = DocumentCell(name, number);
            cells[name].Value = number;
            return result;
        }

        /// <summary>
        /// Sets the contents of this cell to the argued text.
        /// 
        /// If name is invalid, throws an InvalidNameException.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <returns>list consisting of name plus the mames of all other cells whos values,
        /// depends directly or indrectly, on the names of cell</returns>
        /// <exception cref="InvalidNameException">Argued cell name is invalid</exception>
        protected override IList<string> SetCellContents(string name, string text)
        {
            if (text == "")
            {   // Empty cells are not documented
                if (cells.ContainsKey(name))
                {
                    foreach(string dependee in cellDependency.GetDependees(name))
                    {
                        if (cells[dependee].Contents.ToString() == "")
                        {
                            cells.Remove(dependee);
                            cellDependency.RemoveDependency(dependee, name);
                        }
                    }
                    cells.Remove(name);
                }
                return new List<string>();
            }
            return DocumentCell(name, text);
        }

        /// <summary>
        /// Sets the contents of this cell to the argued formula.
        /// 
        /// If name is invalid, throws an InvalidNameException.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="formula"></param>
        /// <returns>list consisting of name plus the mames of all other cells whos values,
        /// depends directly or indrectly, on the names of cell</returns>
        /// <exception cref="InvalidNameException">Argued cell name is invalid</exception>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            IEnumerable<string> dependees = formula.GetVariables();

            // Update dependency status of this cell and its associates
            cellDependency.ReplaceDependees(name, dependees);
            foreach (string dependee in dependees)
            {
                if (!cells.ContainsKey(dependee))
                {   // Document this cell's dependees that do not have contents
                    DocumentCell(dependee, "");
                }
                cells[dependee].DirectDependents.Add(name);
            }

            // Document the cell in this Spreadsheet
            IList<string> result = DocumentCell(name, formula);
            RecalculateValue(name, formula);
            return result;
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return cells[name].DirectDependents;
        }

        /// <summary>
        /// Returns a list consisting of name plus the names of all other cells whos values,
        /// depends directly or indirectly, on the names of cell
        /// 
        /// throws CircularException if a circular dependency is made
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="CircularException">A circular dependency was made</exception>
        private IList<string> GetDescendants(string name)
        {
            List<string> result = new();
            foreach (string descendant in GetCellsToRecalculate(name))
            {
                result.Add(descendant);
                if (cells[descendant].Contents is Formula)
                {   // This cell's value should be recalculated because its ancestor's changed
                    RecalculateValue(descendant, (Formula)(cells[descendant].Contents));
                }
            }
            return result;
        }

        /// <summary>
        /// Documents a cell in the spreadsheet with the given contents
        /// 
        /// throws CircularException if a circular dependency is made
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>list consisting of name plus the mames of all other cells whos values,
        /// depends directly or indrectly, on the names of cell</returns>
        /// <exception cref="CircularException">A circular dependency was made</exception>
        private IList<string> DocumentCell(string name, Object contents)
        {
            IList<string> result;
            if (!cells.ContainsKey(name))
            {   // A previously empty cell is being documented
                cells.Add(name, new Cell(contents));
                return GetDescendants(name);
            }
            else
            {
                // Update Dependency
                if ((contents is string || contents is double) && cellDependency.HasDependees(name))
                {   // This cell no long depends on other cells
                    foreach (string dependee in cellDependency.GetDependees(name))
                    {
                        cells[dependee].DirectDependents.Remove(name);
                        cellDependency.RemoveDependency(dependee, name);
                    }
                }

                // Document the cell
                object prevContents = cells[name].Contents;
                try
                {
                    cells[name].Contents = contents;
                    result = GetDescendants(name);
                }
                catch (CircularException e)
                {
                    // The contents of this cell should not be updated
                    if (prevContents.ToString() == "")
                    {   // This cell was empty
                        cells.Remove(name);
                    }
                    else
                    {   // Revert to previous value
                        cells[name].Contents = prevContents;
                    }
                    throw e;
                }
                return result;
            }
        }

        /// <summary>
        /// Checks if a cell name is valid. 
        /// 
        /// A valid cell name consists of one or more letters followed by one or more digits
        /// </summary>
        /// <param name="n"></param>
        /// <returns>Whether or not this name is valid</returns>
        private static bool IsValidCellName(string n)
        {
            string varPattern = "^[a-zA-Z]+[0-9]+$";
            return Regex.IsMatch(n, varPattern);
        }

        /// <summary>
        /// Recalculates the value of the named cell by looking up the values of 
        /// other cells its contents depend on in this spreadsheet.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        private void RecalculateValue(string name, Formula contents)
        {
            cells[name].Value = contents.Evaluate(LookupCell);
        }

        /// <summary>
        /// Lookups up the value of the named cell
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Value of the named cell</returns>
        private double LookupCell(string name)
        {
            if (cells[name].Value is string)
            {   // Nested dependency
                return LookupCell((string)cells[name].Value);
            }
            else
            {
                return (double)cells[name].Value;  // May throw a FormulaError
            }
        }

        /// <summary>
        /// Compressed spreadsheet for storage purposes
        /// </summary>
        internal class SaveState
        {
            public Dictionary<string, CompressedCell> cells; // cell name -> compressed cell
            public string Version;

            /// <summary>
            /// Constructs a save of this spreadsheet in its current state
            /// </summary>
            /// <param name="cells">Compressed cells in this spreadsheet</param>
            /// <param name="version">Version of this spreadsheet</param>
            public SaveState(Dictionary<string, CompressedCell> cells, string version)
            {
                this.cells = cells;
                this.Version = version;
            }
        }
        /// <summary>
        /// Compressed spreadsheet cell for storage purposes
        /// </summary>
        internal class CompressedCell
        {
            public string stringForm;   // Contents of this cell as they would be input by a user

            /// <summary>
            /// Constructs a compressed cell with these contents
            /// </summary>
            /// <param name="contents"></param>
            public CompressedCell(object contents)
            {
                if (contents != null)
                {
                    this.stringForm = "" + contents.ToString();
                    if (contents is Formula && this.stringForm.Length > 0)
                    {   // Cell's contents is a non-empty Formula
                        this.stringForm = "=" + this.stringForm;
                    }
                }
                else
                {
                    this.stringForm = "";
                }
            }
        }
    }
}
