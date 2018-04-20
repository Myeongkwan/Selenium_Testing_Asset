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
    public class Result_Check
    {
        Bsp_Manager bspm = new Bsp_Manager();
        DBManager dbmanager = new DBManager();
        LogManager logman = new LogManager();

        #region Dashboard - Resource 비교
        public void delete_resource_Table(string company_name)
        {
            string qry = string.Format("Delete from TB_Asset_Dashboard_Resource_Auto_Testing where company_name = '{0}'", company_name);
            dbmanager.ExecuteDataQuery(qry, "Asset_Management");
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
                    delete_resource_Table(item["company_name"].ToString());

                    Asset_Result_Dashboard_resource(item["company_name"].ToString());
                }
            }
        }

        public DataSet check_resource_name_Dash(string resource_name, string service_type)
        {
            string result_Name = string.Empty;
            string qry = string.Format("select * from [dbo].[TB_Asset_Resource_Data] where [dashboard_resource] ='{0}' and service_type='{1}'", resource_name, service_type);
            DataSet ResourceName_Set = dbmanager.ExecuteDataQuery(qry, "Asset_Management");

            return ResourceName_Set;
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
                    if (d_resourcename == "Virtual Network" && company_name == "SBCK_CI")
                    {
                        int ddd = 10;
                    }
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

                if (d_resourcename == "Auto Scaling Instances")
                {
                    isPass = "PASS";
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
        #endregion

        #region Usage - Resource 비교
        public void delete_usage_resource_Table(string company_name)
        {
            string qry = string.Format("Delete from TB_Asset_Usage_Resource_Auto_Testing where company_name = '{0}'", company_name);
            dbmanager.ExecuteDataQuery(qry, "Asset_Management");
        }

        public DataSet Asset_Usage_Resource_itemnamecheck(string usage_resourcename, string servicetype)
        {
            string qry = string.Format("select * from [dbo].[TB_Asset_Resource_Data] where [usage_resource] ='{0}' and service_type = '{1}'", usage_resourcename, servicetype);
            DataSet ds = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            return ds;
        }

        public void Asset_Result_Usage_resource_Auto()
        {
            string usagechart_qry = string.Format("Select * From [Asset_Management].[dbo].[TB_Usage_Chart_Data]");
            DataSet usagechar_dt = dbmanager.ExecuteDataQuery(usagechart_qry, "Asset_Management");
            if (usagechar_dt.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow item in usagechar_dt.Tables[0].Rows)
                {


                    string company_name = item["company_name"].ToString();
                    string status_col = item["status_colname"].ToString();
                    string tablename = string.Format("TB_Usage_Data_{0}_{1}", item["servicetype"], item["resourcename"]);
                    string resourceN = item["servicetype"].ToString();

                    int total_value = 0;
                    int running_value = 0;
                    // total 값을 저장
                    string totalkval_qry = string.Format("select Count(*) AS Cnt from {0} where company_name = '{1}' ", tablename, company_name);
                    string totalvall = dbmanager.ExecuteDataQuery(totalkval_qry, "Asset_Management").Tables[0].Rows[0][0].ToString();


                    string isPass = string.Empty;
                    string enabled = "True";

                    string resource_rsname = "";

                    //delete_usage_resource_Table(company_name);
                    List<string> total_status_cnt = new List<string>();
                    if (status_col != "NON")
                    {
                        /* 현재 자원에 대한 상태값을 가져온다 : ex running, stopped */
                        string rowdata = string.Format("select {0} from {1} where company_name = '{2}' group by {0}"
                        , status_col
                        , tablename
                        , company_name);
                        DataSet status_group = dbmanager.ExecuteDataQuery(rowdata, "Asset_Management");


                        for (int i = 0; i < status_group.Tables[0].Rows.Count; i++)
                        {
                            string val = status_group.Tables[0].Rows[i][0].ToString();
                            string val_ds = string.Format("select Count(*) AS Cnt from {1} where company_name = '{2}' and {0} = '{3}'"
                                , status_col
                                , tablename
                                , company_name
                                , val);
                            string cnt = dbmanager.ExecuteDataQuery(val_ds, "Asset_Management").Tables[0].Rows[0][0].ToString();
                            // running 갑과 stopped 값을 list에 저장한다.
                            total_status_cnt.Add(cnt);
                        }

                        // resource 메뉴의 경우 일반 값과 클래식 값 두가지가 있는 경우가 있기 때문에 해당의 경우 다중 합산이 필요
                        DataSet resourcen = Asset_Usage_Resource_itemnamecheck(item["resourcename"].ToString(), resourceN);

                        foreach (DataRow RN in resourcen.Tables[0].Rows)
                        {
                            string[] resource_rote = RN["resource_resource"].ToString().Split('|');
                            if (resource_rote.Length > 1)
                            {
                                string resourcetype = resource_rote[0];
                                string resource_string = resource_rote[1];
                                string reqry = string.Format("SELECT * FROM [Asset_Management].[dbo].[TB_Resource_instance] where company_name = '{0}' and resource_type = '{1}' and resource_name = '{2}'"
                                    , company_name, resourcetype, resource_string);

                                DataRow data = (DataRow)dbmanager.ExecuteDataQuery(reqry, "Asset_Management").Tables[0].Rows[0];
                                object run_val = data["Run_value"];
                                object total_val = data["total_value"];
                                enabled = data["enabled"].ToString();

                                resource_rsname = resource_string;
                                total_value = total_value + Convert.ToInt32(total_val);
                                running_value = running_value + Convert.ToInt32(run_val);
                            }
                            else
                            {
                                string reqry = string.Format("SELECT * FROM [Asset_Management].[dbo].[TB_Resource_instance] where company_name = '{0}' and resource_type = '{1}' and enabled = 'True'"
                                    , company_name, resource_rote[0]);
                                DataSet ds = dbmanager.ExecuteDataQuery(reqry, "Asset_Management");
                                resource_rsname = resource_rote[0];

                                foreach (DataRow i_1 in ds.Tables[0].Rows)
                                {
                                    object run_val = i_1["Run_value"];
                                    object total_val = i_1["total_value"];

                                    total_value = total_value + Convert.ToInt32(total_val);
                                    running_value = running_value + Convert.ToInt32(run_val);
                                }
                            }
                        }
                    }
                    else //if (status_col == "NON")
                    {
                        // resource 메뉴의 경우 일반 값과 클래식 값 두가지가 있는 경우가 있기 때문에 해당의 경우 다중 합산이 필요
                        DataSet resourcen = Asset_Usage_Resource_itemnamecheck(item["resourcename"].ToString(), resourceN);

                        foreach (DataRow RN in resourcen.Tables[0].Rows)
                        {
                            string[] resource_rote = RN["resource_resource"].ToString().Split('|');
                            if (resource_rote.Length > 1)
                            {
                                string resourcetype = resource_rote[0];
                                string resource_string = resource_rote[1];
                                string reqry = string.Format("SELECT * FROM [Asset_Management].[dbo].[TB_Resource_instance] where company_name = '{0}' and resource_type = '{1}' and resource_name = '{2}'"
                                    , company_name, resourcetype, resource_string);

                                DataRow data = (DataRow)dbmanager.ExecuteDataQuery(reqry, "Asset_Management").Tables[0].Rows[0];
                                object run_val = data["Run_value"];
                                object total_val = data["total_value"];
                                enabled = data["enabled"].ToString();
                                resource_rsname = resource_string;

                                if (total_val != "")
                                    total_value = total_value + Convert.ToInt32(total_val);
                                if (run_val != "")
                                    running_value = running_value + Convert.ToInt32(run_val);
                            }
                            else
                            {
                                string reqry = string.Format("SELECT * FROM [Asset_Management].[dbo].[TB_Resource_instance] where company_name = '{0}' and resource_type = '{1}' and enabled = 'True'"
                                    , company_name, resource_rote[0]);
                                DataSet ds = dbmanager.ExecuteDataQuery(reqry, "Asset_Management");
                                resource_rsname = resource_rote[0];

                                foreach (DataRow i_1 in ds.Tables[0].Rows)
                                {
                                    object run_val = i_1["Run_value"];
                                    object total_val = i_1["total_value"];

                                    total_value = total_value + Convert.ToInt32(total_val);
                                    running_value = running_value + Convert.ToInt32(run_val);
                                }
                            }
                        }

                    }

                    // 값 비교
                    if (total_value.ToString() == totalvall)
                    {
                        isPass = "PASS";
                    }
                    else
                        isPass = "FAIL";
                    /*
                    if(enabled.ToString() == "True")
                        isPass = "PASS";
                    else
                        isPass = "FAIL";
                        */
                    string insert_Data_qry = string.Format("exec prc_Asset_Usage_Resource_Auto_Testing_I '{0}','{1}','{2}','{3}','{4}','{5}','{6}'"
                    , company_name
                    , item["resourcename"]
                    , totalvall
                    , resource_rsname
                    , total_value
                    , enabled
                    , isPass);
                    dbmanager.ExecuteDataQuery(insert_Data_qry, "Asset_Management");
                }
            }
        }
        #endregion

        #region Performance - Resource 비교
        public DataSet Asset_Performance_Resource_itemnamecheck(string usage_resourcename, string servicetype)
        {
            string qry = string.Format("select * from [dbo].[TB_Asset_Resource_Data] where [performance_resource] ='{0}' and service_type = '{1}'", usage_resourcename, servicetype);
            DataSet ds = dbmanager.ExecuteDataQuery(qry, "Asset_Management");
            return ds;
        }

        public void Asset_Result_Performanc_Resource_Auto()
        {
            DataSet performanceresult = dbmanager.ExecuteDataQuery("SELECT [company_name] FROM [Asset_Management].[dbo].[TB_Performance_Instance_Status] group by company_name", "Asset_Management");
            foreach (DataRow company in performanceresult.Tables[0].Rows)
            {
                Asset_Result_Performanc_Resource(company["company_name"].ToString());
            }
        }

        public void Asset_Result_Performanc_Resource(string company_name)
        {
            string Performance_Instance_qry = string.Format("SELECT * FROM [TB_Performance_Instance_Status] where company_name = '{0}'", company_name);
            DataSet Performance_Instance_data = dbmanager.ExecuteDataQuery(Performance_Instance_qry, "Asset_Management");

            foreach (DataRow datarow in Performance_Instance_data.Tables[0].Rows)
            {
                // 1. Performance에서 취합된 데이터 읽어 오기
                string servicetype = datarow["servicetype"].ToString();
                string resourcetype = datarow["Resourcetype"].ToString();
                string totalinstance = datarow["total_instance"].ToString();
                string runninginstance = datarow["running_instance"].ToString();
                string stoppedinstance = datarow["stopped_instance"].ToString();
                string etcinstance = datarow["etc_instance"].ToString();

                string isPass = string.Empty;
                string enabled = "True";
                string resource_rsname = "";
                int total_value = 0;
                int running_value = 0;

                // 2. Service type 과 Resource type으로 Resource Tab 에서 취합된 데이터를 검색하여 읽어 오기
                DataSet dsl = Asset_Performance_Resource_itemnamecheck(resourcetype, servicetype);
                foreach (DataRow item in dsl.Tables[0].Rows)
                {
                    string[] resourcename = item["resource_resource"].ToString().Split('|');
                    if (resourcename.Length > 1)
                    {
                        string rs_resourcetype = resourcename[0];
                        string rs_resource_string = resourcename[1];
                        string reqry = string.Format("SELECT * FROM [Asset_Management].[dbo].[TB_Resource_instance] where company_name = '{0}' and resource_type = '{1}' and resource_name = '{2}'"
                            , company_name, rs_resourcetype, rs_resource_string);

                        DataRow data = (DataRow)dbmanager.ExecuteDataQuery(reqry, "Asset_Management").Tables[0].Rows[0];
                        object run_val = data["Run_value"];
                        object total_val = data["total_value"];
                        enabled = data["enabled"].ToString();

                        resource_rsname = rs_resource_string;
                        total_value = total_value + Convert.ToInt32(total_val);
                        running_value = running_value + Convert.ToInt32(run_val);
                    }
                    else
                    {
                        string reqry = string.Format("SELECT * FROM [Asset_Management].[dbo].[TB_Resource_instance] where company_name = '{0}' and resource_type = '{1}' and enabled = 'True'"
                            , company_name, resourcename[0]);
                        DataSet ds = dbmanager.ExecuteDataQuery(reqry, "Asset_Management");
                        resource_rsname = resourcename[0];

                        foreach (DataRow i_1 in ds.Tables[0].Rows)
                        {
                            object run_val = i_1["Run_value"];
                            object total_val = i_1["total_value"];

                            total_value = total_value + Convert.ToInt32(total_val);
                            running_value = running_value + Convert.ToInt32(run_val);
                        }
                    }
                }

                // 값 비교
                if (total_value.ToString() == totalinstance)
                {
                    if (runninginstance == running_value.ToString())
                    {
                        isPass = "PASS";
                    }
                    else
                    {
                        isPass = "FAIL";
                    }
                }
                else
                    isPass = "FAIL";

                // 비교 결과 DB 저장
                string insert_Data_qry = string.Format("exec prc_Asset_Performance_Resource_Auto_Testing_I '{0}','{1}','{2}','{3}','{4}','{5}','{6}'"
                    , company_name
                    , resourcetype
                    , totalinstance
                    , resource_rsname
                    , total_value
                    , enabled
                    , isPass);
                dbmanager.ExecuteDataQuery(insert_Data_qry, "Asset_Management");
            }
        }
        #endregion
    }
}
