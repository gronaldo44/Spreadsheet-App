using SpreadsheetUtilities;
using SS;

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
        spreadsheet = new();
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
    /// Open a new empty spreadsheet GUI
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NewClicked(Object sender, EventArgs e)
    {
        // Reset the view
        setCellContentsBox.Text = "";
        spreadsheetGrid.Clear();
        spreadsheetGrid.SetSelection(0, 0);
        UpdateNav(0, 0);

        // Reset the model
        spreadsheet = new();
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// 
    /// THIS IS PART OF THE SKELETON CODE AND MAY BE USELESS
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                Console.WriteLine("Successfully chose file: " + fileResult.FileName);

                string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Sets the cell's contents to the text in the cell contents text entry 
    /// when the user completes their input.
    /// 
    /// An input is defined as completed when the user hits the return key
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetCellContentsBox_Completed(object sender, EventArgs e)
    {
        string contents = setCellContentsBox.Text;
        string cell = selectedCellNameBox.Text.Substring(15);
        spreadsheet.SetContentsOfCell(cell, contents);  // TODO: Check if function inputs are working as intended
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.SetValue(col, row, spreadsheet.GetCellValue(cell).ToString());  // TODO: This needs thorough testing
        UpdateNav(col, row);
    }

    /// <summary>
    /// Updates the navigation bar to reflect the values inside the model
    /// 
    /// The navigation bar is used to view and alter the currently selected cell
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
        object contents = spreadsheet.GetCellContents(cellName);
        if (contents is Formula)
        {
            setCellContentsBox.Text = "=" + contents.ToString();
        } else
        { 
            setCellContentsBox.Text = contents.ToString();
        }
    }

    // TODO: Save the spreadsheet

    // TODO: Load a saved spreadsheet

    // TODO: Alert Messages

    /*
     * I don't know how to comment .xaml files so I will note what's going on here.
     * 
     * MenuBarItems: Idk wtf is going on here exactly yet lol
     * 
     * Grid: Just a layout for adding content to the GUI.
     *      The first row is a navigation bar
     *      The Second row is the spreadsheet
     *      It may be a good idea to put an entry for a filepath for saving
     *          the spreadsheet on a third row.
     */
}
