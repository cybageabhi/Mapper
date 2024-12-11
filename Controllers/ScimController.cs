using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Service;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScimController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public ScimController(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers([FromQuery] string? scimFilter = null)
        {
            try
            {
                List<ScimFilterCondition> filterConditions = new();

                // If the SCIM filter is provided, parse it
                if (!string.IsNullOrEmpty(scimFilter))
                {
                    filterConditions = ParseScimFilter(scimFilter);
                }

                string apiUrl = "https://scimtest.secretservercloud.com/api/v1/reports/execute";

                string token = await _tokenService.GetTokenAsync(
                    "https://scimtest.secretservercloud.com/oauth2/token",
                    "abhi",
                    "Abhi@12345"
                );
                Console.WriteLine("We have token as: " + token);

                // Call the user service with or without filter conditions
                var users = await _userService.GetUsersByFilterConditionsAndCallApi(filterConditions, apiUrl, token);

                if (users == null || users.Count == 0)
                {
                    return NotFound("No users found matching the filter.");
                }

                return Ok(users);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private List<ScimFilterCondition> ParseScimFilter(string scimFilter)
        {
            var supportedOperators = new[] { "eq", "co", "gt", "lt", "sw", "ne" };
            var regex = new Regex(@"(\w+(\.\w+)?)[ ]+(eq|co|gt|lt|sw|ne)[ ]+""([^""]+)""");
            var matches = regex.Matches(scimFilter);
            var conditions = new List<ScimFilterCondition>();

            foreach (Match match in matches)
            {
                var op = match.Groups[3].Value;
                if (!Array.Exists(supportedOperators, operatorValue => operatorValue == op))
                {
                    throw new ArgumentException($"Unsupported operator: {op}");
                }

                conditions.Add(new ScimFilterCondition
                {
                    Attribute = match.Groups[1].Value,
                    Operator = op,
                    Value = match.Groups[4].Value
                });
            }

            return conditions;
        }
    }
}
