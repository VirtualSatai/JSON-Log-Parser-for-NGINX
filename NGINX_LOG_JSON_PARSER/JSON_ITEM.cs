using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NGINX_LOG_JSON_PARSER
{
    public class JSON_ITEM
    {
        public string remote_addr { get; set; }
        public string remote_user { get; set; }
        public string time_local { get; set; }
        public string request { get; set; }
        public Int64 status { get; set; }
        public Int64 body_bytes_sent { get; set; }
        public string http_referer { get; set; }
        public string http_user_agent { get; set; }
        public string http_x_forwarded_for { get; set; }
    }

}
