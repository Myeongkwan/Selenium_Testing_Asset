using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selenium_Testing_Asset.Asset_Management
{
    public class Dashboard_Item
    {
        /* Dashboard > Server item */
        string str_total_runing_svr;
        string str_total_stopped_svr;
        string str_total_etc_svr;
        //aws
        string str_aws_runing_svr;
        string str_aws_stopped_svr;
        string str_aws_etc_svr;
        //azure
        string str_azure_runing_sv;
        string str_azure_stopped_svr;
        string str_azure_etc_svr;
        //idc
        string str_idc_runing_svr;
        string str_idc_stopped_svr;
        string str_idc_etc_svr;

        /* Dashboard > DB or Storage item */
        // total
        string str_total_db_usage;
        string str_total_storage_usage;
        string str_total_disk_usage;
        //aws
        string str_aws_db_usage;
        string str_aws_storage_usage;
        string str_aws_disk_usage;
        //azure
        string str_azure_db_usage;
        string str_azure_storage_usage;
        string str_azure_disk_usage;
        //idc
        string str_idc_db_usage;
        string str_idc_storage_usage;
        string str_idc_disk_usage;

        /* Dashboard > Network item */
        //total
        string str_total_network;
        string str_total_loadbalancer;
        string str_total_networkip;
        //aws
        string str_aws_network;
        string str_aws_loadbalancer;
        string str_aws_networkip;
        //azure
        string str_azure_network;
        string str_azure_loadbalancer;
        string str_azure_networkip;
        //idc
        string str_idc_network;
        string str_idc_loadbalancer;
        string str_idc_networkip;

        string summary_total_server_1;
        string summary_total_server_2;
        string summary_total_server_3;
        string summary_total_ondemand;
        string summary_total_autoscaling;
        string summary_tatal_database_1;
        string summary_tatal_database_2;
        string summary_total_ondemand_dbserver;
        string resource_total;
        string resource_ec2_aws;
        string resource_virtualserver_azure;
        string resource_server_idc;
        string resource_rds_aws;
        string resource_sqlserver_azure;
        string resource_database_idc;
        string resource_s3_aws;
        string resource_storage_azure;
        string resource_ebs_aws;
        string resource_vpc_aws;
        string resource_loadbalancer_aws;
        string resource_eip_aws;
        string resource_virtualnetwork_azure;
        string resource_loadbalancer_azure;

        string server_total;
        string database_total;
        string network_total;

        #region Dashboard > Server item
        public string total_runing_svr
        {
            get { return str_total_runing_svr; }
            set { str_total_runing_svr = value; }
        }
        public string total_stopped_svr
        {
            get { return str_total_stopped_svr; }
            set { str_total_stopped_svr = value; }
        }
        public string total_etc_svr
        {
            get { return str_total_etc_svr; }
            set { str_total_etc_svr = value; }
        }

        public string aws_runing_svr
        {
            get { return str_aws_runing_svr; }
            set { str_aws_runing_svr = value; }
        }
        public string aws_stopped_svr
        {
            get { return str_aws_stopped_svr; }
            set { str_aws_stopped_svr = value; }
        }
        public string aws_etc_svr
        {
            get { return str_aws_etc_svr; }
            set { str_aws_etc_svr = value; }
        }
        public string azure_runing_svr
        {
            get { return str_azure_runing_sv; }
            set { str_azure_runing_sv = value; }
        }
        public string azure_stopped_svr
        {
            get { return str_azure_stopped_svr; }
            set { str_azure_stopped_svr = value; }
        }
        public string azure_etc_svr
        {
            get { return str_azure_etc_svr; }
            set { str_azure_etc_svr = value; }
        }

        public string idc_runing_svr
        {
            get { return str_idc_runing_svr; }
            set { str_idc_runing_svr = value; }
        }
        public string idc_stopped_svr
        {
            get { return str_idc_stopped_svr; }
            set { str_idc_stopped_svr = value; }
        }
        public string idc_etc_svr
        {
            get { return str_idc_etc_svr; }
            set { str_idc_etc_svr = value; }
        }
        #endregion

        #region Dashboard > DB or storage item
        public string total_db_usage
        {
            get { return str_total_db_usage; }
            set { str_total_db_usage = value; }
        }
        public string total_storage_usage
        {
            get { return str_total_storage_usage; }
            set { str_total_storage_usage = value; }
        }
        public string total_disk_usage
        {
            get { return str_total_disk_usage; }
            set { str_total_disk_usage = value; }
        }

        public string aws_db_usage
        {
            get { return str_aws_db_usage; }
            set { str_aws_db_usage = value; }
        }
        public string aws_storage_usage
        {
            get { return str_aws_storage_usage; }
            set { str_aws_storage_usage = value; }
        }
        public string aws_disk_usage
        {
            get { return str_aws_disk_usage; }
            set { str_aws_disk_usage = value; }
        }

        public string azure_db_usage
        {
            get { return str_azure_db_usage; }
            set { str_azure_db_usage = value; }
        }
        public string azure_storage_usage
        {
            get { return str_azure_storage_usage; }
            set { str_azure_storage_usage = value; }
        }
        public string azure_disk_usage
        {
            get { return str_azure_disk_usage; }
            set { str_azure_disk_usage = value; }
        }

        public string idc_db_usage
        {
            get { return str_idc_db_usage; }
            set { str_idc_db_usage = value; }
        }
        public string idc_storage_usage
        {
            get { return str_idc_storage_usage; }
            set { str_idc_storage_usage = value; }
        }
        public string idc_disk_usage
        {
            get { return str_idc_disk_usage; }
            set { str_idc_disk_usage = value; }
        }
        #endregion

        #region Dashboard > Network Item
        public string total_network
        {
            get { return str_total_network; }
            set { str_total_network = value; }
        }
        public string total_loadbalancer
        {
            get { return str_total_loadbalancer; }
            set { str_total_loadbalancer = value; }
        }
        public string total_networkip
        {
            get { return str_total_networkip; }
            set { str_total_networkip = value; }
        }

        public string aws_network
        {
            get { return str_aws_network; }
            set { str_aws_network = value; }
        }
        public string aws_loadbalancer
        {
            get { return str_aws_loadbalancer; }
            set { str_aws_loadbalancer = value; }
        }
        public string aws_networkip
        {
            get { return str_aws_networkip; }
            set { str_aws_networkip = value; }
        }

        public string azure_network
        {
            get { return str_azure_network; }
            set { str_azure_network = value; }
        }
        public string azure_loadbalancer
        {
            get { return str_azure_loadbalancer; }
            set { str_azure_loadbalancer = value; }
        }
        public string azure_networkip
        {
            get { return str_azure_networkip; }
            set { str_azure_networkip = value; }
        }

        public string idc_network
        {
            get { return str_idc_network; }
            set { str_idc_network = value; }
        }
        public string idc_loadbalancer
        {
            get { return str_idc_loadbalancer; }
            set { str_idc_loadbalancer = value; }
        }
        public string idc_networkip
        {
            get { return str_idc_networkip; }
            set { str_idc_networkip = value; }
        }
        #endregion

        #region auto check
        public string summary_total_server_1_
        {
            get { return summary_total_server_1; }
            set { summary_total_server_1 = value; }
        }
        public string summary_total_server_2_
        {
            get { return summary_total_server_2; }
            set { summary_total_server_2 = value; }
        }
        public string summary_total_server_3_
        {
            get { return summary_total_server_3; }
            set { summary_total_server_3 = value; }
        }
        public string summary_total_ondemand_
        {
            get { return summary_total_ondemand; }
            set { summary_total_ondemand = value; }
        }
        public string summary_total_autoscaling_
        {
            get { return summary_total_autoscaling; }
            set { summary_total_autoscaling = value; }
        }
        public string summary_tatal_database_1_
        {
            get { return summary_tatal_database_1; }
            set { summary_tatal_database_1 = value; }
        }
        public string summary_tatal_database_2_
        {
            get { return summary_tatal_database_2; }
            set { summary_tatal_database_2 = value; }
        }
        public string summary_total_ondemand_dbserver_
        {
            get { return summary_total_ondemand_dbserver; }
            set { summary_total_ondemand_dbserver = value; }
        }

        public string resource_total_
        {
            get { return resource_total; }
            set { resource_total = value; }
        }
        public string resource_ec2_aws_
        {
            get { return resource_ec2_aws; }
            set { resource_ec2_aws = value; }
        }
        public string resource_virtualserver_azure_
        {
            get { return resource_virtualserver_azure; }
            set { resource_virtualserver_azure = value; }
        }
        public string resource_server_idc_
        {
            get { return resource_server_idc; }
            set { resource_server_idc = value; }
        }
        public string resource_rds_aws_
        {
            get { return resource_rds_aws; }
            set { resource_rds_aws = value; }
        }
        public string resource_sqlserver_azure_
        {
            get { return resource_sqlserver_azure; }
            set { resource_sqlserver_azure = value; }
        }
        public string resource_database_idc_
        {
            get { return resource_database_idc; }
            set { resource_database_idc = value; }
        }
        public string resource_s3_aws_
        {
            get { return resource_s3_aws; }
            set { resource_s3_aws = value; }
        }
        public string resource_storage_azure_
        {
            get { return resource_storage_azure; }
            set { resource_storage_azure = value; }
        }
        public string resource_ebs_aws_
        {
            get { return resource_ebs_aws; }
            set { resource_ebs_aws = value; }
        }
        public string resource_vpc_aws_
        {
            get { return resource_vpc_aws; }
            set { resource_vpc_aws = value; }
        }
        public string resource_loadbalancer_aws_
        {
            get { return resource_loadbalancer_aws; }
            set { resource_loadbalancer_aws = value; }
        }
        public string resource_eip_aws_
        {
            get { return resource_eip_aws; }
            set { resource_eip_aws = value; }
        }
        public string resource_virtualnetwork_azure_
        {
            get { return resource_virtualnetwork_azure; }
            set { resource_virtualnetwork_azure = value; }
        }
        public string resource_loadbalancer_azure_
        {
            get { return resource_loadbalancer_azure; }
            set { resource_loadbalancer_azure = value; }
        }

        public string server_total_
        {
            get { return server_total; }
            set { server_total = value; }
        }
        public string database_total_
        {
            get { return database_total; }
            set { database_total = value; }
        }
        public string network_total_
        {
            get { return network_total; }
            set { network_total = value; }
        }
        #endregion
    }
}
