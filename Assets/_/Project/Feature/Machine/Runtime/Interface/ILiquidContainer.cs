public interface ILiquidContainer
{
    string m_containerName { get; }

    LiquidContainerType m_containerType { get; }

    float m_currentVolume { get; }

    float m_maximumVolume { get; }

    float m_normalizedVolume { get; }

    bool m_isEmpty { get; }

    bool m_isFull { get; }

    float RemoveLiquid(float requestedAmount);

    float AddLiquid(float requestedAmount);
}

public enum LiquidContainerType
{
    PressTank,
    StorageTank,
    Other
}