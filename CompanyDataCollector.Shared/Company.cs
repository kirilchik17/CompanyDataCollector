using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyDataCollector.Shared
{
    public class Company
    {
        public Company()
        {
            ActivityStatistics = new ActivityStatistics() { AreaCases = new List<AreaCase>()};
        }

        public string Phone { get; set; }
        public string TargetGroups { get; set; }
        public string Site { get; set; }
        public string Email { get; set; }
        public string Facebook { get; set; }
        public string Address { get; set; }
        public string GuideStarLink { get; set; }
        public string ActiveArea { get; set; }
        public string Fax { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public string ActivityStatisticsJson { get =>  ActivityStatistics.ToString(); }
        private ActivityStatistics ActivityStatistics { get; set; }

        public void AddCase(AreaCase areaCase)
        {
            ActivityStatistics.AreaCases.Add(areaCase);
            ActivityStatistics.TotalCases += areaCase.Amount;
        }
    }
}
