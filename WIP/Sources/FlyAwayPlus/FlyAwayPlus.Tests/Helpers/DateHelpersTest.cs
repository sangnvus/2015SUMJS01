using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlyAwayPlus.Helpers;

namespace FlyAwayPlus.Tests.Helpers
{
    [TestClass]
    public class DateHelpersTest
    {
        [TestMethod]
        public void TestDisplayRealtime1()
        {
            /*
             * seconds ago
             */
            DateTime now = DateTime.Now.AddSeconds(-29);


            string date = now.ToString(FapConstants.DatetimeFormat);
            string result = DateHelpers.displayRealtime(date);
            string expected = "29 seconds ago";

            Assert.IsTrue(expected.CompareTo(result) == 0);
        }

        [TestMethod]
        public void TestDisplayRealtime2()
        {
            /*
             * minutes ago
             */
            DateTime now = DateTime.Now.AddMinutes(-28);


            string date = now.ToString(FapConstants.DatetimeFormat);
            string result = DateHelpers.displayRealtime(date);
            string expected = "28 minutes ago";

            Assert.IsTrue(expected.CompareTo(result) == 0);
        }

        [TestMethod]
        public void TestDisplayRealtime3()
        {
            /*
             * hours ago
             */
            DateTime now = DateTime.Now.AddHours(-6);


            string date = now.ToString(FapConstants.DatetimeFormat);
            string result = DateHelpers.displayRealtime(date);
            string expected = "6 hours ago";

            Assert.IsTrue(expected.CompareTo(result) == 0);
        }

        [TestMethod]
        public void TestDisplayRealtime4()
        {
            /*
             * yesterday
             */
            DateTime now = DateTime.Now.AddHours(-3).AddDays(-1);


            string date = now.ToString(FapConstants.DatetimeFormat);
            string result = DateHelpers.displayRealtime(date);
            string expected = now.ToString("HH:mm") + " yesterday";

            Assert.IsTrue(expected.CompareTo(result) == 0);
        }
    }
}
