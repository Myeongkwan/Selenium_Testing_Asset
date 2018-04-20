using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Selenium_Testing_Asset
{
    public class xmlParent
    {
        public static List<xmlChild> xmlitem;
    }

    public class xmlChild
    {
        public string Url;
        public string UserID;
        public string Password;
        public string nation;
        public string downloadpath;
        public string Asset_Dashboard;
        public string Asset_Usage;
        public string Asset_Performance;
        public string Asset_Resource;
    }

    class XmlManager
    {
        public static List<xmlChild> xmlLoad(string xmlPath)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlPath));
            XmlNodeList xl = xdoc.SelectNodes("/items");
            foreach (XmlNode xn in xl)
            {
                xmlParent.xmlitem = xmlChildData(xn);
            }
            return xmlParent.xmlitem;
        }

        public static List<xmlChild> xmlChildData(XmlNode scheduleNode)
        {
            List<xmlChild> result = new List<xmlChild>();

            XmlNodeList subXL = scheduleNode.SelectNodes("item");
            for (int s = 0; s < subXL.Count; s++)
            {
                XmlNode subXN = subXL[s];
                xmlChild ftype = new xmlChild();
                ftype.Url = subXN.Attributes["Url"].Value;
                ftype.UserID = subXN.Attributes["UserId"].Value;
                ftype.Password = subXN.Attributes["Password"].Value;
                ftype.nation = subXN.Attributes["nation"].Value;
                ftype.downloadpath = subXN.Attributes["downloadpath"].Value;
                ftype.Asset_Dashboard = subXN.Attributes["Asset_Dashboard"].Value;
                ftype.Asset_Usage = subXN.Attributes["Asset_Usage"].Value;
                ftype.Asset_Performance = subXN.Attributes["Asset_Performance"].Value;
                ftype.Asset_Resource = subXN.Attributes["Asset_Resource"].Value;
                result.Add(ftype);
            }
            return result;
        }
    }
}
