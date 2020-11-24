using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    [Serializable]
    public class ProvInfo
    {
        private string _RegionId;
        private string _RegionName;
        private string _RegionFullName;

        public string RegionId
        {
            set { _RegionId = value; }
            get { return _RegionId; }
        }
        public string RegionName
        {
            set { _RegionName = value; }
            get { return _RegionName; }
        }
        public string RegionFullName
        {
            set { _RegionFullName = value; }
            get { return _RegionFullName; }
        }
    }
}
