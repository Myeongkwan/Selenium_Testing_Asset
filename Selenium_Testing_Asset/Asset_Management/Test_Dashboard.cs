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
using System.Management.Automation.Runspaces;
using System.Management.Automation;

namespace Selenium_Testing_Asset.Asset_Management
{
    public class Test_Dashboard
    {
        Bsp_Manager bspm = new Bsp_Manager();
        DBManager dbmanager = new DBManager();
        Dashboard_Item dashboarditem = new Dashboard_Item();
        LogManager logman = new LogManager();
        string csvdownloadpath = string.Empty;

        /* DB에 회사명 등록시 회사명 조회 */
        public DataSet Select_Companys(string TaskID)
        {
            string tset = "";
            string querystring = @"exec prc_Select_Company_Used_Asset " + string.Format("'{0}'", TaskID);
            DataSet ds = dbmanager.ExecuteDataQuery(querystring, "Asset_Management");

            return ds;
        }
        public DataSet Select_Companys_PASS(string TaskID)
        {
            string querystring = @"exec prc_Select_Company_Used_Asset_PASS " + string.Format("'{0}'", TaskID);
            DataSet ds = dbmanager.ExecuteDataQuery(querystring, "Asset_Management");

            return ds;
        }

        public void insert_company_table(string[] All_companys)
        {
            for (int i = 0; i < All_companys.Length; i++)
            {
                string insertquery = string.Format("exec prc_company_info_I '{0}'", All_companys[i]);
                dbmanager.ExecuteDataQuery(insertquery, "Bsp_Management");
            }
        }

        public void Asset_Dashboard_Process(DataSet ds, string ID, string url, string UserID, string Pwd)
        {
            logman.AppendLogFile(ID, string.Format("Process START"));
            logman.AppendLogFile(ID, string.Format("Task ID : {0}", ID));

            Connection_Dashboard(url, UserID, Pwd);
            bspm.Bsp_Company_list();

            string taskID = ID;
            /* 실제 구현 코드 */
            string[] companylist = bspm.All_companys.Text.Replace("\r\n", "|").Split('|');
            insert_company_table(companylist);
            All_company_total_summary_ds(Select_Companys(taskID), taskID);
            Result_Check(ID);
        }

        /* 예비 코드 */
        public void Asset_Dashboard_Process(string ID, string url, string UserID, string Pwd, string downloadpath)
        {
            logman.AppendLogFile(ID, string.Format("Process START"));
            logman.AppendLogFile(ID, string.Format("Task ID : {0}", ID));

            csvdownloadpath = downloadpath;

            Connection_Dashboard(url, UserID, Pwd, csvdownloadpath);
            bspm.Bsp_Company_list();

            string taskID = ID;
            /* 실제 구현 코드 */
            string[] companylist = bspm.All_companys.Text.Replace("\r\n", "|").Split('|');
            insert_company_table(companylist);
            All_company_total_summary_ds(Select_Companys(taskID), taskID);
            Result_Check(ID);
        }

        public void delete_resource_Table(string company_name)
        {
            string qry = string.Format("Delete from TB_Dashboard_Resources where company_name = '{0}'", company_name);
            dbmanager.ExecuteDataQuery(qry, "Asset_Management");
        }

        public void Runpsfile()
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);

