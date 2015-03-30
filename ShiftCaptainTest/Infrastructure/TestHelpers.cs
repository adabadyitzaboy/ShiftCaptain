using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;

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
        public void WaitForElement(List<ByExists> byExists, TimeSpan? timeout = null)
        {
            _WaitTill(timeout).Until(d=>
                byExists.Count(b=> 
                    ElementExists(b.by) == b.exists
                ) > 0
            );
        }
        public void ClickAndWaitForElements(IWebElement element, List<ByExists> byExists)
        {
            element.Click();
            WaitForElement(byExists);
            //_WaitTill().Until(d=>
            //    (element1Exists && ElementExists(by1)) || 
            //    (element2Exists && ElementExists(by2)) || 
            //    (!element1Exists && !ElementExists(by1)) ||
            //    (!element2Exists && !ElementExists(by2))  
            //);
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
        public void WaitTillNotAvailable(By by, TimeSpan? timeout = null)
        {
            _WaitTill(timeout).Until(e=>!ElementExists(by));
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
        public bool SetDropDownValue(IWebElement element, String value)
        {
            var option = element.FindElement(By.CssSelector("option[value='" + value + "']"));
            bool selected;
            if (bool.TryParse(option.GetAttribute("selected"), out selected) && selected)
            {
                return false;
            }
            element.Click();
            
            //var action = new OpenQA.Selenium.Interactions.Actions(Driver);
            //action.Click(element)
            //    .Click(option);
            //action.Perform();
            option.Click();
            return true;
        }

    }
    public class ByExists
    {
        public By by {get; set;}
        public bool exists { get; set; }
    }
}
