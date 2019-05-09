// Andrew Fryzel

using SS;
using System;
using System.Collections.Generic;
using System.IO;

using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    //CONTROLLER
    public class Controller
    {
        private ISpreadsheetView window;
        private Spreadsheet model;
        /// <summary>
        /// Constructor. Events hooking handlers
        /// </summary>
        /// <param name="window"></param>
        public Controller(ISpreadsheetView window)
        {
            this.window = window;
            this.model = new Spreadsheet();
            window.CloseEvent += HandleClose;
            window.NewEvent += HandleNew;
            window.HelpEvent += HandleHelp;
            window.OpenEvent += HandleOpen;
            window.SaveEvent += HandleSave;
            window.ContentEvent += HandleContent;
            window.GetContentEvent += HandleContentGet;
        }

        /// <summary>
        /// Two parameter constructor to be used with opening a saved file
        /// </summary>
        /// <param name="view"></param>
        /// <param name="model"></param>
        public Controller(ISpreadsheetView view, Spreadsheet model, String filename) : this(view)
        {
            this.model = model;
            this.window = view;
            window.Title = filename;
            window.OpenSS();
        }

        /// <summary>
        /// Handles a new window being opened
        /// </summary>
        private void HandleNew()
        {
            window.OpenNew();
        }

        /// <summary>
        /// Handles a window being closed
        /// </summary>
        private void HandleClose()
        {
            window.DoClose();
        }

        /// <summary>
        /// Handles a request for help
        /// </summary>
        private void HandleHelp()
        {
            window.DoHelp();
        }

        /// <summary>
        /// Handles a new window being opened
        /// </summary>
        private void HandleOpen(String filename)
        {
            Regex varPattern = new Regex(@"^[a-zA-Z][1-9]{1}[0-9]{0,1}$");

            try
            {
                TextReader tr = new StreamReader(File.OpenRead(filename));
                SpreadsheetApplicationContext.GetContext().RunNew(true, new Spreadsheet(tr, varPattern), filename);

            }
            catch (Exception)
            {
                MessageBox.Show("Open Failed");
            }

        }

        /// <summary>
        /// Handles a window being saved
        /// </summary>
        private void HandleSave(String filename)
        {
            try
            {

                TextWriter tw = new StreamWriter(filename);
                this.model.Save(tw);
                window.Title = filename;

            }
            catch (Exception)
            {
                MessageBox.Show("Save Failed");
            }
        }

        /// <summary>
        /// Handles getting the contents of a cell
        /// </summary>
        private void HandleContent(String content)
        {
            //Get row and column locations.
            int col, row;
            char colLoc;
            String cellName;
            window.GetSelection(out col, out row);

            //We need to add 65 to column (in ASCII A = 65), and 1 to the row for correct
            //indicies.
            col = col + 65;
            row = row + 1;
            colLoc = (char)col;

            //Concat the row and col to a string
            cellName = colLoc + "" + row;

            try
            {
                ISet<String> cellsToCalc;
                String parsedContent;
                parsedContent = content.Substring(1, content.Length - 1);


                if (double.TryParse(parsedContent, out double result))
                {
                    cellsToCalc = model.SetContentsOfCell(cellName, parsedContent);
                }
                else
                {
                    cellsToCalc = model.SetContentsOfCell(cellName, content);
                }

                //Get all cells that need recalculated
                Regex r = new Regex(@"([a-zA-Z]+)(\d+)");

                foreach (String cell in cellsToCalc)
                {
                    //Separate the cell name so it can be used to update values.
                    Match m = r.Match(cell);
                    string charString = m.Groups[1].Value;
                    string numString = m.Groups[2].Value;

                    //Now indicies on the grid both need to be parsed to a char or int.
                    char parsedChar;
                    int parsedNum;
                    char.TryParse(charString, out parsedChar);
                    int.TryParse(numString, out parsedNum);

                    //Temp variables for row, column and cell value;
                    int tempCol, tempRow;
                    tempCol = parsedChar - 65;
                    tempRow = parsedNum - 1;
                    object tempValue = model.GetCellValue(cell);

                    //Set the value of the Cell
                    if (model.GetCellValue(cell).ToString().Equals("SS.FormulaError"))
                    {
                        window.SetValue(tempCol, tempRow, "Formula Error");
                    }
                    else
                    {
                        window.SetValue(tempCol, tempRow, tempValue.ToString());
                    }

                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed");
            }

            //TODO

        }

        private string HandleContentGet(String content)
        {
            return "=" + this.model.GetCellContents(content).ToString();
        }
    }
}
