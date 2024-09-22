using System.Diagnostics;

namespace WeaponZ.Game;

public class FrameTimer
{
    private readonly Stopwatch _stopwatch;

    public FrameTimer()
    {
        _stopwatch = new Stopwatch();
    }

    public void SleepUntilTargetFrameTime(uint targetFramesPerSecond)
    {
        double secondsPerFrame = 1.0d / targetFramesPerSecond;

        var targetFrameTime = TimeSpan.FromSeconds(secondsPerFrame);

        var elapsed = _stopwatch.Elapsed;

        if (elapsed < targetFrameTime)
        {
            var timeout = targetFrameTime - elapsed;

            Thread.Sleep(timeout);
        }
    }

    public TimeSpan Restart()
    {
        var totalElapsed = _stopwatch.Elapsed;

        _stopwatch.Restart();

        return totalElapsed;
    }
}
