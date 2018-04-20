using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using System.Threading;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;

namespace Selenium_Testing_Asset
{
    public class Bsp_Manager
    {
        public IWebDriver driver;
        public IWebElement All_companys;

        /* Bsp Log 로직 */
        public void Bsp_Connection_Chrome(string site_url, string userid, string pwd, string downloadpath)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", downloadpath);

            driver = new ChromeDriver(options);

            System.Drawing.Size windowssize_def = driver.Manage().Window.Size;

            System.Drawing.Size windowssize = new System.Drawing.Size(1500, windowssize_def.Height);

            driver.Manage().Window.Size = windowssize;
            driver.Url = site_url;
            driver.Navigate().Refresh();


            driver.FindElement(By.Id("username")).SendKeys(userid);
            driver.FindElement(By.Id("password")).SendKeys(pwd);
            driver.FindElement(By.ClassName("btn-login")).Click();

            Thread.Sleep(2000);
        }

        /* 회사명 검색이후 접속 까지의 단계 */
        public void Bsp_Select_Company(string selectcompany, string menname)
        {
            driver.FindElement(By.XPath(".//div[@id='select-company']/button")).Click();
            Thread.Sleep(1500);

            // 회사 검색
            IWebElement dr = driver.FindElement(By.XPath(".//div[@class='list-container']/fieldset/input"));
            dr.SendKeys(selectcompany);

            Thread.Sleep(2500);
            // 검색한 회사 클릭해서 접속
            //driver.FindElement(By.XPath(".//ul[@class='list-custom-select company']")).Click();
            Bsp_find_Company_Click(selectcompany);
            Thread.Sleep(2500);
            if (menname == "DASHBOARD")
            {
                driver.FindElement(By.XPath(".//nav[@class='sub-menus']/ul/li")).Click();
                Thread.Sleep(1500);
            }
            else
            {
                IReadOnlyCollection<IWebElement> sub = driver.FindElements(By.XPath(".//nav[@class='sub-menus']/ul/li"));
                foreach (IWebElement item in sub)
                {
                    Thread.Sleep(1000);
                    if (item.Text == menname)
                    {
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("window.scrollBy(0,-1000)", "");
                        Thread.Sleep(500);
                        item.Click();
                        Thread.Sleep(1500);
                    }
                }
            }
        }

        /*
         * 회사명 검색 이후 > 검색한 회사에 대해서 Click !!
         * 동일한 회사 명을 포함하고 있을 경우를 대비하여 완벽하게 일치하는 회사에 대해서만 Click 이벤트 발생
         */
        public void Bsp_find_Company_Click(string selectcompany)
        {
            IWebElement ullist = driver.FindElement(By.XPath(".//ul[@class='list-custom-select company']"));
            IReadOnlyCollection<IWebElement> search_Mark = ullist.FindElements(By.XPath(".//li/button/mark"));
            foreach (IWebElement item in search_Mark)
            {
                if (item.Text == selectcompany)
                {
                    item.Click();
                    break;
                }
            }
        }

        /* 
         * Asset을 사용하는 회사들에 대해서 All_companys 변수에 회사 list 를 저장 
         * 프로그램이 실행되고 최초 한번만 저장
        */
        public void Bsp_Company_list()
        {
            driver.FindElement(By.XPath(".//div[@id='select-company']/button")).Click();
            Thread.Sleep(1500);
            if (All_companys == null)
            {
                All_companys = driver.FindElement(By.XPath(".//ul[@class='list-custom-select company']"));
            }

        }


        /* FindElements 에 대해서 xpath만 입력 받아 IWebElement 로 Return */
        public IWebElement findelementitem(string xpath)
        {
            IWebElement dr = driver.FindElement(By.XPath(xpath));
            return dr;
        }

        /* FindElements 에 대해서 xpath만 입력 받아 ReadOnlyCollection<IWebElement> 로 Return */
        public ReadOnlyCollection<IWebElement> findelementitems(string xpath)
        {
            ReadOnlyCollection<IWebElement> resultitems = driver.FindElements(By.XPath(xpath));
            return resultitems;
        }

        /* Company select box click > 입력받은 회사명으로 input field 에 입력 */
        public void select_company(string selectcompany)
        {
            driver.FindElement(By.XPath(".//div[@id='select-company']/button")).Click();
            Thread.Sleep(1500);

            IWebElement dr = driver.FindElement(By.XPath(".//div[@class='list-container']/fieldset/input"));
            dr.SendKeys(selectcompany);
            Thread.Sleep(2500);
        }
    }
}
