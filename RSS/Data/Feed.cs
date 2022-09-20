namespace RSS.Data
{
    public class Feed
    {
        public Guid FeedId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime LastUpdated { get; set; }
        public IEnumerable<FeedItem> FeedItems { get; set; }
    }
}
