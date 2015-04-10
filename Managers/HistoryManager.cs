using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Links;
using Sitecore.Data;

namespace HugeInc.CloudFront.Managers
{
    public static class HistoryManager
    {
        private static string LastUpdate = "LastUpdate";

        public static void ProcessHistoryStorage(Database database, ref List<ID> _cacheQueue)
        {
            _cacheQueue.Clear();

            var utcNow = DateTime.UtcNow;
            var from = LastUpdateTime(database);

            var entrys = Sitecore.Data.Managers.HistoryManager.GetHistory(database, from, utcNow);
            List<ID> queue = new List<ID>();
            if (entrys.Count > 0)
            {
                foreach (var entry in entrys.Where(entry => !queue.Contains(entry.ItemId) && entry.Category == Sitecore.Data.Engines.HistoryCategory.Item))
                {
                    queue.Add(entry.ItemId);
                    database.Properties[LastUpdate] = DateUtil.ToIsoDate(entry.Created, true);
                }
            }
            _cacheQueue = queue;
            database.Properties[LastUpdate] = DateUtil.ToIsoDate(utcNow, true);
        }


        private static DateTime LastUpdateTime(Database database)
        {
            var lastUpdate = database.Properties[LastUpdate];

            if (lastUpdate.Length > 0)
            {
                return DateUtil.ParseDateTime(lastUpdate, DateTime.MinValue);
            }

            return DateTime.MinValue;
        }
    }
}
