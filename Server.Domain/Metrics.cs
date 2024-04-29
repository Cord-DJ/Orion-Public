using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Timer;

namespace Cord.Server.Domain;

public static class MetricRegistry {
    public static readonly CounterOptions OpenSessions = new() { Name = "open_sessions" };

    public static readonly CounterOptions GuestsCreated = new() { Name = "guests_created" };
    public static readonly CounterOptions RoomEnters = new() { Name = "room_enters" };
    public static readonly CounterOptions Woots = new() { Name = "woots" };
    public static readonly CounterOptions Mehs = new() { Name = "mehs" };
    public static readonly CounterOptions Steals = new() { Name = "steals" };
    public static readonly CounterOptions Skips = new() { Name = "skips" };
    public static readonly CounterOptions Messages = new() { Name = "messages" };

    public static readonly TimerOptions YoutubePlaylistImport = new() {
        Name = "youtube_playlist_import",
        DurationUnit = TimeUnit.Milliseconds,
        RateUnit = TimeUnit.Milliseconds,
        MeasurementUnit = Unit.Requests
    };

    public static readonly TimerOptions YoutubeSongInfo = new() {
        Name = "youtube_song_info",
        DurationUnit = TimeUnit.Milliseconds,
        RateUnit = TimeUnit.Milliseconds,
        MeasurementUnit = Unit.Requests
    };

    public static readonly HistogramOptions ImportedSongsPerPlaylist = new() {
        Name = "imported_songs_per_playlist", MeasurementUnit = Unit.Items
    };

    public static readonly GaugeOptions DailyActiveUsers = new() { Name = "daily_active_users" };
    public static readonly GaugeOptions MonthlyActiveUsers = new() { Name = "monthly_active_users" };
    public static readonly HistogramOptions OnlineTime = new() { Name = "online_time" };

    public static readonly GaugeOptions VerifiedUsers = new() { Name = "verified_users" };
    public static readonly GaugeOptions OnlineUsers = new() { Name = "online_users" };
    public static readonly GaugeOptions TotalSongs = new() { Name = "total_songs" };
    public static readonly GaugeOptions TotalPlaylists = new() { Name = "total_playlists" };
    public static readonly GaugeOptions TotalMessages = new() { Name = "total_messages" };
    public static readonly GaugeOptions TotalUsers = new() { Name = "total_users" };
    public static readonly GaugeOptions TotalGuests = new() { Name = "total_guests" };
    public static readonly GaugeOptions TotalSongsPlayed = new() { Name = "total_songs_played" };
}
