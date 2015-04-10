using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HugeInc.CloudFront.Utils
{
    public static class Helpers
    {
        public static List<string> Urls(List<string> allUrls)
        {
            List<string> cdnObjects = new List<string>();

            foreach (string url in allUrls) 
            {
                if (!Configuration.ExcludeUrlPatterns.Any(reg => reg.IsMatch(url)))
                    cdnObjects.Add(url);

            }
            
            foreach (var path in Configuration.PathReplacements)
            {
                for (int i = 0; i < cdnObjects.Count; i++) 
                {
                    if (cdnObjects[i].IndexOf(path.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                        cdnObjects[i] = cdnObjects[i].ToLower().Replace(path.Key.ToLower(), path.Value);
                }
            }

            return cdnObjects;
        }
    }
}
