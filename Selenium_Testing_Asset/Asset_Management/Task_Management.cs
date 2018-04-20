using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selenium_Testing_Asset.Asset_Management
{
    public class Task_Management
    {
        DBManager dbmanager = new DBManager();
        /******************************************/
        // 생성일자 : 2018-04-09 18:57
        // 수정일자 : 2018-04-09 18:57
        // 작성자 : 김명관
        // 작성자 : Global
        // 기능설명 : 실행시 자동으로 Task를 등록 
        /******************************************/
        public string Create_Task_Dashboard(string dbname, string menuname, string nation)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string taskID = string.Format("{0}_{1}_Schedule",today, menuname);

            if (menuname == "dashboard" || menuname == "resource")
            {
                string qryTask = string.Format("exec prc_TB_Auto_Process_Task_I '{0}','{1}','{2}'", taskID, taskID, menuname);
                dbmanager.ExecuteDataQuery(qryTask, dbname);
            }
            else
            {
                string qryTask = string.Format("exec prc_TB_Auto_Process_Task_I_Select_Company '{0}','{1}','{2}'", taskID, taskID, menuname);
                dbmanager.ExecuteDataQuery(qryTask, dbname);

                string companydb = string.Empty;
                if (nation == "global")
                    companydb = "Bsp_Management";
                else
                    companydb = "Bsp_Management_china";

                string comdbstr = string.Format("select company_name from [TB_Company_Info] where islarg = 1");
                DataSet ds = dbmanager.ExecuteDataQuery(comdbstr, companydb);

                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    string td = string.Format("Select COUNT(*) as cnt from TB_Auto_Process_Task_Company where task_no = '{0}' and company_name = '{1}'", taskID, item["company_name"]);
                    DataRow dff = dbmanager.ExecuteDataQuery(td, dbname).Tables[0].Rows[0];
                    if (Convert.ToInt32(dff["cnt"]) == 0)
                    {
                        string qry = string.Format("INSERT INTO TB_Auto_Process_Task_Company (task_no,company_name,process_status) Values('{0}','{1}','NON')", taskID, item["company_name"]);
                        dbmanager.ExecuteDataQuery(qry, dbname);
                    }
                    
                }

            }
            
            
            return taskID;
        }
    }
}
