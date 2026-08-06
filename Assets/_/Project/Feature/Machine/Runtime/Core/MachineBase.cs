using System;
using TheFoundation.Runtime;
using UnityEngine;

namespace Machine.Runtime
{
    public class MachineBase : FBehaviour
    {
        #region Publics

        [Header("Machine Identification")] 
        public string m_machineId;
        
        [Header("Machine Configuration")]
        [Min(0.1f)]
        public float m_cycleDuration = 5f;
        
        public MachineState m_currentState => _currentState;
        public float m_cycleProgress => _cycleTimer.Progress;
        
        public bool m_isRunning => _currentState == MachineState.Running;
        
        public bool m_isCompleted => _currentState == MachineState.Completed;
        
        public event Action<MachineBase> CycleStarted;
        public event Action<MachineBase, float> CycleProgressChanged;
        public event Action<MachineBase> CycleCompleted;
        public event Action<MachineBase, string> MachineError;
        public event Action<MachineBase, MachineState> StateChanged;
        
        #endregion
        
        
        #region API Unity

        protected virtual void Awake()
        {
            _cycleTimer = new MachineCycleTimer();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _cycleTimer.ProgressChanged += HandleCycleProgressChanged;
            _cycleTimer.CycleCompleted += HandleCycleCompleted;
        }

        protected void Update()
        {
            _cycleTimer.Tick(Time.deltaTime);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _cycleTimer.ProgressChanged -= HandleCycleProgressChanged;
            _cycleTimer.CycleCompleted -= HandleCycleCompleted;
            
            _cycleTimer.Reset();
        }

        #endregion
        
        
        #region Utils

        public virtual bool CanStartCycle()
        {
            return _currentState == MachineState.Ready && !_cycleTimer.IsRunning;
        }

        public virtual void TryStartCycle()
        {
            if (!CanStartCycle())
            {
                RaiseMachineError("La machine n'est pas prête à démarrer.");
                return;
            }

            StartMachineCycle();
        }

        public virtual void ResetMachineCycle()
        {
            _cycleTimer.Reset();

            SetState(MachineState.WaitingForInput);

            OnMachineReset();
        }

        public void SetReady()
        {
            if (_currentState == MachineState.Running) return;
            
            SetState(MachineState.Ready);
        }

        public void PauseCycle()
        {
            _cycleTimer.Pause();
        }

        public void ResumeCycle()
        {
            _cycleTimer.Resume();
        }

        public void SkipCycle()
        {
            _cycleTimer.SkipCycle();
        }
        
        #endregion
        
        
        #region Main Methods

        protected virtual void StartMachineCycle()
        {
            if (!_cycleTimer.StartCycle(m_cycleDuration))
            {
                RaiseMachineError("La durée du cycle est invalide.");
                return;
            }
            
            SetState(MachineState.Running);
            
            InfoInProgress($"Cycle démarré pour '{m_machineId}'.",this);

            OnCycleStarted();
            CycleStarted?.Invoke(this);
        }

        protected virtual void CompleteMachineCycle()
        {
            SetState(MachineState.Completed);
            
            InfoDone($"Cycle terminé pour '{m_machineId}'.",this);

            OnCycleCompleted();
            CycleCompleted?.Invoke(this);
        }

        protected virtual void OnCycleStarted()
        {
            
        }

        protected virtual void OnCycleProgress(float progress)
        {
            
        }

        protected virtual void OnCycleCompleted()
        {
            
        }

        protected virtual void OnMachineReset()
        {
            
        }

        protected void SetState(MachineState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            
            StateChanged?.Invoke(this,_currentState);
        }
        
        
        protected void RaiseMachineError(string message)
        {
            SetState(MachineState.Error);
            
            Warning(message, this);
            
            MachineError?.Invoke(this,message);
        }

        private void HandleCycleProgressChanged(float progress)
        {
            OnCycleProgress(progress);
            
            CycleProgressChanged?.Invoke(this,progress);
        }

        private void HandleCycleCompleted()
        {
            CompleteMachineCycle();
        }
        
        #endregion
        
        
        #region Privates and Protected
        
        [SerializeField] private MachineState _currentState = MachineState.WaitingForInput;
        private MachineCycleTimer _cycleTimer;

        #endregion
    }
}
