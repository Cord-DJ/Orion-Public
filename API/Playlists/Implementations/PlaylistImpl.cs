using Newtonsoft.Json;
using System.Text;

namespace Cord;

sealed class PlaylistImpl : IPlaylist, ISnowflakeEntity {
    readonly List<ISong> songs = new();

    public ID Id { get; init; }
    public string Name { get; init; } = default!;
    public bool IsProcessing { get; init; } = default;
    public ID? NextSongId { get; init; }
    public IReadOnlyCollection<ISong> Songs => songs.AsReadOnly();


    public PlaylistImpl(SongImpl[] songs) {
        this.songs.AddRange(songs);
    }

    public async Task ReorderSongs(ISong song, int position) {
        var json = JsonConvert.SerializeObject(new { song.Id, Position = position });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PatchAsync($"/api/playlists/{Id}/songs", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task SetActive() {
        var response = await CordClient.Http.PostAsync($"/api/playlists/{Id}/active", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ISong> AddSong(ISong song) {
        var json = JsonConvert.SerializeObject(new { song.YoutubeId });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PutAsync($"/api/playlists/{Id}/songs", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<SongImpl>();
    }

    public async Task DeleteSong(ISong song) {
        var response = await CordClient.Http.DeleteAsync($"/api/playlists/{Id}/songs/{song.Id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task SetName(string name) {
        var json = JsonConvert.SerializeObject(new { Name = name });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PatchAsync($"/api/playlists/{Id}", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task SetNextSong(ISong song) {
        // TODO: use ID instead of me?
        var response = await CordClient.Http.PostAsync($"/api/playlists/{Id}/songs/{song.Id}/next", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task Delete() {
        var response = await CordClient.Http.DeleteAsync($"/api/playlists/{Id}");
        response.EnsureSuccessStatusCode();
    }

    public override bool Equals(object? obj) => obj is IPlaylist entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
