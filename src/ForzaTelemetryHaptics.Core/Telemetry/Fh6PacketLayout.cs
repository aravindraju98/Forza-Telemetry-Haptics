namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// Official FH6 Data Out wire layout. Packed little-endian, 324 bytes, no padding.
/// Horizon inserts CarGroup / SmashableVelDiff / SmashableMass at 232–243.
/// FH6 does not emit TireWear or TrackOrdinal (Motorsport-only fields).
/// </summary>
public static class Fh6PacketLayout
{
    public const int PacketSize = 324;

    public const int IsRaceOn = 0;
    public const int TimestampMs = 4;
    public const int EngineMaxRpm = 8;
    public const int EngineIdleRpm = 12;
    public const int CurrentEngineRpm = 16;
    public const int AccelerationX = 20;
    public const int AccelerationY = 24;
    public const int AccelerationZ = 28;
    public const int VelocityX = 32;
    public const int VelocityY = 36;
    public const int VelocityZ = 40;
    public const int AngularVelocityX = 44;
    public const int AngularVelocityY = 48;
    public const int AngularVelocityZ = 52;
    public const int Yaw = 56;
    public const int Pitch = 60;
    public const int Roll = 64;
    public const int NormalizedSuspensionTravelFrontLeft = 68;
    public const int NormalizedSuspensionTravelFrontRight = 72;
    public const int NormalizedSuspensionTravelRearLeft = 76;
    public const int NormalizedSuspensionTravelRearRight = 80;
    public const int TireSlipRatioFrontLeft = 84;
    public const int TireSlipRatioFrontRight = 88;
    public const int TireSlipRatioRearLeft = 92;
    public const int TireSlipRatioRearRight = 96;
    public const int WheelRotationSpeedFrontLeft = 100;
    public const int WheelRotationSpeedFrontRight = 104;
    public const int WheelRotationSpeedRearLeft = 108;
    public const int WheelRotationSpeedRearRight = 112;
    public const int WheelOnRumbleStripFrontLeft = 116;
    public const int WheelOnRumbleStripFrontRight = 120;
    public const int WheelOnRumbleStripRearLeft = 124;
    public const int WheelOnRumbleStripRearRight = 128;
    public const int WheelInPuddleDepthFrontLeft = 132;
    public const int WheelInPuddleDepthFrontRight = 136;
    public const int WheelInPuddleDepthRearLeft = 140;
    public const int WheelInPuddleDepthRearRight = 144;
    public const int SurfaceRumbleFrontLeft = 148;
    public const int SurfaceRumbleFrontRight = 152;
    public const int SurfaceRumbleRearLeft = 156;
    public const int SurfaceRumbleRearRight = 160;
    public const int TireSlipAngleFrontLeft = 164;
    public const int TireSlipAngleFrontRight = 168;
    public const int TireSlipAngleRearLeft = 172;
    public const int TireSlipAngleRearRight = 176;
    public const int TireCombinedSlipFrontLeft = 180;
    public const int TireCombinedSlipFrontRight = 184;
    public const int TireCombinedSlipRearLeft = 188;
    public const int TireCombinedSlipRearRight = 192;
    public const int SuspensionTravelMetersFrontLeft = 196;
    public const int SuspensionTravelMetersFrontRight = 200;
    public const int SuspensionTravelMetersRearLeft = 204;
    public const int SuspensionTravelMetersRearRight = 208;
    public const int CarOrdinal = 212;
    public const int CarClass = 216;
    public const int CarPerformanceIndex = 220;
    public const int DrivetrainType = 224;
    public const int NumCylinders = 228;
    public const int CarGroup = 232;
    public const int SmashableVelDiff = 236;
    public const int SmashableMass = 240;
    public const int PositionX = 244;
    public const int PositionY = 248;
    public const int PositionZ = 252;
    public const int Speed = 256;
    public const int Power = 260;
    public const int Torque = 264;
    public const int TireTempFrontLeft = 268;
    public const int TireTempFrontRight = 272;
    public const int TireTempRearLeft = 276;
    public const int TireTempRearRight = 280;
    public const int Boost = 284;
    public const int Fuel = 288;
    public const int DistanceTraveled = 292;
    public const int BestLap = 296;
    public const int LastLap = 300;
    public const int CurrentLap = 304;
    public const int CurrentRaceTime = 308;
    public const int LapNumber = 312;
    public const int RacePosition = 314;
    public const int Accel = 315;
    public const int Brake = 316;
    public const int Clutch = 317;
    public const int HandBrake = 318;
    public const int Gear = 319;
    public const int Steer = 320;
    public const int NormalizedDrivingLine = 321;
    public const int NormalizedAIBrakeDifference = 322;
    public const int TrailingByte = 323;
}
