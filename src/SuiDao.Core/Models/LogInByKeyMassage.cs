﻿using FastTunnel.Core.Models;
using FastTunnel.Core.Models.Massage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuiDao.Client.Models
{
    public class LogInByKeyMassage : TunnelMassage
    {
        public string key { get; set; }

        public long server_id { get; set; }

        public string client_version { get; set; }
    }
}
