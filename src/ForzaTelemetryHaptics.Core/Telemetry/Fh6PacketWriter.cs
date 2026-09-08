using System.Buffers.Binary;

namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>Builds a 324-byte FH6 Data Out datagram. Used by tests.</summary>
public static class Fh6PacketWriter
{
    public static byte[] Write(RawFh6Packet packet)
    {
        var data = new byte[Fh6PacketLayout.PacketSize];
        Write(data, packet);
        return data;
    }

    public static void Write(Span<byte> data, RawFh6Packet packet)
    {
        if (data.Length < Fh6PacketLayout.PacketSize)
        {
            throw new ArgumentException($"Buffer must be at least {Fh6PacketLayout.PacketSize} bytes.", nameof(data));
        }

        static void WriteF32(Span<byte> span, int offset, float value) =>
            BinaryPrimitives.WriteSingleLittleEndian(span.Slice(offset, 4), value);

        static void WriteI32(Span<byte> span, int offset, int value) =>
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset, 4), value);

        static void WriteU32(Span<byte> span, int offset, uint value) =>
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(offset, 4), value);

        static void WriteU16(Span<byte> span, int offset, ushort value) =>
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(offset, 2), value);

        WriteI32(data, Fh6PacketLayout.IsRaceOn, packet.IsRaceOn);
        WriteU32(data, Fh6PacketLayout.TimestampMs, packet.TimestampMs);
        WriteF32(data, Fh6PacketLayout.EngineMaxRpm, packet.EngineMaxRpm);
        WriteF32(data, Fh6PacketLayout.EngineIdleRpm, packet.EngineIdleRpm);
        WriteF32(data, Fh6PacketLayout.CurrentEngineRpm, packet.CurrentEngineRpm);
        WriteF32(data, Fh6PacketLayout.AccelerationX, packet.AccelerationX);
        WriteF32(data, Fh6PacketLayout.AccelerationY, packet.AccelerationY);
        WriteF32(data, Fh6PacketLayout.AccelerationZ, packet.AccelerationZ);
        WriteF32(data, Fh6PacketLayout.VelocityX, packet.VelocityX);
        WriteF32(data, Fh6PacketLayout.VelocityY, packet.VelocityY);
        WriteF32(data, Fh6PacketLayout.VelocityZ, packet.VelocityZ);
        WriteF32(data, Fh6PacketLayout.AngularVelocityX, packet.AngularVelocityX);
        WriteF32(data, Fh6PacketLayout.AngularVelocityY, packet.AngularVelocityY);
        WriteF32(data, Fh6PacketLayout.AngularVelocityZ, packet.AngularVelocityZ);
        WriteF32(data, Fh6PacketLayout.Yaw, packet.Yaw);
        WriteF32(data, Fh6PacketLayout.Pitch, packet.Pitch);
        WriteF32(data, Fh6PacketLayout.Roll, packet.Roll);
        WriteF32(data, Fh6PacketLayout.NormalizedSuspensionTravelFrontLeft, packet.NormalizedSuspensionTravelFrontLeft);
        WriteF32(data, Fh6PacketLayout.NormalizedSuspensionTravelFrontRight, packet.NormalizedSuspensionTravelFrontRight);
        WriteF32(data, Fh6PacketLayout.NormalizedSuspensionTravelRearLeft, packet.NormalizedSuspensionTravelRearLeft);
        WriteF32(data, Fh6PacketLayout.NormalizedSuspensionTravelRearRight, packet.NormalizedSuspensionTravelRearRight);
        WriteF32(data, Fh6PacketLayout.TireSlipRatioFrontLeft, packet.TireSlipRatioFrontLeft);
        WriteF32(data, Fh6PacketLayout.TireSlipRatioFrontRight, packet.TireSlipRatioFrontRight);
        WriteF32(data, Fh6PacketLayout.TireSlipRatioRearLeft, packet.TireSlipRatioRearLeft);
        WriteF32(data, Fh6PacketLayout.TireSlipRatioRearRight, packet.TireSlipRatioRearRight);
        WriteF32(data, Fh6PacketLayout.WheelRotationSpeedFrontLeft, packet.WheelRotationSpeedFrontLeft);
        WriteF32(data, Fh6PacketLayout.WheelRotationSpeedFrontRight, packet.WheelRotationSpeedFrontRight);
        WriteF32(data, Fh6PacketLayout.WheelRotationSpeedRearLeft, packet.WheelRotationSpeedRearLeft);
        WriteF32(data, Fh6PacketLayout.WheelRotationSpeedRearRight, packet.WheelRotationSpeedRearRight);
        WriteI32(data, Fh6PacketLayout.WheelOnRumbleStripFrontLeft, packet.WheelOnRumbleStripFrontLeft);
        WriteI32(data, Fh6PacketLayout.WheelOnRumbleStripFrontRight, packet.WheelOnRumbleStripFrontRight);
        WriteI32(data, Fh6PacketLayout.WheelOnRumbleStripRearLeft, packet.WheelOnRumbleStripRearLeft);
        WriteI32(data, Fh6PacketLayout.WheelOnRumbleStripRearRight, packet.WheelOnRumbleStripRearRight);
        WriteF32(data, Fh6PacketLayout.WheelInPuddleDepthFrontLeft, packet.WheelInPuddleDepthFrontLeft);
        WriteF32(data, Fh6PacketLayout.WheelInPuddleDepthFrontRight, packet.WheelInPuddleDepthFrontRight);
        WriteF32(data, Fh6PacketLayout.WheelInPuddleDepthRearLeft, packet.WheelInPuddleDepthRearLeft);
        WriteF32(data, Fh6PacketLayout.WheelInPuddleDepthRearRight, packet.WheelInPuddleDepthRearRight);
        WriteF32(data, Fh6PacketLayout.SurfaceRumbleFrontLeft, packet.SurfaceRumbleFrontLeft);
        WriteF32(data, Fh6PacketLayout.SurfaceRumbleFrontRight, packet.SurfaceRumbleFrontRight);
        WriteF32(data, Fh6PacketLayout.SurfaceRumbleRearLeft, packet.SurfaceRumbleRearLeft);
        WriteF32(data, Fh6PacketLayout.SurfaceRumbleRearRight, packet.SurfaceRumbleRearRight);
        WriteF32(data, Fh6PacketLayout.TireSlipAngleFrontLeft, packet.TireSlipAngleFrontLeft);
        WriteF32(data, Fh6PacketLayout.TireSlipAngleFrontRight, packet.TireSlipAngleFrontRight);
        WriteF32(data, Fh6PacketLayout.TireSlipAngleRearLeft, packet.TireSlipAngleRearLeft);
        WriteF32(data, Fh6PacketLayout.TireSlipAngleRearRight, packet.TireSlipAngleRearRight);
        WriteF32(data, Fh6PacketLayout.TireCombinedSlipFrontLeft, packet.TireCombinedSlipFrontLeft);
        WriteF32(data, Fh6PacketLayout.TireCombinedSlipFrontRight, packet.TireCombinedSlipFrontRight);
        WriteF32(data, Fh6PacketLayout.TireCombinedSlipRearLeft, packet.TireCombinedSlipRearLeft);
        WriteF32(data, Fh6PacketLayout.TireCombinedSlipRearRight, packet.TireCombinedSlipRearRight);
        WriteF32(data, Fh6PacketLayout.SuspensionTravelMetersFrontLeft, packet.SuspensionTravelMetersFrontLeft);
        WriteF32(data, Fh6PacketLayout.SuspensionTravelMetersFrontRight, packet.SuspensionTravelMetersFrontRight);
        WriteF32(data, Fh6PacketLayout.SuspensionTravelMetersRearLeft, packet.SuspensionTravelMetersRearLeft);
        WriteF32(data, Fh6PacketLayout.SuspensionTravelMetersRearRight, packet.SuspensionTravelMetersRearRight);
        WriteI32(data, Fh6PacketLayout.CarOrdinal, packet.CarOrdinal);
        WriteI32(data, Fh6PacketLayout.CarClass, packet.CarClass);
        WriteI32(data, Fh6PacketLayout.CarPerformanceIndex, packet.CarPerformanceIndex);
        WriteI32(data, Fh6PacketLayout.DrivetrainType, packet.DrivetrainType);
        WriteI32(data, Fh6PacketLayout.NumCylinders, packet.NumCylinders);
        WriteI32(data, Fh6PacketLayout.CarGroup, packet.CarGroup);
        WriteF32(data, Fh6PacketLayout.SmashableVelDiff, packet.SmashableVelDiff);
        WriteF32(data, Fh6PacketLayout.SmashableMass, packet.SmashableMass);
        WriteF32(data, Fh6PacketLayout.PositionX, packet.PositionX);
        WriteF32(data, Fh6PacketLayout.PositionY, packet.PositionY);
        WriteF32(data, Fh6PacketLayout.PositionZ, packet.PositionZ);
        WriteF32(data, Fh6PacketLayout.Speed, packet.Speed);
        WriteF32(data, Fh6PacketLayout.Power, packet.Power);
        WriteF32(data, Fh6PacketLayout.Torque, packet.Torque);
        WriteF32(data, Fh6PacketLayout.TireTempFrontLeft, packet.TireTempFrontLeft);
        WriteF32(data, Fh6PacketLayout.TireTempFrontRight, packet.TireTempFrontRight);
        WriteF32(data, Fh6PacketLayout.TireTempRearLeft, packet.TireTempRearLeft);
        WriteF32(data, Fh6PacketLayout.TireTempRearRight, packet.TireTempRearRight);
        WriteF32(data, Fh6PacketLayout.Boost, packet.Boost);
        WriteF32(data, Fh6PacketLayout.Fuel, packet.Fuel);
        WriteF32(data, Fh6PacketLayout.DistanceTraveled, packet.DistanceTraveled);
        WriteF32(data, Fh6PacketLayout.BestLap, packet.BestLap);
        WriteF32(data, Fh6PacketLayout.LastLap, packet.LastLap);
        WriteF32(data, Fh6PacketLayout.CurrentLap, packet.CurrentLap);
        WriteF32(data, Fh6PacketLayout.CurrentRaceTime, packet.CurrentRaceTime);
        WriteU16(data, Fh6PacketLayout.LapNumber, packet.LapNumber);
        data[Fh6PacketLayout.RacePosition] = packet.RacePosition;
        data[Fh6PacketLayout.Accel] = packet.Accel;
        data[Fh6PacketLayout.Brake] = packet.Brake;
        data[Fh6PacketLayout.Clutch] = packet.Clutch;
        data[Fh6PacketLayout.HandBrake] = packet.HandBrake;
        data[Fh6PacketLayout.Gear] = packet.Gear;
        data[Fh6PacketLayout.Steer] = unchecked((byte)packet.Steer);
        data[Fh6PacketLayout.NormalizedDrivingLine] = unchecked((byte)packet.NormalizedDrivingLine);
        data[Fh6PacketLayout.NormalizedAIBrakeDifference] = unchecked((byte)packet.NormalizedAIBrakeDifference);
        data[Fh6PacketLayout.TrailingByte] = packet.TrailingByte;
    }
}
