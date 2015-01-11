using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syxebot
{

    public class ChatterNames
    {
        public List<string> moderators { get; set; }
        public List<string> staff { get; set; }
        public List<string> admins { get; set; }
        public List<string> global_mods { get; set; }
        public List<string> viewers { get; set; }
    }
    public class Links
    {
    }

   

    public class Chatters
    {
        public Links _links { get; set; }
        public int chatter_count { get; set; }
        public ChatterNames chatters { get; set; }
    }
}
