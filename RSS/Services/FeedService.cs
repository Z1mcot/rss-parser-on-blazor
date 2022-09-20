using System.ServiceModel.Syndication;
using System.Xml;
using Blazored.LocalStorage;
using RSS.Data;
using HtmlAgilityPack;
using System.Net;
using RSS.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace RSS.Services
{
    public class FeedService
    {
        private ILocalStorageService _localStorage;
        private Session _session;
        private int _feedItemLimit;
        
        public FeedService(ILocalStorageService localStorage, Session session)
        {
            _localStorage = localStorage;
            _session = session;
        }

        public async Task SyncStorageAndConfig(List<string> FeedUrls)
        {
            var storage = await GetFeedsFromStorage();
            foreach (var feed in storage)
            {
                if (!FeedUrls.Contains(feed.Link)) await RemoveFeedFromStorager(feed.FeedId.ToString());
                else FeedUrls.Remove(feed.Link);
            }
            if (FeedUrls.Count > 0) // Если в конфиге есть неучтённые фиды
            {
                foreach (var url in FeedUrls)
                {
                    var newFeed = await GetFeed(url);
                    await AddFeedToStorage(newFeed);
                }
            }
        }

        public Task SetFeedItemsLimit(int value = 10)
        {
            return Task.FromResult(_feedItemLimit = value);
        }

        public async Task<Feed> GetFeedFromStorage(string feedId)
        {
            var feeds = await GetFeedsFromStorage();
            return feeds.SingleOrDefault(i => i.FeedId.ToString() == feedId);
        } 

        public async Task<List<Feed>> GetFeedsFromStorage()
        {
            return await _localStorage.GetItemAsync<List<Feed>>("rss.feeds") ?? new List<Feed>();
        }
        public async Task<List<string>> GetUrls()
        {
            List<Feed> Feeds = await GetFeedsFromStorage();
            List<string> Urls = new List<string>();
            foreach (var feed in Feeds) Urls.Add(feed.Link);
            return Urls;
        }

        private async Task<SyndicationFeed> GetRawFeed(string rssLink)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(rssLink);
            httpWebRequest.Proxy = await _session.GetProxy() ?? WebRequest.DefaultWebProxy;

            try
            {
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (Stream responseStream = httpWebResponse.GetResponseStream())
                    {
                        using (var reader = XmlReader.Create(responseStream))
                        {
                            return SyndicationFeed.Load(reader);
                        }
                    }
                }
            }
            catch { return null; }
        }
        public async Task<Feed> GetFeed(string rssLink)
        {
            Feed processedFeed = null;
            SyndicationFeed rawFeed = await GetRawFeed(rssLink);

            if (rawFeed != null)
            {
                processedFeed = await CreateFeed(rawFeed, rssLink);
            }
            return processedFeed;
        }

        private Task<Feed> CreateFeed(SyndicationFeed rawFeed, string rssLink)
        {
            return Task.FromResult(new Feed()
            {
                FeedId = Guid.NewGuid(),
                Title = rawFeed.Title.Text,
                Description = rawFeed.Description.Text,
                Link = rssLink,
                LastUpdated = DateTime.Now,
                FeedItems = null
            });
        }

        public async Task<IEnumerable<FeedItem>> GetFeedItems(Feed feed)
        {
            IEnumerable<FeedItem> feedItems = new List<FeedItem>();
            SyndicationFeed rawFeed = await GetRawFeed(feed.Link);

            if (rawFeed != null)
            {
                feedItems = await CreateFeedItems(rawFeed);
            }
            return feedItems;
        }

        private Task<IEnumerable<FeedItem>> CreateFeedItems(SyndicationFeed rawFeed)
        {
            var formattedFeed = new Rss20FeedFormatter(rawFeed);
            return Task.FromResult(from item in formattedFeed.Feed.Items.Take(_feedItemLimit)
                                   select new FeedItem
                                   {
                                       Title = item.Title.Text,
                                       Description = RemoveHtmlTags(item.Summary.Text) ?? "No description for this item",
                                       Link = item.Links[0].Uri.ToString(),
                                       PubDate = item.PublishDate.DateTime
                                   });
        }

        private string RemoveHtmlTags(string unformattedText)
        {
            var h = new HtmlDocument();
            h.LoadHtml(unformattedText);
            return h.DocumentNode.InnerText;
        }

        public async Task AddFeedToStorage(Feed feed)
        {
            await Task.Run(async () =>
            {
                feed.FeedItems = null; // На всякий пожарный
                var existingFeeds = await GetFeedsFromStorage();
                existingFeeds.Add(feed);
                await _localStorage.SetItemAsync("rss.feeds", existingFeeds);
            });
        }

        public async Task RemoveFeedFromStorager(string feedId)
        {
            await Task.Run(async () =>
            {
                var feeds = await GetFeedsFromStorage();
                var feedToRemove = feeds.SingleOrDefault(i => i.FeedId.ToString() == feedId); ;
                bool debug = 
                feeds.Remove(feedToRemove);
                await _localStorage.SetItemAsync("rss.feeds", feeds);
            }).ConfigureAwait(false);
        }
    }
}
