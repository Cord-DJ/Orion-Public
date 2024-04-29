using Cord.Server.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Cord.Server.Application;

public sealed class ResourceUploader {
    readonly IOptions<CDNOptions> options;

    public ResourceUploader(IOptions<CDNOptions> options) {
        this.options = options;
    }

    public async Task<string> UploadResource(string type, IUser owner, string data) {
        var http = new HttpClient();
        http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", options.Value.AuthToken);

        var json = JsonConvert.SerializeObject(new { UserId = owner.Id, Data = data });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await http.PostAsync($"{options.Value.Endpoint}/upload/{type}", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
