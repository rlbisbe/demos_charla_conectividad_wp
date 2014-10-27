using ClientServer.Common;
using ClientServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace ClientServer
{
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableCollection<ItemViewModel> viewModel = new ObservableCollection<ItemViewModel>();

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            viewModel = new ObservableCollection<ItemViewModel>();
            DataContext = viewModel;

            var infoSender = new CommunicationServer();
            infoSender.DataReceived += InfoSender_DataReceived;
            infoSender.StartListeningAsync();
        }
        
        async void InfoSender_DataReceived(object sender, Result e)
        {
            switch (e.ResultType)
            {
                case ResultType.Text:
                    await AddNewText(e);
                    return;
                case ResultType.Image:
                    await AddNewImage(e);
                    return;
            }
        }

        private async System.Threading.Tasks.Task AddNewImage(Result e)
        {
            var stream = new InMemoryRandomAccessStream();
            var dataWriter = new DataWriter(stream);
            dataWriter.WriteBytes(e.Value);
            await dataWriter.StoreAsync();
            stream.Seek(0);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var image = new BitmapImage();
                image.SetSource(stream);
                viewModel.Add(new ItemViewModel() { Image = image });
            });
        }

        private async System.Threading.Tasks.Task AddNewText(Result e)
        {
            string value = Encoding.UTF8.GetString(e.Value, 0, e.Value.Length);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                viewModel.Add(new ItemViewModel() { Text = value });
            });
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
