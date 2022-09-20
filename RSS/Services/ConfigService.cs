using RSS.Services;
using RSS.Data;
using System.Collections;
using System.Xml;
using System.Net;
using System.Xml.Serialization;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace RSS.Services
{
    public class Proxy
    {
        public string Uri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class Config
    {
        public List<string> Feeds { get; set; }
        public int UpdateFrequency { get; set; }
        public Proxy Proxy { get; set; } = new Proxy();
    }
    
    public class ConfigService
    {
        private FeedService _feedService;
        private Session _session;
        private Config _config;
        public ConfigService(FeedService feedService, Session session)
        {
            _feedService = feedService;
            _session = session;
        }

        public async Task SaveFeedsConfig()
        {
            _config.Feeds = await _feedService.GetUrls();
            
            XmlDocument configFile = new XmlDocument();
            configFile.Load("Config/Config.xml");
            XmlNode node = configFile.SelectSingleNode("/Config/Feeds");
            node.RemoveAll();
            foreach (var feed in _config.Feeds)
            {
                XmlNode xmlFeed = configFile.CreateElement("string");
                xmlFeed.InnerText = feed;
                node.AppendChild(xmlFeed);
            }
            configFile.Save("Config/Config.xml");
        }

        public async Task SaveUpdateTimerConfig()
        {
            int time = await _session.GetUpdateTime();

            XmlDocument configFile = new XmlDocument();
            configFile.Load("Config/Config.xml");

            XmlNode node = configFile.SelectSingleNode("/Config/UpdateFrequency");
            node.InnerText = $"{time/60000}";
            configFile.Save("Config/Config.xml");
        }

        public async Task SaveProxyConfig()
        {
            var proxyClient = await _session.GetProxy();
            _config.Proxy.Uri = proxyClient.Address.AbsoluteUri;
            NetworkCredential credential = (NetworkCredential)proxyClient.Credentials;
            _config.Proxy.Username = credential.UserName;
            _config.Proxy.Password = credential.Password;

            XmlDocument configFile = new XmlDocument();
            configFile.Load("Config/Congfig.xml");

            XmlNode node = configFile.SelectSingleNode("/Config/Proxy");
            node.SelectSingleNode("/Uri").InnerText = $"{_config.Proxy.Uri}";
            node.SelectSingleNode("/Username").InnerText = $"{_config.Proxy.Username}";
            node.SelectSingleNode("/Password").InnerText = $"{_config.Proxy.Password}";
            configFile.Save("Config/Config.xml");
        }

        public async Task ReadFromXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            
            using (Stream reader = new FileStream("Config/Config.xml", FileMode.Open))
            {
                _config = (Config)serializer.Deserialize(reader);
            }
        }

        public async Task SyncServicesAndConfig()
        {
            await _feedService.SyncStorageAndConfig(_config.Feeds);
            await _session.SyncUpdateTimerAndConfig(_config.UpdateFrequency);
            await _session.SyncProxyAndConfig(_config.Proxy);
        }
    }
}
