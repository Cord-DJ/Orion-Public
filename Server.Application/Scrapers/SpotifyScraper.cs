using Cord.Server.Domain;
using Newtonsoft.Json.Linq;

namespace Cord.Server.Application.Scrapers;

public class SpotifyScraper : IScraper {
    readonly YoutubeScraper youtubeScraper;
    readonly HttpClient client;

    bool isAuth;

    public SpotifyScraper(YoutubeScraper youtubeScraper) {
        this.youtubeScraper = youtubeScraper;

        client = new();
        client.BaseAddress = new("https://api.spotify.com");
    }

    public async IAsyncEnumerable<ISong> GetSongsFromPlaylist(string id) {
        await foreach (var (artist, name) in ScrapePlaylist(id)) {
            yield return await youtubeScraper.FindSongByName($"{artist} - {name}");
        }
    }

    public Task<ISong> GetSong(string id) => throw new NotImplementedException();

    public Task<ISong> FindSongByName(string name) => throw new NotImplementedException();

    public async Task<string> GetPlaylistName(string idOrLink) {
        await EnsureAuthenticated();

        var id = ParseId(idOrLink);
        var response = await client.GetAsync($"v1/playlists/{id}");
        response.EnsureSuccessStatusCode();

        dynamic payload = await response.Content.ReadAsAsync<JObject>();
        return payload.name;
    }

    public async IAsyncEnumerable<(string Id, string Name)> GetUserPlaylists(string idOrLink) {
        await EnsureAuthenticated();

        var id = ParseId(idOrLink);
        var response = await client.GetAsync($"v1/users/{id}/playlists");
        response.EnsureSuccessStatusCode();


        dynamic payload = await response.Content.ReadAsAsync<JObject>();
        foreach (var x in payload.items) {
            yield return (x.id, x.name);
        }
    }

    public async IAsyncEnumerable<(string Id, string Name, string Image)> FindUsers(string query) {
        await EnsureAuthenticated();

        var prms = "{\"searchTerm\":\"" + query + "\",\"offset\":0,\"limit\":10,\"numberOfTopResults\":5}";
        var test =
            "https://api-partner.spotify.com/pathfinder/v1/query?operationName=searchDesktop&variables=%7B%22searchTerm%22%3A%22michal+stanko%22%2C%22offset%22%3A0%2C%22limit%22%3A10%2C%22numberOfTopResults%22%3A5%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22bd2672b138f522a983b934eb47872fc413d1074142d783eded62defb260b853c%22%7D%7D";

        // var response = await client.GetAsync($"v1/search?variables={HttpUtility.UrlEncode(prms)}");
        var response = await client.GetAsync(test);
        response.EnsureSuccessStatusCode();

        dynamic payload = await response.Content.ReadAsAsync<JObject>();
        foreach (var x in payload.data.searchV2.users) {
            yield return (x.id, x.name, x.image.smallImageUrl);
        }
    }

    async Task<string> GetToken() {
        var http = new HttpClient();
        var data = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } });

        var secret = "OGI2YmY1NWVlNmIyNDhlYWJkNTA1ODFmMzBkYjMxOTU6N2E1MzMxMjFjNzFjNDE3N2E3OTQyZjhjYWRhYWNmODU==";
        http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {secret}");

        var response = await http.PostAsync("https://accounts.spotify.com/api/token", data);
        response.EnsureSuccessStatusCode();

        dynamic payload = await response.Content.ReadAsAsync<JObject>();
        return payload.access_token;
    }

    async Task EnsureAuthenticated() {
        if (!isAuth) {
            var token = await GetToken();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            isAuth = true;
        }
    }

    async IAsyncEnumerable<(string Artist, string Name)> ScrapePlaylist(string idOrLink) {
        await EnsureAuthenticated();

        var id = ParseId(idOrLink);
        var offset = 0;

        while (true) {
            var response = await client.GetAsync($"v1/playlists/{id}/tracks?offset={offset}");
            response.EnsureSuccessStatusCode();

            dynamic payload = await response.Content.ReadAsAsync<JObject>();
            foreach (var item in payload.items) {
                yield return (item.track.artists[0].name, item.track.name);
            }

            if (payload.items.Count < 100) {
                yield break;
            }

            offset += 100;
        }
    }

    static string ParseId(string link) =>
        Uri.TryCreate(link, UriKind.Absolute, out var uri) ? uri.Segments.Last() : link;
}
