using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teach_GenMssqlDML.Models
{
    public class HomeModel
    {
        public class GetDMLIn
        {
            public string Q_DB_IP { get; set; }
            public string Q_USER_ID { get; set; }
            public string Q_USER_PWD { get; set; }
            public string Q_DB_NAME { get; set; }
            public string Q_TABLE_NAME { get; set; }
        }

        public class GetDMLOut
        {
            public string DmlSelect { get; set; }
            public string DmlInsert { get; set; }
            public string DmlUpdate { get; set; }
            public string DmlDelete { get; set; }
        }
    }
}