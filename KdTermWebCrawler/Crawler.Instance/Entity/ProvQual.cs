using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    [Serializable]
    public class ProvQual
    {
        private string _RegionId;
        private string _RegionName;
        private string _RegionFullName;
        private string _qualCode;
        private string _qualName;
        private int _pageIndex;
        public int PageIndex
        {
            set { _pageIndex = value; }
            get { return _pageIndex; }
        }
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
        public string QualCode
        {
            get { return _qualCode; }
            set
            {
                _qualCode = value;
            }
        } 
        public string QualName
        {
            set { _qualName = value; }
            get { return _qualName; }
        }
    }
}
