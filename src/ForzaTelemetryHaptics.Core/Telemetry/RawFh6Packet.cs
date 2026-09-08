namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// Verbatim little-endian decode of the official FH6 324-byte Data Out packet.
/// Field names and types follow Forza Horizon 6 Data Out documentation.
/// https://support.forza.net/hc/en-us/articles/51744149102611-Forza-Horizon-6-Data-Out-Documentation
/// </summary>
public sealed class RawFh6Packet
{
    public int IsRaceOn { get; init; }
    public uint TimestampMs { get; init; }
    public float EngineMaxRpm { get; init; }
    public float EngineIdleRpm { get; init; }
    public float CurrentEngineRpm { get; init; }
    public float AccelerationX { get; init; }
    public float AccelerationY { get; init; }
    public float AccelerationZ { get; init; }
    public float VelocityX { get; init; }
    public float VelocityY { get; init; }
    public float VelocityZ { get; init; }
    public float AngularVelocityX { get; init; }
    public float AngularVelocityY { get; init; }
    public float AngularVelocityZ { get; init; }
    public float Yaw { get; init; }
    public float Pitch { get; init; }
    public float Roll { get; init; }
    public float NormalizedSuspensionTravelFrontLeft { get; init; }
    public float NormalizedSuspensionTravelFrontRight { get; init; }
    public float NormalizedSuspensionTravelRearLeft { get; init; }
    public float NormalizedSuspensionTravelRearRight { get; init; }
    public float TireSlipRatioFrontLeft { get; init; }
    public float TireSlipRatioFrontRight { get; init; }
    public float TireSlipRatioRearLeft { get; init; }
    public float TireSlipRatioRearRight { get; init; }
    public float WheelRotationSpeedFrontLeft { get; init; }
    public float WheelRotationSpeedFrontRight { get; init; }
    public float WheelRotationSpeedRearLeft { get; init; }
    public float WheelRotationSpeedRearRight { get; init; }
    public int WheelOnRumbleStripFrontLeft { get; init; }
    public int WheelOnRumbleStripFrontRight { get; init; }
    public int WheelOnRumbleStripRearLeft { get; init; }
    public int WheelOnRumbleStripRearRight { get; init; }
    public float WheelInPuddleDepthFrontLeft { get; init; }
    public float WheelInPuddleDepthFrontRight { get; init; }
    public float WheelInPuddleDepthRearLeft { get; init; }
    public float WheelInPuddleDepthRearRight { get; init; }
    public float SurfaceRumbleFrontLeft { get; init; }
    public float SurfaceRumbleFrontRight { get; init; }
    public float SurfaceRumbleRearLeft { get; init; }
    public float SurfaceRumbleRearRight { get; init; }
    public float TireSlipAngleFrontLeft { get; init; }
    public float TireSlipAngleFrontRight { get; init; }
    public float TireSlipAngleRearLeft { get; init; }
    public float TireSlipAngleRearRight { get; init; }
    public float TireCombinedSlipFrontLeft { get; init; }
    public float TireCombinedSlipFrontRight { get; init; }
    public float TireCombinedSlipRearLeft { get; init; }
    public float TireCombinedSlipRearRight { get; init; }
    public float SuspensionTravelMetersFrontLeft { get; init; }
    public float SuspensionTravelMetersFrontRight { get; init; }
    public float SuspensionTravelMetersRearLeft { get; init; }
    public float SuspensionTravelMetersRearRight { get; init; }
    public int CarOrdinal { get; init; }
    public int CarClass { get; init; }
    public int CarPerformanceIndex { get; init; }
    public int DrivetrainType { get; init; }
    public int NumCylinders { get; init; }
    public int CarGroup { get; init; }
    public float SmashableVelDiff { get; init; }
    public float SmashableMass { get; init; }
    public float PositionX { get; init; }
    public float PositionY { get; init; }
    public float PositionZ { get; init; }
    public float Speed { get; init; }
    public float Power { get; init; }
    public float Torque { get; init; }
    public float TireTempFrontLeft { get; init; }
    public float TireTempFrontRight { get; init; }
    public float TireTempRearLeft { get; init; }
    public float TireTempRearRight { get; init; }
    public float Boost { get; init; }
    public float Fuel { get; init; }
    public float DistanceTraveled { get; init; }
    public float BestLap { get; init; }
    public float LastLap { get; init; }
    public float CurrentLap { get; init; }
    public float CurrentRaceTime { get; init; }
    public ushort LapNumber { get; init; }
    public byte RacePosition { get; init; }
    public byte Accel { get; init; }
    public byte Brake { get; init; }
    public byte Clutch { get; init; }
    public byte HandBrake { get; init; }
    public byte Gear { get; init; }
    public sbyte Steer { get; init; }
    public sbyte NormalizedDrivingLine { get; init; }
    public sbyte NormalizedAIBrakeDifference { get; init; }
    public byte TrailingByte { get; init; }
}
