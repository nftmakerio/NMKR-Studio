using System;
using System.Timers;

namespace NMKR.Shared.Classes
{
    public class TimerServices
    {
        private Timer _timer;

        public TimerServices()
        {
          //  SetTimer(1000);
        }

        public void SetTimer(double interval)
        {
            if (_timer != null)
            {
                _timer.Enabled = true;
                return;
            }

            _timer = new(interval);
            _timer.Elapsed += NotifyTimerElapsed;
            _timer.Enabled = true;
        }
        public event Action OnElapsed;

        private void NotifyTimerElapsed(Object source, ElapsedEventArgs e)
        {
            OnElapsed?.Invoke();

        }
        public void Stop()
        {
            if (_timer == null)
                return;

            _timer.Enabled = false;

            // _timer = null;
        }

        public void Start()
        {
            if (_timer == null)
                return;

            _timer.Enabled = true;

        }
    }
}
