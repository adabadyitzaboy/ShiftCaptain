using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

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

        public String EncodeVersionName(String VersionName)
        {
            if (String.IsNullOrEmpty(VersionName))
            {
                return String.Empty;
            }
            return VersionName.Replace(" ", "_");
        }
        //Gets the submit button for this specific page based on form action link
        public By GetSubmitButton()
        {
            var uri = new Uri(Driver.Url);
            return By.CssSelector("form[action='" + uri.PathAndQuery + "'] input[type='submit']");
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
        public bool ElementExists(IWebElement element, By by)
        {
            try
            {
                element.FindElement(by);
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
            By by = By.CssSelector("option[value='" + value + "']");
            if(!ElementExists(element, by)){
                return false;
            }
            var option = element.FindElement(by);
            bool selected;
            if (bool.TryParse(option.GetAttribute("selected"), out selected) && selected)
            {
                return false;
            }
            new OpenQA.Selenium.Interactions.Actions(Driver).MoveToElement(element).Perform();
            element.Click();
            option.Click();
            if (GetDropDownValue(element) != value)
            {
                _ExecJavascript(String.Format("function(element){$(element).val({0});};", value), element);
            }
            else
            {
                element.Click();//close the dropdown
            }
            //ClickElement(element); 
            //ClickElement(option);
            return true;
        }

        public string GetDropDownValue(IWebElement element)
        {
            return element.GetAttribute("value");
        }

        public void ClickElement(IWebElement element)
        {
            new OpenQA.Selenium.Interactions.Actions(Driver).MoveToElement(element).Click(element).Perform();
        }
        public void RemoveFixedHeader()
        {
            AddCss("header .content-wrapper { position: initial; }");
        }
        public bool AddCss(String css, String id = null)
        {
            if (String.IsNullOrEmpty(id))
            {
                id = DateTime.Now.Ticks.ToString();
            }
            var obj = _ExecJavascript(String.Format("return $('<style id=\"{0}\" type=\"text/css\">{1}</style>').appendTo('html > head').length > 0;", id, css));
            bool rtn;
            return bool.TryParse(obj.ToString(), out rtn) && rtn;
        }
        private object _ExecJavascript(String script, params object[] args)
        {
            try
            {
                return ((IJavaScriptExecutor)Driver).ExecuteScript(script, args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return "";
        }
        public bool _ExecJavascriptBool(String script, params object[] args)
        {
            var obj = _ExecJavascript(script, args);
            bool rtn;
            return bool.TryParse(obj.ToString(), out rtn) && rtn;
        }
        #region ShiftDuration Helper
        public bool SetDurationDropDownValue(string value)
        {
            By by = By.Id("ShiftDuration");
            if (!ElementExists(by)) {
                return false;
            }
            var dd = Driver.FindElement(by);
            dd.SendKeys(value);
            Thread.Sleep(1000);
            return GetDropDownValue(dd) == value;
        }
        #endregion
        public bool DragElement(IWebElement dragElement, IWebElement dropElement, double dragSeconds = .5)
        {
            var dragSuccess = _ExecJavascriptBool("return $(arguments[0]).trigger({type: 'mousedown', which: 1}) != null; ", dragElement);
            if (dragSuccess)
            {
                if (dragSeconds > 0)
                {
                    Thread.Sleep((int)(dragSeconds * 1000));
                }
                var dropSuccess = _ExecJavascriptBool(@"return (function(dropElement){
                                                    $dropElement = $(dropElement); 
                                                    var position = $dropElement.position();
                                                    if($dropElement.offsetParent() && $dropElement.offsetParent().position().top > position.top){
                                                       position.top += $dropElement.offsetParent().position().top;
                                                    }
                                                    return $dropElement.trigger({ type: 'mouseup', clientX: position.left, clientY: position.top}) != null;
                                                })(arguments[0]);", dropElement);
                return dragSuccess && dropSuccess;
            }
            return false;
        }
    }
    public class ByExists
    {
        public By by {get; set;}
        public bool exists { get; set; }
    }
}
