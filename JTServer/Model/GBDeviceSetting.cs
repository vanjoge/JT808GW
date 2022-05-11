using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model
{
    public class GBDeviceSetting
    {
        public GBDeviceSetting()
        {
            Enable = true;
            Port = 5060;
        }
        public class ChannelItem
        {
            public byte Channel;
            public string ID;

            public ChannelItem(byte Channel, string ID)
            {
                this.Channel = Channel;
                this.ID = ID;
            }
        }
        public bool Enable { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public bool UseTcp { get; set; }
        public string ServerId { get; set; }
        public string Password { get; set; }
        public string DeviceID { get; set; }
        public List<ChannelItem> Channels { get; set; }

        public string GetServerSipStr()
        {
            return $"sip:{ServerId}@{Server}:{Port}{(UseTcp ? ";transport=tcp" : "")}";
        }
        public GB28181.XML.DeviceInfo GetDeviceInfo()
        {
            var deviceInfo = new GB28181.XML.DeviceInfo();
            deviceInfo.Manufacturer = "RTVS";
            deviceInfo.Firmware = "V1.0";
            deviceInfo.Model = "RTVSDEV V1.0";
            deviceInfo.Result = "OK";
            deviceInfo.DeviceID = DeviceID;
            deviceInfo.DeviceName = "RTVS 1078";
            deviceInfo.Channel = Channels.Count;

            return deviceInfo;
        }
        public List<GB28181.XML.Catalog.Item> GetDeviceList()
        {
            var deviceList = new List<GB28181.XML.Catalog.Item>();
            foreach (var item in Channels)
            {
                var devItem = new GB28181.XML.Catalog.Item();
                devItem.Manufacturer = "RTVS";
                devItem.Name = "Channel" + item.Channel;
                devItem.Model = "1078Camera";
                devItem.Owner = "Owner";
                devItem.CivilCode = "CivilCode";
                devItem.Address = "Address";
                devItem.Parental = 0;
                //devItem.SafetyWay = 0;
                devItem.RegisterWay = 1;
                devItem.Status = GB28181.Enums.DevStatus.ON;
                devItem.Secrecy = 0;
                devItem.ParentID = DeviceID;
                devItem.DeviceID = item.ID;
                deviceList.Add(devItem);
            }
            return deviceList;
        }
    }
}
