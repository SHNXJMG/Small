using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    public class Province
    {
        string _provname;
        public string ProvName
        {
            set { _provname=value; }
            get { return _provname; }
        }
    }
}
