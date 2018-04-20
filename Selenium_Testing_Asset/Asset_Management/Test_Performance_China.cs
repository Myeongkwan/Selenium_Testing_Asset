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
    public class Test_Performance_China
    {
        Bsp_Manager bspm = new Bsp_Manager();
        DBManager dbmanager = new DBManager();
        LogManager logman = new LogManager();
        string csvdownloadpath = string.Empty;

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
            DataSet ds = dbmanager.ExecuteDataQuery(querystring, "Asset_Management_china");

            return ds;
        }

        public void dropTable(string tablename)
        {

            string isexistqry = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tablename);
            DataSet ds = dbmanager.ExecuteDataQuery(isexistqry, "Asset_Management_china");
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string drop_table = string.Format("drop Table {0}", tablename);
                    dbmanager.ExecuteDataQuery(drop_table, "Asset_Management_china");
                }
            }
        }

        public void deleteTable(string tablename, string company_name)
        {
            string isexistqry = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tablename);
            DataSet ds = dbmanager.ExecuteDataQuery(isexistqry, "Asset_Management_china");
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string Deletetb = string.Format("Delete from {0} where company_name = '{1}' ", tablename, company_name);
                    dbmanager.ExecuteDataQuery(Deletetb, "Asset_Management_china");
                }
            }
        }

        public void All_company_total_summary_ds(DataSet companylist, string taskno)
        {
            bspm.Bsp_Company_list();

            for (int i = 0; i < companylist.Tables[0].Rows.Count; i++)
            {
                DataRow dr = companylist.Tables[0].Rows[i];
                string company_name = dr["company_name"].ToString();
                bspm.Bsp_Select_Company(company_name, "PERFORMANCE");
                Thread.Sleep(8000);

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
                dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management_china");

                dbmanager.ExecuteDataQuery(string.Format("exec prc_Task_Status_Update '{0}'", taskno), "Asset_Management_china");
                //string reult = string.Format("exec prc_Usage_Result_ReportData '{0}'", company_name);
                //dbmanager.ExecuteDataQuery(reult, "Asset_Management_china");
            }
        }

        public void Click_Type(string servicetype, string company_name, string taskno)
        {
            IJavaScriptExecutor js = bspm.driver as IJavaScriptExecutor;
            string path = string.Format(".//bg-perf-{0}-curr//div[@class='common-tabs']", servicetype);
            string upqy = string.Format("{0}|", servicetype);
            Thread.Sleep(4000);
            IWebElement Tab = bspm.findelementitem(path);
            Thread.Sleep(1000);
            ReadOnlyCollection<IWebElement> type_tab = Tab.FindElements(By.XPath(".//button"));
            foreach (IWebElement item in type_tab)
            {
                IWebElement Tabvdv = bspm.findelementitem(string.Format(".//button[@id='{0}Toggle']", servicetype));
                js.ExecuteScript("arguments[0].scrollIntoView(true);", Tabvdv);
                Thread.Sleep(1000);
                string selectsource = string.Format("exec prc_TB_Auto_Process_Task_Q '{0}','{1}'", taskno, company_name);
                DataSet ds = dbmanager.ExecuteDataQuery(selectsource, "Asset_Management_china");
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        if (dr["process_status"].ToString() == "NON")
                        {
                            //log
                            logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : START", company_name, servicetype, item.Text.Replace(" ", "")));
                            deleteTable(string.Format("TB_Performance_Data_{0}_{1}", servicetype, item.Text.Replace(" ", "")), company_name);
                            Thread.Sleep(1000);
                            item.Click();
                            Thread.Sleep(21000);
                            Insert_instance_status(company_name, servicetype, item.Text.Replace(" ", ""));
                            Thread.Sleep(1000);
                            Insert_Aws_Usage_Chart_json(company_name, servicetype, item.Text.Replace(" ", ""));
                            Thread.Sleep(1000);
                            InsertAws_Performance_Grid_Data(company_name, servicetype, item.Text.Replace(" ", ""));

                            upqy = upqy + item.Text.Replace(" ", "");
                            string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskno, company_name, upqy);
                            dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management_china");

                            //log
                            logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : END", company_name, servicetype, item.Text.Replace(" ", "")));
                        }
                        else
                        {
                            string options = dr["process_status"].ToString();
                            if (!options.Contains(item.Text.Replace(" ", "")))
                            {
                                //log
                                logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : START", company_name, servicetype, item.Text.Replace(" ", "")));
                                //dropTable(string.Format("TB_Usage_Data_{0}_{1}", servicetype, item.Text.Replace(" ","")));
                                deleteTable(string.Format("TB_Performance_Data_{0}_{1}", servicetype, item.Text.Replace(" ", "")), company_name);
                                Thread.Sleep(1000);
                                item.Click();
                                Thread.Sleep(21000);
                                Insert_instance_status(company_name, servicetype, item.Text.Replace(" ", ""));
                                Thread.Sleep(1000);
                                Insert_Aws_Usage_Chart_json(company_name, servicetype, item.Text.Replace(" ", ""));
                                Thread.Sleep(1000);
                                InsertAws_Performance_Grid_Data(company_name, servicetype, item.Text.Replace(" ", ""));
                                string upqyrce = string.Format("exec prc_TB_Auto_Process_Task_U '{0}','{1}','{2}'", taskno, company_name, options + "," + item.Text.Replace(" ", ""));
                                dbmanager.ExecuteDataQuery(upqyrce, "Asset_Management_china");

                                //log
                                logman.AppendLogFile(taskno, string.Format("Company Name :{0}, Service Type : {1}, Resouce Name : {2}, Status : END", company_name, servicetype, item.Text.Replace(" ", "")));
                            }
                        }
                    }
                }
            }
        }

        public void CreateTable(string tablename, string Query)
        {
            tablename = tablename.Replace(" ", "");
            string isexistqry = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tablename);
            DataSet ds = dbmanager.ExecuteDataQuery(isexistqry, "Asset_Management_china");
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
                    dbmanager.ExecuteDataQuery(qry, "Asset_Management_china");
                }
            }
            else
            {
                string qry = string.Format(@"Create Table {0} (
                                            {1}
                                            )", tablename, Query);
                dbmanager.ExecuteDataQuery(qry, "Asset_Management_china");
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
                        if (dr[j].ToString() == string.Empty)
                        {
                            dr[j] = 0;
                        }
                        if (j == 0)
                            rowstr = rowstr + string.Format("{0}", dr[j]);
                        else
                            rowstr = rowstr + string.Format(",'{0}'", dr[j]);
                    }
                    rowsdatastring = string.Format("{0} {1}", qrystring, rowsdatastring.Replace("$val", rowstr));
                    dbmanager.ExecuteDataQuery(rowsdatastring, "Asset_Management_china");
                }
            }
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

        public void Insert_instance_status(string company_name, string servicetype, string resourcename)
        {
            Thread.Sleep(500);
            IWebElement total = bspm.findelementitem(string.Format(".//bg-perf-{0}-curr", servicetype));
            IWebElement val = total.FindElement(By.XPath(".//div[@class='total-instance']//div[@class='values']"));
            Thread.Sleep(500);
            string[] valstring = val.Text.Replace("ea", "").Split(' ');

            string qrystring = string.Format("exec prc_Performance_instance_Status_I '{0}','{1}','{2}',{3},{4},{5},{6}"
                , company_name
                , servicetype
                , resourcename
                , valstring[0]
                , valstring[2]
                , valstring[4]
                , valstring[6]);
            dbmanager.ExecuteDataQuery(qrystring, "Asset_Management_china");
        }

        public void Insert_Aws_Usage_Chart_json(string company_name, string servicetype, string resourcename)
        {
            string json_main_string = @"{ '$Chart1':{$Chart_val1}, '$Chart2':{$Chart_val2},'$Chart3':{$Chart_val3} }";
            ReadOnlyCollection<IWebElement> ChartStatus = bspm.findelementitems(string.Format(".//div[@id='ps-{0}']//div[@class='usage-chart performance']//ul/li", servicetype.Replace("azu", "azure")));
            for (int i = 0; i < ChartStatus.Count; i++)
            {
                IWebElement title = ChartStatus[i].FindElement(By.XPath(".//dt"));
                string charttitle = string.Empty;
                if (resourcename == "EC2" || resourcename == "VirtualMachine")
                {
                    if (title.Text.Contains("CPU"))
                        charttitle = "CPU Utilization";
                    else if (title.Text.Contains("MEMORY"))
                        charttitle = "MEMORY Utilization";
                    else if (title.Text.Contains("DISK"))
                        charttitle = "DISK SPACE Utilization";
                    else
                        charttitle = title.Text;
                }
                else if (resourcename == "RDS")
                {
                    if (title.Text.Contains("CPU"))
                        charttitle = "CPU Utilization";
                    else if (title.Text.Contains("MEMORY"))
                        charttitle = "Freeable Memory";
                    else if (title.Text.Contains("STORAGE"))
                        charttitle = "Free Storage Space";
                    else
                        charttitle = title.Text;
                }
                else if (resourcename == "SQLDatabases")
                {
                    if (title.Text.Contains("CPU"))
                        charttitle = "CPU Utilization";
                    else if (title.Text.Contains("IO PERCENT"))
                        charttitle = "Physical Data Read Percent";
                    else if (title.Text.Contains("CONSUMPTION"))
                        charttitle = "DTU Consumption";
                    else
                        charttitle = title.Text;
                }
                else
                {
                    charttitle = title.Text;
                }


                json_main_string = json_main_string.Replace(string.Format("$Chart{0}", i + 1), charttitle);
                Thread.Sleep(500);
                string chartstring = string.Empty;
                ReadOnlyCollection<IWebElement> val1 = ChartStatus[i].FindElements(By.XPath(".//chart-pie-perf"));
                if (val1.Count > 0)
                {
                    IWebElement chart_val = val1[0];
                    string[] cstring = chart_val.Text.Replace("\r\n", "|").Split('|');
                    if (cstring.Length > 3)
                    {
                        chartstring = chartstring + string.Format("'{0}':'{1}'", cstring[3], cstring[2]);
                    }
                    else
                    {
                        chartstring = chartstring + string.Format("'{0}':'{1}'", cstring[2], cstring[1]);
                    }
                }

                ReadOnlyCollection<IWebElement> val2 = ChartStatus[i].FindElements(By.XPath(".//dd/dl"));
                if (val2.Count > 0)
                {
                    IWebElement chart_val = val2[0];
                    string[] cstring = chart_val.Text.Replace("\r\n", "|").Split('|');
                    string text = string.Empty;
                    if (chartstring != string.Empty)
                    {

                        for (int j = 0; j < cstring.Length; j++)
                        {

                            if (j % 2 == 0)
                            {
                                string key_ = cstring[j];
                                text = text + string.Format(",'{0}':", key_);

                            }
                            else
                            {
                                string value_ = cstring[j].Replace(" %", "").Replace(" MB", "").Replace(" iops", "").Replace(" EA", "");
                                text = text + string.Format("'{0}'", value_);
                            }

                        }
                    }
                    else
                    {
                        for (int j = 0; j < cstring.Length; j++)
                        {

                            if (j % 2 == 0 && j == 0)
                            {
                                string key_ = cstring[j];
                                text = text + string.Format("'{0}':", key_);

                            }
                            else if (j % 2 == 0)
                            {
                                string key_ = cstring[j];
                                text = text + string.Format(",'{0}':", key_);
                            }
                            else
                            {
                                string value_ = cstring[j].Replace(" %", "").Replace(" MB", "").Replace(" iops", "").Replace(" EA", "");
                                text = text + string.Format("'{0}'", value_);
                            }

                        }
                    }
                    chartstring = chartstring + text;
                    string sdf = string.Format("$Chart_val{0}", i + 1);
                    json_main_string = json_main_string.Replace(sdf, chartstring);
                }
                else
                {
                    chartstring = chartstring + string.Format("'NON':'NON'");
                    string sdf = string.Format("$Chart_val{0}", i + 1);
                    json_main_string = json_main_string.Replace(sdf, chartstring);
                }

            }
            string qry = string.Format("exec prc_Performance_Chart_Data_I '{0}','{1}','{2}','{3}'", company_name, servicetype, resourcename, json_main_string.Replace("'", "\""));
            dbmanager.ExecuteDataQuery(qry, "Asset_Management_china");
        }

        public void InsertAws_Performance_Grid_Data(string company_name, string servicetype, string resourcename)
        {
            IWebElement grid = bspm.findelementitem(string.Format("//div[@id='grid-{0}']", servicetype));
            ReadOnlyCollection<IWebElement> grid_header = grid.FindElements(By.XPath(".//div[@class='ag-header-container']/div[@class='ag-header-row']"));

            List<string> list = new List<string>();
            List<string> tablecolumns = new List<string>();
            int dccnt = 0;
            for (int i = 0; i < grid_header.Count; i++)
            {
                IWebElement headeritem = grid_header[i];
                if (headeritem.Text != "")
                {
                    string[] headersplit = headeritem.Text.Replace("\r\n", "|").Split('|'); ;
                    if (i == 0)
                    {
                        for (int j = 0; j < headersplit.Length; j++)
                        {
                            if (j != 0)
                            {
                                if (j != headersplit.Length)
                                {
                                    list.Add(headersplit[j].Replace(" ", "").Replace("(%)", ""));
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < headersplit.Length; j++)
                        {
                            string colname = string.Empty;
                            if (headersplit[j] == "Min" || headersplit[j] == "Max" || headersplit[j] == "Average")
                            {
                                colname = list[dccnt] + "_" + headersplit[j];
                                if (headersplit[j] == "Average")
                                    dccnt++;
                            }
                            else if (headersplit[j] == "Avg")
                            {
                                colname = list[dccnt] + "_Average";
                                if (headersplit[j] == "Avg")
                                    dccnt++;
                            }
                            else
                            {
                                colname = headersplit[j];
                            }
                            tablecolumns.Add(colname);
                        }
                    }
                }
            }
            string tablename = string.Format("TB_Performance_Data_{0}_{1}", servicetype, resourcename);
            string insertQry = string.Format("INSERT INTO {0} ($col) ", tablename);
            string tablestring = string.Empty;
            string colqry = string.Empty;
            DataTable dt = new DataTable();
            for (int i = 0; i < tablecolumns.Count(); i++)
            {
                dt.Columns.Add(tablecolumns[i].Replace(" ", "_").Replace("(EA)", "").Replace("(MB)", "").Replace("(%)", "").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                if (i == 0)
                {
                    tablestring = tablestring + string.Format("{0} INT NULL", tablecolumns[i].Replace(" ", "_").Replace("(EA)", "").Replace("(MB)", "").Replace("(%)", "").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                    colqry = colqry + string.Format("{0}", tablecolumns[i].Replace(" ", "_").Replace("(EA)", "").Replace("(MB)", "").Replace("(%)", "").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                }
                else
                {
                    tablestring = tablestring + string.Format(", {0} NVARCHAR(200) NULL", tablecolumns[i].Replace(" ", "_").Replace("(EA)", "").Replace("(MB)", "").Replace("(%)", "").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                    colqry = colqry + string.Format(",{0}", tablecolumns[i].Replace(" ", "_").Replace("(EA)", "").Replace("(MB)", "").Replace("(%)", "").Replace("(", "").Replace(")", "").Replace("Default", "Def").Replace("-", "_"));
                }


            }
            dt.Columns.Add("company_name");

            tablestring = tablestring + string.Format(", {0} NVARCHAR(200) NULL", "company_name");
            colqry = colqry + string.Format(",{0}", "company_name");
            insertQry = insertQry.Replace("$col", colqry);

            CreateTable(tablename, tablestring);

            Thread.Sleep(1000);
            bspm.findelementitem(string.Format(".//div[@id='ps-{0}']//button[@class='button-icon-txt icon-download']", servicetype.Replace("azu", "azure"))).Click();
            Thread.Sleep(20000);
            string path = string.Format(@"{0}\export.csv", csvdownloadpath);
            //DataTable dt = CsvFileToDatatable(path, true);
            DataTable dts = ConvertCSVtoDataTable(path);
            for (int i = 1; i < dts.Rows.Count; i++)
            {
                DataRow newrow = dt.NewRow();
                newrow.ItemArray = dts.Rows[i].ItemArray;
                newrow["company_name"] = company_name;
                dt.Rows.Add(newrow);
            }

            Thread.Sleep(1000);
            File.Delete(string.Format(@"{0}\export.csv", csvdownloadpath));

            insertTableData_inDB(dt, insertQry);
            ResultData(company_name, servicetype, resourcename, tablename);
            Thread.Sleep(1000);
        }

        public void ResultData(string company_name, string servicetype, string resourcename, string tablename)
        {
            /* result 변수 */
            string instanceResult = string.Empty;
            string runResourceResultG = string.Empty;
            string runResourceResultC = string.Empty;
            string chart1Result = string.Empty;
            string chart2Result = string.Empty;
            string chart3Result = string.Empty;
            /* Table 결과 테이블 */
            string selectqry = string.Format("Select * From {0} Where company_name = '{1}'", tablename, company_name);
            DataSet ResultData_Grid = dbmanager.ExecuteDataQuery(selectqry, "Asset_Management_china");

            /* Chart 결과 테이블 */
            string chartqry = string.Format("SELECT * FROM [TB_Performance_Chart_Data] where company_name = '{0}' and servicetype = '{1}' and resourcename = '{2}'"
                , company_name, servicetype, resourcename);
            DataSet ResultChart = dbmanager.ExecuteDataQuery(chartqry, "Asset_Management_china");
            DataRow ChartData = ResultChart.Tables[0].Rows[0];
            JObject studentObj = JObject.Parse(ChartData["JsonData"].ToString());


            /* Status 결과 테이블 */
            string statusqry = string.Format("SELECT * FROM [TB_Performance_Instance_Status] Where company_name = '{0}' and servicetype = '{1}' and Resourcetype = '{2}'"
                , company_name, servicetype, resourcename);
            DataSet ResultStatus = dbmanager.ExecuteDataQuery(statusqry, "Asset_Management_china");
            DataRow statusData = ResultStatus.Tables[0].Rows[0];

            /* Run Resource cnt */
            string runcntQ = string.Empty;
            if (resourcename == "EC2" || resourcename == "RDS")
            {
                runcntQ = string.Format(@"SELECT Count(*) AS Cnt
                                          FROM [Asset_Management_china].[dbo].[TB_Performance_Data_{2}_{1}]
                                          where company_name = '{0}' and CPUUtilization_Min is not null ", company_name, resourcename, servicetype);
            }
            else
            {
                runcntQ = string.Format(@"SELECT Count(*) AS Cnt
                                          FROM [Asset_Management_china].[dbo].[TB_Performance_Data_{2}_{1}]
                                          where company_name = '{0}'", company_name, resourcename, servicetype);
            }

            DataSet runcntD = dbmanager.ExecuteDataQuery(runcntQ, "Asset_Management_china");
            DataRow runcntR = runcntD.Tables[0].Rows[0];

            if (Convert.ToInt32(statusData["total_instance"]) == Convert.ToInt32(statusData["running_instance"]) + Convert.ToInt32(statusData["stopped_instance"]) + Convert.ToInt32(statusData["etc_instance"]))
            {
                instanceResult = "PASS";
            }
            else
            {
                instanceResult = "FAIL";
            }

            if (Convert.ToInt32(statusData["running_instance"]) == Convert.ToInt32(runcntR["Cnt"]))
                runResourceResultG = "PASS";
            else
                runResourceResultG = "FAIL";

            int Chartcnt = 0;
            foreach (dynamic item in studentObj.Children().ToList())
            {
                string name = (string)item.Name;
                JObject val = (JObject)item.Value;

                string isPass = string.Empty;
                foreach (dynamic subitem in val.Children().ToList())
                {
                    string subname = (string)subitem.Name;
                    string subval = (string)subitem.Value;
                    isPass = string.Empty;
                    if (subname == "Instances")
                    {
                        if (statusData["running_instance"].ToString() == subval)
                            runResourceResultC = "PASS";
                        else
                            runResourceResultC = "FAIL";
                    }
                    else if (subname == "AVG") { }
                    else if (subname == "NON") { }
                    else
                    {
                        string qry = string.Format(@"SELECT ROUND({0}(Convert(float,$col)),2) AS 'Val' FROM [Asset_Management_china].[dbo].[{1}] Where  company_name = '{2}'"
                            , subname, tablename, company_name);

                        if (subname == "AVG")
                            subname = "Average";
                        string col = name.Replace(" ", "") + "_" + subname;
                        qry = qry.Replace("$col", col);

                        DataSet rs = dbmanager.ExecuteDataQuery(qry, "Asset_Management_china");
                        DataRow dr = rs.Tables[0].Rows[0];
                        if (Convert.ToDouble(dr["Val"].ToString()) == Convert.ToDouble(subval.Replace("%", "")))
                        {
                            isPass = "PASS";
                        }
                        else
                        {
                            isPass = "FAIL";
                        }
                    }
                }

                if (Chartcnt == 0)
                    chart1Result = isPass;
                else if (Chartcnt == 1)
                    chart2Result = isPass;
                else if (Chartcnt == 2)
                    chart3Result = isPass;

                Chartcnt++;
            }
            string insertResult = string.Format("exec prc_Performance_Auto_Check_Result '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}'"
                , company_name, servicetype, resourcename, instanceResult, runResourceResultC, runResourceResultG, chart1Result, chart2Result, chart3Result);
            dbmanager.ExecuteDataQuery(insertResult, "Asset_Management_china");

            dbmanager.ExecuteDataQuery(string.Format("exec prc_Performance_Result_ReportData '{0}'", company_name), "Asset_Management_china");
            //string test = string.Empty;
        }

        public void selectTable()
        {

        }

        public void Asset_Performance_Process(string ID, string url, string UserID, string Pwd, string downloadpath)
        {
            logman.AppendLogFile(ID, string.Format("Process START"));
            logman.AppendLogFile(ID, string.Format("Task ID : {0}", ID));

            string taskno = ID;
            csvdownloadpath = downloadpath;

            Connection_Dashboard(url, UserID, Pwd, downloadpath);
            bspm.Bsp_Company_list();
            All_company_total_summary_ds(Select_Companys(taskno), taskno);
            //ResultData("hanwha_smartcam", "aws", "EC2", "TB_Performance_Data_aws_EC2");
            //Click_Type("aws", "베스핀 컨설팅 본부", "20180130_Performance_1");
            logman.AppendLogFile(taskno, string.Format("Process END"));
        }
    }
}
