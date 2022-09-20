namespace RSS.Services
{
    public class NotificationService
    {
        public event Func<Task> OnFeedUpdated;
        public async Task NotifyFeedChange()
        {
            await OnFeedUpdated?.Invoke();
        }
    }
}
