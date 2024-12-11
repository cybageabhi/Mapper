using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Server.Helpers
{
    public static class DataConversionHelper
    {
        public static T TryGetValue<T>(object value, ILogger logger = null)
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    if (typeof(T) == typeof(int) && jsonElement.TryGetInt32(out var intValue))
                        return (T)(object)intValue;

                    if (typeof(T) == typeof(string) && jsonElement.ValueKind == JsonValueKind.String)
                        return (T)(object)jsonElement.GetString();

                    if (typeof(T) == typeof(DateTime) && jsonElement.TryGetDateTime(out var dateTimeValue))
                        return (T)(object)dateTimeValue;

                    if (typeof(T) == typeof(bool) && jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                        return (T)(object)jsonElement.GetBoolean();

                    if (jsonElement.ValueKind == JsonValueKind.Null)
                    {
                        return default;
                    }

                }
                else if (value is T validValue)
                {
                    return validValue;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError("Error converting value: {Value}. Exception: {Exception}", value, ex.Message);
            }

            return default;
        }

    }
}
