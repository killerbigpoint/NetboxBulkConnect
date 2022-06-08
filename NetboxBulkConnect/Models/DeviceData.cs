using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetboxBulkConnect.Models
{
    public class DeviceData
    {
        public int id { get; set; }
        public List<Port> ports { get; set; }
    }
}
