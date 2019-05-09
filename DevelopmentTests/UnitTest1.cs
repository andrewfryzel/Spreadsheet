using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetGUI;
using SS;

namespace DevelopmentTests
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Testing close method
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.FireCloseEvent();
            Assert.IsTrue(stub.CalledDoClose);
        }

        /// <summary>
        /// Testing open method
        /// </summary>
        [TestMethod]
        public void TestMethod2()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.FireNewEvent();
            Assert.IsTrue(stub.CalledOpenNew);
        }

        /// <summary>
        /// Testing help method
        /// </summary>
        [TestMethod]
        public void TestMethod3()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.FireHelpEvent();
            Assert.IsTrue(stub.CalledDoHelp);
        }

        /// <summary>
        /// Testing simple input
        /// </summary>
        [TestMethod]
        public void TestMethod4()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(1, 1);
            stub.FireContentEvent("=2+2");
            object value = stub.GetValue("B2");
            string val = value.ToString();
            double.TryParse(val, out double result);
            Assert.AreEqual(4, result, .00000000001);
        }

        /// <summary>
        /// Testing adding two cells
        /// </summary>
        [TestMethod]
        public void TestMethod5()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(1, 1);
            stub.FireContentEvent("=2+2");
            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=3+3");
            stub.SetNewSelection(1, 0);
            stub.FireContentEvent("=B2+A1");
            object value = stub.GetValue("B1");
            string val = value.ToString();
            double.TryParse(val, out double result);
            Assert.AreEqual(10, result, .00000000001);
        }

        /// <summary>
        /// Testing changes in a depended upon cell
        /// </summary>
        [TestMethod]
        public void TestMethod6()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(1, 1);
            stub.FireContentEvent("=2+5");
            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=3-2");
            stub.SetNewSelection(1, 0);
            stub.FireContentEvent("=B2+A1");
            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=5+5");
            object value = stub.GetValue("B1");
            string val = value.ToString();
            double.TryParse(val, out double result);
            Assert.AreEqual(17, result, .00000000001);
        }

        /// <summary>
        /// Testing sending a double in
        /// </summary>
        [TestMethod]
        public void TestMethod7()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(1, 1);
            stub.FireContentEvent("=2.3");
            object value = stub.GetValue("B2");
            string val = value.ToString();
            double.TryParse(val, out double result);
            Assert.AreEqual(2.3, result, .00000000001);
        }

        /// <summary>
        /// Testing invalid content
        /// </summary>
        [TestMethod]
        public void TestMethod8()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(1, 1);
            stub.FireContentEvent("=x");
            object value = stub.GetValue("B2");
            string val = value.ToString();
            Assert.AreEqual("Formula Error", val);
        }

        /// <summary>
        /// Testing changing content
        /// </summary>
        [TestMethod]
        public void TestMethod9()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(1, 1);
            stub.FireContentEvent("=2");
            stub.SetNewSelection(2, 2);
            stub.FireContentEvent("=1");
            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=b2+c3");
            string val = stub.FireGetContentEvent("A1");
            Assert.AreEqual("=B2+C3", val);
        }

        /// <summary>
        /// Testing help event
        /// </summary>
        [TestMethod]
        public void TestMethod10()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.FireHelpEvent();
            Assert.IsTrue(stub.CalledDoHelp);
        }

        /// <summary>
        /// Testing save event
        /// </summary>
        [TestMethod]
        public void TestMethod11()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=2");
            stub.FireSaveEvent("test.ss");
            Assert.AreEqual(stub.Title, "test.ss");
        }

        /// <summary>
        /// Testing open
        /// </summary>
        [TestMethod]
        public void TestMethod12()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);

            SpreadsheetViewStub stub1 = new SpreadsheetViewStub();
            String fileName = "test.ss";
            Spreadsheet ss = new Spreadsheet();
            Controller contro1l = new Controller(stub1, ss, fileName);

            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=2");
            stub.FireSaveEvent("test.ss");
            Assert.AreEqual(stub.Title.ToString(), "test.ss");

            stub1.FireOpenEvent(fileName);
            Assert.AreEqual(stub1.Title.ToString(), "test.ss");
        }

        /// <summary>
        /// Testing open event fail
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.NullReferenceException))]
        public void testOFail()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.FireOpenEvent("testfail.ss");
            stub.Title.ToString();
        }

        /// <summary>
        /// Testing save event fail
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.NullReferenceException))]
        public void testSFail()
        {
            SpreadsheetViewStub stub = new SpreadsheetViewStub();
            Controller control = new Controller(stub);
            stub.SetNewSelection(0, 0);
            stub.FireContentEvent("=1");
            stub.FireSaveEvent("");
            stub.Title.ToString();
        }
    }
}
