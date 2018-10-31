using System;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using WheelsAppUpdateComponent.Services;

namespace WheelsAppUpdateComponent
{
    public sealed class PackageUpdater : IBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        private AppServiceConnection connection;        

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            connection = details.AppServiceConnection;

            connection.RequestReceived += OnRequestReceived;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                serviceDeferral.Complete();
            }
        }

        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();

            var message = args.Request.Message;
            var returnData = new ValueSet();

            var packageManagerServices = new PackageManagerServices();

            try
            {
                string packageFamilyName = message["PackageFamilyName"] as string;
                string packageLocation = message["PackageLocation"] as string;

                packageManagerServices.MessageEvent += (obj, evt)=> {
                    returnData.Add(evt.Key, evt.Value);
                };

                await packageManagerServices.UpdateAsync(packageLocation);

                await args.Request.SendResponseAsync(returnData);
            }
            finally
            {
                messageDeferral.Complete();
            }
        }
    }
}
