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
        public Company(Company other)
        {
            ActivityStatistics = other.ActivityStatistics;
            Name = other.Name;
            Phone = other.Phone;
            TargetGroups = other.TargetGroups;
            Site = other.Site;
            Email = other.Email;
            Facebook = other.Facebook;
            Address = other.Address;
            GuideStarLink = other.GuideStarLink;
            ActiveArea = other.ActiveArea;
            Fax = other.Fax;
            CompanyId = other.CompanyId;
            Status = other.Status;
            AreaOfExpertise = other.AreaOfExpertise;
            Price = other.Price;
            ParentOrganization = other.ParentOrganization;
            PrimarySpeciality = other.PrimarySpeciality;
            SecondarySpeciality = other.SecondarySpeciality;
            Category = other.Category;
            Insurance = other.Insurance;
            IntervantionType = other.IntervantionType;
            GroupAverageWait = other.GroupAverageWait;
            IndividualAverageWait = other.IndividualAverageWait;
            IntakeWait = other.IntakeWait;
            ImgLink = other.ImgLink;

        }
        
        public string Name { get; set; }
        public string? Phone { get; set; }
        public string TargetGroups { get; set; }
        public string? Site { get; set; }
        public string? Email { get; set; }
        public string? Facebook { get; set; }
        public string? Address { get; set; }
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
        public string Insurance {  get; set; }
        public string IntervantionType {  get; set; }
        public string GroupAverageWait {  get; set; }
        public string IndividualAverageWait {  get; set; }
        public string IntakeWait {  get; set; }
        public string ImgLink {  get; set; }
        public string ActivityStatisticsJson { get =>  ActivityStatistics.ToString(); }
        private ActivityStatistics ActivityStatistics { get; set; }

        public void AddCase(AreaCase areaCase)
        {
            ActivityStatistics.AreaCases.Add(areaCase);
            ActivityStatistics.TotalCases += areaCase.Amount;
        }
    }
}
