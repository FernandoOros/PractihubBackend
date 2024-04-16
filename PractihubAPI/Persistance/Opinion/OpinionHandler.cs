using Dapper;
using Microsoft.Extensions.Configuration;
using PractihubAPI.Models;
using PractihubAPI.Models.Opinion;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace PractihubAPI.Persistance.Opinion
{
    public class OpinionHandler:IOpinionHandler
    {
        private readonly IConfiguration configuration;

        public OpinionHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<List<Organization>> RetrieveOrganizations()
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT * FROM Organization;";

            var multiResponse = await connection.QueryMultipleAsync(query, commandTimeout: 0).ConfigureAwait(false);

            var previewList = new List<Organization>();

            while (!multiResponse.IsConsumed)
            {
                var resultSet = await multiResponse.ReadAsync<Organization>().ConfigureAwait(false);
                previewList.AddRange(resultSet);
            }

            return previewList;
        }

        public async Task<Organization> SearchSpecificOrganization(string organizationId)
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT * FROM Organization WHERE OrganizationId = '" + organizationId + "';";

            var response = await connection.QueryFirstOrDefaultAsync<Organization>(query, commandTimeout: 0).ConfigureAwait(false);

            return response;
        }


        public async Task<List<Organization>> SearchForOrganization(string organization)
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT * FROM Organization WHERE OrganizationName LIKE '" + organization +"%'";

            var multiResponse = await connection.QueryMultipleAsync(query, commandTimeout: 0).ConfigureAwait(false);

            var previewList = new List<Organization>();

            while (!multiResponse.IsConsumed)
            {
                var resultSet = await multiResponse.ReadAsync<Organization>().ConfigureAwait(false);
                previewList.AddRange(resultSet);
            }
            return previewList;
        }

        public async Task<OrganizationDetail> SearchOrganizationDetail(string organizationId)
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT 
                                o.OrganizationId,
                                o.OrganizationName,
                                o.OrganizationDescription,
                                o.OrganizationAddress,
                                COALESCE(AVG(op.EaseActivities), 0) AS AverageEaseActivities,
                                COALESCE(AVG(op.Environment), 0) AS AverageEnvironment,
                                COALESCE(AVG(op.Help), 0) AS AverageHelp
                            FROM 
                                Organization o
                            LEFT JOIN 
                                Opinion op ON o.OrganizationId = op.OrganizationId
                            WHERE 
                                o.OrganizationId = '" + organizationId +
                            @"'GROUP BY 
                                o.OrganizationId, o.OrganizationName, o.OrganizationDescription, o.OrganizationAddress;";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { SpecificOrganizationId = organizationId }, commandTimeout: 0).ConfigureAwait(false);

            if (result == null) return null;

            var organizationDetail = new OrganizationDetail
            {
                OrganizationInfo = new Organization
                {
                    OrganizationId = result.OrganizationId,
                    OrganizationName = result.OrganizationName,
                    OrganizationDescription = result.OrganizationDescription,
                    OrganizationAddress = result.OrganizationAddress
                },
                AverageEaseActivities = result.AverageEaseActivities,
                AverageEnvironment = result.AverageEnvironment,
                AverageHelp = result.AverageHelp
            };

            return organizationDetail;
        }

        public async Task<List<OrganizationService>> SearchOrganizationServices(string organizationId)
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT 
                                O.OrganizationName,
                                OS.ServiceName
                            FROM 
                                Organization AS O
                            INNER JOIN 
                                Opinion AS Op ON O.OrganizationId = Op.OrganizationId
                            INNER JOIN 
                                OpinionService AS Ops ON Op.OpinionId = Ops.OpinionId
                            INNER JOIN 
                                OrganizationService AS OS ON Ops.ServiceId = OS.ServiceId
                            WHERE 
                                O.OrganizationId = '" + organizationId +
                            @"'GROUP BY 
                                O.OrganizationName, OS.ServiceName";

            var multiResponse = await connection.QueryMultipleAsync(query, commandTimeout: 0).ConfigureAwait(false);

            var previewList = new List<OrganizationService>();

            while (!multiResponse.IsConsumed)
            {
                var resultSet = await multiResponse.ReadAsync<OrganizationService>().ConfigureAwait(false);
                previewList.AddRange(resultSet);
            }
            return previewList;
        }

        public async Task<List<Service>> SearchServices()
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT * FROM OrganizationService";

            var multiResponse = await connection.QueryMultipleAsync(query, commandTimeout: 0).ConfigureAwait(false);

            var previewList = new List<Service>();

            while (!multiResponse.IsConsumed)
            {
                var resultSet = await multiResponse.ReadAsync<Service>().ConfigureAwait(false);
                previewList.AddRange(resultSet);
            }
            return previewList;
        }


        public async Task<CustomResponse> SubmitOpinion(DynamicParameters parameters)
        {
            using IDbConnection connection = Connection;

            string query = @"
                        DECLARE @OpinionId UNIQUEIDENTIFIER = NEWID();

                        INSERT INTO Opinion (OpinionId, OrganizationId, PreparationType, EaseActivities, Environment, Help, Comment, Email, SubmitIn)
                        VALUES (@OpinionId, @OrganizationId, @PreparationType, @EaseActivities, @Environment, @Help, @Comment, @Email, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'UTC' AT TIME ZONE 'Central Standard Time (Mexico)' AS datetime));

                        IF @Service1 IS NOT NULL
                        BEGIN
                            INSERT INTO OpinionService (OpinionId, ServiceId)
                            SELECT @OpinionId, ServiceId FROM OrganizationService WHERE ServiceName = @Service1;
                        END

                        IF @Service2 IS NOT NULL
                        BEGIN
                            INSERT INTO OpinionService (OpinionId, ServiceId)
                            SELECT @OpinionId, ServiceId FROM OrganizationService WHERE ServiceName = @Service2;
                        END

                        IF @Service3 IS NOT NULL
                        BEGIN
                            INSERT INTO OpinionService (OpinionId, ServiceId)
                            SELECT @OpinionId, ServiceId FROM OrganizationService WHERE ServiceName = @Service3;
                        END

                        SET @MessageResponse = 'successfully' 
                    ";

            await connection.ExecuteAsync(query, parameters, commandTimeout: 0).ConfigureAwait(false);

            return new CustomResponse
            {
                Status = parameters.Get<string>("MessageResponse") == "successfully" ? true : false,
                Message = parameters.Get<string>("MessageResponse") == "successfully" ? "successfully" : "error in db"
            };
        }

        public async Task<IEnumerable<OpinionETL>> SearchOpinions(string organizationId)
        {
            using IDbConnection connection = Connection;

            var opinionsDictionary = new Dictionary<Guid, OpinionETL>();

            var query = @"
                SELECT 
                    DISTINCT(o.OpinionId),
                    o.OrganizationId,
                    o.PreparationType,
                    o.EaseActivities,
                    o.Environment,
                    o.Help,
                    o.Comment,
                    o.SubmitIn,
                    o.IsReported,
                    os.ServiceName
                FROM 
                    Opinion o
                    LEFT JOIN OpinionService ops ON o.OpinionId = ops.OpinionId
                    LEFT JOIN OrganizationService os ON ops.ServiceId = os.ServiceId
                WHERE 
                    o.OrganizationId = @OrganizationId 
                ORDER BY o.SubmitIn DESC
            ";

            var result = await connection.QueryAsync<OpinionETL, string, OpinionETL>(
                query,
                (opinion, serviceName) =>
                {
                    OpinionETL opinionEntry;

                    if (!opinionsDictionary.TryGetValue(opinion.OpinionId, out opinionEntry))
                    {
                        opinionEntry = opinion;
                        opinionEntry.Services = new List<string>();

                        // Hide the content of reported opinions
                        if (opinionEntry.IsReported)
                        {
                            opinionEntry.Comment = "This opinion has been reported and is under review.";
                        }

                        opinionsDictionary.Add(opinionEntry.OpinionId, opinionEntry);
                    }

                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        opinionEntry.Services.Add(serviceName);
                    }

                    return opinionEntry;
                },
                new { OrganizationId = organizationId },
                splitOn: "ServiceName"
            );

            return opinionsDictionary.Values;
        }


        public async Task<CustomResponse> HideOpinion(DynamicParameters parameters)
        {
            using IDbConnection connection = Connection;

            string query = @"
                       BEGIN TRY
                            UPDATE Opinion
                            SET IsReported = 1
                            WHERE OpinionId = @OpinionId;
                            SET @MessageResponse = 'successfully' 
                        END TRY
                        BEGIN CATCH
                            PRINT ERROR_MESSAGE();
                        END CATCH;";


            await connection.ExecuteAsync(query, parameters, commandTimeout: 0).ConfigureAwait(false);

            return new CustomResponse
            {
                Status = parameters.Get<string>("MessageResponse") == "successfully" ? true : false,
                Message = parameters.Get<string>("MessageResponse") == "successfully" ? "successfully" : "error while reporting"
            };
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(configuration.GetConnectionString("DBConnection"));
            }
        }
    }
}
