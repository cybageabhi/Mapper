using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Helpers;
using Server.Models;

namespace Server.Service
{
    public class UserService
    {
        private readonly ApiService _apiService;
        private readonly ILogger<UserService> _logger;

        public UserService(ApiService apiService, ILogger<UserService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<List<User>> GetUsersByFilterConditionsAndCallApi(List<ScimFilterCondition> conditions, string apiUrl, string token)
        {
            string sqlQuery = BuildSqlQuery(conditions);
            _logger.LogInformation("Generated SQL Query: {SqlQuery}", sqlQuery);

            var payload = new
            {
                id = 0,
                previewSql = sqlQuery
            };

            Console.WriteLine("we are started ");
            var rawResponse = await _apiService.PostAsync(apiUrl, token, payload);
            Console.WriteLine("we not ended ");
            
            return ParseUsersFromResponse(rawResponse);
        }

        private string BuildSqlQuery(List<ScimFilterCondition> conditions)
        {
            var sqlQuery = "SELECT UserId, UserName, DisplayName, Password, Created, EmailAddress FROM dbo.tbUser WHERE 1=1";
            foreach (var condition in conditions)
            {
                sqlQuery += $" AND {BuildCondition(condition)}";
            }

            return sqlQuery;
        }

        private string BuildCondition(ScimFilterCondition condition)
        {
            return condition.Attribute.ToLower() switch
            {
                "userid" => BuildNumericCondition("UserId", condition),
                "username" => BuildStringCondition("UserName", condition),
                "displayname" => BuildStringCondition("DisplayName", condition),
                "password" => BuildStringCondition("Password", condition),
                "enabled" => BuildBooleanCondition("Enabled", condition),
                "lastmodifieddate" => BuildDateCondition("LastModifiedDate", condition),
                "created" => BuildDateCondition("Created", condition),
                "emailaddress" => BuildStringCondition("EmailAddress", condition),
                _ => throw new NotSupportedException($"Unsupported attribute: {condition.Attribute}")
            };
        }

        private string BuildNumericCondition(string column, ScimFilterCondition condition) =>
            condition.Operator switch
            {
                "gt" => $"{column} > {condition.Value}",
                "lt" => $"{column} < {condition.Value}",
                "eq" => $"{column} = {condition.Value}",
                _ => throw new NotSupportedException($"Unsupported numeric operator: {condition.Operator}")
            };

        private string BuildStringCondition(string column, ScimFilterCondition condition)
        {
            string value = condition.Value.Replace("'", "''");
            return condition.Operator switch
            {
                "eq" => $"{column} = '{value}'",
                "co" => $"{column} LIKE '%{value}%'",
                "sw" => $"{column} LIKE '{value}%'",
                _ => throw new NotSupportedException($"Unsupported string operator: {condition.Operator}")
            };
        }

        private string BuildBooleanCondition(string column, ScimFilterCondition condition)
        {
            string value = condition.Value.ToLower() switch
            {
                "true" => "1",
                "false" => "0",
                _ => throw new FormatException($"Invalid boolean value: {condition.Value}")
            };

            return condition.Operator switch
            {
                "eq" => $"{column} = {value}",
                "ne" => $"{column} != {value}",
                _ => throw new NotSupportedException($"Unsupported boolean operator: {condition.Operator}")
            };
        }

        private string BuildDateCondition(string column, ScimFilterCondition condition)
        {
            string value = DateTime.Parse(condition.Value).ToString("yyyy-MM-dd HH:mm:ss.fff");
            return condition.Operator switch
            {
                "gt" => $"{column} > '{value}'",
                "lt" => $"{column} < '{value}'",
                "eq" => $"{column} = '{value}'",
                _ => throw new NotSupportedException($"Unsupported date operator: {condition.Operator}")
            };
        }

        private List<User> ParseUsersFromResponse(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse)) return new List<User>();

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(rawResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Rows == null) return new List<User>();

                return apiResponse.Rows.Select(row => new User
                {
                    UserId = DataConversionHelper.TryGetValue<int>(row.ElementAtOrDefault(0), _logger),
                    UserName = DataConversionHelper.TryGetValue<string>(row.ElementAtOrDefault(1), _logger),
                    DisplayName = DataConversionHelper.TryGetValue<string>(row.ElementAtOrDefault(2), _logger),
                    Password = DataConversionHelper.TryGetValue<string>(row.ElementAtOrDefault(3), _logger),
                    Created = DataConversionHelper.TryGetValue<DateTime>(row.ElementAtOrDefault(4), _logger),
                    EmailAddress = DataConversionHelper.TryGetValue<string>(row.ElementAtOrDefault(5), _logger),
                    Enabled = apiResponse.Enabled
                }).ToList();
            }
            catch (JsonException ex)
            {
                _logger.LogError("JSON parsing error: {Error}", ex.Message);
                throw;
            }
        }
    }
}
