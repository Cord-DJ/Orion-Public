using App.Metrics;
using Cord.Server.Application.Users;
using Cord.Server.Domain;
using Cord.Server.Domain.Messages;
using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;

namespace Cord.Server;

public static class Scripts {
    public static void AutoDJ(IServiceProvider serviceProvider) {
        Task.Run(
            async () => {
                while (true) {
                    try {
                        using var service = serviceProvider.CreateScope();
                        var roomProvider = service.ServiceProvider.GetRequiredService<RoomProvider>();

                        await foreach (var room in roomProvider.GetRoomsWithExpiredSong()) {
                            var cur = room.CurrentSong;

                            if (cur?.EndTime <= DateTimeOffset.UtcNow) {
                                if (cur.Song.Duration > TimeSpan.FromMinutes(2)) {
                                    var votes = await room.GetVotes();

                                    await room.DistributeExp(
                                        (calc, user) => {
                                            if (room.CurrentSong?.UserId != user.Id) {
                                                return ValueTask.FromResult(0);
                                            }

                                            return ValueTask.FromResult(
                                                calc.PlayedSongCalculation(
                                                    votes[Vote.Upvote].Count,
                                                    votes[Vote.Downvote].Count,
                                                    room.GetOnlineNonGuestUsersCount(),
                                                    cur.Song.Duration
                                                )
                                            );
                                        }
                                    );
                                }

                                try {
                                    await room.ForceNewSong();
                                } catch (Exception) {
                                    await room.StopPlaying();
                                }
                            }
                        }
                    } catch (Exception e) {
                        Log.Warning(e, "Exception was thrown in AutoDJ");
                    }

                    await Task.Delay(1000);
                }
            }
        );
    }

    public static void DisconnectTimeout(IServiceProvider serviceProvider) {
        Task.Run(
            async () => {
                while (true) {
                    Log.Information("Executing disconnect timer");
                    try {
                        using var service = serviceProvider.CreateScope();
                        var userProvider = service.ServiceProvider.GetRequiredService<UserProvider>();

                        await foreach (var user in userProvider.GetDisconnectingUsers(new(0, 3, 0))) {
                            Log.Information("Disconnecting {Name}", user.Name);
                            // metrics.Measure.Histogram.Update(MetricRegistry.OnlineTime, (long)Math.Round(user.OnlineTime.TotalSeconds));

                            await user.RemoveActiveRooms();
                        }
                    } catch (Exception e) {
                        Log.Warning(e, "Exception was thrown in DisconnectTimer");
                    }

                    await Task.Delay(10_000);
                }
            }
        );
    }

    public static void MetricsUpdate(IServiceProvider serviceProvider) {
        Task.Run(
            async () => {
                while (true) {
                    Log.Information("Executing metrics update");
                    try {
                        using var service = serviceProvider.CreateScope();
                        var metrics = service.ServiceProvider.GetRequiredService<IMetrics>();

                        {
                            var userProvider = service.ServiceProvider.GetRequiredService<UserProvider>();

                            var daily = await userProvider.GetActiveUsersCount(new(1, 0, 0, 0));
                            var monthly = await userProvider.GetActiveUsersCount(new(30, 0, 0, 0));

                            metrics.Measure.Gauge.SetValue(MetricRegistry.DailyActiveUsers, daily);
                            metrics.Measure.Gauge.SetValue(MetricRegistry.MonthlyActiveUsers, monthly);
                        }
                        {
                            var onlineRepository = service.ServiceProvider.GetRequiredService<IOnlineRepository>();
                            var total = await onlineRepository.TotalOnline();

                            metrics.Measure.Gauge.SetValue(MetricRegistry.OnlineUsers, total);
                        }
                        {
                            var songRepository = service.ServiceProvider.GetRequiredService<ISongRepository>();
                            var total = await songRepository.TotalSongs();

                            metrics.Measure.Gauge.SetValue(MetricRegistry.TotalSongs, total);
                        }
                        {
                            var playlistRepository = service.ServiceProvider.GetRequiredService<IPlaylistRepository>();
                            var total = await playlistRepository.TotalPlaylists();

                            metrics.Measure.Gauge.SetValue(MetricRegistry.TotalPlaylists, total);
                        }
                        {
                            var messageRepository = service.ServiceProvider.GetRequiredService<IMessageRepository>();
                            var total = await messageRepository.TotalMessages();

                            metrics.Measure.Gauge.SetValue(MetricRegistry.TotalMessages, total);
                        }
                        {
                            var userRepository = service.ServiceProvider.GetRequiredService<IUserRepository>();
                            var total = await userRepository.TotalUsers();
                            var verified = await userRepository.TotalVerifiedUsers();

                            metrics.Measure.Gauge.SetValue(MetricRegistry.TotalUsers, total);
                            metrics.Measure.Gauge.SetValue(MetricRegistry.VerifiedUsers, verified);
                        }
                        {
                            var songsPlayedRepository =
                                service.ServiceProvider.GetRequiredService<ISongsPlayedRepository>();
                            var total = await songsPlayedRepository.TotalSongsPlayed();

                            metrics.Measure.Gauge.SetValue(MetricRegistry.TotalSongsPlayed, total);
                        }
                    } catch (Exception e) {
                        Log.Warning(e, "Exception was thrown in MetricsUpdate");
                    }

                    await Task.Delay(20_000);
                }
            }
        );
    }
}
