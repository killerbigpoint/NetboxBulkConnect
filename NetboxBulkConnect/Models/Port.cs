namespace NetboxBulkConnect.Models
{
    public class Port
    {
		public enum Type : int
		{
			Rearport = 0,
			Frontport = 1,
			Interface = 2
		}

		public int id;
		public string name;
		public Type type;

		public Port(int id, string name, Type type)
        {
            this.id = id;
            this.name = name;
            this.type = type;
        }

		public string GetApiName()
        {
			return GetApiName(type);
        }

        public string GetCSVName()
        {
            return GetCSVName(type);
        }

        public static string TypeToEndpoint(Type type)
        {
			switch (type)
            {
				case Type.Rearport: return "rear-ports";
				case Type.Frontport: return "front-ports";
				case Type.Interface: return "interfaces";
				default: return "unknown";
            }
        }

		public static string GetApiName(Type type)
		{
			switch (type)
			{
				case Type.Rearport: return "dcim.rearport";
				case Type.Frontport: return "dcim.frontport";
				case Type.Interface: return "dcim.interface";
				default: return "dcim.unknown";
			}
		}

        public static string GetCSVName(Type type)
        {
            switch (type)
            {
                case Type.Rearport: return "rearport";
                case Type.Frontport: return "frontport";
                case Type.Interface: return "interface";
                default: return "unknown";
            }
        }
    }
}