using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DEMO3
{
    public sealed partial class Client : Page
    {
        private StreamSocket chatSocket;
        private DataWriter chatWriter;
        private RfcommDeviceService service;
        private DeviceInformationCollection chatServiceInfoCollection;
        private string stringBuffer;

        public Client()
        {
            this.InitializeComponent();

            chatSocket = null;
            chatWriter = null;
            service = null;
            chatServiceInfoCollection = null;

            App.Current.Suspending += App_Suspending;
        }

        void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Disconnect();
        }
        
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            chatServiceInfoCollection = await DeviceInformation.FindAllAsync(
                RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            if (chatServiceInfoCollection.Count > 0)
            {
                List<string> items = new List<string>();
                foreach (var chatServiceInfo in chatServiceInfoCollection)
                {
                    items.Add(chatServiceInfo.Name);
                }

                cvs.Source = items;
                ServiceSelector.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                var dialog = new MessageDialog("No serial port services were found.");
                dialog.ShowAsync();
            }
        }

        private async void ServiceList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                RunButton.IsEnabled = false;
                ServiceSelector.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                var chatServiceInfo = chatServiceInfoCollection[ServiceList.SelectedIndex];
                service = await RfcommDeviceService.FromIdAsync(chatServiceInfo.Id);

                if (service == null)
                {
                    var dialog = new MessageDialog("Connection error");
                    await dialog.ShowAsync();
                }

                lock (this)

                {
                    chatSocket = new StreamSocket();
                }

                await chatSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);

                chatWriter = new DataWriter(chatSocket.OutputStream);
                StatusBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                DataReader chatReader = new DataReader(chatSocket.InputStream);
                ReceiveStringLoop(chatReader);
            }
            catch (Exception ex)
            {
                RunButton.IsEnabled = true;
                var dialog = new MessageDialog("Connection error");
                dialog.ShowAsync();
            }
        }
        private async void ReceiveStringLoop(DataReader chatReader)
        {
            try
            {
                byte[] buffer = new byte[10];
                await chatSocket.InputStream.ReadAsync(buffer.AsBuffer(), 10, InputStreamOptions.Partial);

                string result = System.Text.Encoding.UTF8.GetString(buffer, 0, 10);
                stringBuffer += result;

                if (result.IndexOf('\0') != -1)
                {
                    if (!string.IsNullOrEmpty(stringBuffer))
                    {
                        ConversationList.Items.Add(stringBuffer);
                        stringBuffer = string.Empty;
                    }
                }

                ReceiveStringLoop(chatReader);
            }
            catch
            {
                
            }
        }

        private void Disconnect()
        {
            if (chatWriter != null)
            {
                chatWriter.DetachStream();
                chatWriter = null;
            }

            lock (this)
            {
                if (chatSocket != null)
                {
                    chatSocket.Dispose();
                    chatSocket = null;
                }
            }

            RunButton.IsEnabled = true;
            ServiceSelector.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            StatusBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            var dialog = new MessageDialog("Disconnected");
            await dialog.ShowAsync();

        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                chatWriter.WriteString("open");
                await chatWriter.StoreAsync();
                ConversationList.Items.Add("Sent: open");
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("Error: " + ex.HResult.ToString() + " - " + ex.Message);
                dialog.ShowAsync();
            }
        }
    }
}
