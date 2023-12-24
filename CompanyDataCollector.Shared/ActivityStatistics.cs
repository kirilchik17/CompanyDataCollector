using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace CompanyDataCollector.Shared
{
    public class ActivityStatistics
    {
        public int TotalCases {  get; set; }
        public IList<AreaCase> AreaCases { get; set; }

        public override string? ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
    public class AreaCase
    {
        public string AreaName { get; set; }
        public int Amount { get; set; }
    }
}
