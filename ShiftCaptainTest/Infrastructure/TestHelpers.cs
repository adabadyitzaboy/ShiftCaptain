using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Configuration;

namespace ShiftCaptainTest.Infrastructure
{
    public class TestHelpers : Base
    {
        public Double ElapsedTimeThreshold = Double.Parse(ConfigurationManager.AppSettings["ElapsedTimeThreshold"]);
        public Boolean LogLoadTimes = Boolean.Parse(ConfigurationManager.AppSettings["LogLoadTimes"]);
        public bool IsLoggedIn()
        {
            return true;
        }
        public void ClickAndWaitForNextPage(IWebElement element, String nextPage)
        {
            var currentUrl = Driver.Url;
            var start = DateTime.Now;
            element.Click();
            WaitForNextPage(currentUrl);
            var ellapsed = (DateTime.Now - start).TotalSeconds;
            Assert.IsTrue(ellapsed < ElapsedTimeThreshold, String.Format("{0} exceeded Timeout threshold - {1}", nextPage, ellapsed));
            if (LogLoadTimes)
            {
                Logger.InfoFormat("{0}\t{1}\t{2}\t{3}\t{4}", DateTime.Now.ToString("MM/dd/yy H:mm:ss"), nextPage, ellapsed, DriverType, IsLoggedIn());
            }
        }
        public void ClickAndWaitForEitherElement(IWebElement element, By by1, By by2, bool element1Exists = true, bool element2Exists = true)
        {
            element.Click();
            _WaitTill().Until(d=>
                (element1Exists && ElementExists(by1)) || 
                (element2Exists && ElementExists(by2)) || 
                (!element1Exists && !ElementExists(by1)) ||
                (!element2Exists && !ElementExists(by2))  
            );
        }
        public void ClickAndWaitForElements(IWebElement element, By by1, By by2, By by3, bool element1Exists = true, bool element2Exists = true, bool element3Exists = true)
        {
            element.Click();
            _WaitTill().Until(d =>
                (element1Exists && ElementExists(by1)) ||
                (element2Exists && ElementExists(by2)) ||
                (element3Exists && ElementExists(by3)) ||
                (!element1Exists && !ElementExists(by1)) ||
                (!element2Exists && !ElementExists(by2)) ||
                (!element3Exists && !ElementExists(by3))
            );
        }
        public bool ElementExists(By by)
        {
            try
            {
                Driver.FindElement(by);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public IWebElement WaitTillAvailable(By by, TimeSpan? timeout = null)
        {
            return _WaitTill(timeout).Until(ExpectedConditions.ElementExists(by));
        }
        public void WaitForNextPage(String currentUrl)
        {
            try
            {
                _WaitTill().Until(d => d.Url != currentUrl);
            }
            catch (Exception ex)
            {
                Logger.InfoFormat("Wait till next page failed.  currentUrl - {0} driver - {1} \r\nstack: {2}", currentUrl, DriverType, ex.StackTrace);
            }
        }
        private WebDriverWait _WaitTill(TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = DefaultTimeout;
            }
            return new WebDriverWait(Driver, (TimeSpan)timeout);
        }
        public void SetTextBoxValue(IWebElement element, String value)
        {
            element.Clear();
            element.SendKeys(value);
        }
    }
}
