using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyDataCollector.Shared
{
    public class ActivityStatistics
    {
        public int TotalCases {  get; set; }
        public IList<AreaCase> AreaCases { get; set; }
    }
    public class AreaCase
    {
        public string AreaName { get; set; }
        public int Amount { get; set; }
    }
}
