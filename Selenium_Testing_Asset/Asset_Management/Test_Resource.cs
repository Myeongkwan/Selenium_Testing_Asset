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
using System.Data;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Data.OleDb;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Selenium_Testing_Asset.Asset_Management
{
    public class Test_Resource
    {
        Bsp_Manager bspm = new Bsp_Manager();
        DBManager dbmanager = new DBManager();
        LogManager logman = new LogManager();

        public void Connection_Dashboard()
        {
            bspm.Bsp_Connection_Chrome("https://asset.bespinglobal.com", "myeongkwan.kim@bespinglobal.com", "pass@word!01", @"C:\Users\myeongkwan.kim\Downloads");
        }

        public void Connection_Dashboard(string url, string userid, string pwd, string downpath)
        {
            bspm.Bsp_Connection_Chrome(url, userid, pwd, downpath);
        }

        public DataSet Select_Companys(string TaskID)
        {
            string querystring = @"exec prc_Select_Company_Used_Asset " + string.Format("'{0}'", TaskID);
            DataSet ds = dbmanager.ExecuteDataQuery(querystring, "Asset_Management");

            return ds;
        }

        public void deleteTable(string tablename, string company_name)
        {
            string isexistqry = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tablename);
            DataSet ds = dbmanager.ExecuteDataQuery(isexistqry, "Asset_Management");
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string Deletetb = string.Format("Delete from {0} where company_name = '{1}' ", tablename, company_name);
                    dbmanager.ExecuteDataQuery(Deletetb, "Asset_Management");
                }
            }
        }

        public void deletecompanydata(string company_name)
        {
            string qry = string.Format("delete from [Asset_Management].[dbo].[TB_Resource_instance] where company_name ='{0}'", company_name);
            dbmanager.ExecuteDataQuery(qry, "Asset_Management");
        }

        public void All_company_total_summary_ds(DataSet companylist, string taskno)
        {
            bspm.Bsp_Company_list();

            for (int i = 0; i < companylist.Tables[0].Rows.Count; i++)
            {
                DataRow dr = companylist.Tables[0].Rows[i];
                string company_name = dr["company_name"].ToString();
                bspm.Bsp_Select_Company(company_name, "RESOURCE");
                Thread.Sleep(6000);
                logman.AppendLogFile(taskno, string.Format("{0}.{1} Testing Start", i, company_name));
                deletecompanydata(company_name);
                Asset_Resource(company_name);

                logman.AppendLogFile(taskno, string.Format("===> {0}.{1} : PASS", i, company_name));
                //logmanager.AppendLogFile(string.Format("{0}.{1} : PASS", i, company_name));

                string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskno, company_name, "pass");
                dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management");

                dbmanager.ExecuteDataQuery(string.Format("exec prc_Task_Status_Update '{0}'", taskno), "Asset_Management");
            }
        }

        public void Asset_Resource(string company_name)
        {
            Thread.Sleep(1000);

            bool enable = false;
            ReadOnlyCollection<IWebElement> Resources = bspm.findelementitems(".//dl[@class='resource-product']");
            foreach (IWebElement resourceitem in Resources)
            {
                int vendercnt = 0;
                ReadOnlyCollection<IWebElement> ul_list = resourceitem.FindElements(By.XPath(".//ul[@class='list-resource-item']"));
                foreach (IWebElement li_list in ul_list)
                {
                    string vendor = resourceitem.FindElements(By.XPath(".//span[@class='vendor']"))[vendercnt].Text;
                    string ritem = resourceitem.FindElements(By.XPath(".//p[@class='name']"))[vendercnt].Text;

                    ReadOnlyCollection<IWebElement> li = li_list.FindElements(By.XPath(".//li/button"));
                    foreach (IWebElement item in li)
                    {
                        string val = string.Empty;
                        string totalvalue = string.Empty;

                        if (item.GetAttribute("disabled") == null)
                            enable = true;
                        else
                            enable = false;

                        string key = item.FindElement(By.XPath(".//div[@class='key']")).Text;

                        ReadOnlyCollection<IWebElement> tvalues = item.FindElements(By.XPath(".//div[@class='value']"));
                        ReadOnlyCollection<IWebElement> value = item.FindElements(By.XPath(".//div[@class='value']/span"));
                        if (value.Count > 0)
                        {
                            val = value[0].Text;
                        }

                        if (tvalues.Count > 0)
                        {
                            if (val != string.Empty)
                            {
                                string[] cnt = tvalues[0].Text.Replace(val, val + "/").Split('/');
                                //totalvalue = value[0].Text.Replace(val, val+"/");
                                if (cnt.Length > 2)
                                {
                                    totalvalue = cnt[1] + cnt[2];
                                }
                                else
                                    totalvalue = cnt[1];
                            }
                            else
                                totalvalue = tvalues[0].Text;
                        }

                        dbmanager.ExecuteDataQuery(string.Format(@"exec prc_Resource_instance_I '{0}','{1}','{2}','{3}','{4}','{5}','{6}'
                                                        ", company_name, vendor, ritem, key, val, totalvalue, enable), "Asset_Management");

                    }
                    vendercnt++;
                }
            }
            //Asset_Result_Dashboard_resource(company_name);
        }

        public void Asset_Result_Dashboard_resource_Auto()
        {
            string str = string.Format(@"SELECT [company_name]
                              FROM [Asset_Management].[dbo].[TB_Dashboard_Resources]
                              group by company_name 
                              order by company_name Asc");
            DataSet ds = dbmanager.ExecuteDataQuery(str, "Asset_Management");
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    Asset_Result_Dashboard_resource(item["company_name"].ToString());
                }
            }
        }

        public void Asset_Result_Dashboard_resource(string company_name)
        {
            string qry = string.Format("Select * from [dbo].[TB_Dashboard_Resources] Where company_name = '{0}'", company_name);
            DataSet Dashboard_Resource_Set = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            foreach (DataRow item in Dashboard_Resource_Set.Tables[0].Rows)
            {
                string d_resourcename = item["Resource_Name"].ToString();
                string d_resourcecnt = item["Resource_Cnt"].ToString();
                string d_servicetype = item["cloud_type"].ToString();

                //string r_resource = check_resource_name_Dash(d_resourcename, d_servicetype);
                DataSet ds = check_resource_name_Dash(d_resourcename, d_servicetype);
                string r_resourcename = string.Empty;
                string r_resourcecnt = string.Empty;
                string r_resourceenable = string.Empty;
                string isPass = "FAIL";
                if (ds.Tables[0].Rows.Count > 1)
                {
                    int cccc = 10;
                }
                string r_resource = string.Empty;
                int totalval = 0;
                foreach (DataRow resource in ds.Tables[0].Rows)
                {
                    r_resource = resource["resource_resource"].ToString();
                    if (r_resource != string.Empty)
                    {
                        string[] resource_check = r_resource.Split('|');

                        if (resource_check.Length > 1)
                        {
                            string re_in = string.Format("select * from [dbo].[TB_Resource_instance] Where company_name = '{0}' and resource_type = '{1}' and resource_name = '{2}' and service_type = '{3}'",
                               company_name, resource_check[0], resource_check[1], d_servicetype);
                            DataSet Resource_Set = dbmanager.ExecuteDataQuery(re_in, "Asset_Management");
                            DataRow resource_row = Resource_Set.Tables[0].Rows[0];
                            r_resourcename = r_resource;
                            //r_resourcecnt = resource_row["total_value"].ToString();
                            object _val = resource_row["total_value"];
                            if (_val == "")
                            {
                                _val = 0;
                            }

                            totalval = totalval + Convert.ToInt32(_val);
                            r_resourcecnt = totalval.ToString();
                            r_resourceenable = resource_row["enabled"].ToString();
                        }
                        else
                        {

                            string re_in = string.Format("select * from [dbo].[TB_Resource_instance] Where company_name = '{0}' and resource_type = '{1}' and enabled = 'True' and service_type = '{2}'",
                               company_name, resource_check[0], d_servicetype);
                            DataSet Resource_Set = dbmanager.ExecuteDataQuery(re_in, "Asset_Management");
                            for (int i = 0; i < Resource_Set.Tables[0].Rows.Count; i++)
                            {
                                DataRow resource_row = Resource_Set.Tables[0].Rows[i];
                                object _val = resource_row["total_value"];
                                if (_val == "")
                                {
                                    _val = 0;
                                }
                                totalval = totalval + Convert.ToInt32(_val);
                            }
                            //DataRow resource_row = Resource_Set.Tables[0].Rows[0];
                            r_resourcename = r_resource;
                            r_resourcecnt = totalval.ToString();
                            r_resourceenable = "True";
                        }
                    }
                }


                if (r_resourceenable == "True")
                {
                    isPass = "PASS";
                }
                if (d_resourcecnt == r_resourcecnt)
                {
                    isPass = "PASS";
                }
                else
                {
                    isPass = "FAIL";
                }

                string insert_Data_qry = string.Format("exec prc_Asset_Dashboard_Resource_Auto_Testing_I '{0}','{1}','{2}','{3}','{4}','{5}','{6}'"
                    , company_name
                    , d_resourcename
                    , d_resourcecnt
                    , r_resourcename
                    , r_resourcecnt
                    , r_resourceenable
                    , isPass);
                dbmanager.ExecuteDataQuery(insert_Data_qry, "Asset_Management");
            }

        }
        /*
        public string check_resource_name_Dash(string resource_name, string service_type)
        {
            string result_Name = string.Empty;
            string qry = string.Format("select * from [dbo].[TB_Asset_Resource_Data] where [dashboard_resource] ='{0}' and service_type='{1}'", resource_name, service_type);
            DataSet ResourceName_Set = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            if (ResourceName_Set.Tables.Count > 0)
            {
                if (ResourceName_Set.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ResourceName_Set.Tables[0].Rows[0];
                    result_Name = dr["resource_resource"].ToString();
                }
            }
            return result_Name;
        }
        */
        public DataSet check_resource_name_Dash(string resource_name, string service_type)
        {
            string result_Name = string.Empty;
            string qry = string.Format("select * from [dbo].[TB_Asset_Resource_Data] where [dashboard_resource] ='{0}' and service_type='{1}'", resource_name, service_type);
            DataSet ResourceName_Set = dbmanager.ExecuteDataQuery(qry, "Asset_Management");

            return ResourceName_Set;
        }

        public void Asset_Resource_Process(string ID, string url, string UserID, string Pwd, string downloadpath)
        {
            logman.AppendLogFile(ID, string.Format("Process START"));
            logman.AppendLogFile(ID, string.Format("Task ID : {0}", ID));
            string csvdownloadpath = string.Empty;
            string taskID = ID;
            Connection_Dashboard(url, UserID, Pwd, downloadpath);
            bspm.Bsp_Company_list();

            All_company_total_summary_ds(Select_Companys(taskID), taskID);
        }
    }
}
