using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Leveling;

public sealed class ExpCalculator {
    public const int MaxLevel = 99;
    const int SongExperiencePerMinute = 4;
    // const int MessageConstant = 1;
    // const int OnlineTimeConstant = 1;
    // const int VoteConstant = 1;

    public ExpCalculator(User user) { }

    public int PlayedSongCalculation(int woots, int mehs, long onlineCount, TimeSpan songDuration) {
        var positivity = Math.Clamp(woots - mehs + 1, 1, 50);
        onlineCount = Math.Clamp(onlineCount, 1, 20);
        return positivity * (int)onlineCount * SongExperiencePerMinute * (int)songDuration.TotalMinutes;
    }

    // public int MessageCalculation(string message, long onlineCount) {
    //     if (message.Length < 20 && onlineCount < 10) {
    //         return 0;
    //     }
    //
    //     return MessageConstant;
    // }
    //
    // public int OnlineTimeCalculation() => OnlineTimeConstant;
    //
    // public int VoteCalculation() => VoteConstant;

    // DEPRECATED! Based onto WoW leveling
    public static int MaxExpForLevel(int level) => (8 * level + DifficultyMultiplier(level)) * (45 + 5 * level);

    static int DifficultyMultiplier(int level) {
        if (level <= 10) {
            return 0;
        }

        if (level == 11) {
            return 1;
        }

        if (level == 12) {
            return 3;
        }

        if (level == 13) {
            return 6;
        }

        return 5 * (level - 12);
    }
    
    
    // Taken from runescape
    public static int ExperienceForLevel(int level) {
        level = Math.Clamp(level, 1, MaxLevel); // Some of the users have a 0 level
        float total = 0;
        for (var i = 1; i < level; i++) {
            total += MathF.Floor(i + 300 * MathF.Pow(2, i / 7f));
        }

        return (int)MathF.Floor(total / 4f);
    }
}
