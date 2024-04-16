using System;
using System.Collections.Generic;
using System.Globalization;

namespace PractihubAPI.Models.Opinion
{
    //Modelo de la Opinión
    public class OpinionETL
    {
        public Guid OpinionId { get; set; }
        public string OrganizationId { get; set; }
        public string PreparationType {  get; set; }
        public string EaseActivities { get; set; }
        public string Environment { get; set; }
        public string Help { get; set; }
        public string Comment { get; set; }
        public List<string> Services { get; set; }
        public string Email { get; set; }
        public DateTime? SubmitIn { get; set; }
        public bool IsReported { get; set; }
    }
}
