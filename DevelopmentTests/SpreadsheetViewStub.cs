using SpreadsheetGUI;
using SS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevelopmentTests
{
    class SpreadsheetViewStub : ISpreadsheetView
    {
        public string Title { get; set; }
        public string cellValue { set => throw new NotImplementedException(); }
        public string cellContents { set => throw new NotImplementedException(); }
        public string selection { get; private set; }
        public string currentCellValue { get; private set; }
        public bool CalledDoClose { get; private set; }
        public bool CalledOpenNew { get; private set; }
        public bool CalledDoHelp { get; private set; }
        public bool CalledSave { get; private set; }
        public bool CalledOpen { get; private set; }

        public Dictionary<String, String> cells = new Dictionary<string, string>();
        public Spreadsheet ss = new Spreadsheet();

        public event Action<string> OpenEvent;
        public event Action CloseEvent;
        public event Action NewEvent;
        public event Action HelpEvent;
        public event Action<string> FileChosenEvent;
        public event Action<string> SaveEvent;
        public event Action<string> ContentEvent;
        public event Func<string, string> GetContentEvent;

        /// <summary>
        /// Firing a close click
        /// </summary>
        public void FireCloseEvent()
        {
            if (CloseEvent != null)
            {
                CloseEvent();
            }
        }

        /// <summary>
        /// Sets CalledDoClose to true;
        /// </summary>
        public void DoClose()
        {
            CalledDoClose = true;
        }

        /// <summary>
        /// Firing a help event
        /// </summary>
        public void FireHelpEvent()
        {
            if (OpenEvent != null)
            {
                HelpEvent();
            }
        }

        /// <summary>
        /// Do help set to true
        /// </summary>
        public void DoHelp()
        {
            CalledDoHelp = true;
        }

        /// <summary>
        /// Firing a save event
        /// </summary>
        /// <param name="fileName"></param>
        public void FireSaveEvent(String fileName)
        {
            if (SaveEvent != null)
            {
                SaveEvent(fileName);
            }
        }

        public void FireOpenEvent(String fileName)
        {
            if (OpenEvent != null)
            {
                OpenEvent(fileName);
            }
        }

        public void GetSelection(out int col, out int row)
        {
            if (selection == null)
            {
                col = 0;
                row = 0;
                return;
            }

            //Get row and column locations.
            char tempChar;
            int tempInt;
            string letters = selection;

            Regex r = new Regex(@"([a-zA-Z]+)(\d+)");
            Match m = r.Match(letters);

            string charString = m.Groups[1].Value;
            string numString = m.Groups[2].Value;

            char.TryParse(charString, out tempChar);
            int.TryParse(numString, out tempInt);

            col = tempChar - 65;
            row = tempInt - 1;
        }
        
        /// <summary>
        /// Firing an open new window method.
        /// </summary>
        public void FireNewEvent()
        {
            if (NewEvent != null)
            {
                NewEvent();
            }
        }
        
        /// <summary>
        /// Sets OpenNew to true
        /// </summary>
        public void OpenNew()
        {
            CalledOpenNew = true;
        }

        public void FireContentEvent(String cell)
        {
            if (ContentEvent != null)
            {
                ContentEvent(cell);
            }
        }

        public string FireGetContentEvent(String cell)
        {
            if (GetContentEvent != null)
            {
                return GetContentEvent(cell);
            }
            return "";
        }

        /// <summary>
        /// Sets the selection for the GUI
        /// </summary>
        public void SetNewSelection(int col, int row)
        {
            int tempCol, tempRow;
            char tempChar;
            tempCol = col + 65;
            tempRow = row + 1;
            tempChar = (char)tempCol;

            selection = tempChar + "" + tempRow;
        }

        public void SetValue(int col, int row, string value)
        {
            int tempCol, tempRow;
            char tempChar;
            string location;
            tempCol = col + 65;
            tempRow = row + 1;
            tempChar = (char)tempCol;

            location = tempChar + "" + tempRow;
            ss.SetContentsOfCell(location, value);
        }

        public object GetValue(String cell)
        {
            object returnVal = ss.GetCellValue(cell);
            return returnVal;
        }

        public void OpenSS()
        {
            Char character;

            for (int i = 0; i < 26; i++)
            {
                character = (Char)(65 + i);

                for (int j = 0; j < 99; j++)
                {
                    String val = GetContentEvent(character + "" + (j + 1));
                    if (val.Substring(0, 1).Equals("="))
                    {
                        this.SetValue(i, j, val.Substring(1, val.Length - 1));
                    }

                    else
                        this.SetValue(i, j, GetContentEvent(character + "" + (j + 1)));
                }
            }
        }
    }
}
