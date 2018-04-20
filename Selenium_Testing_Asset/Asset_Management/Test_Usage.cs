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

namespace Selenium_Testing_Asset.Asset_Management
{
    public class Test_Usage
    {
        Bsp_Manager bspm = new Bsp_Manager();
        DBManager dbmanager = new DBManager();
        Dashboard_Item dashboarditem = new Dashboard_Item();
        //Usage_Item usageitem = new Usage_Item();
        LogManager logman = new LogManager();
        string csvdownloadpath = string.Empty;

        public void Connection_Dashboard(string downloadpath)
        {
            bspm.Bsp_Connection_Chrome("https://asset.opsnow.com/", "myeongkwan.kim@bespinglobal.com", "pass@word!01", downloadpath);
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
        public DataSet Select_Companys_PASS(string TaskID)
        {
            string querystring = @"exec prc_Select_Company_Used_Asset_PASS " + string.Format("'{0}'", TaskID);
            DataSet ds = dbmanager.ExecuteDataQuery(querystring, "Asset_Management");

            return ds;
        }

        public void Asset_Usage_Process(string task_no, string url, string UserID, string Pwd, string downloadpath)
        {
            logman.AppendLogFile(task_no, string.Format("Process START"));
            logman.AppendLogFile(task_no, string.Format("Task ID : {0}", task_no));
            string taskno = task_no;

            csvdownloadpath = downloadpath;
            Connection_Dashboard(url, UserID, Pwd, downloadpath);

            //bspm.Bsp_Company_list();
            bspm.Bsp_Company_list();
            //string[] companylist = bspm.All_companys.Text.Replace("\r\n", "|").Split('|');
            //js.ExecuteScript("window.scrollBy(0,5)", "");
            //Insert_Aws_GridData();
            //Click_Type();
            //Grid_Row_Count("aws");

            //IWebElement totalitem = bspm.findelementitem(".//div[@id='ps-idc']//span[@class='total']");
            //Insert_Aws_Usage_Chart("BSP Company","aws","EC2");
            /* Grid 읽어 오기 */
            //string[] companylist = { "BSP Company" };

            //All_company_total_summary(companylist, taskno);

            All_company_total_summary_ds(Select_Companys(taskno), taskno);
            logman.AppendLogFile(task_no, string.Format("Process END"));

            //dropTable("TB_Usage_Data_Aws_EC2");
            //Insert_Aws_GridData_I("EC2","(주)리파인");

            //Insert_Aws_Usage_Chart();
            //bspm.Bsp_Company_list();

        }

        public void All_company_total_summary(string[] companylist, string taskno)
        {
            bspm.Bsp_Company_list();

            for (int i = 0; i < companylist.Length; i++)
            {
                bspm.Bsp_Select_Company(companylist[i], "USAGE");
                Thread.Sleep(10000);

                ReadOnlyCollection<IWebElement> grid_aws = bspm.findelementitems(".//div[@id='grid-aws']");
                if (grid_aws.Count > 0)
                {
                    Click_Type("aws", "BSP Company", taskno);
                }
                ReadOnlyCollection<IWebElement> grid_azure = bspm.findelementitems(".//div[@id='grid-azu']");
                if (grid_azure.Count > 0)
                {
                    Click_Type("azu", "BSP Company", taskno);
                }
                ReadOnlyCollection<IWebElement> grid_idc = bspm.findelementitems(".//div[@id='grid-idc']");
                if (grid_idc.Count > 0)
                {
                    Click_Type("idc", "BSP Company", taskno);
                }
                dbmanager.ExecuteDataQuery(string.Format("exec prc_Task_Status_Update '{0}'", taskno), "Asset_Management");
            }

        }

        public void All_company_total_summary_ds(DataSet companylist, string taskno)
        {
            bspm.Bsp_Company_list();

            for (int i = 0; i < companylist.Tables[0].Rows.Count; i++)
            {
                DataRow dr = companylist.Tables[0].Rows[i];
                string company_name = dr["company_name"].ToString();
                bspm.Bsp_Select_Company(company_name, "USAGE");
                Thread.Sleep(10000);

                ReadOnlyCollection<IWebElement> grid_aws = bspm.findelementitems(".//div[@id='grid-aws']");
                if (grid_aws.Count > 0)
                {
                    Click_Type("aws", company_name, taskno);
                }
                ReadOnlyCollection<IWebElement> grid_azure = bspm.findelementitems(".//div[@id='grid-azu']");
                if (grid_azure.Count > 0)
                {
                    Click_Type("azu", company_name, taskno);
                }
                ReadOnlyCollection<IWebElement> grid_idc = bspm.findelementitems(".//div[@id='grid-idc']");
                if (grid_idc.Count > 0)
                {
                    Click_Type("idc", company_name, taskno);
                }
                string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskno, company_name, "PASS");
                dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management");

                string reult = string.Format("exec prc_Usage_Result_ReportData '{0}'", company_name);
                dbmanager.ExecuteDataQuery(reult, "Asset_Management");

                Thread.Sleep(500);
                Result_Data_Binding();
            }
        }

