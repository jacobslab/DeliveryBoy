using UnityEngine;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System.Collections.ObjectModel;

public class SyncBox : MonoBehaviour
{
    void Start()
    {
        UsbDevice MyUsbDevice;

        // Dump all devices and descriptor information to console output.
        UsbRegDeviceList allDevices = UsbDevice.AllDevices;
        //Debug.Log(allDevices.Count);
        foreach (UsbRegistry usbRegistry in allDevices)
        {
            if (usbRegistry.Open(out MyUsbDevice))
            {
                if ((MyUsbDevice.Info.ProductString != null) && MyUsbDevice.Info.ProductString.Equals("LabJack U3"))
                {
                    IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // This is a "whole" USB device. Before it can be used, 
                        // the desired configuration and interface must be selected.

                        // Select config #1
                        wholeUsbDevice.SetConfiguration(1);

                        // Claim interface #0.
                        wholeUsbDevice.ClaimInterface(0);
                    }

                    // open read endpoint 1.
                    UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                    // open write endpoint 1.
                    UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                    int bytesWritten;
                    Debug.Log(writer.Write(short.MaxValue, 0, out bytesWritten));
                    Debug.Log(bytesWritten);
                }
            }
        }

        // Free usb resources.
        // This is necessary for libusb-1.0 and Linux compatibility.
        // UsbDevice.Exit();
    }
}