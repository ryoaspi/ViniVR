using System;

namespace Machine.Runtime
{
    public class MachineCycleTimer
    {
        #region Publics

        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;

        public float Duration => _duration;
        public float ElapsedTime => _elapsedTime;
        public float RemainingTime => Math.Max(0f, _duration - _elapsedTime);

        public float Progress
        {
            get
            {
                if (_duration <= 0f)
                {
                    return 0f;
                }

                return Math.Clamp(_elapsedTime / _duration, 0f, 1f);
            }
        }

        public event Action<float> ProgressChanged;
        public event Action CycleCompleted;
        public event Action CycleStopped;

        #endregion
        

        #region Utils

        public bool StartCycle(float duration)
        {
            if (duration <= 0f)
            {
                return false;
            }

            _duration = duration;
            _elapsedTime = 0f;
            _isRunning = true;
            _isPaused = false;

            ProgressChanged?.Invoke(0f);

            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!_isRunning || _isPaused)
            {
                return;
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            _elapsedTime += deltaTime;

            if (_elapsedTime >= _duration)
            {
                _elapsedTime = _duration;

                ProgressChanged?.Invoke(1f);

                _isRunning = false;
                _isPaused = false;

                CycleCompleted?.Invoke();

                return;
            }

            ProgressChanged?.Invoke(Progress);
        }

        public void Pause()
        {
            if (!_isRunning)
            {
                return;
            }

            _isPaused = true;
        }

        public void Resume()
        {
            if (!_isRunning)
            {
                return;
            }

            _isPaused = false;
        }

        public void StopCycle()
        {
            if (!_isRunning && _elapsedTime <= 0f)
            {
                return;
            }

            ResetInternalState();
            ProgressChanged?.Invoke(0f);
            CycleStopped?.Invoke();
        }

        public void SkipCycle()
        {
            if (!_isRunning)
            {
                return;
            }

            _elapsedTime = _duration;
            _isRunning = false;
            _isPaused = false;

            ProgressChanged?.Invoke(1f);
            CycleCompleted?.Invoke();
        }

        public void Reset()
        {
            ResetInternalState();
            ProgressChanged?.Invoke(0f);
        }

        #endregion
        

        #region Main Methods

        private void ResetInternalState()
        {
            _duration = 0f;
            _elapsedTime = 0f;
            _isRunning = false;
            _isPaused = false;
        }

        #endregion
        

        #region Private and Protected

        private float _duration;
        private float _elapsedTime;

        private bool _isRunning;
        private bool _isPaused;

        #endregion
    }
}