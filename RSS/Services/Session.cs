using Blazored.LocalStorage;
using System.Net;

namespace RSS.Services
{
    public class Session
    {
        private ILocalStorageService _localStorage;
        public Session(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }
        public async Task SetProxyProfile(Uri proxyUri, string userName, string userPassword)
        {
            WebProxy proxyClient = new WebProxy();
            proxyClient.Address = proxyUri;
            if (!string.IsNullOrEmpty(userName)) proxyClient.Credentials = new NetworkCredential(userName, userPassword);
            await _localStorage.SetItemAsync<WebProxy>("rss.proxy", proxyClient);
        }
        public async Task<WebProxy?> GetProxy()
        {
            if (!await _localStorage.ContainKeyAsync("rss.proxy")) return null;
            return _localStorage.GetItemAsync<WebProxy>("rss.proxy").Result;
        }

        public async Task SetUpdateTime(int updateTime)
        {
            await _localStorage.SetItemAsync("rss.update.time", updateTime * 60000);
        }

        public async Task<int> GetUpdateTime()
        {
            if (!await _localStorage.ContainKeyAsync("rss.update.time")) SetUpdateTime(0);
            return await _localStorage.GetItemAsync<int>("rss.update.time");
        }

        public async Task SyncUpdateTimerAndConfig(int updateFrequency)
        {
            await SetUpdateTime(updateFrequency);
        }

        public async Task SyncProxyAndConfig(Proxy proxy)
        {
            if (!string.IsNullOrEmpty(proxy.Uri))
            {
                Uri uri = new Uri(proxy.Uri);
                await SetProxyProfile(uri, proxy.Username, proxy.Password);
            }
        }
    }
}
