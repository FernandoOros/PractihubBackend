using System.Collections.Generic;

namespace PractihubAPI.Models.Opinion
{
    public class OrganizationDetail
    {
        //Modelo de organización con servicios ofrecidos
        public Organization OrganizationInfo { get; set; }
        public double AverageEaseActivities { get; set; }
        public double AverageEnvironment { get; set; }
        public double AverageHelp { get; set; } 
    }
}
