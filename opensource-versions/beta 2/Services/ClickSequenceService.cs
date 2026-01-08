using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoClicker.Models;

namespace AutoClicker.Services
{
    public class ClickSequenceService
    {
        private readonly ClickSettings _settings;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;

        public ClickSequenceService(ClickSettings settings)
        {
            _settings = settings;
        }

        public async Task StartSequenceAsync()
        {
            if (_isRunning || _settings.Sequences.Count == 0)
                return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                for (int repeat = 0; repeat < _settings.RepeatSequences && !_cancellationTokenSource.Token.IsCancellationRequested; repeat++)
                {
                    await ExecuteSequenceAsync(_cancellationTokenSource.Token);
                    
                    if (repeat < _settings.RepeatSequences - 1 && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(_settings.ClickInterval, _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isRunning = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void StopSequence()
        {
            _cancellationTokenSource?.Cancel();
        }

        public bool IsRunning => _isRunning;

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

        public void AddClickToSequence(Point position, ClickType clickType, int delayAfter = 100, int holdDuration = 50)
        {
            var sequence = new ClickSequence
            {
                Position = position,
                ClickType = clickType,
                DelayAfter = delayAfter,
                HoldDuration = holdDuration
            };

            _settings.Sequences.Add(sequence);
        }

        public void RemoveClickFromSequence(int index)
        {
            if (index >= 0 && index < _settings.Sequences.Count)
            {
                _settings.Sequences.RemoveAt(index);
            }
        }

        public void ClearSequence()
        {
            _settings.Sequences.Clear();
        }

        public void MoveSequenceItem(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && oldIndex < _settings.Sequences.Count &&
                newIndex >= 0 && newIndex < _settings.Sequences.Count)
            {
                var item = _settings.Sequences[oldIndex];
                _settings.Sequences.RemoveAt(oldIndex);
                _settings.Sequences.Insert(newIndex, item);
            }
        }

        public (int minX, int minY, int maxX, int maxY) GetSequenceBounds()
        {
            if (_settings.Sequences.Count == 0)
                return (0, 0, 0, 0);

            var minX = _settings.Sequences.Min(s => s.Position.X);
            var minY = _settings.Sequences.Min(s => s.Position.Y);
            var maxX = _settings.Sequences.Max(s => s.Position.X);
            var maxY = _settings.Sequences.Max(s => s.Position.Y);

            return (minX, minY, maxX, maxY);
        }

        public TimeSpan GetEstimatedDuration()
        {
            if (_settings.Sequences.Count == 0)
                return TimeSpan.Zero;

            var totalDelay = _settings.Sequences.Sum(s => s.DelayAfter);
            var totalDuration = totalDelay * _settings.RepeatSequences;
            
            if (_settings.RepeatSequences > 1)
            {
                totalDuration += (_settings.RepeatSequences - 1) * _settings.ClickInterval;
            }

            return TimeSpan.FromMilliseconds(totalDuration);
        }
    }
}
