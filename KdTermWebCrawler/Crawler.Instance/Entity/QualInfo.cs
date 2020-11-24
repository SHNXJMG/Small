using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    [Serializable]
    public class QualInfo
    {
        private string _qualCode;
        private string _qualName;

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
