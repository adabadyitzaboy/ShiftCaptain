using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftCaptainTest.Infrastructure
{
    [TestClass]
    [DeploymentItem("DataSources/Users.csv", "")]
    [DeploymentItem("chromedriver.exe", "")]
    public class Base
    {
        public IWebDriver Driver;
        public string BaseUrl = ConfigurationManager.AppSettings["BASE_URL"];
        public string DriverType = ConfigurationManager.AppSettings["DriverType"];
        public TimeSpan DefaultTimeout;
        public ILog Logger = LogManager.GetLogger(typeof(Base));
        public ICollection<IDictionary<string, string>> UserTable;
        public ShiftCaptainEntities db = new ShiftCaptainEntities();

        public Base()
        {
            UserTable = new DataParser("Users.csv").Tables["Users"];
        }
    
        [TestInitialize]
        [Timeout(TestTimeout.Infinite)]
        public void Setup()
        {
            DefaultTimeout = TimeSpan.FromSeconds(Int32.Parse(ConfigurationManager.AppSettings["DefaultTimeout"]));
            switch (DriverType)
            {
                case ("ie"):
                    Driver = new InternetExplorerDriver();
                    break;
                case ("internet explorer"):
                    Driver = new InternetExplorerDriver();
                    break;
                case ("firefox"):
                    Driver = new FirefoxDriver();
                    break;
                case ("chrome"):
                    Driver = new ChromeDriver();
                    break;
                case ("safari"):
                    Driver = new SafariDriver();
                    break;
                default:
                    Driver = new FirefoxDriver();
                    break;
            }

            if (Driver.Url != BaseUrl)
            {
                var currentUrl = Driver.Url;
                Driver.Navigate().GoToUrl(BaseUrl);
                new WebDriverWait(Driver, DefaultTimeout).Until(d => d.Url != currentUrl);
            }

        }
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                Driver.Quit();
            }
            catch (Exception)
            { }
        }
    }
}
