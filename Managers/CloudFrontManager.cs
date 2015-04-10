using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using HugeInc.CloudFront.Utils;
using Sitecore.Diagnostics;

namespace HugeInc.CloudFront.Managers
{
    public static class CloudFrontManager
    {
        public static void CloudBuster(List<string> items)
        {
            if (items.Count == 0) return;

            AmazonCloudFrontClient client = new AmazonCloudFrontClient(Configuration.AWSAccessKey, Configuration.AWSSecret, Amazon.RegionEndpoint.USEast1);
            
            // break number of objects into batches of AWSCDNBatchLimit items. Value can be altered through configuration file.
            
            int groups = items.Count / Configuration.AWSCDNBatchLimit + 1;
            int index = 0;
            int count = items.Count / groups;
            try
            {
                for(int i = 0; i < groups; i++)
                {
                    client.CreateInvalidation(new CreateInvalidationRequest
                    {
                        DistributionId = Configuration.AWSCDNDistributionId,
                        InvalidationBatch = new InvalidationBatch
                        {                            
                            Paths = new Paths
                            {
                                Quantity = count,
                                Items = items.GetRange(index, count)
                            },
                            CallerReference = DateTime.Now.Ticks.ToString()
                        }
                    });

                    Log.Info("Requested to clear CloudFront cache: " + String.Join(",", items.GetRange(index,count).ToArray()), new object());
                    index = count * (i + 1);
                }
                
            }
            catch (Exception ex)
            {
                Log.Error("Error sending CloudFront object invalidation request. " + ex.Message, ex);
            }
        }
    }
}
