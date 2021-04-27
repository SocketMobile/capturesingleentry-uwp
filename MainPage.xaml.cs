///
///  SingleEntry Capture for UWP Platform
///
/// Sample app that displays the scanner name
/// and the decoded data in a list box
/// 
/// Follow the steps from 1 to 4
/// ©2021 Socket Mobile, Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using Windows.UI.Core;

// 1 - add the SocketMobile Capture namespace
// Make sure Socket Companion service is installed and running
using SocketMobile.Capture;

namespace CaptureSingleEntryUWP
{   
    public partial class MainPage : Page
    {
        // main window dependent properties
        public string ScannerName
        {
            get { return (string)GetValue(ScannerNameProperty); }
            set { SetValue(ScannerNameProperty, value); }
        }
        
        public static readonly DependencyProperty ScannerNameProperty =
            DependencyProperty.Register("ScannerName", typeof(string), typeof(MainPage), new PropertyMetadata(string.Empty));

        public string SdkVersion
        {
            get { return (string)GetValue(SdkVersionProperty); }
            set { SetValue(SdkVersionProperty, value); }
        }

        public static readonly DependencyProperty SdkVersionProperty =
            DependencyProperty.Register("SdkVersion", typeof(string), typeof(MainPage), new PropertyMetadata(string.Empty));

        // 2 - Create a CaptureHelper member
        CaptureHelper capture;

        // timer for opening Capture Helper 
        System.Timers.Timer openTimer;

        public MainPage()
        {
            this.InitializeComponent();

            ScannerName = "scannerid init";
            SdkVersion = string.Empty;

            // 3 - instantiate and configure CaptureHelper
            capture = new CaptureHelper { ContextForEvents = SynchronizationContext.Current }; 
            capture.DeviceArrival += Capture_DeviceArrival;
            capture.DeviceRemoval += Capture_DeviceRemoval;
            capture.DecodedData += Capture_DecodedData;

            // this is to handle the case of Socket Mobile Companion 
            // being stopped for some reason
            capture.Terminate += Capture_Terminate;

            UpdateStatus();

            // 4- Start opening the connection to Socket Mobile Companion
            // which must be running. Here it is done with a timer
            // because we restart this timer in case the Socket Mobile Companion service
            // is not running when the app starts.
            openTimer = new System.Timers.Timer
            {
                Interval = 200,     // milliseconds
                AutoReset = false,  // one shot
                Enabled = true,
            };
            openTimer.Elapsed += OpenTimerTick;
            openTimer.Start();
        }
       
        private async void OpenTimerTick(object sender, ElapsedEventArgs e)
        {
            openTimer.Stop();

            try
            {
                long Result = await capture.OpenAsync(
                    "windows:com.socketmobile.singleentry",
                    "BB57D8E1-F911-47BA-B510-693BE162686A",
                    "MCwCFAJIzwgOK1fYEE5KdmPOe+Lm+5x6AhR6kYJKPLvEsh8TO7jaivECRe5C9A==");

                if (SktErrors.SKTSUCCESS(Result))
                {
                    CaptureHelper.VersionResult version = await capture.GetCaptureVersionAsync();
                    if (version.IsSuccessful())
                    {
                        // Note: timer does not run on UI thread, so must switch to UI thread to update UI control
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            SdkVersion = $"Capture version: {version.ToStringVersion()}";
                        });
                    }
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Debug.WriteLine($"Capture OpenAsync failed. Result code: {Result}");
                        ShowOpenFailDialog();
                    });
                }
            } 
            catch (Exception ex)
            {
                Debug.WriteLine($"OpenTimerTick exception {ex.Message}");
            }
        }

        private async void ShowOpenFailDialog()
        {
            ContentDialog openFailDialog = new ContentDialog
            {
                Title = "Capture Service Open Error",
                Content = "Cannot open Capture service. Is Socket Mobile Companion Service running?",
                PrimaryButtonText = "Retry",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await openFailDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                openTimer.Start();
            }
        }

        // received when something is wrong with Socket Mobile Companion
        // or when aborting Capture Helper (result is no error in that case)
        private void Capture_Terminate(object sender, CaptureHelper.TerminateArgs e)
        {
            if (!SktErrors.SKTSUCCESS(e.Result))
            {
                openTimer.Start();
            }
        }

        // received when a barcode has been decoded correctly
        private void Capture_DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            string data = $"{e.DecodedData.SymbologyName} : {e.DecodedData.DataToUTF8String}";
            Debug.WriteLine($"scanned data: {data}");
            DataList.Items.Add(data);
        }

        // received when a scanner disconnects
        private void Capture_DeviceRemoval(object sender, CaptureHelper.DeviceArgs e)
        {
            UpdateStatus();
        }

        // received when a scanner connects
        private void Capture_DeviceArrival(object sender, CaptureHelper.DeviceArgs e)
        {
            UpdateStatus();
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            DataList.Items.Clear();
        }

        private async void UpdateStatus()
        {
            List<CaptureHelperDevice> devices = capture.GetDevicesList();
            string scannerNames = string.Empty;

            if (!devices.Any())
            {
                scannerNames = "No scanner connected";
            }
            else
            {
                foreach (CaptureHelperDevice device in devices)
                {
                    if (!string.IsNullOrEmpty(scannerNames))
                    {
                        scannerNames += ";";
                    }
                    scannerNames = device.GetDeviceInfo().Name;
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ScannerName = scannerNames;
            });

            Debug.WriteLine($"scanner name: {scannerNames}");
        }
    }
}