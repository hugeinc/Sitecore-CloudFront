using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Publishing.Pipelines.Publish;
using System.Collections.Generic;
using System.Diagnostics;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore;
using Sitecore.Links;
using HugeInc.CloudFront.Utils;

namespace HugeInc.CloudFront.Pipelines
{
    public class CacheRefresh: PublishProcessor
    {
        private List<ID> _cacheQueue = new List<ID>();

        private readonly string LastUpdate = "LastUpdate";
        public override void Process(PublishContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            ProcessPublishedItems(context);
        }

        protected virtual void ProcessPublishedItems(PublishContext context)
        {
            if (!Configuration.EnableModule || context == null || context.PublishOptions == null || context.PublishOptions.TargetDatabase == null || context.PublishOptions.TargetDatabase.Name != Configuration.TargetDatabase)
                return;

            Managers.HistoryManager.ProcessHistoryStorage(context.PublishOptions.TargetDatabase, ref _cacheQueue);

            var urls = new List<string>();

            foreach (var id in _cacheQueue)
            {
                if (context.PublishOptions.TargetDatabase.Items.GetItem(id) != null)
                {
                    var item = context.PublishOptions.TargetDatabase.Items[id];
                    if (item != null && (item.Paths.IsMediaItem || item.Visualization.Layout != null))
                        urls.Add(GetUrl(item));
                }
            }

            if (urls.Count > 0)
            {
                Managers.CloudFrontManager.CloudBuster(HugeInc.CloudFront.Utils.Helpers.Urls(urls));
            }
        }

        private string GetUrl(Sitecore.Data.Items.Item item)
        {
            if (item == null) return string.Empty;

            if (item.Paths.IsMediaItem)
            {
                return Sitecore.Resources.Media.MediaManager.GetMediaUrl(item);
            }

            if (item.Paths.IsContentItem && !string.IsNullOrEmpty(item.Visualization.Layout.Name))
            {
                return LinkManager.GetItemUrl(item);
            }

            return string.Empty;
        }

    }
}