﻿using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using SpreadsheetUtilities;
using SS;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI;

/// <summary>
/// Spreadsheet GUI
/// </summary>
public partial class MainPage : ContentPage
{
    private Spreadsheet spreadsheet;    // model

    /// <summary>
    /// Constructs an empty spreadsheet GUI
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += clickedCell;

        // Initialize the Model
        spreadsheet = new Spreadsheet(validator, normalizer, "ps6");
        // Initialize the View
        spreadsheetGrid.SetSelection(0, 0);
        UpdateNav(0, 0);
    }

    /// <summary>
    /// A new cell has been clicked
    /// 
    /// Focus the cell that got clicked in the grid and navigation bar
    /// 
    /// The Navigation bar is used to view and alter the currently selected cell
    /// </summary>
    /// <param name="grid"></param>
    private void clickedCell(SpreadsheetGrid grid)
    {
        // Update Grid
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.GetValue(col, row, out string value);
        // Update Nav
        UpdateNav(col, row);
    }

    /// <summary>
    /// Checks if the argued variable is a valid cell name in this spreadsheet GUI
    /// 
    /// A variable is considered valid if it corresponds to a cell. 
    /// In other words: A01-Z99
    /// </summary>
    /// <param name="var"></param>
    /// <returns></returns>
    private bool validator(string var)
    {
        string varPattern = "^[A-Z][0-9]?[0-9]$";
        return Regex.IsMatch(var, varPattern) && var != "A0" && var != "A00";
    }

    /// <summary>
    /// Returns an upper-case copy of the argued variable
    /// 
    /// A2 and a2 are treated the same after being normalized.
    /// </summary>
    /// <param name="var"></param>
    /// <returns></returns>
    private string normalizer(string var)
    {
        return var.ToUpper();
    }

    /// <summary>
    /// Open a new empty spreadsheet GUI
    /// 
    /// Alerts if the user is attempting to overwrite an unsaved spreadsheet
    /// 
    /// Function is called when "New" is clicked in the GUI menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void New_Clicked(Object sender, EventArgs e)
    {
        // Protect unsaved data in the current spreadsheet
        bool saveOld = true;
        if (spreadsheet.Changed)
        { // ALERT if they are sure they want to overwrite this spreadsheet's data
            saveOld = await DisplayAlert("Unsaved Spreadsheet", "This operation would " +
                "result in the loss of data. Do you wist to save first?", "Yes", "No");
        }
        if (saveOld)
        {
            // TODO: save the old spreadsheet
        }


        // Reset the model
        spreadsheet = new Spreadsheet(validator, normalizer, "ps6");
        // Reset the view
        spreadsheetGrid.Clear();
        spreadsheetGrid.SetSelection(0, 0);
        UpdateNav(0, 0);

    }

    /// <summary>
    /// Open a saved spreadsheet and intializes the View and Model to the loaded spreadsheet
    /// 
    /// Alerts if the user is attempting to overwrite and unsaved spreadsheet
    /// 
    /// Function is called when "Open" is clicked in the GUI menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Open_Clicked(Object sender, EventArgs e)
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {   // Overwrite this spreadsheet with the saved file
                Debug.WriteLine("Succesfully chose file: " + fileResult.FileName);

                // Protect unsaved data in the current spreadsheet
                bool saveOld = true;
                if (spreadsheet.Changed)
                { // ALERT if they are sure they want to overwrite this spreadsheet's data
                    saveOld = await DisplayAlert("Unsaved Spreadsheet", "This operation would " +
                        "result in the loss of data. Do you wist to save first?", "Yes", "No");
                }
                if (saveOld)
                {
                    // TODO: save the old spreadsheet
                }

                // Overwrite the model
                Spreadsheet oldSpreadsheet = spreadsheet;  // Safety in case of bad load
                bool validSpreadsheet = true;
                try
                {
                    spreadsheet = new Spreadsheet(fileResult.FullPath, validator, normalizer, "ps6");
                }
                catch
                {
                    validSpreadsheet = false;
                }
                // Overwrite the view
                IEnumerable<string> namedCells = spreadsheet.GetNamesOfAllNonemptyCells();
                if (!UpdateGrid(namedCells))
                {
                    validSpreadsheet = false;
                }
                if (validSpreadsheet)
                {
                    spreadsheetGrid.Clear();
                    spreadsheetGrid.SetSelection(0, 0);
                    UpdateNav(0, 0);
                }
                else
                {
                    spreadsheet = oldSpreadsheet;
                    Alert_SpreadsheetCannotBeLoaded();
                }

            }
            else
            {
                Debug.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error opening file:");
            Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// Sets the cell's contents to the text in the cell contents text entry.
    /// 
    /// Function is called when the user completes their input.
    /// 
    /// An input is defined as completed when the user hits the return key
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetCellContentsBox_Completed(object sender, EventArgs e)
    {
        string cell = selectedCellNameBox.Text.Substring(15);
        string oldContents = InterpretCellContents(cell);
        string newContents = setCellContentsBox.Text;
        spreadsheetGrid.GetSelection(out int col, out int row);

        bool dontUpdate = false;
        IList<string> changed = spreadsheet.SetContentsOfCell(cell, newContents);
        foreach (string c in changed)
        {
            if (spreadsheet.GetCellValue(c) is FormulaError)
            {
                spreadsheet.SetContentsOfCell(cell, oldContents);
                dontUpdate = true;
            }
        }
        if (dontUpdate)
        {   // The cell was reverted to its old value and the user is allerted
            Alert_InvalidContentsEntry();
        }
        else
        {
            UpdateGrid(changed);
        }
        UpdateNav(col, row);
    }

    /// <summary>
    /// Updates the contents of the argued cell to the argued contents and 
    /// returns whether or not the cell was updated.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="contents"></param>
    /// <param name="col">Column of the cell in the spreadsheetGUI</param>
    /// <param name="row">Row of the cell in the spreadsheetGUI</param>
    /// <returns>Whether or not the cell was updated</returns>
    private bool UpdateCell(string cell, int col, int row)
    {
        if (spreadsheet.GetCellValue(cell) is FormulaError)
        {
            return false;
        }
        else
        {
            spreadsheetGrid.SetValue(col, row, spreadsheet.GetCellValue(cell).ToString());
            return true;
        }
    }

    /// <summary>
    /// Updates the grid to reflect the values inside the Model if it 
    /// would not result in any formula errors and returns whether or 
    /// not the grid was updated
    /// 
    /// The grid is used to View the values inside the Model
    /// </summary>
    /// <returns>Whether or not the grid was updated</returns>
    private bool UpdateGrid(IEnumerable<string> cellsToBeRecalculated)
    {
        bool updated = true;

        foreach (string cell in cellsToBeRecalculated)
        {
            CalculateGridPosition(cell, out int col, out int row);
            if (!UpdateCell(cell, col, row))
            {
                updated = false;
            }
        }
        return updated;
    }

    /// <summary>
    /// Updates the navigation bar to reflect the values inside the Model
    /// 
    /// The navigation bar is used to View and alter the currently selected cell
    /// </summary>
    /// <param name="col">Column of cell being focused</param>
    /// <param name="row">Row of cell being focused</param>
    private void UpdateNav(int col, int row)
    {
        char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        string cellName = "" + alphabet[col] + (row + 1);

        spreadsheetGrid.GetValue(col, row, out string value);
        selectedCellValueBox.Text = "Value: " + value;
        selectedCellNameBox.Text = "Selected Cell: " + cellName;
        string contents = InterpretCellContents(cellName);
        setCellContentsBox.Text = contents;
    }

    /// <summary>
    /// Returns a reinterpreted version of the contents in the argued cell 
    /// as it would be argued as contents being ADDED to a spreadsheet.
    /// </summary>
    /// <param name="contents"></param>
    /// <returns></returns>
    private string InterpretCellContents(string cell)
    {
        object contents = spreadsheet.GetCellContents(cell);
        string result = contents.ToString();

        if (contents is Formula)
        {
            result = "=" + result;
        }
        return result;
    }

    /// <summary>
    /// Calculates the grid position of the argued cell
    /// 
    /// For example, the grid position of cell A1 is column-0, row-0.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void CalculateGridPosition(string cell, out int col, out int row)
    {
        // Convert the cell letter to its corresponding numerical value (a = 0)
        int index = char.ToUpper(cell.First()) - 64;
        col = index - 1;
        // Convert the model interpretation of the row to the view interpretation
        index = int.Parse(cell.Substring(1));
        row = index - 1;
    }

    // TODO: Save the spreadsheet
    /*
     * Possible solution: use a prompt
     */

    // TODO: Help menu

    // TODO: Alert Messages

    /*
     * Alert for overwriting saved data: COMPLETE
     */

    /// <summary>
    /// Alerts the User that their entry would result in a FormulaError
    /// </summary>
    private async void Alert_InvalidContentsEntry()
    {
        await DisplayAlert("Invalid Entry", "That entry would result in a Formula Error", "OK");
    }

    /// <summary>
    /// Alerts the User that they cannot open a file as a spreadsheet
    /// </summary>
    private async void Alert_SpreadsheetCannotBeLoaded()
    {
        await DisplayAlert("Invalid Spreadsheet File", "The selected file contains errors.", "OK");
    }

    // TODO: General problems
    /*
     * See if formulas are working as intended
     * What should we do when we open a spreadsheet with incompatible version
     */

    // TODO: Additional Content

    /*
     * I don't know how to comment .xaml files so I will note what's going on here.
     * 
     * MenuBarItems: Idk wtf is going on here exactly yet lol
     * 
     * Grid: Just a layout for adding content to the GUI.
     *      The first row is a navigation bar
     *      The Second row is the spreadsheet
     *      --It may be a good idea to put an entry for a filepath for saving
     *          the spreadsheet on a third row.
     */
}
