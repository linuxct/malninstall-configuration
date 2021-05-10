using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using space.linuxct.malninstall.Configuration.Exceptions;
using space.linuxct.malninstall.Configuration.Models.Hcaptcha;

namespace space.linuxct.malninstall.Configuration.Helpers.Hcaptcha
{
    public static class HcaptchaValidationHelper
    {
        public static async Task<bool> CheckHcaptchaResponseIsValid(string clientResponse, string secret)
        {
            using var client = new HttpClient();
            var hCaptchaContents = new List<KeyValuePair<string, string>>
            {
                new ("secret", secret),
                new ("response", clientResponse)
            };

            var content = new FormUrlEncodedContent(hCaptchaContents);
            try
            {
                var response = await client.PostAsync("https://hcaptcha.com/siteverify", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var hCaptchaResponse = JsonSerializer.Deserialize<HcaptchaResponse>(resultJson,
                        new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                    if (hCaptchaResponse != null && hCaptchaResponse.Success)
                    {
                        return await Task.FromResult(hCaptchaResponse.Success);
                    }
                }
            }
            catch (Exception)
            {
                throw new InvalidSignatureException("Web validation failed");
            }

            return await Task.FromResult(false);
        }
    }
}