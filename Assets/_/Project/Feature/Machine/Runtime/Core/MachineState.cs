namespace Machine.Runtime
{
    public enum MachineState
    {
        Inactive,
        WaitingForInput,
        Ready,
        Running,
        Completed,
        Error
    }
}
