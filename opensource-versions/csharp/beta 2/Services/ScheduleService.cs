using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoClicker.Models;

namespace AutoClicker.Services
{
    public class ScheduleService : IDisposable
    {
        private readonly ClickSettings _settings;
        private readonly MouseService _mouseService;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;

        public event EventHandler? ScheduleStarted;
        public event EventHandler? ScheduleStopped;
        public event EventHandler<ScheduleProgressEventArgs>? ProgressUpdated;

        public ScheduleService(ClickSettings settings)
        {
            _settings = settings;
            _mouseService = new MouseService();
        }

        public bool IsRunning => _isRunning;

        public async Task StartScheduledClickingAsync()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            ScheduleStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                await ExecuteScheduledClicksAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isRunning = false;
                ScheduleStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StopScheduledClicking()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task ExecuteScheduledClicksAsync(CancellationToken cancellationToken)
        {
            int totalClicks = _settings.InfiniteClicks ? int.MaxValue : _settings.ClickCount;
            int completedClicks = 0;

            while (completedClicks < totalClicks && !cancellationToken.IsCancellationRequested)
            {
                var clickInterval = CalculateNextInterval();
                
                await Task.Delay(clickInterval, cancellationToken);

                if (cancellationToken.IsCancellationRequested) break;

                PerformClick();
                completedClicks++;

                ProgressUpdated?.Invoke(this, new ScheduleProgressEventArgs
                {
                    CompletedClicks = completedClicks,
                    TotalClicks = totalClicks,
                    ProgressPercentage = totalClicks == int.MaxValue ? 0 : (double)completedClicks / totalClicks * 100
                });
            }
        }

        private int CalculateNextInterval()
        {
            int baseInterval = _settings.ClickInterval;

            if (_settings.EnableRandomization)
            {
                var random = new Random();
                baseInterval = random.Next(_settings.RandomDelayMin, _settings.RandomDelayMax + 1);
            }

            if (_settings.EnableHumanSimulation)
            {
                var random = new Random();
                var variation = (int)(baseInterval * _settings.HumanSpeedVariation);
                baseInterval += random.Next(-variation, variation + 1);
            }

            return Math.Max(1, baseInterval);
        }

        private void PerformClick()
        {
            Point clickPosition;

            if (_settings.UseCurrentPosition)
            {
                clickPosition = MouseService.GetCursorPosition();
            }
            else
            {
                clickPosition = _settings.ClickPosition;
            }

            if (_settings.EnableRandomization)
            {
                clickPosition = MouseService.GetRandomizedPosition(
                    clickPosition, 
                    _settings.RandomXRange, 
                    _settings.RandomYRange
                );
            }

            switch (_settings.ClickMode)
            {
                case ClickMode.Single:
                    MouseService.Click(_settings.ClickType, clickPosition);
                    break;
                case ClickMode.Double:
                    MouseService.DoubleClick(_settings.ClickType, clickPosition);
                    break;
                case ClickMode.Triple:
                    MouseService.TripleClick(_settings.ClickType, clickPosition);
                    break;
            }
        }

        public async Task StartScheduledSequenceAsync()
        {
            if (_isRunning || _settings.Sequences.Count == 0) return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            ScheduleStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                for (int repeat = 0; repeat < _settings.RepeatSequences && !_cancellationTokenSource.Token.IsCancellationRequested; repeat++)
                {
                    await ExecuteSequenceAsync(_cancellationTokenSource.Token);

                    if (repeat < _settings.RepeatSequences - 1 && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(_settings.ClickInterval, _cancellationTokenSource.Token);
                    }

                    ProgressUpdated?.Invoke(this, new ScheduleProgressEventArgs
                    {
                        CompletedClicks = repeat + 1,
                        TotalClicks = _settings.RepeatSequences,
                        ProgressPercentage = (double)(repeat + 1) / _settings.RepeatSequences * 100
                    });
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isRunning = false;
                ScheduleStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task ExecuteSequenceAsync(CancellationToken cancellationToken)
        {
            foreach (var sequence in _settings.Sequences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Point clickPosition = sequence.Position;

                if (_settings.EnableRandomization)
                {
                    clickPosition = MouseService.GetRandomizedPosition(
                        clickPosition, 
                        _settings.RandomXRange, 
                        _settings.RandomYRange
                    );
                }

                int holdDuration = sequence.HoldDuration;
                if (_settings.EnableHumanSimulation)
                {
                    var random = new Random();
                    var variation = (int)(holdDuration * _settings.HumanSpeedVariation);
                    holdDuration += random.Next(-variation, variation + 1);
                    holdDuration = Math.Max(10, holdDuration);
                }

                MouseService.Click(sequence.ClickType, clickPosition, holdDuration);

                if (sequence.DelayAfter > 0)
                {
                    int delay = sequence.DelayAfter;
                    if (_settings.EnableHumanSimulation)
                    {
                        var random = new Random();
                        var variation = (int)(delay * _settings.HumanSpeedVariation);
                        delay += random.Next(-variation, variation + 1);
                        delay = Math.Max(10, delay);
                    }

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        public void Dispose()
        {
            StopScheduledClicking();
            _cancellationTokenSource?.Dispose();
        }
    }

    public class ScheduleProgressEventArgs : EventArgs
    {
        public int CompletedClicks { get; set; }
        public int TotalClicks { get; set; }
        public double ProgressPercentage { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
    }
}
