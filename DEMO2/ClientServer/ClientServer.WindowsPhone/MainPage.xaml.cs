using ClientServer.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ClientServer
{
    public sealed partial class MainPage : Page
    {
        private Dictionary<Border, TranslateTransform> transforms = new Dictionary<Border, TranslateTransform>();
        private CommunicationClient infoSender = new CommunicationClient();
        
        public MainPage()
        {
            this.InitializeComponent();
            InitializeFigures();
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ConnectViewModel viewModel = e.Parameter as ConnectViewModel;
            try
            {
                await infoSender.ConnectAsync(viewModel.IP, viewModel.Port);
                var dialog = new MessageDialog("Connected");
                await dialog.ShowAsync();
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("Not connected");
                dialog.ShowAsync();
            }
        }
    }
}
