using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace WheelsApp.Services
{
    public class AppUpdateService
    {
        private const string APP_UPDATE_BASE_URL = "https://wheelspkodicalmlcstorage.blob.core.windows.net/app-updates";
        private const string APP_SERVICE_NAME = "com.wheels.appupdater";
        private const string APP_SERVICE_PACKAGE_NAME = "wheels-app-update_p4k72t4j12gsm";
        private const string APP_UPDATE_PACKAGE_FILE = "packagename.txt";

        public event Action<string> LogEvent;
        public async Task UpdateAsync()
        {
            using (var updaterService = new AppServiceConnection())
            {
                updaterService.AppServiceName = APP_SERVICE_NAME;
                updaterService.PackageFamilyName = APP_SERVICE_PACKAGE_NAME;

                var status = await updaterService.OpenAsync();
                if (status != AppServiceConnectionStatus.Success)
                {
                    LogEvent?.Invoke("Failed to connect!");
                }
                else
                {
                    LogEvent?.Invoke($"Connected: {status}");

                    try
                    {
                        var bundleName = await GetNewBundleName();

                        var updatePackage = $"{APP_UPDATE_BASE_URL}/{bundleName}";
                        var message = new ValueSet();
                        message.Add("PackageFamilyName", Windows.ApplicationModel.Package.Current.Id.FamilyName);
                        message.Add("PackageLocation", updatePackage);

                        LogEvent?.Invoke($"{bundleName} executing...");
                        var response = await updaterService.SendMessageAsync(message);
                        LogEvent?.Invoke($"{bundleName} executing...done");

                        if (response?.Status == AppServiceResponseStatus.Success)
                        {
                            LogEvent?.Invoke("Update started, the app will automatically shutdown and restart.");
                        }
                        var msg = $"{response?.Status} - ";
                        if (response?.Message != null)
                        {
                            msg += string.Join(",", response?.Message?.Select(x => $"{x.Key} = {x.Value}").ToArray());
                        }
                        LogEvent?.Invoke(msg);
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke($"Exception: {GetException(ex)}");
                    }
                }
            }
        }

        private string GetException(Exception ex)
        {
            if (ex != null)
            {
                return $"{ex.Message} - {ex.StackTrace} - {GetException(ex.InnerException)}";
            }
            return string.Empty;
        }

        private async Task<string> GetNewBundleName()
        {
            using (var client = new HttpClient())
            {
                var result = await client.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{APP_UPDATE_BASE_URL}/{APP_UPDATE_PACKAGE_FILE}")
                });

                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
