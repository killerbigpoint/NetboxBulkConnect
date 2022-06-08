using System;

namespace NetboxBulkConnect.Models
{
    public class DeviceResponse
    {
        public int count { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public Devices[] results { get; set; }
    }

    public class Devices
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public Device_Type device_type { get; set; }
        public Device_Role device_role { get; set; }
        public Tenant tenant { get; set; }
        public Platform platform { get; set; }
        public string serial { get; set; }
        public string asset_tag { get; set; }
        public Site site { get; set; }
        public Rack rack { get; set; }
        public int? position { get; set; }
        public Face face { get; set; }
        public Parent_Device parent_device { get; set; }
        public Status status { get; set; }
        public Primary_Ip primary_ip { get; set; }
        public Primary_Ip4 primary_ip4 { get; set; }
        public object primary_ip6 { get; set; }
        public Cluster cluster { get; set; }
        public object virtual_chassis { get; set; }
        public object vc_position { get; set; }
        public object vc_priority { get; set; }
        public string comments { get; set; }
        public object local_context_data { get; set; }
        public object[] tags { get; set; }
        public Custom_Fields custom_fields { get; set; }
        public Config_Context config_context { get; set; }
        public string created { get; set; }
        public DateTime last_updated { get; set; }
    }

    public class Device_Type
    {
        public int id { get; set; }
        public string url { get; set; }
        public Manufacturer manufacturer { get; set; }
        public string model { get; set; }
        public string slug { get; set; }
        public string display_name { get; set; }
    }

    public class Manufacturer
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Device_Role
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Tenant
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Platform
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Site
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Rack
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
    }

    public class Face
    {
        public string value { get; set; }
        public string label { get; set; }
    }

    public class Parent_Device
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public Device_Bay device_bay { get; set; }
    }

    public class Device_Bay
    {
        public int id { get; set; }
        public string url { get; set; }
        public Device device { get; set; }
        public string name { get; set; }
    }

    public class Device
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
    }

    public class Status
    {
        public string value { get; set; }
        public string label { get; set; }
    }

    public class Primary_Ip
    {
        public int id { get; set; }
        public string url { get; set; }
        public int family { get; set; }
        public string address { get; set; }
    }

    public class Primary_Ip4
    {
        public int id { get; set; }
        public string url { get; set; }
        public int family { get; set; }
        public string address { get; set; }
    }

    public class Cluster
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
    }

    public class Custom_Fields
    {
    }

    public class Config_Context
    {
    }
}