            Pipeline pipeline = runspace.CreatePipeline();

        }

        public void All_company_total_summary_ds(DataSet companylist, string taskID)
        {
            bspm.Bsp_Company_list();

            for (int i = 0; i < companylist.Tables[0].Rows.Count; i++)
            {
                DataRow dr = companylist.Tables[0].Rows[i];
                string company_name = dr["company_name"].ToString();
                bspm.Bsp_Select_Company(company_name, "DASHBOARD");
                Thread.Sleep(20000);
                logman.AppendLogFile(taskID, string.Format("{0}.{1} Testing Start", i, company_name));
                total_summary(company_name);
                delete_resource_Table(company_name);
                Resources_check(company_name);
                table_server_db_network(company_name);
                logman.AppendLogFile(taskID, string.Format("===> {0}.{1} : PASS", i, company_name));
                //logmanager.AppendLogFile(string.Format("{0}.{1} : PASS", i, company_name));

                string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskID, company_name, "pass");
                dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management");

                dbmanager.ExecuteDataQuery(string.Format("exec prc_Task_Status_Update '{0}'", taskID), "Asset_Management");
            }

        }

        public void All_company_total_summary(string[] companylist)
        {
            bspm.Bsp_Company_list();

            for (int i = 0; i < companylist.Length; i++)
            {
                bspm.Bsp_Select_Company(companylist[i], "DASHBOARD");
                Thread.Sleep(20000);

                total_summary(companylist[i]);
                Resources_check(companylist[i]);
                table_server_db_network(companylist[i]);
            }

        }

        public void Connection_Dashboard(string url, string userid, string pwd, string downpath)
        {
            bspm.Bsp_Connection_Chrome(url, userid, pwd, downpath);
        }

        public void Connection_Dashboard(string url, string userID, string pwd)
        {

            bspm.Bsp_Connection_Chrome(url, userID, pwd, @"D:\download");
            //bspm.Bsp_Connection_Chrome("https://asset.opsnow.com", "myeongkwan.kim@bespinglobal.com", "pass@word!01");
        }

        public void total_summary(string companyNum)
        {
            ReadOnlyCollection<IWebElement> resultitem = bspm.findelementitems(".//dl[@class='list-count']");
            foreach (IWebElement summarys in resultitem)
            {
                if (summarys.Text.Replace("\r\n", "|").Split('|')[0] == "Total Server")
                {
                    string summaryitems = summarys.Text.Replace("\r\n", "|").Replace("ea", "");

                    string autoscaling_Run = string.Empty;
                    string autoscaling_stop = string.Empty;
                    string autoscaling_etc = string.Empty;

                    if (summaryitems.Split('|')[7].Split(' ').Length < 3)
                    {
                        autoscaling_Run = summaryitems.Split('|')[7].Split(' ')[0];
                        autoscaling_stop = "0";
                        autoscaling_etc = summaryitems.Split('|')[7].Split(' ')[1];
                    }
                    else
                    {
                        autoscaling_Run = summaryitems.Split('|')[7].Split(' ')[0];
                        autoscaling_stop = summaryitems.Split('|')[7].Split(' ')[1];
                        autoscaling_etc = summaryitems.Split('|')[7].Split(' ')[2];
                    }


                    string querystring = string.Format("exec prc_Dashboard_summary_server_I '{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        companyNum
                        , summaryitems.Split('|')[1]
                        , summaryitems.Split('|')[3]
                        , summaryitems.Split('|')[4].Split(' ')[0]
                        , summaryitems.Split('|')[4].Split(' ')[1]
                        , summaryitems.Split('|')[4].Split(' ')[2]
                        , summaryitems.Split('|')[6]
                        , autoscaling_Run
                        , autoscaling_stop
                        , autoscaling_etc
                        );
                    dbmanager.ExecuteDataQuery(querystring, "Asset_Management");
                }
                else if (summarys.Text.Replace("\r\n", "|").Split('|')[0] == "Total Database")
                {
                    string summaryitems = summarys.Text.Replace("\r\n", "|").Replace("ea", "");
                    string querystring = string.Format("exec prc_Dashboard_summary_db_I '{0}',{1},{2},{3},{4},{5}",
                        companyNum
                        , summaryitems.Split('|')[1]
                        , summaryitems.Split('|')[3]
                        , summaryitems.Split('|')[4].Split(' ')[0]
                        , summaryitems.Split('|')[4].Split(' ')[1]
                        , summaryitems.Split('|')[4].Split(' ')[2]
                        );
                    dbmanager.ExecuteDataQuery(querystring, "Asset_Management");
                }

            }
        }

        public void Resources_check(string companyname)
        {

            string total_Resource = string.Empty;
            ReadOnlyCollection<IWebElement> resourcetotal = bspm.findelementitems(".//div[@class='dashboard-item resources']/div[@class='dashboard-item-box']/div/em");

            if (resourcetotal.Count > 0)
            {
                IWebElement resourcetotal_i = bspm.findelementitem(".//div[@class='dashboard-item resources']/div[@class='dashboard-item-box']/div/em");
                total_Resource = resourcetotal_i.Text.Replace("ea", "");
            }

            ReadOnlyCollection<IWebElement> resultitem = bspm.findelementitems(".//ul[@class='list-product-container']/li/div");

            foreach (IWebElement item in resultitem)
            {
                //span
                IWebElement ResourceName = item.FindElement(By.XPath(".//span"));
                //em
                IWebElement ResourceValue = item.FindElement(By.XPath(".//em"));

                string cloud_type = ResourceName.GetAttribute("class").Split(' ')[1];

                string querystring = string.Format("exec prc_Dashboard_Resource_I '{0}',{1},'{2}',{3},'{4}'", companyname, total_Resource, ResourceName.Text, ResourceValue.Text, cloud_type);
                dbmanager.ExecuteDataQuery(querystring, "Asset_Management");
            }
        }

        public string Check_Null_Value(string value)
        {
            string returevalue = string.Empty;
            if (value == string.Empty || value == "" || value == null)
            {
                returevalue = "0";
            }
            else
            {
                returevalue = value;
            }
            return returevalue;
        }

        public void table_server_db_network(string companyname)
        {
            //companyname = "BSP Company";

            //ReadOnlyCollection<IWebElement> resourcetotal = bspm.findelementitems(".//div[@class='float-dashboard-section']/section[@class='dashboard-section']/div/aside");
            ReadOnlyCollection<IWebElement> resourcetotal = bspm.findelementitems(".//div[@class='float-dashboard-section']/section[@class='dashboard-section']");
            foreach (IWebElement item in resourcetotal)
            {
                string title = item.FindElement(By.XPath(".//h2")).Text;
                if (title == "SERVER")
                {
                    ReadOnlyCollection<IWebElement> Serverresource = item.FindElements(By.XPath(".//div/aside/table"));
                    foreach (IWebElement sitem in Serverresource)
                    {
                        string tb_t = sitem.FindElement(By.XPath(".//thead")).Text;
                        if (tb_t == "TOTAL")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "RUNNING SERVER")
                                {
                                    dashboarditem.total_runing_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STOPPED SERVER")
                                {
                                    dashboarditem.total_stopped_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "ETC SERVER")
                                {
                                    dashboarditem.total_etc_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                            }
                            string total_query = string.Format("exec [dbo].[prc_Dashboard_Server_total_I] '{0}',{1},{2},{3}", companyname, dashboarditem.total_runing_svr, dashboarditem.total_stopped_svr, dashboarditem.total_etc_svr);
                            dbmanager.ExecuteDataQuery(total_query, "Asset_Management");
                        }
                        else if (tb_t == "AWS")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "RUNNING SERVER")
                                {
                                    dashboarditem.aws_runing_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STOPPED SERVER")
                                {
                                    dashboarditem.aws_stopped_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "ETC SERVER")
                                {
                                    dashboarditem.aws_etc_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                            }
                            string total_query = string.Format("exec [dbo].[prc_Dashboard_Server_aws_I] '{0}',{1},{2},{3}", companyname, dashboarditem.aws_runing_svr, dashboarditem.aws_stopped_svr, dashboarditem.aws_etc_svr);
                            dbmanager.ExecuteDataQuery(total_query, "Asset_Management");
                        }
                        else if (tb_t == "AZURE")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "RUNNING SERVER")
                                {
                                    dashboarditem.azure_runing_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STOPPED SERVER")
                                {
                                    dashboarditem.azure_stopped_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "ETC SERVER")
                                {
                                    dashboarditem.azure_etc_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                            }
                            string total_query = string.Format("exec [dbo].[prc_Dashboard_Server_azure_I] '{0}',{1},{2},{3}", companyname, dashboarditem.azure_runing_svr, dashboarditem.azure_stopped_svr, dashboarditem.azure_etc_svr);
                            dbmanager.ExecuteDataQuery(total_query, "Asset_Management");
                        }
                        else if (tb_t == "IDC")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "RUNNING SERVER")
                                {
                                    dashboarditem.idc_runing_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STOPPED SERVER")
                                {
                                    dashboarditem.idc_stopped_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "ETC SERVER")
                                {
                                    dashboarditem.idc_etc_svr = table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "");
                                }
                            }
                            string total_query = string.Format("exec [dbo].[prc_Dashboard_Server_idc_I] '{0}',{1},{2},{3}", companyname, dashboarditem.idc_runing_svr, dashboarditem.idc_stopped_svr, dashboarditem.idc_etc_svr);
                            dbmanager.ExecuteDataQuery(total_query, "Asset_Management");
                        }
                    }
                }
                else if (title == "DATABASE & STORAGE")
                {
                    ReadOnlyCollection<IWebElement> Serverresource = item.FindElements(By.XPath(".//div/aside/table"));
                    foreach (IWebElement sitem in Serverresource)
                    {
                        string tb_t = sitem.FindElement(By.XPath(".//thead")).Text;
                        if (tb_t == "TOTAL")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_database_total_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "DB AVAILABLE USAGE")
                                {
                                    dashboarditem.total_db_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.total_db_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STORAGE USAGE")
                                {
                                    dashboarditem.total_storage_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.total_storage_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "DISK USAGE")
                                {
                                    dashboarditem.total_disk_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.total_disk_usage);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                        else if (tb_t == "AWS")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_database_aws_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "DB AVAILABLE USAGE")
                                {
                                    dashboarditem.aws_db_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.aws_db_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STORAGE USAGE")
                                {
                                    dashboarditem.aws_storage_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.aws_storage_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "DISK USAGE")
                                {
                                    dashboarditem.aws_disk_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.aws_disk_usage);
                                }


                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                        else if (tb_t == "AZURE")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_database_azure_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "DB AVAILABLE USAGE")
                                {
                                    dashboarditem.azure_db_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.azure_db_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STORAGE USAGE")
                                {
                                    dashboarditem.azure_storage_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.azure_storage_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "DISK USAGE")
                                {
                                    dashboarditem.azure_disk_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.azure_disk_usage);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                        else if (tb_t == "IDC")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_database_idc_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "DB AVAILABLE USAGE")
                                {
                                    dashboarditem.idc_db_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.idc_db_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "STORAGE USAGE")
                                {
                                    dashboarditem.idc_storage_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.idc_storage_usage);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "DISK USAGE")
                                {
                                    dashboarditem.idc_disk_usage = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.idc_disk_usage);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                    }
                }
                else if (title == "NETWORK")
                {
                    ReadOnlyCollection<IWebElement> Serverresource = item.FindElements(By.XPath(".//div/aside/table"));
                    foreach (IWebElement sitem in Serverresource)
                    {
                        string tb_t = sitem.FindElement(By.XPath(".//thead")).Text;
                        if (tb_t == "TOTAL")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_Network_total_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "PRIVATE NETWORK")
                                {
                                    dashboarditem.total_network = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.total_network);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "LOAD BALANCER")
                                {
                                    dashboarditem.total_loadbalancer = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.total_loadbalancer);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "NETWORK IP")
                                {
                                    dashboarditem.total_networkip = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.total_networkip);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                        else if (tb_t == "AWS")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_Network_aws_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "PRIVATE NETWORK")
                                {
                                    dashboarditem.aws_network = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.aws_network);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "LOAD BALANCER")
                                {
                                    dashboarditem.aws_loadbalancer = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.aws_loadbalancer);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "NETWORK IP")
                                {
                                    dashboarditem.aws_networkip = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.aws_networkip);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                        else if (tb_t == "AZURE")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_Network_azure_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "PRIVATE NETWORK")
                                {
                                    dashboarditem.azure_network = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.azure_network);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "LOAD BALANCER")
                                {
                                    dashboarditem.azure_loadbalancer = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.azure_loadbalancer);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "NETWORK IP")
                                {
                                    dashboarditem.azure_networkip = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.azure_networkip);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                        else if (tb_t == "IDC")
                        {
                            ReadOnlyCollection<IWebElement> table1 = sitem.FindElements(By.XPath(".//tbody/tr"));
                            string total_qurey_string = string.Format("exec [dbo].[prc_Dashboard_Network_idc_I] '{0}'", companyname);
                            foreach (IWebElement table1_item in table1)
                            {
                                if (table1_item.FindElement(By.XPath(".//th")).Text == "PRIVATE NETWORK")
                                {
                                    dashboarditem.idc_network = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.idc_network);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "LOAD BALANCER")
                                {
                                    dashboarditem.idc_loadbalancer = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.idc_loadbalancer);
                                }
                                else if (table1_item.FindElement(By.XPath(".//th")).Text == "NETWORK IP")
                                {
                                    dashboarditem.idc_networkip = Check_Null_Value(table1_item.FindElement(By.XPath(".//td")).Text.Replace("ea", "").Split('|')[0]);
                                    total_qurey_string = total_qurey_string + string.Format(",{0}", dashboarditem.idc_networkip);
                                }

                            }
                            if (table1.Count < 3)
                            {
                                total_qurey_string = total_qurey_string + string.Format(",{0}", "0");
                            }
                            dbmanager.ExecuteDataQuery(total_qurey_string, "Asset_Management");
                        }
                    }
                }
            }
        }

        public void Test_1()
        {
            bspm.select_company("HOSTWAY");
            string str = string.Empty;
            IWebElement sdfsdf = bspm.findelementitem(".//ul[@class='list-custom-select company']");
            IReadOnlyCollection<IWebElement> sdfsdfs = sdfsdf.FindElements(By.XPath(".//li/button/mark"));
            foreach (IWebElement item in sdfsdfs)
            {
                if (item.Text == "HOSTWAY")
                {
                    item.Click();
                    break;
                }
            }
            string strr = string.Empty;
        }

        /// <summary>
        /// 각 테이블의 데이터를 select > 가져온 데이터를 비교하여 값을 확인
        /// </summary>
        public void Result_Check(string taskno)
        {
            DataSet companylist = Select_Companys_PASS(taskno);
            foreach (DataTable table in companylist.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    string tb_summary_query = string.Format(@"select * from [dbo].[TB_Dashboard_summary] where [company_name] = '{0}'", row["company_name"]);
                    DataSet tb_summary_data = dbmanager.ExecuteDataQuery(tb_summary_query, "Asset_Management");

                    string tb_resource_query = string.Format("select * from [dbo].[TB_Dashboard_Resources] where [company_name] = '{0}'", row["company_name"]);
                    DataSet tb_resource_data = dbmanager.ExecuteDataQuery(tb_resource_query, "Asset_Management");

                    string tb_server_query = string.Format("exec pro_Dashboard_Server_Q '{0}'", row["company_name"]);
                    DataSet tb_server_data = dbmanager.ExecuteDataQuery(tb_server_query, "Asset_Management");

                    string tb_db_query = string.Format(@"exec pro_Dashboard_Database_Q '{0}'", row["company_name"]);
                    DataSet tb_db_data = dbmanager.ExecuteDataQuery(tb_db_query, "Asset_Management");

                    string tb_Network_query = string.Format(@"exec pro_Dashboard_Network_Q '{0}'", row["company_name"]);
                    DataSet tb_Network_data = dbmanager.ExecuteDataQuery(tb_Network_query, "Asset_Management");

                    Result_Check_Detail(row["company_name"].ToString(), tb_summary_data, tb_resource_data, tb_server_data, tb_db_data, tb_Network_data);

                    string autocheck_result = string.Format(@"exec prc_Dashboard_Auto_Check_Result");
                    dbmanager.ExecuteDataQuery(autocheck_result, "Asset_Management");
                }
            }
        }
        public void initValue()
        {
            dashboarditem.resource_total_ = string.Empty;
            dashboarditem.resource_ec2_aws_ = string.Empty;
            dashboarditem.resource_virtualserver_azure_ = string.Empty;
            dashboarditem.resource_server_idc_ = string.Empty;
            dashboarditem.resource_rds_aws_ = string.Empty;
            dashboarditem.resource_sqlserver_azure_ = string.Empty;
            dashboarditem.resource_database_idc_ = string.Empty;
            dashboarditem.resource_s3_aws_ = string.Empty;
            dashboarditem.resource_storage_azure_ = string.Empty;
            dashboarditem.resource_ebs_aws_ = string.Empty;
            dashboarditem.resource_vpc_aws_ = string.Empty;
            dashboarditem.resource_loadbalancer_aws_ = string.Empty;
            dashboarditem.resource_eip_aws_ = string.Empty;
            dashboarditem.resource_virtualnetwork_azure_ = string.Empty;
            dashboarditem.resource_loadbalancer_azure_ = string.Empty;
        }
        public void Result_Check_Detail(string company_name, DataSet summary, DataSet resource, DataSet server, DataSet db, DataSet network)
        {
            /* summary */
            DataRow summaryrow = null;
            DataRow serverrows = null;
            DataRow dbrows = null;
            DataRow networkrow = null;
            initValue();

            int ec2 = 0;
            int virtualmachine = 0;
            int idcserver = 0;

            int rds = 0;
            int sqlserver = 0;
            int database = 0;
            dashboarditem.resource_rds_aws_ = "PASS";
            if (summary.Tables.Count > 0)
            {
                if (summary.Tables[0].Rows.Count > 0)
                {
                    summaryrow = summary.Tables[0].Rows[0];
                }
            }
            if (server.Tables.Count > 0)
            {
                if (server.Tables[0].Rows.Count > 0)
                {
                    serverrows = server.Tables[0].Rows[0];
                }
            }
            if (db.Tables.Count > 0)
            {
                if (db.Tables[0].Rows.Count > 0)
                {
                    dbrows = db.Tables[0].Rows[0];
                }
            }
            if (network.Tables.Count > 0)
            {
                if (network.Tables[0].Rows.Count > 0)
                {
                    networkrow = network.Tables[0].Rows[0];
                }
            }
            /* summary */
            if (summaryrow != null)
            {
                //summary_total_server_1 | total_server_Cnt = svr_ondemand_total + Auto_Scaling_total
                if (Convert.ToInt32(summaryrow["total_server_Cnt"]) == (Convert.ToInt32(summaryrow["svr_ondemand_total"]) + Convert.ToInt32(summaryrow["Auto_Scaling_total"])))
                {
                    // summary_total_server_1 = 1
                    dashboarditem.summary_total_server_1_ = "PASS";
                }
                else
                {
                    // summary_total_server_1 = 1
                    dashboarditem.summary_total_server_1_ = "PASS";
                }

                //summary_total_server_2 | total_server_Cnt = [total_runing_svr] + [total_stopped_svr] + [total_ctc_svr]
                if (Convert.ToInt32(summaryrow["total_server_Cnt"]) == (Convert.ToInt32(serverrows["total_runing_svr"]) + Convert.ToInt32(serverrows["total_stopped_svr"]) + Convert.ToInt32(serverrows["total_ctc_svr"])))
                {
                    dashboarditem.summary_total_server_2_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_total_server_2_ = "FAIL";
                }

                // summary_total_ondemand | [svr_ondemand_total] = [svr_ondemand_run] + [svr_ondemand_stop] + [svr_ondemand_etc]
                if (Convert.ToInt32(summaryrow["svr_ondemand_total"]) == (Convert.ToInt32(summaryrow["svr_ondemand_run"]) + Convert.ToInt32(summaryrow["svr_ondemand_stop"]) + Convert.ToInt32(summaryrow["svr_ondemand_etc"])))
                {
                    dashboarditem.summary_total_ondemand_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_total_ondemand_ = "FAIL";
                }
                // summary_total_autoscaling | [Auto_Scaling_total] = [Auto_Scaling_run] + [Auto_Scaling_stop] + [Auto_Scaling_etc]
                if (Convert.ToInt32(summaryrow["Auto_Scaling_total"]) == (Convert.ToInt32(summaryrow["Auto_Scaling_run"]) + Convert.ToInt32(summaryrow["Auto_Scaling_stop"]) + Convert.ToInt32(summaryrow["Auto_Scaling_etc"])))
                {
                    dashboarditem.summary_total_autoscaling_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_total_autoscaling_ = "FAIL";
                }
                // summary_tatal_database_1 | [total_db_Cnt] = [db_ondemand_total]
                if (Convert.ToInt32(summaryrow["total_db_Cnt"]) == Convert.ToInt32(summaryrow["db_ondemand_total"]))
                {
                    dashboarditem.summary_tatal_database_1_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_tatal_database_1_ = "FAIL";
                }

                // summary_total_ondemand_dbserver | [db_ondemand_total] = [db_ondemand_run] + [db_ondemand_stop] + [db_ondemand_etc]
                if (Convert.ToInt32(summaryrow["db_ondemand_total"]) == (Convert.ToInt32(summaryrow["db_ondemand_run"]) + Convert.ToInt32(summaryrow["db_ondemand_stop"]) + Convert.ToInt32(summaryrow["db_ondemand_etc"])))
                {
                    dashboarditem.summary_total_ondemand_dbserver_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_total_ondemand_dbserver_ = "FAIL";
                }

                string Qry = string.Format("exec prc_Dashboard_Auto_Check_Summary_I '{0}','{1}','{2}','{3}','{4}','{5}','{6}'",
                    company_name
                    , dashboarditem.summary_total_server_1_
                    , dashboarditem.summary_total_server_2_
                    , dashboarditem.summary_total_ondemand_
                    , dashboarditem.summary_total_autoscaling_
                    , dashboarditem.summary_tatal_database_1_
                    , dashboarditem.summary_total_ondemand_dbserver_);
                DataSet ds = dbmanager.ExecuteDataQuery(Qry, "Asset_Management");
            }

            /* Resource */
            if (resource.Tables.Count > 0)
            {
                if (resource.Tables[0].Rows.Count > 0)
                {
                    int sumresource = 0;
                    int totalresource = 0;
                    for (int i = 0; i < resource.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = resource.Tables[0].Rows[i];
                        totalresource = Convert.ToInt32(dr["total_Resource_cnt"]);

                        if (dr["Resource_Name"].ToString() == "EC2")
                        {
                            ec2 = Convert.ToInt32(dr["Resource_Cnt"]);
                            int totalec2 = Convert.ToInt32(serverrows["aws_runing_svr"]) + Convert.ToInt32(serverrows["aws_stopped_svr"]) + Convert.ToInt32(serverrows["aws_etc_svr"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == totalec2)
                                dashboarditem.resource_ec2_aws_ = "PASS";
                            else
                                dashboarditem.resource_ec2_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "Virtual Machine")
                        {
                            virtualmachine = Convert.ToInt32(dr["Resource_Cnt"]);
                            int total = Convert.ToInt32(serverrows["azure_runing_svr"]) + Convert.ToInt32(serverrows["azure_stopped_svr"]) + Convert.ToInt32(serverrows["azure_etc_svr"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_virtualserver_azure_ = "PASS";
                            else
                                dashboarditem.resource_virtualserver_azure_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "IDC Server")
                        {
                            idcserver = Convert.ToInt32(dr["Resource_Cnt"]);
                            int total = Convert.ToInt32(serverrows["idc_runing_svr"]) + Convert.ToInt32(serverrows["idc_stopped_svr"]) + Convert.ToInt32(serverrows["idc_etc_svr"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_server_idc_ = "PASS";
                            else
                                dashboarditem.resource_server_idc_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "RDS")
                        {
                            rds = Convert.ToInt32(dr["Resource_Cnt"]);
                            int total = Convert.ToInt32(dbrows["aws_db_usage"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_rds_aws_ = "PASS";
                            else
                                dashboarditem.resource_rds_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "SQL Database")
                        {
                            // Azure의 경우 Data Base = SQL Database + Data Warehouse
                            string qry = string.Format(@"SELECT 
      SUM([Resource_Cnt]) AS [Resource_Cnt]
  FROM [TB_Dashboard_Resources]
  where company_name = '{0}' and Resource_Name IN ('Data Warehouse','SQL Database') and cloud_type ='azure'", company_name);
                            DataSet sqldb = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
                            DataRow ddr = sqldb.Tables[0].Rows[0];

                            sqlserver = Convert.ToInt32(dr["Resource_Cnt"]);
                            int total = Convert.ToInt32(dbrows["azure_db_usage"]);
                            if (Convert.ToInt32(ddr["Resource_Cnt"]) == total)
                                dashboarditem.resource_sqlserver_azure_ = "PASS";
                            else
                                dashboarditem.resource_sqlserver_azure_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "IDC Database")
                        {
                            database = Convert.ToInt32(dr["Resource_Cnt"]);
                            int total = Convert.ToInt32(dbrows["idc_db_usage"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_database_idc_ = "PASS";
                            else
                                dashboarditem.resource_database_idc_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "S3")
                        {
                            int total = Convert.ToInt32(dbrows["aws_storage_usage"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_s3_aws_ = "PASS";
                            else
                                dashboarditem.resource_s3_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "Storage")
                        {
                            int total = Convert.ToInt32(dbrows["azure_storage_usage"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_storage_azure_ = "PASS";
                            else
                                dashboarditem.resource_storage_azure_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "EBS")
                        {
                            int total = Convert.ToInt32(dbrows["aws_disk_usage"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_ebs_aws_ = "PASS";
                            else
                                dashboarditem.resource_ebs_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "VPC")
                        {
                            int total = Convert.ToInt32(networkrow["aws_privated_network"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_vpc_aws_ = "PASS";
                            else
                                dashboarditem.resource_vpc_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "Load Balancer" && dr["cloud_type"].ToString() == "aws")
                        {
                            int total = Convert.ToInt32(networkrow["aws_load_balancer"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_loadbalancer_aws_ = "PASS";
                            else
                                dashboarditem.resource_loadbalancer_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "EIP")
                        {
                            int total = Convert.ToInt32(networkrow["aws_network_ip"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_eip_aws_ = "PASS";
                            else
                                dashboarditem.resource_eip_aws_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "Load Balancer" && dr["cloud_type"].ToString() == "azure")
                        {
                            int total = Convert.ToInt32(networkrow["azure_load_balancer"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_loadbalancer_azure_ = "PASS";
                            else
                                dashboarditem.resource_loadbalancer_azure_ = "FAIL";

                        }
                        if (dr["Resource_Name"].ToString() == "Virtual Network")
                        {
                            int total = Convert.ToInt32(networkrow["azure_privated_network"]);
                            if (Convert.ToInt32(dr["Resource_Cnt"]) == total)
                                dashboarditem.resource_virtualnetwork_azure_ = "PASS";
                            else
                                dashboarditem.resource_virtualnetwork_azure_ = "FAIL";

                        }

                        sumresource = sumresource + Convert.ToInt32(dr["Resource_Cnt"]);
                    }
                    if (totalresource == sumresource)
                    {
                        dashboarditem.resource_total_ = "PASS";
                    }
                    else
                    {
                        dashboarditem.resource_total_ = "FAIL";
                    }

                    string Qry = string.Format("exec prc_Dashboard_Auto_Check_Resource_I '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}'",
                    company_name
                    , dashboarditem.resource_total_
                    , dashboarditem.resource_ec2_aws_
                    , dashboarditem.resource_virtualserver_azure_
                    , dashboarditem.resource_server_idc_
                    , dashboarditem.resource_rds_aws_
                    , dashboarditem.resource_sqlserver_azure_
                    , dashboarditem.resource_database_idc_
                    , dashboarditem.resource_s3_aws_
                    , dashboarditem.resource_storage_azure_
                    , dashboarditem.resource_ebs_aws_
                    , dashboarditem.resource_vpc_aws_
                    , dashboarditem.resource_loadbalancer_aws_
                    , dashboarditem.resource_eip_aws_
                    , dashboarditem.resource_virtualnetwork_azure_
                    , dashboarditem.resource_loadbalancer_azure_
                    );
                    DataSet ds = dbmanager.ExecuteDataQuery(Qry, "Asset_Management");


                }
            }

            if (summaryrow != null)
            {

                if (Convert.ToInt32(summaryrow["total_server_Cnt"]) == (ec2 + virtualmachine + idcserver))
                {
                    dashboarditem.summary_total_server_3_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_total_server_3_ = "FAIL";
                }

                if (Convert.ToInt32(summaryrow["total_db_Cnt"]) == (rds + sqlserver + database))
                {
                    dashboarditem.summary_tatal_database_2_ = "PASS";
                }
                else
                {
                    dashboarditem.summary_tatal_database_2_ = "FAIL";
                }
                string qry = string.Format(@"update TB_Dashoboard_Auto_Check_Result_1 
                                            set summary_total_server_3 ='{0}'
                                                , summary_tatal_database_2='{1}' where company_name = '{2}'", dashboarditem.summary_total_server_3_, dashboarditem.summary_tatal_database_2_, company_name);
                DataSet ds = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            }

            if (serverrows != null)
            {
                bool result = false;
                if (Convert.ToInt32(serverrows["total_runing_svr"]) == (Convert.ToInt32(serverrows["aws_runing_svr"]) + Convert.ToInt32(serverrows["azure_runing_svr"]) + Convert.ToInt32(serverrows["idc_runing_svr"])))
                {
                    result = true;
                }
                if (Convert.ToInt32(serverrows["total_stopped_svr"]) == (Convert.ToInt32(serverrows["aws_stopped_svr"]) + Convert.ToInt32(serverrows["azure_stopped_svr"]) + Convert.ToInt32(serverrows["idc_stopped_svr"])))
                {
                    result = true;
                }
                if (Convert.ToInt32(serverrows["total_ctc_svr"]) == (Convert.ToInt32(serverrows["aws_etc_svr"]) + Convert.ToInt32(serverrows["azure_etc_svr"]) + Convert.ToInt32(serverrows["idc_etc_svr"])))
                {
                    result = true;
                }
                if (result == true)
                {
                    dashboarditem.server_total_ = "PASS";
                }
                else
                    dashboarditem.server_total_ = "FAIL";

                string qry = string.Format("update [TB_Dashoboard_Auto_Check_Result_1] set [server_total] = '{0}' where company_name = '{1}'", dashboarditem.server_total_, company_name);
                DataSet ds = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            }
            if (dbrows != null)
            {
                bool result = false;
                if (Convert.ToInt32(dbrows["total_db_usage"]) == (Convert.ToInt32(dbrows["aws_db_usage"]) + Convert.ToInt32(dbrows["azure_db_usage"]) + Convert.ToInt32(dbrows["idc_db_usage"])))
                {
                    result = true;
                }
                if (Convert.ToInt32(dbrows["total_storage_usage"]) == (Convert.ToInt32(dbrows["aws_storage_usage"]) + Convert.ToInt32(dbrows["azure_storage_usage"]) + Convert.ToInt32(dbrows["idc_storage_usage"])))
                {
                    result = true;
                }
                if (Convert.ToInt32(dbrows["total_disk_usage"]) == (Convert.ToInt32(dbrows["aws_disk_usage"]) + Convert.ToInt32(dbrows["azure_disk_usage"]) + Convert.ToInt32(dbrows["idc_disk_usage"])))
                {
                    result = true;
                }
                if (result == true)
                {
                    dashboarditem.database_total_ = "PASS";
                }
                else
                    dashboarditem.database_total_ = "FAIL";

                string qry = string.Format("update [TB_Dashoboard_Auto_Check_Result_1] set [database_total] = '{0}' where company_name = '{1}'", dashboarditem.database_total_, company_name);
                DataSet ds = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            }
            if (networkrow != null)
            {
                bool result = false;
                if (Convert.ToInt32(networkrow["total_privated_network"]) == (Convert.ToInt32(networkrow["aws_privated_network"]) + Convert.ToInt32(networkrow["azure_privated_network"]) + Convert.ToInt32(networkrow["idc_privated_network"])))
                {
                    result = true;
                }
                if (Convert.ToInt32(networkrow["total_load_balancer"]) == (Convert.ToInt32(networkrow["aws_load_balancer"]) + Convert.ToInt32(networkrow["azure_load_balancer"]) + Convert.ToInt32(networkrow["idc_load_balancer"])))
                {
                    result = true;
                }
                if (Convert.ToInt32(networkrow["total_network_ip"]) == (Convert.ToInt32(networkrow["aws_network_ip"]) + Convert.ToInt32(networkrow["azure_network_ip"]) + Convert.ToInt32(networkrow["idc_network_ip"])))
                {
                    result = true;
                }
                if (result == true)
                {
                    dashboarditem.network_total_ = "PASS";
                }
                else
                    dashboarditem.network_total_ = "FAIL";

                string qry = string.Format("update [TB_Dashoboard_Auto_Check_Result_1] set [network_total] = '{0}' where company_name = '{1}'", dashboarditem.database_total_, company_name);
                DataSet ds = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            }


        }

    }
}
