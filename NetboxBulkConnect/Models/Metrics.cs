namespace NetboxBulkConnect.Models
{
	public class Metrics
    {
		public enum Type : int
		{
			Meters = 0,
			CM = 1,
			Feet = 2,
			Inches = 3
		}

		public static string TypeToApiType(Type type)
		{
			switch (type)
			{
				case Type.Meters: return "m";
				case Type.CM: return "cm";
				case Type.Feet: return "ft";
				case Type.Inches: return "in";
				default: return string.Empty;
			}
		}
	}
}
