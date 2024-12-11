using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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

            int pageNumber = 1;  // Start from the first page
            int recordsPerPage = 50;  // Records per page
            int startRecordNumber = 1;  // Start from the first record

            List<User> allUsers = new List<User>();

            while (true)
            {
                var payload = new
                {
                    id = 0,
                    pageNumber = pageNumber,
                    previewSql = sqlQuery,
                    recordsPerPage = recordsPerPage,
                    startRecordNumber = startRecordNumber,
                    useDatabasePaging = true,
                    useDefaultParameters = true,
                    encodeHtml = true,


                };

                _logger.LogInformation("Starting API call for page {PageNumber}...", pageNumber);
                var rawResponse = await _apiService.PostAsync(apiUrl, token, payload);
                _logger.LogInformation("API call completed for page {PageNumber}.", pageNumber);

                var users = ParseUsersFromResponse(rawResponse);
                if (users == null || users.Count == 0)
                {
                    Console.WriteLine("wer are inside");
                    break;  // No more data to fetch
                }

                allUsers.AddRange(users);

                pageNumber++;
                startRecordNumber += recordsPerPage;
                Console.WriteLine($"count it: {allUsers.Count}");

            }
            Console.WriteLine("the response we got is: {allUsers.Count}");
            return allUsers;
        }



        private string BuildSqlQuery(List<ScimFilterCondition> conditions)
        {
            //var sqlQuery = "SELECT UserId,Password,Created,Enabled,LastModifiedDate FROM dbo.tbUser WHERE 1=1";
            var sqlQuery = "SELECT UserId,Password,Created,Enabled,LastModifiedDate,EmailAddress,UserName,DisplayName FROM dbo.tbUser WHERE 1=1";

            foreach (var condition in conditions)
            {
                sqlQuery += " AND " + BuildCondition(condition);
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
            Console.WriteLine("We are inside ParseUsersFromResponse");

            if (string.IsNullOrEmpty(rawResponse))
            {
                _logger.LogWarning("Empty response received from API.");
                return new List<User>();
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Deserialize the response into ApiResponse
                var response = JsonSerializer.Deserialize<ApiResponse>(rawResponse, options);
                //Console.WriteLine("The response we got is: " + rawResponse);

                if (response?.Rows == null || response.Rows.Count == 0)
                {
                    _logger.LogWarning("No rows found in the response.");
                    return new List<User>();
                }

                // Map rows to List<User>
                var users = new List<User>();
                foreach (var row in response.Rows)
                {
                    if (row.Count > 0) // Ensure there is data in the row
                    {
                        var user = new User
                        {
                            //UserId = row.Count > 0 ? DataConversionHelper.TryGetValue<int>(row[0], _logger) : 0,
                            //EmailAddress = row.Count > 1 ? DataConversionHelper.TryGetValue<string>(row[1], _logger) : null,
                            UserId = row.Count > 0 ? DataConversionHelper.TryGetValue<int>(row[0], _logger) : 0,

                            Password = row.Count > 1 ? DataConversionHelper.TryGetValue<string>(row[1], _logger) : null,
                            Created = row.Count > 2 ? DataConversionHelper.TryGetValue<DateTime>(row[2], _logger) : DateTime.MinValue,
                            Enabled = row.Count > 3 ? DataConversionHelper.TryGetValue<bool>(row[3], _logger) : false,
                           
                            LastModifiedDate = row.Count > 4 ? DataConversionHelper.TryGetValue<DateTime>(row[4], _logger) : DateTime.MinValue,
                            EmailAddress = row.Count > 5 ? DataConversionHelper.TryGetValue<string>(row[5], _logger) : null,
                            UserName = row.Count > 6 ? DataConversionHelper.TryGetValue<string>(row[6], _logger) : null,
                            DisplayName = row.Count > 7 ? DataConversionHelper.TryGetValue<string>(row[7], _logger) : null,

                        };
                        users.Add(user);
                    }
                    else
                    {
                        _logger.LogWarning("Row has no elements: {Row}", row);
                    }
                }

                Console.WriteLine(users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to parse API response: {Error}", ex.Message);
                return new List<User>();
            }
        }


    }
}