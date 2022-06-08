namespace NetboxBulkConnect.Models
{
    public class PortResponse
    {
        public int count { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public ActualPort[] results { get; set; }
    }

    public class ActualPort
    {
        public int id { get; set; }
        public string url { get; set; }
        public CableDevice device { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public Type type { get; set; }
        public int positions { get; set; }
        public string description { get; set; }
        public object cable { get; set; }
        public object[] tags { get; set; }
    }

    public class CableDevice
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
    }

    public class Type
    {
        public string value { get; set; }
        public string label { get; set; }
    }
}
