using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyDataCollector.Shared
{
    public class Company
    {
        public string Phone { get; set; }
        public string Site { get; set; }
        public string Email { get; set; }
        public string Facebook { get; set; }
        public string Address { get; set; }
        public string GuideStarLink { get; set; }
        public string ActiveArea { get; set; }
        public string Fax { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public ActivityStatistics ActivityStatistics { get; set; }
    }
}
