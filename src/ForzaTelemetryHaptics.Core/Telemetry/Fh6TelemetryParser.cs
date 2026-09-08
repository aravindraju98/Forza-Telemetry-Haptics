using System.Buffers.Binary;

namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// Length-strict FH6 Data Out parser. Malformed packets are rejected; never thrown to callers.
/// </summary>
public sealed class Fh6TelemetryParser : ITelemetryParser
{
    public bool TryParse(ReadOnlySpan<byte> data, out RawFh6Packet? packet, out string? error)
    {
        packet = null;
        error = null;

        if (data.Length != Fh6PacketLayout.PacketSize)
        {
            error = $"Expected {Fh6PacketLayout.PacketSize} bytes, received {data.Length}.";
            return false;
        }

        try
        {
            var raw = Read(data);
            if (!IsFinitePhysics(raw))
            {
                error = "Packet contained non-finite physics values.";
                return false;
            }

            packet = raw;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Packet decode failed: {ex.Message}";
            return false;
        }
    }

    private static RawFh6Packet Read(ReadOnlySpan<byte> data)
    {
        static float F32(ReadOnlySpan<byte> span, int offset) =>
            BinaryPrimitives.ReadSingleLittleEndian(span.Slice(offset, 4));

        static int I32(ReadOnlySpan<byte> span, int offset) =>
            BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset, 4));

        static uint U32(ReadOnlySpan<byte> span, int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, 4));

        static ushort U16(ReadOnlySpan<byte> span, int offset) =>
            BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, 2));

        return new RawFh6Packet
        {
            IsRaceOn = I32(data, Fh6PacketLayout.IsRaceOn),
            TimestampMs = U32(data, Fh6PacketLayout.TimestampMs),
            EngineMaxRpm = F32(data, Fh6PacketLayout.EngineMaxRpm),
            EngineIdleRpm = F32(data, Fh6PacketLayout.EngineIdleRpm),
            CurrentEngineRpm = F32(data, Fh6PacketLayout.CurrentEngineRpm),
            AccelerationX = F32(data, Fh6PacketLayout.AccelerationX),
            AccelerationY = F32(data, Fh6PacketLayout.AccelerationY),
            AccelerationZ = F32(data, Fh6PacketLayout.AccelerationZ),
            VelocityX = F32(data, Fh6PacketLayout.VelocityX),
            VelocityY = F32(data, Fh6PacketLayout.VelocityY),
            VelocityZ = F32(data, Fh6PacketLayout.VelocityZ),
            AngularVelocityX = F32(data, Fh6PacketLayout.AngularVelocityX),
            AngularVelocityY = F32(data, Fh6PacketLayout.AngularVelocityY),
            AngularVelocityZ = F32(data, Fh6PacketLayout.AngularVelocityZ),
            Yaw = F32(data, Fh6PacketLayout.Yaw),
            Pitch = F32(data, Fh6PacketLayout.Pitch),
            Roll = F32(data, Fh6PacketLayout.Roll),
            NormalizedSuspensionTravelFrontLeft = F32(data, Fh6PacketLayout.NormalizedSuspensionTravelFrontLeft),
            NormalizedSuspensionTravelFrontRight = F32(data, Fh6PacketLayout.NormalizedSuspensionTravelFrontRight),
            NormalizedSuspensionTravelRearLeft = F32(data, Fh6PacketLayout.NormalizedSuspensionTravelRearLeft),
            NormalizedSuspensionTravelRearRight = F32(data, Fh6PacketLayout.NormalizedSuspensionTravelRearRight),
            TireSlipRatioFrontLeft = F32(data, Fh6PacketLayout.TireSlipRatioFrontLeft),
            TireSlipRatioFrontRight = F32(data, Fh6PacketLayout.TireSlipRatioFrontRight),
            TireSlipRatioRearLeft = F32(data, Fh6PacketLayout.TireSlipRatioRearLeft),
            TireSlipRatioRearRight = F32(data, Fh6PacketLayout.TireSlipRatioRearRight),
            WheelRotationSpeedFrontLeft = F32(data, Fh6PacketLayout.WheelRotationSpeedFrontLeft),
            WheelRotationSpeedFrontRight = F32(data, Fh6PacketLayout.WheelRotationSpeedFrontRight),
            WheelRotationSpeedRearLeft = F32(data, Fh6PacketLayout.WheelRotationSpeedRearLeft),
            WheelRotationSpeedRearRight = F32(data, Fh6PacketLayout.WheelRotationSpeedRearRight),
            WheelOnRumbleStripFrontLeft = I32(data, Fh6PacketLayout.WheelOnRumbleStripFrontLeft),
            WheelOnRumbleStripFrontRight = I32(data, Fh6PacketLayout.WheelOnRumbleStripFrontRight),
            WheelOnRumbleStripRearLeft = I32(data, Fh6PacketLayout.WheelOnRumbleStripRearLeft),
            WheelOnRumbleStripRearRight = I32(data, Fh6PacketLayout.WheelOnRumbleStripRearRight),
            WheelInPuddleDepthFrontLeft = F32(data, Fh6PacketLayout.WheelInPuddleDepthFrontLeft),
            WheelInPuddleDepthFrontRight = F32(data, Fh6PacketLayout.WheelInPuddleDepthFrontRight),
            WheelInPuddleDepthRearLeft = F32(data, Fh6PacketLayout.WheelInPuddleDepthRearLeft),
            WheelInPuddleDepthRearRight = F32(data, Fh6PacketLayout.WheelInPuddleDepthRearRight),
            SurfaceRumbleFrontLeft = F32(data, Fh6PacketLayout.SurfaceRumbleFrontLeft),
            SurfaceRumbleFrontRight = F32(data, Fh6PacketLayout.SurfaceRumbleFrontRight),
            SurfaceRumbleRearLeft = F32(data, Fh6PacketLayout.SurfaceRumbleRearLeft),
            SurfaceRumbleRearRight = F32(data, Fh6PacketLayout.SurfaceRumbleRearRight),
            TireSlipAngleFrontLeft = F32(data, Fh6PacketLayout.TireSlipAngleFrontLeft),
            TireSlipAngleFrontRight = F32(data, Fh6PacketLayout.TireSlipAngleFrontRight),
            TireSlipAngleRearLeft = F32(data, Fh6PacketLayout.TireSlipAngleRearLeft),
            TireSlipAngleRearRight = F32(data, Fh6PacketLayout.TireSlipAngleRearRight),
            TireCombinedSlipFrontLeft = F32(data, Fh6PacketLayout.TireCombinedSlipFrontLeft),
            TireCombinedSlipFrontRight = F32(data, Fh6PacketLayout.TireCombinedSlipFrontRight),
            TireCombinedSlipRearLeft = F32(data, Fh6PacketLayout.TireCombinedSlipRearLeft),
            TireCombinedSlipRearRight = F32(data, Fh6PacketLayout.TireCombinedSlipRearRight),
            SuspensionTravelMetersFrontLeft = F32(data, Fh6PacketLayout.SuspensionTravelMetersFrontLeft),
            SuspensionTravelMetersFrontRight = F32(data, Fh6PacketLayout.SuspensionTravelMetersFrontRight),
            SuspensionTravelMetersRearLeft = F32(data, Fh6PacketLayout.SuspensionTravelMetersRearLeft),
            SuspensionTravelMetersRearRight = F32(data, Fh6PacketLayout.SuspensionTravelMetersRearRight),
            CarOrdinal = I32(data, Fh6PacketLayout.CarOrdinal),
            CarClass = I32(data, Fh6PacketLayout.CarClass),
            CarPerformanceIndex = I32(data, Fh6PacketLayout.CarPerformanceIndex),
            DrivetrainType = I32(data, Fh6PacketLayout.DrivetrainType),
            NumCylinders = I32(data, Fh6PacketLayout.NumCylinders),
            CarGroup = I32(data, Fh6PacketLayout.CarGroup),
            SmashableVelDiff = F32(data, Fh6PacketLayout.SmashableVelDiff),
            SmashableMass = F32(data, Fh6PacketLayout.SmashableMass),
            PositionX = F32(data, Fh6PacketLayout.PositionX),
            PositionY = F32(data, Fh6PacketLayout.PositionY),
            PositionZ = F32(data, Fh6PacketLayout.PositionZ),
            Speed = F32(data, Fh6PacketLayout.Speed),
            Power = F32(data, Fh6PacketLayout.Power),
            Torque = F32(data, Fh6PacketLayout.Torque),
            TireTempFrontLeft = F32(data, Fh6PacketLayout.TireTempFrontLeft),
            TireTempFrontRight = F32(data, Fh6PacketLayout.TireTempFrontRight),
            TireTempRearLeft = F32(data, Fh6PacketLayout.TireTempRearLeft),
            TireTempRearRight = F32(data, Fh6PacketLayout.TireTempRearRight),
            Boost = F32(data, Fh6PacketLayout.Boost),
            Fuel = F32(data, Fh6PacketLayout.Fuel),
            DistanceTraveled = F32(data, Fh6PacketLayout.DistanceTraveled),
            BestLap = F32(data, Fh6PacketLayout.BestLap),
            LastLap = F32(data, Fh6PacketLayout.LastLap),
            CurrentLap = F32(data, Fh6PacketLayout.CurrentLap),
            CurrentRaceTime = F32(data, Fh6PacketLayout.CurrentRaceTime),
            LapNumber = U16(data, Fh6PacketLayout.LapNumber),
            RacePosition = data[Fh6PacketLayout.RacePosition],
            Accel = data[Fh6PacketLayout.Accel],
            Brake = data[Fh6PacketLayout.Brake],
            Clutch = data[Fh6PacketLayout.Clutch],
            HandBrake = data[Fh6PacketLayout.HandBrake],
            Gear = data[Fh6PacketLayout.Gear],
            Steer = unchecked((sbyte)data[Fh6PacketLayout.Steer]),
            NormalizedDrivingLine = unchecked((sbyte)data[Fh6PacketLayout.NormalizedDrivingLine]),
            NormalizedAIBrakeDifference = unchecked((sbyte)data[Fh6PacketLayout.NormalizedAIBrakeDifference]),
            TrailingByte = data[Fh6PacketLayout.TrailingByte]
        };
    }

    private static bool IsFinitePhysics(RawFh6Packet raw)
    {
        return float.IsFinite(raw.CurrentEngineRpm)
               && float.IsFinite(raw.EngineMaxRpm)
               && float.IsFinite(raw.EngineIdleRpm)
               && float.IsFinite(raw.Speed)
               && float.IsFinite(raw.AccelerationX)
               && float.IsFinite(raw.AccelerationY)
               && float.IsFinite(raw.AccelerationZ);
    }
}
