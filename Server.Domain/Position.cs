using System.Numerics;

namespace Cord.Server.Domain;

public sealed record Position(int X, int Y) : IPosition {
    public static Position NewRandomPosition() {
        var random = new Random();
        return new(random.Next(0, 20) * 5, random.Next(0, 20) * 5);
        // return new(random.Next(0, 24), random.Next(0, 18)); this is for 3D
    }

    //
    // public static List<Vector2> GeneratePoints(
    //     float radius,
    //     Vector2 sampleRegionSize,
    //     int numSamplesBeforeRejection = 30
    // ) {
    //     var random = new Random();
    //     var cellSize = radius / Math.Sqrt(2);
    //
    //     var grid = new int[Math.CeilToInt(sampleRegionSize.X / cellSize), Math.CeilToInt(sampleRegionSize.y / cellSize)];
    //     var points = new List<Vector2>();
    //     var spawnPoints = new List<Vector2> { sampleRegionSize / 2 };
    //
    //     while (spawnPoints.Count > 0) {
    //         int spawnIndex = random.Next(0, spawnPoints.Count);
    //         var spawnCentre = spawnPoints[spawnIndex];
    //         var candidateAccepted = false;
    //
    //         for (var i = 0; i < numSamplesBeforeRejection; i++) {
    //             double angle = random.Next() * Math.PI * 2;
    //             var dir = new Vector2(Math.Sin(angle), Math.Cos(angle));
    //             var candidate = spawnCentre + dir * random.Next(radius, 2 * radius);
    //             if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid)) {
    //                 points.Add(candidate);
    //                 spawnPoints.Add(candidate);
    //                 
    //                 grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;
    //                 candidateAccepted = true;
    //                 break;
    //             }
    //         }
    //
    //         if (!candidateAccepted) {
    //             spawnPoints.RemoveAt(spawnIndex);
    //         }
    //     }
    //
    //     return points;
    // }
    //
    // static bool IsValid(
    //     Vector2 candidate,
    //     Vector2 sampleRegionSize,
    //     double cellSize,
    //     float radius,
    //     List<Vector2> points,
    //     int[,] grid
    // ) {
    //     if (candidate.X >= 0
    //         && candidate.X < sampleRegionSize.X
    //         && candidate.Y >= 0
    //         && candidate.Y < sampleRegionSize.Y
    //        ) {
    //         var cellX = (int)(candidate.X / cellSize);
    //         var cellY = (int)(candidate.Y / cellSize);
    //
    //         var searchStartX = Math.Max(0, cellX - 2);
    //         var searchEndX = Math.Min(cellX + 2, grid.GetLength(0) - 1);
    //         var searchStartY = Math.Max(0, cellY - 2);
    //         var searchEndY = Math.Min(cellY + 2, grid.GetLength(1) - 1);
    //
    //         for (var x = searchStartX; x <= searchEndX; x++) {
    //             for (var y = searchStartY; y <= searchEndY; y++) {
    //                 var pointIndex = grid[x, y] - 1;
    //
    //                 if (pointIndex != -1) {
    //                     float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
    //                     if (sqrDst < radius * radius) {
    //                         return false;
    //                     }
    //                 }
    //             }
    //         }
    //
    //         return true;
    //     }
    //
    //     return false;
    // }
}
