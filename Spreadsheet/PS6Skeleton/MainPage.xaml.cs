﻿using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Storage;
using SpreadsheetUtilities;
using SS;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WinRT;

namespace SpreadsheetGUI;

/// <summary>
/// Spreadsheet GUI
/// </summary>
public partial class MainPage : ContentPage
{
    private delegate void AfterSaving(string filepath);    // Method for following up saves
    private Spreadsheet spreadsheet;    // model

    /// <summary>
    /// Constructs an empty spreadsheet GUI
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        spreadsheetGrid.SelectionChanged += clickedCell;

        // Initialize the Model
        spreadsheet = new Spreadsheet(validator, normalizer, "ps6");
        // Initialize the View
        spreadsheetGrid.SetSelection(0, 0);
        UpdateNav(0, 0);
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
    /// A new cell has been clicked
    /// 
    /// Focus the cell that got clicked in the grid and navigation bar
    /// 
    /// The Navigation bar is used to view and alter the currently selected cell
    /// </summary>
    /// <param name="grid"></param>
    private void clickedCell(SpreadsheetGrid grid)
    {
        // Protect integrity of spreadsheet data
        bool changed = spreadsheet.Changed;

        // Get the grid position of the cell that was clicked
        spreadsheetGrid.GetSelection(out int col, out int row);
        // Get each cell associated with the clicked cell
        string cell = CalculateCellName(col, row);
        string contents = InterpretCellContents(cell);
        IEnumerable<string> associatedCells = spreadsheet.SetContentsOfCell(cell, contents);
        // Highlight each cell associated with the clicked cell
        UpdateGrid(associatedCells);
        UpdateNav(col, row);

        spreadsheet.Changed = changed;
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
        bool saveOld = false;
        if (spreadsheet.Changed)
        { // ALERT if they are sure they want to overwrite this spreadsheet's data
            saveOld = await DisplayAlert("Unsaved Spreadsheet", "This operation would " +
                "result in the loss of data. Do you wist to save first?", "Yes", "No");
        }
        if (saveOld)
        {   // ALERT ask where they want to save the spreadsheet
            Alert_SaveAs(AlterSpreadsheet, "reset");
        }
        else
        {
            AlterSpreadsheet("reset");
        }
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
                bool saveOld = false;
                if (spreadsheet.Changed)
                {   // ALERT if they are sure they want to overwrite this spreadsheet's data
                    saveOld = await DisplayAlert("Unsaved Spreadsheet", "This operation would " +
                        "result in the loss of data. Do you wist to save first?", "Yes", "No");
                }
                if (saveOld)
                {   // ALERT ask where they want to save the spreadsheet
                    Alert_SaveAs(OverwriteSpreadsheet, fileResult.FullPath);
                }
                else
                {
                    OverwriteSpreadsheet(fileResult.FullPath);
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
    /// Alerts the user to enter a filepath and saves the spreadsheet there
    /// 
    /// Function is called when a user clicks the Save As button in the File menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveAs_Clicked(object sender, EventArgs e)
    {
        Alert_SaveAs(AlterSpreadsheet, "");
    }

    private void CellInputs_Clicked(object sender, EventArgs e)
    {
        // TODO: Finish writing help menu
        DisplayAlert("Help Menu: Cell Inputs",
            "To edit a cell, select it and click the \"Cell Contents\" box and enter your content.\n" +
            "The cell 'A1' is selected by default upon opening a new spreadsheet.\n" +
            "Then, press enter to set the cell and update its value.\n\n" +
            "The cell 'A1' is selected by default upon opening a new spreadsheet.\n",
            "OK");
    }

    private void Saving_Clicked(object sender, EventArgs e)
    {
        // TODO: Finish writing help menu
        DisplayAlert("Help Menu: Saving",
            "To save the spreadsheet, select \"File\" in the header then \"Save As\".\n" +
            "Enter your chosen file path and press OK.",
            "OK");
    }

    private void AdditionalContent_Clicked(object sender, EventArgs e)
    {
        // TODO: Explain how highlighting cells works
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

        // Attempt to update the spreadsheet
        bool dontUpdate = false;
        IList<string> changed = null;   // this will never be used while null
        try
        {
            // Add the input to the spreadsheet
            changed = spreadsheet.SetContentsOfCell(cell, newContents);
            foreach (string c in changed)
            {   // Check if this update would break any dependencies
                if (spreadsheet.GetCellValue(c) is FormulaError)
                {   // A dependency broke and the changes should be reverted
                    spreadsheet.SetContentsOfCell(cell, oldContents);
                    dontUpdate = true;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is FormulaFormatException || ex is CircularException)
            {
                dontUpdate = true;
            }
            else
            {
                throw;
            }
        }

        // Did the spreadsheet update properly?
        if (dontUpdate)
        {   // The cell was reverted to its old value and the user is allerted
            Alert_InvalidContentsEntry();
        }
        else
        {   // Each dependent cell's value is updated in the View
            UpdateGrid(changed);
        }
        UpdateNav(col, row);
    }

    /// <summary>
    /// Alters the spreadsheet based on the argued action
    /// </summary>
    /// <param name="action"></param>
    private void AlterSpreadsheet(string action)
    {
        if (action == "reset")
        {
            // Reset the model
            spreadsheet = new Spreadsheet(validator, normalizer, "ps6");
            // Reset the view
            spreadsheetGrid.Clear();
            spreadsheetGrid.SetSelection(0, 0);
            UpdateNav(0, 0);
        }
    }

    /// <summary>
    /// Overwrites the spreadsheet with the spreadsheet saved in the argued filepath
    /// </summary>
    /// <param name="filepath"></param>
    private void OverwriteSpreadsheet(string filepath)
    {
        // Overwrite the model
        Spreadsheet oldSpreadsheet = spreadsheet;  // Safety in case of bad load
        bool validSpreadsheet = true;
        try
        {
            spreadsheet = new Spreadsheet(filepath, validator, normalizer, "ps6");
        }
        catch
        {
            validSpreadsheet = false;
        }

        // Overwrite the view
        IEnumerable<string> namedCells = spreadsheet.GetNamesOfAllNonemptyCells();
        // Update the grid with all values in the spreadsheet
        spreadsheetGrid.Clear();
        if (!UpdateGrid(namedCells))
        {   // The loaded spreadsheet is defined as invalid for a spreadsheetGUI
            validSpreadsheet = false;
        }
        // Update the nav
        if (validSpreadsheet)
        {
            spreadsheet.Changed = false;
            spreadsheetGrid.SetSelection(0, 0);
            UpdateNav(0, 0);
        }
        else
        {   // The loaded spreadsheet is invalid and does not overwrite
            spreadsheet = oldSpreadsheet;
            Alert_SpreadsheetCannotBeLoaded();
        }
    }

    /// <summary>
    /// Updates the contents of the argued cell to the argued contents and 
    /// returns whether or not the cell updated without errors.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="contents"></param>
    /// <param name="col">Column of the cell in the spreadsheetGUI</param>
    /// <param name="row">Row of the cell in the spreadsheetGUI</param>
    /// <returns>Whether or not the cell update is valid</returns>
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
    /// not the grid will update without errors
    /// 
    /// The grid is used to View the values inside the Model
    /// </summary>
    /// <returns>Whether or not the grid should update</returns>
    private bool UpdateGrid(IEnumerable<string> cellsToBeRecalculated)
    {
        bool updated = true;

        // Update the grid in the View with each cell value that changed
        foreach (string cell in cellsToBeRecalculated)
        {
            CalculateGridPosition(cell, out int col, out int row);
            if (!UpdateCell(cell, col, row))
            {   // One of the new values is invalid and the grid should not update
                updated = false;
            }
        }
        // Highlight all associated cells
        spreadsheetGrid.associatedCells = cellsToBeRecalculated;

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
        string cellName = CalculateCellName(col, row);
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
    /// Calcualtes a cell's name based off of its position in the spreadsheet 
    /// grid.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns>Cell name</returns>
    private string CalculateCellName(int col, int row)
    {
        char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        string cellName = "" + alphabet[col] + (row + 1);
        return cellName;
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

    /// <summary>
    /// Alerts the User that they entered a bad filepath
    /// </summary>
    private async void Alert_InvalidFilePath()
    {
        await DisplayAlert("Invalid Filepath", "Could not write to that file path.\n" +
            "please enter the exact file path followed by the spreadsheet name\n\n" +
            "Example: C:\\Users\\myAccount\\Documents\\mySpreadsheet", "OK");
    }

    /// <summary>
    /// Alerts the User that they have overwritten a file on their machine
    /// </summary>
    private async void Alert_FileOverwritten()
    {
        await DisplayAlert("File Overwritten", "This operation resulted in a file " +
                    "being overwritten.", "OK");
    }

    /// <summary>
    /// Alerts the user asking where they want to save the spreadsheet, saves it there, 
    /// then continues onward to another method.
    /// </summary>
    /// <param name="continueAfterSave">Where this method continues on completion</param>
    /// <param name="methodArg">argument for the continuation method</param>
    private async void Alert_SaveAs(AfterSaving continueAfterSave, string methodArg)
    {
        string filepath = await DisplayPromptAsync("Save As", "Enter the full filepath and name" +
            "\n(Format: \"filepath\\filename\"");
        Debug.WriteLine("filepath: " + filepath);
        if (filepath != null)
        {
            filepath += ".sprd";
            if (File.Exists(filepath))
            {
                Alert_FileOverwritten();
            }
            try
            {
                spreadsheet.Save(filepath);
                continueAfterSave(methodArg);
            }
            catch (SpreadsheetReadWriteException)
            {
                Alert_InvalidFilePath();
            }
        }
    }

    /*
     * I don't know how to comment .xaml files so I will note what's going on here.
     * 
     * MenuBarItems: Tabs for putting drop-down lists
     *      1 First tab is file
     *      --1.1 New: reset the spreadsheet
     *      --1.2 Open: Open a new spreadsheet from a save file
     *      --1.3 Save As: Save the spreadsheet
     *      2 Second tab is help
     *      --2.1 Cell Inputs: explain how cell inputs work
     *      --2.2 Saving: explain how saving works
     *      --2.3 ADDITIONAL-CONTENT: explain our additional content
     * 
     * Grid: Just a layout for adding content to the GUI.
     *      The first row is a navigation bar
     *          First column of the navbar is for entering cell inputs
     *          I am setting the second column of the nav aside for ADDITIONAL-CONTENT
     *      The Second row is the spreadsheet
     *    
     */

    // TODO: Write a README
}
