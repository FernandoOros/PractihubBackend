using Dapper;
using Microsoft.Extensions.Configuration;
using PractihubAPI.Models;
using PractihubAPI.Models.Opinion;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PractihubAPI.Persistance.Opinion
{
    public interface IOpinionHandler
    {
        public Task<List<Organization>> RetrieveOrganizations();
        public Task<List<Organization>> SearchForOrganization(string organization);
        public Task<OrganizationDetail> SearchOrganizationDetail(string organizationId);
        public Task<List<OrganizationService>> SearchOrganizationServices(string organizationId);
        public Task<Organization> SearchSpecificOrganization(string organizationId);
        public Task<List<Service>> SearchServices();
        public Task<CustomResponse> SubmitOpinion(DynamicParameters parameters);
        public Task<IEnumerable<OpinionETL>> SearchOpinions(string organizationId);
        public Task<CustomResponse> HideOpinion(DynamicParameters parameters);
    }
}
