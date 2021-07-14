using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuiDao.Client.Models
{
    public class SuiDaoServerInfo
    {
        public string ip { get; set; }

        public int bind_port { get; set; }

        public string server_name { get; set; }

        public long server_id { get; set; }
    }
}
