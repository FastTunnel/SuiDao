using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuiDao.Client.Models
{
    public class ApiResponse<T>
    {
        public bool success { get; set; }

        public string errorMsg { get; set; }

        public T data { get; set; }
    }
}