        public void Click_Type(string servicetype, string company_name, string taskno)
        {

            IJavaScriptExecutor js = bspm.driver as IJavaScriptExecutor;
            string path = string.Format(".//bg-use-{0}-curr//div[@class='common-tabs']", servicetype);
            string upqy = string.Format("{0}|", servicetype);
            int islarge = Convert.ToInt32(dbmanager.ExecuteDataQuery(string.Format("prc_Select_large_company '{0}'", company_name), "BSP_Management").Tables[0].Rows[0][0]);
            IWebElement Tab = bspm.findelementitem(path);
            ReadOnlyCollection<IWebElement> type_tab = Tab.FindElements(By.XPath(".//button"));
            foreach (IWebElement item in type_tab)
            {
                IWebElement Tabvdv = bspm.findelementitem(string.Format(".//button[@id='{0}Toggle']", servicetype));
                js.ExecuteScript("arguments[0].scrollIntoView(true);", Tabvdv);

                string selectsource = string.Format("exec prc_TB_Auto_Process_Task_Q '{0}','{1}'", taskno, company_name);
                DataSet ds = dbmanager.ExecuteDataQuery(selectsource, "Asset_Management");
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        if (dr["process_status"].ToString() == "NON")
                        {
                            //dropTable(string.Format("TB_Usage_Data_{0}_{1}", servicetype, item.Text.Replace(" ","")));
                            deleteTable(string.Format("TB_Usage_Data_{0}_{1}", servicetype, item.Text.Replace(" ", "")), company_name);
                            Thread.Sleep(1000);
                            item.Click();

                            if (islarge == 1)
                                Thread.Sleep(20000);

                            Thread.Sleep(6000);
                            //log
                            logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : START", company_name, servicetype, item.Text.Replace(" ", "")));
                            int rowcnt = Grid_Row_Count(servicetype);
                            Thread.Sleep(2000);
                            Insert_Aws_Usage_Chart_json(company_name, servicetype, item.Text.Replace(" ", ""));
                            Insert_Aws_GridData_I(item.Text.Replace(" ", ""), company_name, servicetype, rowcnt);

                            upqy = upqy + item.Text.Replace(" ", "");
                            string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskno, company_name, upqy);
                            dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management");

                            //log
                            logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : END", company_name, servicetype, item.Text.Replace(" ", "")));
                        }
                        else
                        {
                            string options = dr["process_status"].ToString();
                            if (!options.Contains(item.Text.Replace(" ", "")))
                            {
                                //dropTable(string.Format("TB_Usage_Data_{0}_{1}", servicetype, item.Text.Replace(" ","")));
                                deleteTable(string.Format("TB_Usage_Data_{0}_{1}", servicetype, item.Text.Replace(" ", "")), company_name);
                                Thread.Sleep(1000);
                                item.Click();

                                if (islarge == 1)
                                    Thread.Sleep(15000);

                                Thread.Sleep(6000);
                                //log
                                logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : START", company_name, servicetype, item.Text.Replace(" ", "")));
                                int rowcnt = Grid_Row_Count(servicetype);
                                Thread.Sleep(2000);

                                Insert_Aws_Usage_Chart_json(company_name, servicetype, item.Text.Replace(" ", ""));
                                Insert_Aws_GridData_I(item.Text.Replace(" ", ""), company_name, servicetype, rowcnt);
                                string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskno, company_name, options + "," + item.Text.Replace(" ", ""));
                                dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management");

                                //log
                                logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : END", company_name, servicetype, item.Text.Replace(" ", "")));
                            }
                        }
                    }

                }

            }

        }

        /* Data가 1000이상일 경우 csv 다운로드 하여 진행 */
        public void insert_csv_data1(DataTable mainTable, string company_name)
        {
            bspm.findelementitem(".//button[@class='button-icon-txt icon-download']").Click();
            Thread.Sleep(15000);
            string path = string.Format(@"{0}\export.csv", csvdownloadpath);
            //DataTable dt = CsvFileToDatatable(path, true);
            DataTable dt = ConvertCSVtoDataTable(path);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow rd = dt.Rows[i];
                DataRow newrow = mainTable.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    newrow[j] = rd[j];
                }
                newrow["company_name"] = company_name;
                mainTable.Rows.Add(newrow);
            }

            Thread.Sleep(1000);
            File.Delete(string.Format(@"{0}\export.csv", csvdownloadpath));
        }

        public void insert_csv_data(DataTable mainTable, string company_name)
        {
            bspm.findelementitem(".//button[@class='button-icon-txt icon-download']").Click();
            Thread.Sleep(10000);
            //string path = @"D:\download\export.csv";
            string path = string.Format(@"{0}\export.csv", csvdownloadpath);
            //DataTable dt = CsvFileToDatatable(path, true);
            DataTable dt = ConvertCSVtoDataTable(path);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow newrow = mainTable.NewRow();
                newrow.ItemArray = dt.Rows[i].ItemArray;
                newrow["company_name"] = company_name;
                mainTable.Rows.Add(newrow);
            }

            Thread.Sleep(1000);
            File.Delete(string.Format(@"{0}\export.csv", csvdownloadpath));
            //File.Delete(@"D:\download\export.csv");
        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            //string filepath = "C:\\params.csv";
            StreamReader sr = new StreamReader(strFilePath);
            string line = sr.ReadLine();
            string[] value = line.Split(',');
            DataTable dt = new DataTable();
            DataRow row;
            foreach (string dc in value)
            {
                dt.Columns.Add(new DataColumn(dc.Replace("\"", "")));
            }

            while (!sr.EndOfStream)
            {
                value = sr.ReadLine().Replace("\",\"", "|").Replace("\"", "").Split('|');
                if (value.Length == dt.Columns.Count)
                {
                    row = dt.NewRow();
                    row.ItemArray = value;
                    dt.Rows.Add(row);
                }
            }
            sr.Close();
            return dt;
        }
        public void testlinq()
        {

        }

        public int Grid_Row_Count(string servicetype)
        {

            Thread.Sleep(2000);
            string path = string.Format(".//div[@id='ps-{0}']//span[@class='total']", servicetype.Replace("azu", "azure"));
            IWebElement totalitem = bspm.findelementitem(path);
            if (Convert.ToInt32(totalitem.Text.Split('/')[1].Replace(" ", "")) >= 100)
            {
                string conpath = string.Format(".//div[@id='ps-{0}']//span[@class='count']/select", servicetype.Replace("azu", "azure"));
                IWebElement countitem = bspm.findelementitem(conpath);
                ReadOnlyCollection<IWebElement> options = countitem.FindElements(By.XPath(".//option"));
                foreach (IWebElement option in options)
                {
                    if (option.GetAttribute("Value") == "1000")
                    {
                        option.Click();
                        Thread.Sleep(1000);
                    }
                }
                //int num = Convert.ToInt32(totalitem.Text.Split('/')[1].Replace(" ", ""));
            }
            return Convert.ToInt32(totalitem.Text.Split('/')[1].Replace(" ", ""));
        }

        public void dropTable(string tablename)
        {

            string isexistqry = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tablename);
            DataSet ds = dbmanager.ExecuteDataQuery(isexistqry, "Asset_Management");
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string drop_table = string.Format("drop Table {0}", tablename);
                    dbmanager.ExecuteDataQuery(drop_table, "Asset_Management");
                }
            }
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

        public void Result_Data_Binding()
        {
            string str = @"SELECT * FROM [Asset_Management].[dbo].[TB_Usage_Report_Data]";
            DataSet ds = dbmanager.ExecuteDataQuery(str, "Asset_Management");
            if (ds.Tables[0].Rows.Count > 0)
            {
                string[] servicelist = { "aws", "azure", "idc" };
                for (int h = 0; h < servicelist.Length; h++)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = ds.Tables[0].Rows[i];
                        string qry = string.Format(@"SELECT company_name  
  FROM [Asset_Management].[dbo].[TB_Usage_Auto_Check_Result]
	where company_name = '{0}' and service_type ='{1}' ", dr["company_name"], servicelist[h]
                        );
                        DataSet dg = dbmanager.ExecuteDataQuery(qry, "Asset_management");
                        if (dg.Tables[0].Rows.Count > 0)
                        {
                            string awsq = string.Format("exec prc_Usage_Auto_Check_Result_1 '{0}', '{1}'", dr["company_name"], servicelist[h]);
                            dbmanager.ExecuteDataQuery(awsq, "Asset_Management");
                        }
                    }
                }

            }
        }

        public void CreateTable(string tablename, string Query)
        {
            string isexistqry = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tablename);
            DataSet ds = dbmanager.ExecuteDataQuery(isexistqry, "Asset_Management");
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {

                }
                else
                {
                    string qry = string.Format(@"Create Table {0} (
                                            {1}
                                            )", tablename, Query);
                    dbmanager.ExecuteDataQuery(qry, "Asset_Management");
                }
            }
            else
            {
                string qry = string.Format(@"Create Table {0} (
                                            {1}
                                            )", tablename, Query);
                dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            }

        }

        public void Result_Check_Usage_1(string tablename, string company_name)
        {
            string servicename = tablename.Split('_')[3];
            string resourcename = tablename.Split('_')[4];
            string selectqry = string.Format("Select * From {0} Where company_name = '{1}'", tablename, company_name);
            DataSet ResultData_Grid = dbmanager.ExecuteDataQuery(selectqry, "Asset_Management");

            string chartdata = string.Format("exec prc_Usage_Chart_Data_Q '{0}','{1}','{2}'", company_name, servicename, resourcename);
            DataSet ResultData_Chart = dbmanager.ExecuteDataQuery(chartdata, "Asset_Management");
            DataRow chartdatcrda = ResultData_Chart.Tables[0].Rows[0];
            JObject studentObj = new JObject();
            dynamic STATUS;
            dynamic REGIONS;
            dynamic TYPES;
            string isStatus = string.Empty;
            string isRegions = string.Empty;
            string isTypes = string.Empty;
            string whereStatus = string.Empty;
            string whereRegions = string.Empty;
            string whereTypes = string.Empty;

            //Chart Table에서 status, region, type 에 대한 정보를 변수에 저장
            if (ResultData_Chart.Tables.Count > 0)
            {
                if (ResultData_Chart.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ResultData_Chart.Tables[0].Rows.Count; i++)
                    {
                        DataRow rw = ResultData_Chart.Tables[0].Rows[i];
                        studentObj = JObject.Parse(rw["jsonData"].ToString());
                        STATUS = studentObj["STATUS"].ToArray();
                        int cntstatus = 0;
                        int cntregion = 0;
                        int cntistype = 0;
                        foreach (dynamic item in STATUS)
                        {
                            if (cntstatus == 0)
                                whereStatus = whereStatus + string.Format("$colname='{0}'", (string)item.Name);
                            else
                                whereStatus = whereStatus + string.Format(" or $colname='{0}'", (string)item.Name);

                            isStatus = isStatus + string.Format("|{0}", (string)item.Name);
                            cntstatus++;
                        }
                        REGIONS = studentObj["REGIONS"].ToArray();
                        foreach (dynamic item in REGIONS)
                        {
                            if (cntregion == 0)
                                whereRegions = whereRegions + string.Format("$colname='{0}'", (string)item.Name);
                            else
                                whereRegions = whereRegions + string.Format(" or $colname='{0}'", (string)item.Name);

                            isRegions = isRegions + string.Format("|{0}", (string)item.Name);
                            cntregion++;
                        }
                        TYPES = studentObj["TYPES"].ToArray();
                        foreach (dynamic item in TYPES)
                        {
                            if (cntistype == 0)
                                whereTypes = whereTypes + string.Format("$colname='{0}'", (string)item.Name);
                            else
                                whereTypes = whereTypes + string.Format(" or $colname='{0}'", (string)item.Name);

                            isTypes = isTypes + string.Format("|{0}", (string)item.Name);
                            cntistype++;
                        }
                    }
                }
            }
            //status, region, type에 대한 정보를 그리드와 비교하여 컬럼값을 찾는다.
            int stcnt = 0;
            int recnt = 0;
            int tycnt = 0;
            DataTable dt = ResultData_Grid.Tables[0];
            if (ResultData_Grid.Tables.Count > 0)
            {
                if (ResultData_Grid.Tables[0].Rows.Count > 0)
                {
                    DataRow rw = ResultData_Grid.Tables[0].Rows[0];
                    for (int j = 0; j < ResultData_Grid.Tables[0].Columns.Count; j++)
                    {
                        if (j != 0)
                        {
                            string col1 = whereStatus.Replace("$colname", ResultData_Grid.Tables[0].Columns[j].ColumnName);
                            DataRow[] drset = dt.Select(col1);
                            if (drset.Count() > stcnt)
                            {
                                isStatus = ResultData_Grid.Tables[0].Columns[j].ColumnName;
                                stcnt = drset.Count();
                            }
                            string col2 = whereRegions.Replace("$colname", ResultData_Grid.Tables[0].Columns[j].ColumnName);
                            DataRow[] drset2 = dt.Select(col2);
                            if (drset2.Count() > recnt)
                            {
                                isRegions = ResultData_Grid.Tables[0].Columns[j].ColumnName;
                                recnt = drset2.Count();
                            }
                            string col3 = whereTypes.Replace("$colname", ResultData_Grid.Tables[0].Columns[j].ColumnName);
                            DataRow[] drset3 = dt.Select(col3);
                            if (drset3.Count() > tycnt)
                            {
                                isTypes = ResultData_Grid.Tables[0].Columns[j].ColumnName;
                                tycnt = drset3.Count();
                            }
                        }
                    }
                }

            }

            if (isStatus.Split('|').Length > 1)
                isStatus = "NON";
            if (isRegions.Split('|').Length > 1)
                isRegions = "NON";
            if (isTypes.Split('|').Length > 1)
                isTypes = "NON";

            dbmanager.ExecuteDataQuery(string.Format("update [TB_Usage_Chart_Data] set status_colname = '{0}' where company_name = '{1}' and servicetype = '{2}' and resourcename = '{3}'"
                , isStatus, company_name, servicename, resourcename)
                   , "Asset_Management");

            if (isStatus != "NON")
            {
                string isPass = "FAIL";
                string isInstance = "FAIL";
                string isStatusNamecheck = "FAIL";

                List<string> list_isPass = new List<string>();
                List<string> list_isInstance = new List<string>();
                List<string> list_isStatusNamecheck = new List<string>();



                string s1 = string.Format(@"select {0}, Count(*) AS cnt From {1} 
                                                    Where company_name = '{2}'
                                            Group by {0}", isStatus, tablename, company_name);
                DataSet d1 = dbmanager.ExecuteDataQuery(s1, "Asset_Management");
                if (d1.Tables.Count > 0)
                {
                    int grid_total_cnt = 0;
                    int chart_total_cnt = 0;

                    for (int i = 0; i < d1.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr1 = d1.Tables[0].Rows[i];
                        string col_status = string.Empty;
                        // Chart의 Instance total 값과 , Grid의 Instance total 값을 비교하기 위한 값 저장
                        if (dr1[isStatus] == null || dr1[isStatus].ToString() == "" && dr1[isStatus].ToString() == string.Empty)
                            col_status = "Unkown";
                        else
                            col_status = dr1[isStatus].ToString();
                        grid_total_cnt = grid_total_cnt + Convert.ToInt32(dr1["cnt"]);

                        if (col_status != "Unkown")
                        {
                            chart_total_cnt = chart_total_cnt + (int)studentObj["STATUS"][col_status];
                        }

                        // Status Name Check
                        STATUS = studentObj["STATUS"].ToArray();
                        foreach (dynamic item in STATUS)
                        {// (string)item.Name
                            if ((string)item.Name == dr1[isStatus].ToString())
                            {
                                isStatusNamecheck = "PASS";
                                list_isStatusNamecheck.Add(isStatusNamecheck);
                            }
                        }

                        // instance_count_check : 각 Status 별 항목 비교 
                        string chartval = (string)studentObj["STATUS"][dr1[isStatus]];
                        if (chartval == dr1["cnt"].ToString())
                            isPass = "PASS";
                        else
                            isPass = "FAIL";

                        list_isPass.Add(isPass);

                    }
                    if (grid_total_cnt == chart_total_cnt && grid_total_cnt == (int)studentObj["Instances"])
                        isInstance = "PASS";
                    else
                        isInstance = "FAIL";

                    list_isInstance.Add(isInstance);

                    for (int k = 0; k < list_isPass.Count; k++)
                    {
                        if (list_isPass[k] == "FAIL")
                            isPass = "FAIL";
                    }
                    for (int k = 0; k < list_isInstance.Count; k++)
                    {
                        if (list_isInstance[k] == "FAIL")
                            isInstance = "FAIL";
                    }
                    for (int k = 0; k < list_isStatusNamecheck.Count; k++)
                    {
                        if (list_isStatusNamecheck[k] == "FAIL")
                            isStatusNamecheck = "FAIL";
                    }

                    Result_Update("total_instance_check", isInstance, company_name, servicename, resourcename);
                    Result_Update("instance_group_check", isStatusNamecheck, company_name, servicename, resourcename);
                    Result_Update("instance_count_check", isPass, company_name, servicename, resourcename);
                }
            }
            else
            {
                Result_Update("total_instance_check", isStatus, company_name, servicename, resourcename);
                Result_Update("instance_group_check", isStatus, company_name, servicename, resourcename);
                Result_Update("instance_count_check", isStatus, company_name, servicename, resourcename);
            }
            if (isRegions != "NON")
            {
                string isPass = "FAIL";
                string isRegioncheck = "FAIL";
                string isStatusNamecheck = "FAIL";

                List<string> list_isPass = new List<string>();
                List<string> list_isRegioncheck = new List<string>();
                List<string> list_isStatusNamecheck = new List<string>();

                string s1 = string.Format(@"select {0}, Count(*) AS cnt From {1} 
                                                    Where company_name = '{2}'
                                            Group by {0}", isRegions, tablename, company_name);
                DataSet d1 = dbmanager.ExecuteDataQuery(s1, "Asset_Management");
                if (d1.Tables.Count > 0)
                {
                    int grid_total_cnt = 0;
                    int chart_total_cnt = 0;

                    for (int i = 0; i < d1.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr1 = d1.Tables[0].Rows[i];
                        // Chart의 Instance total 값과 , Grid의 Instance total 값을 비교하기 위한 값 저장
                        grid_total_cnt = grid_total_cnt + Convert.ToInt32(dr1["cnt"]);
                        chart_total_cnt = chart_total_cnt + (int)studentObj["REGIONS"][dr1[isRegions]];

                        // Status Name Check
                        REGIONS = studentObj["REGIONS"].ToArray();
                        foreach (dynamic item in REGIONS)
                        {// (string)item.Name
                            if ((string)item.Name == dr1[isRegions].ToString())
                            {
                                isStatusNamecheck = "PASS";
                                list_isStatusNamecheck.Add(isStatusNamecheck);
                            }
                        }

                        // instance_count_check : 각 Status 별 항목 비교 
                        string chartval = (string)studentObj["REGIONS"][dr1[isRegions]];
                        if (chartval == dr1["cnt"].ToString())
                            isPass = "PASS";
                        else
                            isPass = "FAIL";

                        list_isPass.Add(isPass);

                    }
                    if (grid_total_cnt == chart_total_cnt)
                        isRegioncheck = "PASS";
                    else
                        isRegioncheck = "FAIL";

                    list_isRegioncheck.Add(isRegioncheck);

                    for (int k = 0; k < list_isPass.Count; k++)
                    {
                        if (list_isPass[k] == "FAIL")
                            isPass = "FAIL";
                    }
                    for (int k = 0; k < list_isRegioncheck.Count; k++)
                    {
                        if (list_isRegioncheck[k] == "FAIL")
                            isRegioncheck = "FAIL";
                    }
                    for (int k = 0; k < list_isStatusNamecheck.Count; k++)
                    {
                        if (list_isStatusNamecheck[k] == "FAIL")
                            isStatusNamecheck = "FAIL";
                    }

                    Result_Update("total_regions_check", isRegioncheck, company_name, servicename, resourcename);
                    Result_Update("regions_group_check", isStatusNamecheck, company_name, servicename, resourcename);
                    Result_Update("regions_count_check", isPass, company_name, servicename, resourcename);
                }
            }
            else
            {
                Result_Update("total_regions_check", isRegions, company_name, servicename, resourcename);
                Result_Update("regions_group_check", isRegions, company_name, servicename, resourcename);
                Result_Update("regions_count_check", isRegions, company_name, servicename, resourcename);
            }
            if (isTypes != "NON")
            {
                string isPass = "FAIL";
                string isRegioncheck = "FAIL";
                string isStatusNamecheck = "FAIL";

                List<string> list_isPass = new List<string>();
                List<string> list_isRegioncheck = new List<string>();
                List<string> list_isStatusNamecheck = new List<string>();

                string s1 = string.Format(@"select {0}, Count(*) AS cnt From {1} 
                                                    Where company_name = '{2}'
                                            Group by {0}", isTypes, tablename, company_name);
                DataSet d1 = dbmanager.ExecuteDataQuery(s1, "Asset_Management");
                if (d1.Tables.Count > 0)
                {
                    int grid_total_cnt = 0;
                    int chart_total_cnt = 0;

                    for (int i = 0; i < d1.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr1 = d1.Tables[0].Rows[i];
                        // Chart의 Instance total 값과 , Grid의 Instance total 값을 비교하기 위한 값 저장
                        if (dr1[isTypes].ToString() != "")
                        {
                            grid_total_cnt = grid_total_cnt + Convert.ToInt32(dr1["cnt"]);
                            chart_total_cnt = chart_total_cnt + (int)studentObj["TYPES"][dr1[isTypes]];

                            // Status Name Check
                            TYPES = studentObj["TYPES"].ToArray();
                            foreach (dynamic item in TYPES)
                            {// (string)item.Name
                                if ((string)item.Name == dr1[isTypes].ToString())
                                {
                                    isStatusNamecheck = "PASS";
                                    list_isStatusNamecheck.Add(isStatusNamecheck);
                                }
                            }

                            // instance_count_check : 각 Status 별 항목 비교 
                            string chartval = (string)studentObj["TYPES"][dr1[isTypes]];
                            if (chartval == dr1["cnt"].ToString())
                                isPass = "PASS";
                            else
                                isPass = "FAIL";

                            list_isPass.Add(isPass);
                        }
                        else
                        {
                            list_isStatusNamecheck.Add("FAIL");
                            list_isPass.Add("FAIL");
                        }


                    }
                    if (grid_total_cnt == chart_total_cnt)
                        isRegioncheck = "PASS";
                    else
                        isRegioncheck = "FAIL";

                    list_isRegioncheck.Add(isRegioncheck);

                    for (int k = 0; k < list_isPass.Count; k++)
                    {
                        if (list_isPass[k] == "FAIL")
                            isPass = "FAIL";
                    }
                    for (int k = 0; k < list_isRegioncheck.Count; k++)
                    {
                        if (list_isRegioncheck[k] == "FAIL")
                            isRegioncheck = "FAIL";
                    }
                    for (int k = 0; k < list_isStatusNamecheck.Count; k++)
                    {
                        if (list_isStatusNamecheck[k] == "FAIL")
                            isStatusNamecheck = "FAIL";
                    }

                    Result_Update("total_types_check", isRegioncheck, company_name, servicename, resourcename);
                    Result_Update("types_group_check", isStatusNamecheck, company_name, servicename, resourcename);
                    Result_Update("types_count_check", isPass, company_name, servicename, resourcename);
                }
            }
            else
            {
                Result_Update("total_types_check", isTypes, company_name, servicename, resourcename);
                Result_Update("types_group_check", isTypes, company_name, servicename, resourcename);
                Result_Update("types_count_check", isTypes, company_name, servicename, resourcename);
            }
        }

        public void Result_Check_Usage(string tablename, string company_name)
        {
            string servicename = tablename.Split('_')[3];
            string resourcename = tablename.Split('_')[4];
            string selectqry = string.Format("Select * From {0} Where company_name = '{1}'", tablename, company_name);
            DataSet ResultData_Grid = dbmanager.ExecuteDataQuery(selectqry, "Asset_Management");

            string chartdata = string.Format("exec prc_Usage_Chart_Data_Q '{0}','{1}','{2}'", company_name, servicename, resourcename);
            DataSet ResultData_Chart = dbmanager.ExecuteDataQuery(chartdata, "Asset_Management");
            DataRow chartdatcrda = ResultData_Chart.Tables[0].Rows[0];
            JObject studentObj = new JObject();
            dynamic STATUS;
            dynamic REGIONS;
            dynamic TYPES;
            string isStatus = string.Empty;
            string isRegions = string.Empty;
            string isTypes = string.Empty;

            //Chart Table에서 status, region, type 에 대한 정보를 변수에 저장
            if (ResultData_Chart.Tables.Count > 0)
            {
                if (ResultData_Chart.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ResultData_Chart.Tables[0].Rows.Count; i++)
                    {
                        DataRow rw = ResultData_Chart.Tables[0].Rows[i];
                        studentObj = JObject.Parse(rw["jsonData"].ToString());
                        STATUS = studentObj["STATUS"].ToArray();
                        foreach (dynamic item in STATUS)
                        {
                            isStatus = isStatus + string.Format("|{0}", (string)item.Name);
                        }
                        REGIONS = studentObj["REGIONS"].ToArray();
                        foreach (dynamic item in REGIONS)
                        {
                            isRegions = isRegions + string.Format("|{0}", (string)item.Name);
                        }
                        TYPES = studentObj["TYPES"].ToArray();
                        foreach (dynamic item in TYPES)
                        {
                            isTypes = isTypes + string.Format("|{0}", (string)item.Name);
                        }
                    }
                }
            }
            //status, region, type에 대한 정보를 그리드와 비교하여 컬럼값을 찾는다.
            if (ResultData_Grid.Tables.Count > 0)
            {
                if (ResultData_Grid.Tables[0].Rows.Count > 0)
                {
                    DataRow rw = ResultData_Grid.Tables[0].Rows[0];
                    for (int j = 0; j < ResultData_Grid.Tables[0].Columns.Count; j++)
                    {
                        if (j != 0)
                        {
                            if (rw[j].ToString() != "")
                            {
                                if (isStatus.Contains(rw[j].ToString()))
                                {
                                    isStatus = ResultData_Grid.Tables[0].Columns[j].ColumnName;
                                }
                                if (isRegions.Contains(rw[j].ToString()))
                                {
                                    isRegions = ResultData_Grid.Tables[0].Columns[j].ColumnName;
                                }
                                if (isTypes.Contains(rw[j].ToString()))
                                {
                                    isTypes = ResultData_Grid.Tables[0].Columns[j].ColumnName;
                                }
                            }
                        }
                    }
                }

            }
            if (isStatus.Split('|').Length > 1)
                isStatus = "NON";
            if (isRegions.Split('|').Length > 1)
                isRegions = "NON";
            if (isTypes.Split('|').Length > 1)
                isTypes = "NON";


            if (isStatus != "NON")
            {
                string isPass = "FAIL";
                string isInstance = "FAIL";
                string isStatusNamecheck = "FAIL";
                string s1 = string.Format(@"select {0}, Count(*) AS cnt From {1} 
                                                    Where company_name = '{2}'
                                            Group by {0}", isStatus, tablename, company_name);
                DataSet d1 = dbmanager.ExecuteDataQuery(s1, "Asset_Management");
                if (d1.Tables.Count > 0)
                {
                    int grid_total_cnt = 0;
                    int chart_total_cnt = 0;

                    for (int i = 0; i < d1.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr1 = d1.Tables[0].Rows[i];
                        // Chart의 Instance total 값과 , Grid의 Instance total 값을 비교하기 위한 값 저장
                        grid_total_cnt = grid_total_cnt + Convert.ToInt32(dr1["cnt"]);
                        chart_total_cnt = chart_total_cnt + (int)studentObj["STATUS"][dr1[isStatus]];

                        // Status Name Check
                        STATUS = studentObj["STATUS"].ToArray();
                        foreach (dynamic item in STATUS)
                        {// (string)item.Name
                            if ((string)item.Name == dr1[isStatus].ToString())
                                isStatusNamecheck = "PASS";
                        }

                        // instance_count_check : 각 Status 별 항목 비교 
                        string chartval = (string)studentObj["STATUS"][dr1[isStatus]];
                        if (chartval == dr1["cnt"].ToString())
                            isPass = "PASS";
                        else
                            isPass = "FAIL";

                    }
                    if (grid_total_cnt == chart_total_cnt && grid_total_cnt == (int)studentObj["Instances"])
                        isInstance = "PASS";
                    else
                        isInstance = "FAIL";

                    Result_Update("total_instance_check", isInstance, company_name, servicename, resourcename);
                    Result_Update("instance_group_check", isStatusNamecheck, company_name, servicename, resourcename);
                    Result_Update("instance_count_check", isPass, company_name, servicename, resourcename);
                }
            }
            else
            {
                Result_Update("total_instance_check", isStatus, company_name, servicename, resourcename);
                Result_Update("instance_group_check", isStatus, company_name, servicename, resourcename);
                Result_Update("instance_count_check", isStatus, company_name, servicename, resourcename);
            }
            if (isRegions != "NON")
            {
                string isPass = "FAIL";
                string isRegioncheck = "FAIL";
                string isStatusNamecheck = "FAIL";
                string s1 = string.Format(@"select {0}, Count(*) AS cnt From {1} 
                                                    Where company_name = '{2}'
                                            Group by {0}", isRegions, tablename, company_name);
                DataSet d1 = dbmanager.ExecuteDataQuery(s1, "Asset_Management");
                if (d1.Tables.Count > 0)
                {
                    int grid_total_cnt = 0;
                    int chart_total_cnt = 0;

                    for (int i = 0; i < d1.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr1 = d1.Tables[0].Rows[i];
                        // Chart의 Instance total 값과 , Grid의 Instance total 값을 비교하기 위한 값 저장
                        grid_total_cnt = grid_total_cnt + Convert.ToInt32(dr1["cnt"]);
                        chart_total_cnt = chart_total_cnt + (int)studentObj["REGIONS"][dr1[isRegions]];

                        // Status Name Check
                        REGIONS = studentObj["REGIONS"].ToArray();
                        foreach (dynamic item in REGIONS)
                        {// (string)item.Name
                            if ((string)item.Name == dr1[isRegions].ToString())
                                isStatusNamecheck = "PASS";
                        }

                        // instance_count_check : 각 Status 별 항목 비교 
                        string chartval = (string)studentObj["REGIONS"][dr1[isRegions]];
                        if (chartval == dr1["cnt"].ToString())
                            isPass = "PASS";
                        else
                            isPass = "FAIL";

                    }
                    if (grid_total_cnt == chart_total_cnt)
                        isRegioncheck = "PASS";
                    else
                        isRegioncheck = "FAIL";

                    Result_Update("total_regions_check", isRegioncheck, company_name, servicename, resourcename);
                    Result_Update("regions_group_check", isStatusNamecheck, company_name, servicename, resourcename);
                    Result_Update("regions_count_check", isPass, company_name, servicename, resourcename);
                }
            }
            else
            {
                Result_Update("total_regions_check", isRegions, company_name, servicename, resourcename);
                Result_Update("regions_group_check", isRegions, company_name, servicename, resourcename);
                Result_Update("regions_count_check", isRegions, company_name, servicename, resourcename);
            }
            if (isTypes != "NON")
            {
                string isPass = "FAIL";
                string isRegioncheck = "FAIL";
                string isStatusNamecheck = "FAIL";
                string s1 = string.Format(@"select {0}, Count(*) AS cnt From {1} 
                                                    Where company_name = '{2}'
                                            Group by {0}", isTypes, tablename, company_name);
                DataSet d1 = dbmanager.ExecuteDataQuery(s1, "Asset_Management");
                if (d1.Tables.Count > 0)
                {
                    int grid_total_cnt = 0;
                    int chart_total_cnt = 0;

                    for (int i = 0; i < d1.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr1 = d1.Tables[0].Rows[i];
                        // Chart의 Instance total 값과 , Grid의 Instance total 값을 비교하기 위한 값 저장
                        grid_total_cnt = grid_total_cnt + Convert.ToInt32(dr1["cnt"]);
                        chart_total_cnt = chart_total_cnt + (int)studentObj["TYPES"][dr1[isTypes]];

                        // Status Name Check
                        TYPES = studentObj["TYPES"].ToArray();
                        foreach (dynamic item in TYPES)
                        {// (string)item.Name
                            if ((string)item.Name == dr1[isTypes].ToString())
                                isStatusNamecheck = "PASS";
                        }

                        // instance_count_check : 각 Status 별 항목 비교 
                        string chartval = (string)studentObj["TYPES"][dr1[isTypes]];
                        if (chartval == dr1["cnt"].ToString())
                            isPass = "PASS";
                        else
                            isPass = "FAIL";

                    }
                    if (grid_total_cnt == chart_total_cnt)
                        isRegioncheck = "PASS";
                    else
                        isRegioncheck = "FAIL";

                    Result_Update("total_types_check", isRegioncheck, company_name, servicename, resourcename);
                    Result_Update("types_group_check", isStatusNamecheck, company_name, servicename, resourcename);
                    Result_Update("types_count_check", isPass, company_name, servicename, resourcename);
                }
            }
            else
            {
                Result_Update("total_types_check", isTypes, company_name, servicename, resourcename);
                Result_Update("types_group_check", isTypes, company_name, servicename, resourcename);
                Result_Update("types_count_check", isTypes, company_name, servicename, resourcename);
            }
        }

        public void Result_Update(string colname, string colval, string company_name, string servicetype, string resourcename)
        {
            string updateq = string.Format(@"Update TB_Usage_Auto_Check_Result Set {0} = '{1}' 
                                            Where company_name = '{2}' 
                                                AND service_type = '{3}' 
                                                AND resource_name = '{4}'", colname, colval, company_name, servicetype, resourcename);
            dbmanager.ExecuteDataQuery(updateq, "Asset_Management");
        }

        /*
         * Total rows값을 읽어 표기되는 NO 를 순차적으로 DataTable에 반영 
             */
        public void Insert_Aws_GridData_I(string Type, string company_name, string servicetype, int rowcnt)
        {
            DataSet Aws_Dataset = new DataSet();
            DataTable Aws_Table = new DataTable();
            IJavaScriptExecutor js = bspm.driver as IJavaScriptExecutor;

            string path = string.Format(".//div[@id='ps-{0}']//span[@class='total']", servicetype.Replace("azu", "azure"));
            //IWebElement totalitem = bspm.findelementitem(".//span[@class='total']");
            IWebElement totalitem = bspm.findelementitem(path);
            int totalrownum = Convert.ToInt32(totalitem.Text.Split('/')[1].Replace(" ", "")) - 1;

            string tablename = string.Format("TB_Usage_Data_{0}_{1}", servicetype, Type);
            string Createtablequery = string.Empty;
            string insertqry = string.Format("Insert Into {0}( $col )", tablename);
            //string rownum = string.Empty;
            int max = 0;
            int evenroww = 0;
            int oddroww = 1;
            // 1. odd Row
            IWebElement grid_aws = bspm.findelementitem(string.Format(".//div[@id='grid-{0}']", servicetype));
            //foreach (IWebElement aws in grid_aws)
            {
                IWebElement aws_item1 = grid_aws.FindElement(By.XPath(".//div[@id='borderLayout_eGridPanel']/div/div/div[@class='ag-header']/div[@class='ag-header-viewport']"));
                js.ExecuteScript("arguments[0].scrollIntoView(true);", aws_item1);

                Thread.Sleep(300);
                string[] healerlist = aws_item1.Text.Replace("\r\n", "|").Split('|');
                string col = string.Empty;
                for (int i = 0; i < healerlist.Length; i++)
                {
                    if (i == 0)
                    {
                        Createtablequery = Createtablequery + string.Format(" {0} INT NULL", healerlist[i].Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                        col = col + string.Format("{0}", healerlist[i].Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                    }
                    else
                    {
                        Createtablequery = Createtablequery + string.Format(", {0} NVARCHAR(300) NULL", healerlist[i].Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                        col = col + string.Format(",{0}", healerlist[i].Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                    }
                    Aws_Table.Columns.Add(healerlist[i].Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                }
                Aws_Table.Columns.Add("company_name");
                Createtablequery = Createtablequery + string.Format(", company_name NVARCHAR(100) NULL");
                col = col + string.Format(",{0}", "company_name");

                insertqry = insertqry.Replace("$col", col);
                CreateTable(tablename, Createtablequery);

                IWebElement focu = bspm.findelementitem(string.Format(".//bg-use-{0}-curr//div[@class='usage-chart']", servicetype));
                js.ExecuteScript("arguments[0].scrollIntoView(true);", focu);

                Thread.Sleep(200);
                if (rowcnt > 40)
                {
                    insert_csv_data(Aws_Table, company_name);
                    insertTableData_inDB(Aws_Table, insertqry);
                }
                else
                {
                    if (totalrownum % 2 == 1)
                    {
                        totalrownum = totalrownum + 1;
                    }

                    //홀수 로우
                    ReadOnlyCollection<IWebElement> aws_item2 = grid_aws.FindElements(By.XPath(".//div[@id='borderLayout_eGridPanel']/div/div/div[@class='ag-body']/div[@class='ag-body-viewport-wrapper']"));
                    //foreach (IWebElement aws_body in aws_item2)

                    for (int H = 0; H < totalrownum + 1; H = H + 2)
                    {
                        Thread.Sleep(1000);
                        IWebElement br1 = bspm.findelementitem(string.Format(".//div[@id='grid-{0}']//div/div[@class='ag-body-container']", servicetype));
                        Thread.Sleep(1000);
                        ReadOnlyCollection<IWebElement> bdr1 = br1.FindElements(By.XPath(".//div[@class='ag-row ag-row-no-focus ag-row-no-animation ag-row-level-0 ag-row-even']"));
                        Thread.Sleep(1000);
                        foreach (IWebElement item in bdr1)
                        {
                            string kmk1 = item.GetAttribute("row");
                            if (evenroww.ToString() == kmk1)
                            {
                                Aws_Table.Rows.Add(Table_insert_Date(Aws_Table, item, company_name));
                                evenroww = evenroww + 2;
                                js.ExecuteScript("arguments[0].scrollIntoView(true);", item);
                                break;
                            }
                        }
                        if (totalrownum < 5)
                        {
                            Thread.Sleep(1700);
                        }
                        Thread.Sleep(1000);
                        IWebElement br2 = bspm.findelementitem(string.Format(".//div[@id='grid-{0}']//div/div[@class='ag-body-container']", servicetype));
                        Thread.Sleep(1000);
                        ReadOnlyCollection<IWebElement> bdr = br2.FindElements(By.XPath(".//div[@class='ag-row ag-row-no-focus ag-row-no-animation ag-row-level-0 ag-row-odd']"));
                        Thread.Sleep(1000);
                        foreach (IWebElement item in bdr)
                        {
                            string kmk1 = item.GetAttribute("row");
                            if (oddroww.ToString() == kmk1)
                            {
                                Aws_Table.Rows.Add(Table_insert_Date(Aws_Table, item, company_name));
                                oddroww = oddroww + 2;
                                js.ExecuteScript("arguments[0].scrollIntoView(true);", item);
                                break;
                            }
                        }

                    }
                    insertTableData_inDB(Aws_Table, insertqry);

                    Up_Scrolling_Grid(Aws_Table.Rows.Count, string.Format(".//div[@id='grid-{0}']//div/div[@class='ag-body-container']", servicetype), ".//div[@class='ag-row ag-row-no-focus ag-row-no-animation ag-row-level-0 ag-row-even']", js);

                }

                string jh = string.Format("exec prc_Usage_Auto_Check_I_ba '{0}','{1}','{2}'", company_name, servicetype, Type);
                dbmanager.ExecuteDataQuery(jh, "Asset_Management");
                //hahaha
                Result_Check_Usage_1(tablename, company_name);
            }


        }

        public void Insert_Aws_Usage_Chart_json(string company_name, string servicetype, string resourcename)
        {
            string json_main_string = @"{ 'Instances':'$Instances', 'STATUS':{$STATUS},'REGIONS':{$REGIONS},'TYPES':{$TYPES} }";
            string qry = "exec prc_Usage_Chart_Data_I $Param";
            Thread.Sleep(3500);
            //ReadOnlyCollection<IWebElement> ChartStatus = bspm.findelementitems(string.Format(".//div[@id='ps-{0}']//dl[@class='chart-summary status']/dd/chart-pie-use","aws"));
            ReadOnlyCollection<IWebElement> ChartStatus = bspm.findelementitems(string.Format(".//div[@id='ps-{0}']//div[@class='usage-chart']//ul/li", servicetype.Replace("azu", "azure")));
            foreach (IWebElement item in ChartStatus)
            {
                string key = item.FindElement(By.XPath(".//dt")).Text;


                string text = string.Empty;
                Thread.Sleep(1000);
                if (item.FindElements((By.XPath(string.Format(".//dd/chart-pie-use", "aws")))).Count > 0)
                {
                    string[] values = item.FindElement((By.XPath(string.Format(".//dd/chart-pie-use", "aws")))).Text.Replace("\r\n", "|").Split('|');
                    json_main_string = json_main_string.Replace("$Instances", values[0]);

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i > 1)
                        {
                            if (i % 2 == 0)
                            {
                                string key_ = values[i];
                                if (text == string.Empty)
                                    text = text + string.Format("'{0}':", key_);
                                else
                                    text = text + string.Format(",'{0}':", key_);

                            }
                            else
                            {
                                string value_ = values[i].Replace(" EA", "");
                                text = text + string.Format("'{0}'", value_);
                            }
                        }
                    }
                    if (text == string.Empty)
                        text = "'NON':'NON'";
                    if (key == "STATUS")
                        json_main_string = json_main_string.Replace("$STATUS", text);
                    if (key == "REGIONS")
                        json_main_string = json_main_string.Replace("$REGIONS", text);
                    if (key == "TYPES")
                        json_main_string = json_main_string.Replace("$TYPES", text);

                    text = string.Empty;
                }

            }
            json_main_string = json_main_string.Replace("$STATUS", "'NON':'NON'");
            json_main_string = json_main_string.Replace("$REGIONS", "'NON':'NON'");
            json_main_string = json_main_string.Replace("$TYPES", "'NON':'NON'");
            string str = string.Format("'{0}','{1}','{2}','{3}'", company_name, servicetype, resourcename, json_main_string.Replace("'", "\""));
            dbmanager.ExecuteDataQuery(qry.Replace("$Param", str), "Asset_Management");
        }

        public void Insert_Aws_Usage_Chart(string company_name, string servicetype, string resourcename)
        {
            //ReadOnlyCollection<IWebElement> ChartStatus = bspm.findelementitems(string.Format(".//div[@id='ps-{0}']//dl[@class='chart-summary status']/dd/chart-pie-use","aws"));
            ReadOnlyCollection<IWebElement> ChartStatus = bspm.findelementitems(string.Format(".//div[@id='ps-{0}']//div[@class='usage-chart']//ul/li", servicetype));
            foreach (IWebElement item in ChartStatus)
            {
                string key = item.FindElement(By.XPath(".//dt")).Text;

                string qry = "exec prc_Usage_Chart_Data_I $Param";
                string text = string.Empty;
                Thread.Sleep(1000);
                if (item.FindElements((By.XPath(string.Format(".//dd/chart-pie-use", "aws")))).Count > 0)
                {
                    string[] values = item.FindElement((By.XPath(string.Format(".//dd/chart-pie-use", "aws")))).Text.Replace("\r\n", "|").Split('|');
                    string str = string.Format("'{0}','{1}','{2}','{3}',{4}", company_name, servicetype, resourcename, key, values[0]);
                    for (int i = 0; i < values.Length; i++)
                    {

                        if (i > 1)
                        {
                            if (i % 2 == 0)
                            {
                                string key_ = values[i];
                                text = text + string.Format(",'{0}'", key_);
                            }
                            else
                            {
                                string value_ = values[i].Replace(" EA", "");
                                text = text + string.Format(",{0}", value_);
                                dbmanager.ExecuteDataQuery(qry.Replace("$Param", str + text), "Asset_Management");
                                text = string.Empty;
                            }
                        }

                    }
                }
            }
        }

        public void insertTableData_inDB(DataTable dt, string qrystring)
        {
            if (dt.Rows.Count > 0)
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string rowsdatastring = string.Format("Values( $val )");
                    string rowstr = string.Empty;
                    DataRow dr = dt.Rows[i];
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j == 0)
                            rowstr = rowstr + string.Format("{0}", dr[j].ToString().Replace("'", ""));
                        else
                            rowstr = rowstr + string.Format(",'{0}'", dr[j].ToString().Replace("'", ""));
                    }
                    rowsdatastring = string.Format("{0} {1}", qrystring, rowsdatastring.Replace("$val", rowstr));
                    dbmanager.ExecuteDataQuery(rowsdatastring, "Asset_Management");
                }
            }
        }

        public void Up_Scrolling_Grid(int rowcont, string container, string row_class, IJavaScriptExecutor js)
        {
            IWebElement bodyrow3 = bspm.findelementitem(container);
            ReadOnlyCollection<IWebElement> bodyrows3 = bodyrow3.FindElements(By.XPath(".//div[@class='ag-row ag-row-no-focus ag-row-no-animation ag-row-level-0 ag-row-even']"));
            int minrow = 1000;
            for (int i = 0; i < rowcont / 2; i++)
            {
                if (minrow != 1)
                {
                    IWebElement test = bspm.findelementitem(container);
                    ReadOnlyCollection<IWebElement> test1 = test.FindElements(By.XPath(".//div[@class='ag-row ag-row-no-focus ag-row-no-animation ag-row-level-0 ag-row-even']"));
                    foreach (IWebElement item in test1)
                    {
                        if (minrow > Convert.ToInt32(item.GetAttribute("row")))
                        {
                            minrow = Convert.ToInt32(item.GetAttribute("row"));
                        }
                    }
                    IWebElement test3 = bspm.findelementitem(container);
                    ReadOnlyCollection<IWebElement> test4 = test3.FindElements(By.XPath(".//div[@class='ag-row ag-row-no-focus ag-row-no-animation ag-row-level-0 ag-row-even']"));
                    foreach (IWebElement item in test4)
                    {
                        if (minrow == Convert.ToInt32(item.GetAttribute("row")))
                        {
                            js.ExecuteScript("arguments[0].scrollIntoView(true);", item);
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                }
                else
                    break;

            }
        }

        public DataRow Table_insert_Date(DataTable Aws_Table, IWebElement row, string company_name)
        {
            DataRow dRow = Aws_Table.NewRow();
            //Aws_Table.Columns;
            ReadOnlyCollection<IWebElement> bodyrow = row.FindElements(By.XPath(".//div"));
            for (int i = 0; i < bodyrow.Count; i++)
            {
                IWebElement row_value = bodyrow[i];
                dRow[i] = row_value.Text;

            }
            dRow["company_name"] = company_name;

            return dRow;
        }

    }
}
