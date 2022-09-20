using Blazored.LocalStorage;
using RSS.Services;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RSS.Pages.Settings
{
    public class SettingsModel
    {
        Session _session;
        FeedService _feedService;
        ConfigService _configService;
        public SettingsModel(Session session, FeedService feedService, ConfigService configService)
        {
            _session = session;
            _feedService = feedService;
            _configService = configService;
        }
        
        [Range(0, 999, ErrorMessage = "Введите положительное число или 0")]
        public int NewUpdateTime { get; set; }
        [Range(1, 999, ErrorMessage = "Введите положительное число")]
        public int FeedItemsLimit { get; set; } = 10;

        [DataType(DataType.Url, ErrorMessage = "Неправильный формат адреса прокси сервера")]
        public string ProxyUrl { get; set; }
        public string ProxyUsername { get; set; }
        [PasswordPropertyText]
        public string ProxyPassword { get; set; }


        public async Task SetUpdateSettings()
        {
            if (!string.IsNullOrEmpty(NewUpdateTime.ToString()))
            {
                await _session.SetUpdateTime(NewUpdateTime);
                await _configService.SaveUpdateTimerConfig();
            }
            if (!string.IsNullOrEmpty(FeedItemsLimit.ToString())) 
            {
                await _feedService.SetFeedItemsLimit(FeedItemsLimit);
                await _configService.SaveFeedsConfig();
            }
            if (!string.IsNullOrEmpty(ProxyUrl))
            {
                await _session.SetProxyProfile(new Uri(ProxyUrl), ProxyUsername, ProxyPassword);
                await _configService.SaveProxyConfig();
            }
        }
    }
}
