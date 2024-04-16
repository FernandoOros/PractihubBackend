using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PractihubAPI.Persistance;
using System.Threading.Tasks;
using System;
using PractihubAPI.Persistance.Opinion;
using PractihubAPI.Models.Opinion;
using Dapper;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace PractihubAPI.Controllers.Opinion
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpinionController : ControllerBase
    {
        private readonly IOpinionHandler handler;

        public OpinionController(IOpinionHandler handler)
        {
            this.handler = handler;
        }


        //Listar las organizaciones
        [HttpGet("list-organizations")]
        public async Task<IActionResult> GetOrganizations()
        {
            try
            {
                return Ok(await handler.RetrieveOrganizations());
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

        //Buscar organización en general
        [HttpGet("search-organization")]
        public async Task<IActionResult> SearchOrganization([FromQuery] string organization)
        {
            try
            {
                return Ok(await handler.SearchForOrganization(organization));
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

        //Traer datos de la organización
        [HttpGet("organization")]
        public async Task<IActionResult> GetOrganization([FromQuery] string organizationId)
        {
            try
            {
                return Ok(await handler.SearchSpecificOrganization(organizationId));
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

        //Buscar organización y promedios
        [HttpGet("organization-detail")]
        public async Task<IActionResult> GetOrganizationDetail([FromQuery] string organizationId)
        {
            try
            {
                return Ok(await handler.SearchOrganizationDetail(organizationId));
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

        //Listar servicios asociados 
        [HttpGet("organization-services")]
        public async Task<IActionResult> GetOrganizationServices([FromQuery] string organizationId)
        {
            try
            {
                return Ok(await handler.SearchOrganizationServices(organizationId));
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

        //Recuperar todos los servicios
        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            try
            {
                return Ok(await handler.SearchServices());
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

        //Guardar opinion
        [Authorize]
        [HttpPost("opinion-submit")]
        public async Task<IActionResult> PostOpinion([FromBody] OpinionETL opinion)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    throw new ArgumentNullException("An error occurred: Bad Argument(s)");
                }

                DynamicParameters parameters = new();

                parameters.Add("OrganizationId", opinion.OrganizationId);
                parameters.Add("PreparationType", opinion.PreparationType);
                parameters.Add("EaseActivities", opinion.EaseActivities);
                parameters.Add("Environment", opinion.Environment);
                parameters.Add("Help", opinion.Help);
                parameters.Add("Comment", opinion.Comment);

                parameters.Add("Service1", opinion.Services.Count > 0 ? opinion.Services[0] : null);
                parameters.Add("Service2", opinion.Services.Count > 1 ? opinion.Services[1] : null);
                parameters.Add("Service3", opinion.Services.Count > 2 ? opinion.Services[2] : null);
                parameters.Add("Email", opinion.Email);


                parameters.Add("MessageResponse", dbType:DbType.String, direction:ParameterDirection.Output,size:100);

                return Ok(await handler.SubmitOpinion(parameters)); ;    
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }


        //Recuperar todos los servicios
        [HttpGet("opinions")]
        public async Task<IActionResult> GetOpinions([FromQuery] string organizationId)
        {
            try
            {
                return Ok(await handler.SearchOpinions(organizationId));
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }


        //Reportar opinion
        [HttpGet("report")]
        public async Task<IActionResult> ReportOpinion([FromQuery] string opinionId)
        {
            try
            {
                DynamicParameters parameters = new();

                parameters.Add("OpinionId", opinionId);
                parameters.Add("MessageResponse", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

                return Ok(await handler.HideOpinion(parameters));
            }
            catch (Exception e)
            {
                throw new Exception("An error has ocurred", e);
            }
        }

    }
}
