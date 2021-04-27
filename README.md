# Single Entry using Capture SDK

[screenshot]()


## Introduction
This is a sample application to show how to use the Socket Mobile Capture SDK.

The Socket Mobile Capture SDK is available as NuGet from NuGet.org.

When loading the project in Visual Studio, the Capture NuGet should restore itself from NuGet.org.

The Socket Mobile Capture relies on a Windows service running on the PC in order to manage the connection to a Socket Mobile scanner.

This service is called Socket Mobile Companion. If you have installed the Socket Mobile Companion package, this service comes with an application called Socket Mobile Companion. The version and the status of the service can be checked through Socket Mobile Companion.

Socket Mobile Companion app is also required when connecting a scanner for the first time as it will configure the scanner during the first connection process.

Exiting the Socket Mobile Companion app won't terminate the Socket Mobile Companion service. If a scanner is already configured to connect to the PC, Socket Mobile Companion UI is no longer required but can be used to show the scanner connection status, and to configure a new scanner.

## Using Capture SDK
The scanner connection and disconnection process is independent of the application. The application receives a device arrival notification when a Socket Mobile scanner is connected to the host, and a device removal when it disconnects.

The best way to use the Capture API is to use the Capture Helper class, found in CaptureHelper.cs.

Capture Helper provides most of the APIs Capture offers, and hides most the complexity of handling the scanner.

Capture Helper asynchronous event handlers should be set up before opening Capture Helper.

Here is an example from the source of this sample application:
```
mCapture.DeviceArrival += mCapture_DeviceArrival;
mCapture.DeviceRemoval += mCapture_DeviceRemoval;
mCapture.DecodedData += mCapture_DecodedData;
```

If the application UI needs to be updated from these handlers, or in the callbacks of the Capture Helper API methods, a context can be set once also before opening Capture Helper as follow:
`capture.ContextForEvents = WindowsFormsSynchronizationContext.Current;`

The sample app uses a timer to open Capture. This is to handle the case where Socket Mobile Companion service is not running when the application is trying to use it. If the Capture service is not running when the sample starts, you can start it and retry by restarting the timer. 

The scanner decoded data are received by the application by subscribing to the Capture Helper event: `DecodedData`.

## Opening Capture - Application Registration
Opening Capture requires some application information, such as the application ID, a developer ID and the application AppKey.
To set these up go to the Socket Mobile developer portal register and for an account. 

## Documentation
For more information please consult the Capture SDK documentation at: https://docs.socketmobile.com/capture/csharp/en/latest/
