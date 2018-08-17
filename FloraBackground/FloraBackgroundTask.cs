using Windows.ApplicationModel.Background;

namespace FloraBackground
{
    public sealed class FloraBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            _deferral.Complete();
        }
    }
}
