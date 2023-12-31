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
        public string Name { get; set; }
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
        public string AreaOfExpertise { get; set; }
        public string Price { get; set; }
        public string ParentOrganization { get; set; }
        public string PrimarySpeciality { get; set; }
        public string SecondarySpeciality { get; set; }
        public string Category {  get; set; }
        public string ActivityStatisticsJson { get =>  ActivityStatistics.ToString(); }
        private ActivityStatistics ActivityStatistics { get; set; }

        public void AddCase(AreaCase areaCase)
        {
            ActivityStatistics.AreaCases.Add(areaCase);
            ActivityStatistics.TotalCases += areaCase.Amount;
        }
    }
}
