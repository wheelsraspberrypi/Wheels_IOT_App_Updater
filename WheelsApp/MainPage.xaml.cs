using System;
using WheelsApp.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WheelsApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AppUpdateService appUpdateService;
        
        public MainPage()
        {
            InitializeComponent();

            appUpdateService = new AppUpdateService();
            appUpdateService.LogEvent += (msg)=> {
                LogMessage(msg);
            };

            var v = Windows.ApplicationModel.Package.Current.Id.Version;
            txtVersion.Text = $"Version: {v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            txtOutput.Text = "";
        }        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await appUpdateService.UpdateAsync();
        }

        private void LogMessage(string v)
        {
            txtOutput.Text = v + Environment.NewLine + txtOutput.Text;
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
