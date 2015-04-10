using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Sitecore.Configuration;
using System.Xml;
using System.Text.RegularExpressions;

namespace HugeInc.CloudFront.Utils
{
    public static class Configuration
    {
        #region Public Members
        public static bool EnableModule { get { return bool.Parse(Setting("EnableModule")); } }
        public static string AWSAccessKey { get { return Setting("AWSAccessKey"); } }
        public static string AWSSecret { get { return Setting("AWSSecret"); } }
        public static string AWSCDNDistributionId { get { return Setting("AWSCDNDistributionId"); } }
        public static int AWSCDNBatchLimit { get { return int.Parse(Setting("AWSCDNBatchLimit")); } } 
        public static string TargetDatabase { get { return Setting("TargetDatabase"); } }
        public static Regex[] ExcludeUrlPatterns { get { return ExcludeUrls(); } }
        public static Dictionary<string, string> PathReplacements { get { return ReplaceUrlPaths(); } }
        #endregion

        #region Privates
        private static Regex[] _excludeUrls;
        private static Dictionary<string, string> _replacePaths;
        
        private static readonly object _lock = new object();

        private static Regex[] ExcludeUrls()
        {

            if (_excludeUrls == null)
            {
                lock (_lock)
                {
                    if (_excludeUrls == null)
                    {
                        List<Regex> regexes = new List<Regex>();
                        foreach (XmlNode regexNode in Factory.GetConfigNodes("HugeInc/CloudFront/ExcludeUrls/Url"))
                        {
                            string pattern = Sitecore.Xml.XmlUtil.GetAttribute("regex", regexNode);
                            if (!string.IsNullOrEmpty(pattern))
                            {
                                regexes.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                            }
                        }
                        _excludeUrls = regexes.ToArray();
                    }
                }
            }
            return _excludeUrls;
            
        }

        private static Dictionary<string,string> ReplaceUrlPaths()
        {
            if (_replacePaths == null)
            {
                Dictionary<string, string> paths = new Dictionary<string, string>();

                lock (_lock)
                {
                    if (_replacePaths == null)
                    {

                        foreach (XmlNode node in Factory.GetConfigNodes("HugeInc/CloudFront/ReplaceUrlPaths/Url"))
                        {
                            string key = Sitecore.Xml.XmlUtil.GetAttribute("replace", node);
                            string value = Sitecore.Xml.XmlUtil.GetAttribute("with", node) ?? string.Empty;
                            if (!string.IsNullOrEmpty(key))
                            {
                                paths.Add(key, value);
                            }
                        }
                        _replacePaths = paths;
                    }
                }
            }

            return _replacePaths;

        }

        private static string Setting(string key)
        {
            string value = string.Empty;
            foreach (XmlNode node in Factory.GetConfigNodes("HugeInc/CloudFront/Settings"))
            {
                XmlNode keyNode = node.SelectSingleNode(key);
                value = keyNode.InnerText;
                break;
            }
            
            return value;
        }

        #endregion
    }
}
