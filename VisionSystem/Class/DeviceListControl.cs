/*
 * @file DeviceListControl.cs
 *
 * Copyright (c) 2019 KEYENCE CORPORATION.
 * All rights reserved.
 *
 */

using Keyence.NET;
using System.Collections.Generic;

public class DeviceListControl
{
    public static DeviceListControl instance = new DeviceListControl();

    private List<Device> _deviceList = new List<Device>();

    private DeviceListControl()
    {
        KglSystem.initialize();
    }

    public class Device
    {
        public KglDeviceInfo kglDeviceInfo;
        public string sIpAddress = "";
        public string sModelName;

        public static bool IsValidDevice(string sModelName)
        {
            if (string.Equals(sModelName, "CA-H048MX") ||
                string.Equals(sModelName, "CA-H200MX") ||
                string.Equals(sModelName, "CA-H500MX") ||
                string.Equals(sModelName, "CA-H048CX") ||
                string.Equals(sModelName, "CA-H200CX") ||
                string.Equals(sModelName, "CA-H500CX") ||
                string.Equals(sModelName, "VJ-H048MX") ||
                string.Equals(sModelName, "VJ-H200MX") ||
                string.Equals(sModelName, "VJ-H500MX") ||
                string.Equals(sModelName, "VJ-H048CX") ||
                string.Equals(sModelName, "VJ-H200CX") ||
                string.Equals(sModelName, "VJ-H500CX") ||
                string.Equals(sModelName, "CA-HX048M") ||
                string.Equals(sModelName, "CA-HX200M") ||
                string.Equals(sModelName, "CA-HX500M") ||
                string.Equals(sModelName, "CA-HX048C") ||
                string.Equals(sModelName, "CA-HX200C") ||
                string.Equals(sModelName, "CA-HX500C") ||
                string.Equals(sModelName, "RB-500") ||
                string.Equals(sModelName, "RB-800") ||
                string.Equals(sModelName, "RB-1200") ||
                string.Equals(sModelName, "XT-024") ||
                string.Equals(sModelName, "XT-060"))
            {
                return true;
            }
            else if (string.Equals(sModelName, "CA-HL02MX") ||
                string.Equals(sModelName, "CA-HL04MX") ||
                string.Equals(sModelName, "CA-HL08MX"))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", sIpAddress, sModelName);
        }
    }

    public void FindDevices()
    {
        KglSystem kglSystem = new KglSystem();
        string sModelName;

        _deviceList.Clear();

        if (KglResult.KGL_SUCCESS != kglSystem.find())
        {
            return;
        }

        uint count = kglSystem.getDeviceCount();
        if (count == 0)
        {
            return;
        }

        for (uint i = 0; i < count; ++i)
        {
            KglDeviceInfo deviceInfo = kglSystem.getDeviceInfo(i);
            sModelName = deviceInfo.getModelName();

            if (Device.IsValidDevice(sModelName))
            {
                Device device = new Device
                {
                    kglDeviceInfo = deviceInfo,
                    sIpAddress = deviceInfo.getIPAddress(),
                    sModelName = sModelName
                };
                _deviceList.Add(device);
            }
        }
    }

    public List<Device> FoundDevices
    {
        get
        {
            return _deviceList;
        }
    }

}
