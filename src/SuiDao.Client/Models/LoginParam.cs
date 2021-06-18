using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuiDao.Client.Models
{
    public class LoginParam
    {
        public string key { get; set; }

        public SuiDaoServerInfo server { get; set; }
    }
}
