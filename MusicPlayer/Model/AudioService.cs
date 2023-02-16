using System;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VKApplication.Model
{
    public class AudioService : BaseVM
    {
        private AudioService()
        {
            _MediaPlayer = new MediaPlayer();
            _MediaTimeline = new MediaTimeline();

            _MediaPlayer.MediaOpened += _MediaOpened;
            _MediaTimeline.CurrentTimeInvalidated += _CurrentTimeInvalidated;

            _MediaPlayer.Volume = 0.01;
        }

        private static AudioService _Instance = new AudioService();
        private static MediaPlayer _MediaPlayer { get; set; }
        private static MediaTimeline _MediaTimeline { get; set; }
        public static AudioService GetInstance() => _Instance;
        public static MediaPlayer GetMediaPlayer() => _MediaPlayer;
        public Item CurrentItem { get; set; }
        public MediaClock Clock { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public bool IsPaused { get; private set; } = true;
        public long _CurrentTime
        {
            get
            {
                return CurrentTime.Ticks;
            }
            set
            {
                CurrentTime = new TimeSpan(value);
                _MediaPlayer.Clock.Controller.Seek(CurrentTime, TimeSeekOrigin.BeginTime);
            }
        }
        public int _Volume
        {
            get
            {
                return (int)(_MediaPlayer.Volume * 100);
            }
            set
            {
                _MediaPlayer.Volume = (double)value / 100;
            }
        }

        private void _MediaOpened(object sender, EventArgs e)
        {
            if (_MediaPlayer.Clock.NaturalDuration.HasTimeSpan)
                TotalTime = _MediaPlayer.Clock.NaturalDuration.TimeSpan;
        }

        private void _CurrentTimeInvalidated(object sender, EventArgs e)
        {
            CurrentTime = _MediaPlayer.Clock.CurrentTime.Value;
        }

        public void StartPlay(Item item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item), "Объект для старта не был передан");
            CurrentItem = item;
            _MediaTimeline.Source = item.Path;
            Clock = _MediaTimeline.CreateClock();
            _MediaPlayer.Clock = Clock;
            IsPaused = false;
        }
        public void PlayPause()
        {
            if (_MediaPlayer.Clock.IsPaused == true)
            {
                _MediaPlayer.Clock.Controller.Resume();
                IsPaused = false;
            }
            else if (_MediaPlayer.Clock.IsPaused == false)
            {
                _MediaPlayer.Clock.Controller.Pause();
                IsPaused = true;
            }
        }

        public void Pause() => _MediaPlayer.Clock.Controller.Pause();
        public void Resume() => _MediaPlayer.Clock.Controller.Resume();
        public void Restart()
        {
            Stop();
            Resume();
        }
        public void Stop()
        {
            if (_MediaPlayer.Clock is null) return;
            if (_MediaPlayer.Clock.IsPaused == false)
            {
                _MediaPlayer.Clock.Controller.Pause();
                IsPaused = true;
            }
            _MediaPlayer.Clock.Controller.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
        }

        public void MoveForward()
        {
            if (TotalTime - CurrentTime > TimeSpan.FromSeconds(5))
            {
                _MediaPlayer.Clock.Controller.Seek(
                    CurrentTime.Add(TimeSpan.FromSeconds(5)),
                    TimeSeekOrigin.BeginTime);
            }
            else
            {
                throw new InvalidOperationException();
            }

        }

        public void MoveBack()
        {
            if (CurrentTime >= TimeSpan.FromSeconds(5))
            {
                _MediaPlayer.Clock.Controller.Seek(
                    CurrentTime.Subtract(TimeSpan.FromSeconds(5)),
                    TimeSeekOrigin.BeginTime);
            }
            else
            {
                _MediaPlayer.Clock.Controller.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            }

        }
    }
}
