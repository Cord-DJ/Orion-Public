using Cord.Server.Domain;
using System.Net.Http.Json;

namespace Cord.Server.Application;

public class HCaptcha : ICaptcha {
    public async Task<bool> Verify(string token) {
        var http = new HttpClient();
        var values = new Dictionary<string, string> {
            { "secret", "" }, { "response", token }
        };

        var content = new FormUrlEncodedContent(values);
        var response = await http.PostAsync("https://hcaptcha.com/siteverify", content);
        var res = await response.Content.ReadFromJsonAsync<HCaptchaResponse>();

        return res?.Success ?? false;
    }
}

record HCaptchaResponse(bool Success);
