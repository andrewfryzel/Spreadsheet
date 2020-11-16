# Spreadsheet
### A C# implementation of Microsoft Excel spreadsheets

Each cell can contain a String, a double or a Formula.

Click on a cell, input a data value into the box at the top to submit. 

Example: if B2 = '3' and C2= '2' then typing the formula 'B2 + C2=' into box D2 will display the result '5' in box D2

**Dependancy Graph -** represents a graph of dependencies. This is what will determine if one cell changes how that can effect cells that are 
either a dependee or dependent of that cell.

**Formula -** determines what a valid formula is for a cell

**Spreadsheet GUI -** the front end visual representation of the Spreadsheet

**Spreadsheet Panel -** A helper class that represents the visual cells of a spreadsheet
