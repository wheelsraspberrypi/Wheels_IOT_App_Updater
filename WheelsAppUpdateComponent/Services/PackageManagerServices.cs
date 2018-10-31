using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace WheelsAppUpdateComponent.Services
{
    public sealed class PackageManagerServices
    {
        public event EventHandler<KeyValuePair<string, object>> MessageEvent;        
        public IAsyncOperation<bool> UpdateAsync(string packageLocation)
        {
            return InstallOrUpdateAsync(packageLocation, false).AsAsyncOperation();
        }

        public IAsyncOperation<bool> InstallAsync(string packageLocation)
        {
            return InstallOrUpdateAsync(packageLocation, true).AsAsyncOperation();
        }

        private async Task<bool> InstallOrUpdateAsync(string packageLocation, bool blnNew)
        {
            DeploymentResult result = null;
            try
            {
                var fileName = await DownloadFileAsync(packageLocation);
                var packageManager = new PackageManager();
                
                if (blnNew)
                {
                    result = await packageManager.RegisterPackageAsync(new Uri(fileName), new List<Uri>(), DeploymentOptions.ForceApplicationShutdown);
                }
                else
                {
                    result = await packageManager.UpdatePackageAsync(new Uri(fileName), new List<Uri>(), DeploymentOptions.ForceApplicationShutdown);
                }                
            }
            catch (Exception ex)
            {
                MessageEvent?.Invoke(this, new KeyValuePair<string, object>("Error", ex.Message));
                return false;
            }

            MessageEvent?.Invoke(this, new KeyValuePair<string, object>("IsRegistered", result?.IsRegistered));
            MessageEvent?.Invoke(this, new KeyValuePair<string, object>("ErrorText", result?.ErrorText));
            MessageEvent?.Invoke(this, new KeyValuePair<string, object>("ExtendedErrorCode", result?.ExtendedErrorCode));

            return true;
        }

        private async Task<string> DownloadFileAsync(string fileName)
        {
            var parts = fileName.Split(new char[] { '/' });
            var name = parts.ToList().Last();
            var strPath = System.IO.Path.GetTempPath();

            await DeleteAsync(name);

            var storageFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(name, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

            using (var httpClient = new HttpClient())
            {
                var buffer = await httpClient.GetByteArrayAsync(fileName);
                await Windows.Storage.FileIO.WriteBytesAsync(storageFile, buffer);
            }
            return storageFile.Path;
        }

        private async Task DeleteAsync(string name)
        {
            try
            {
                var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(name);
                await file.DeleteAsync();
            }
            catch
            {
            }
        }
    }
}
