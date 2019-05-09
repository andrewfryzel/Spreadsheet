// Andrew Fryzel
using SS;
using SSGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    //MODEL
    public partial class Form1 : Form, ISpreadsheetView
    {
        private Spreadsheet spreadsheet;

        //Displays an messages or "title" values
        public String Title
        {
            set
            {
                MessageBox.Show(value);
            }
        }

        /// <summary>
        /// Fired when a close action is requested.
        /// </summary>
        public event Action CloseEvent;

        /// <summary>
        /// Fired when a new action is requested.
        /// </summary>
        /// //2
        public event Action NewEvent;

        /// <summary>
        /// Fired when a help action is requested.
        /// </summary>
        public event Action HelpEvent;

        /// <summary>
        /// Fired when an open action is requested.
        /// </summary>
        public event Action<String> OpenEvent;

        /// <summary>
        /// Fired when a savep action is requested.
        /// </summary>
        public event Action<String> SaveEvent;

        /// <summary>
        /// Fired when action is requested for the contents of the cell.
        /// </summary>
        public event Func<String, String> GetContentEvent;

        /// <summary>
        /// Fired when a content (is changed) action is requested.
        /// </summary>
        public event Action<String> ContentEvent;


        /// <summary>
        /// Constructor for Form1 
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            spreadsheet = new Spreadsheet();

            //Actually uses the displaySelection
            spreadsheetPanel1.SelectionChanged += displaySelection;
            spreadsheetPanel1.SetSelection(0, 0);

            textBox1.Text = "A1";
            textBox2.Text = "=";

        }
        /// <summary>
        /// Displays the selection of the cell including the value and contents.
        /// </summary>
        /// <param name="ss"></param>
        private void displaySelection(SpreadsheetPanel ss)
        {
            int row, col;
            String val;

            // Get the column and row value
            ss.GetSelection(out col, out row);

            // Change the column and row value to a function 'A1' value.
            ss.GetValue(col, row, out val);
            col = col + 65;
            row = row + 1;
            String ans;
            ans = (char)col + "" + row;

            // Assign the 'CharNumber' cell name to textBox1
            textBox1.Text = ans;

            // Handle textBox2
            if (GetContentEvent != null)
            {
                textBox2.Text = GetContentEvent(ans);
                textBox2.SelectionStart = textBox2.Text.Length;
                textBox2.SelectionLength = 0;
            }
        }

        /// <summary>
        /// Gets the selected cell 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void GetSelection(out int col, out int row)
        {
            spreadsheetPanel1.GetSelection(out col, out row);
        }

        /// <summary>
        /// Sets the selected cell
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void SetNewSelection(int col, int row)
        {
            spreadsheetPanel1.SetSelection(col, row);
        }

        /// <summary>
        /// Sets the cell value, not the contents
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="c"></param>
        public void SetValue(int col, int row, string c)
        {
            spreadsheetPanel1.SetValue(col, row, c);
        }

        /// <summary>
        /// Represent the 'File' menu that contains options for 'Open', 'New', 'Save', 
        /// 'Help', and 'Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Represents the 'New' button within the file menu strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NewEvent != null)
            {
                NewEvent();
            }
        }

        /// <summary>
        /// Opens a new window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenNew()
        {
            SpreadsheetApplicationContext.GetContext().RunNew();
        }

        /// <summary>
        /// Represents the 'Close' button within the file menu strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CloseEvent != null)
            {
                CloseEvent();
            }
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoClose()
        {
            this.Close();
        }

        /// <summary>
        /// Represents the 'Help' button within the file menu strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (HelpEvent != null)
            {
                HelpEvent();
            }
        }

        /// <summary>
        /// Displays the help message
        /// </summary>
        public void DoHelp()
        {
            //TODO
            MessageBox.Show("Click on a cell, or use the arrow keys, to select a cell. \n Enter a value into the \"Value\" box and hit the 'Enter' key or click the 'Enter' button.");
        }
        /// <summary>
        /// Represents the 'Open' button within the file menu strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string openFile = "";
            saveFileDialog.InitialDirectory = "C:/Users";
            saveFileDialog.Filter = "Spreadsheet File (*.ss)|*.ss|All Files (*.*)|*.*";
            DialogResult result = fileDialog.ShowDialog();

            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if (OpenEvent != null)
                {
                    openFile = fileDialog.FileName;
                    OpenEvent(openFile);
                }
            }

        }

        /// <summary>
        /// Represents the 'Save' button within the file menu strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.InitialDirectory = "C:/Users";
            saveFileDialog.FileName = "";
            saveFileDialog.Filter = "Spreadsheet File (*.ss)|*.ss|All Files (*.*)|*.*";

            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if (SaveEvent != null)
                {
                    SaveEvent(saveFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// Moves the selected cell depending on which arrow key is pressed. 
        /// Updates spreadsheet to refelect the changes in selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyDown(object sender, KeyEventArgs e)

        {
            int col;
            int row;

            spreadsheetPanel1.GetSelection(out col, out row);
            string val;
            String cell;

            if (e.KeyCode == Keys.Right)
            {
                col++;
                spreadsheetPanel1.SetSelection(col, row);
                e.Handled = true;

                spreadsheetPanel1.GetValue(col, row, out val);
                textBox2.Text = "=" + val;

                String col2;

                // Can use a Regex but IDC tbh
                List<String> list = new List<String> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", };
                if (col >= 0 && col <= 25)
                {
                    col2 = list[col];
                }
                else
                {
                    col2 = "Z";
                }

                row++;

                cell = col2 + "" + row;
                textBox1.Text = cell;


            }
            if (e.KeyCode == Keys.Left)
            {
                col--;
                spreadsheetPanel1.SetSelection(col, row); e.Handled = true;

                e.Handled = true;

                spreadsheetPanel1.GetValue(col, row, out val);
                textBox2.Text = "=" + val;
                String col2;

                // Can use a Regex but IDC tbh
                List<String> list = new List<String> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", };
                if (col >= 0 && col <= 25)
                {
                    col2 = list[col];
                }
                else
                {
                    col2 = "A";
                }
                row++;

                cell = col2 + "" + row;
                textBox1.Text = cell;
            }
            if (e.KeyCode == Keys.Up)
            {

                if (row > 0)
                {
                    row--;
                }

                spreadsheetPanel1.SetSelection(col, row); e.Handled = true;

                e.Handled = true;

                spreadsheetPanel1.GetValue(col, row, out val);
                textBox2.Text = "=" + val;

                // Can use a Regex but IDC tbh
                List<String> list = new List<String> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", };
                String col2 = list[col];

                row++;

                cell = col2 + "" + row;
                textBox1.Text = cell;
            }
            if (e.KeyCode == Keys.Down)
            {
                if (row < 98)
                {
                    row++;
                }

                spreadsheetPanel1.SetSelection(col, row); e.Handled = true;

                e.Handled = true;

                spreadsheetPanel1.GetValue(col, row, out val);
                textBox2.Text = "=" + val;

                // Can use a Regex but IDC tbh
                List<String> list = new List<String> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", };
                String col2 = list[col];
                row++;

                cell = col2 + "" + row;
                textBox1.Text = cell;
            }
        }

        /// <summary>
        /// Text box 2 represents the value of the cell.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_TextChanged(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                int col, row;

                spreadsheetPanel1.GetSelection(out col, out row);

                if (ContentEvent != null)
                {
                    ContentEvent(textBox2.Text);
                    spreadsheetPanel1.SetSelection(col, row);
                }

                //This mutes the dinging noise when pressing Enter
                e.Handled = true;

                ActiveControl = spreadsheetPanel1;
            }
        }

        /// <summary>
        /// Handles physically clicking the 'Enter' button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (ContentEvent != null)
                ContentEvent(textBox2.Text);
        }

        /// <summary>
        /// The purpose of the superclass <see cref="Panel"/> is to be a
        /// container. As such, it was not designed to be available for
        /// focus when selected by mouse; this way it does not steal the
        /// focus from a child control. To change that behavior, we force
        /// this <see cref="SpreadsheetPanel"/> class to take focus when
        /// selected.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
            ActiveControl = spreadsheetPanel1;
        }

        /// <summary>
        /// Opens a spreadsheet from file. Fills in the cells with the appropriate value
        /// </summary>
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
                        spreadsheetPanel1.SetValue(i, j, val.Substring(1, val.Length - 1));
                    }

                    else
                        spreadsheetPanel1.SetValue(i, j, GetContentEvent(character + "" + (j + 1)));
                }
            }
        }
    }
}
