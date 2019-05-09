// Andrew Fryzel

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    //Declaration of variables
    public interface ISpreadsheetView

    {
        /// <summary>
        /// Events for the spreadsheet
        /// </summary>
        event Action<String> OpenEvent;
        event Action CloseEvent;
        event Action NewEvent;
        event Action HelpEvent;
        event Action<String> SaveEvent;
        event Action<String> ContentEvent;
        event Func<String, string> GetContentEvent;

        /// <summary>
        /// Displays an messages or "title" values
        /// </summary>
        String Title { set; }

        /// <summary>
        /// Handlers
        /// </summary>
        void OpenNew();
        void DoClose();
        void DoHelp();
        void GetSelection(out int col, out int row);
        void SetNewSelection(int col, int row);
        void SetValue(int col, int row, string value);
        void OpenSS();
    }
}
