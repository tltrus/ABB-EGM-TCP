using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Tools
{
    class Tools
    {
        // Utility: test conversion consistency
        public string TestQuaternionConversion(double rx, double ry, double rz)
        {
            var quat = EulerToQuaternionABB(rx, ry, rz);
            var back = QuaternionToEulerABB(quat[0], quat[1], quat[2], quat[3]);

            return $"Input: Rx={rx:F3}°, Ry={ry:F3}°, Rz={rz:F3}°\n" +
                   $"→ Quaternion: w={quat[0]:F4}, x={quat[1]:F4}, y={quat[2]:F4}, z={quat[3]:F4}\n" +
                   $"→ Back: Rx={back[0]:F3}°, Ry={back[1]:F3}°, Rz={back[2]:F3}°\n" +
                   $"Error: ΔRx={rx - back[0]:F4}°, ΔRy={ry - back[1]:F4}°, ΔRz={rz - back[2]:F4}°";
        }

        // Converts ABB-style Euler angles (Rx, Ry, Rz in degrees) → quaternion [w, x, y, z] for EGM
        private double[] EulerToQuaternionABB(double rxDeg, double ryDeg, double rzDeg)
        {
            // ABB uses left-handed ZYX Euler angles.
            // To get correct right-handed quaternion, negate angles.
            double rx = -rxDeg * Math.PI / 180.0;
            double ry = -ryDeg * Math.PI / 180.0;
            double rz = -rzDeg * Math.PI / 180.0;

            // Precompute
            double cr = Math.Cos(rx * 0.5);
            double sr = Math.Sin(rx * 0.5);
            double cp = Math.Cos(ry * 0.5);
            double sp = Math.Sin(ry * 0.5);
            double cy = Math.Cos(rz * 0.5);
            double sy = Math.Sin(rz * 0.5);

            // ZYX order: R = Rz * Ry * Rx
            double w = cr * cp * cy + sr * sp * sy;
            double x = sr * cp * cy - cr * sp * sy;
            double y = cr * sp * cy + sr * cp * sy;
            double z = cr * cp * sy - sr * sp * cy;

            return new double[] { w, x, y, z };
        }
        // Converts ABB quaternion (right-handed) to ABB-style Euler angles (Rx, Ry, Rz) in degrees (left-handed ZYX)
        private double[] QuaternionToEulerABB(double w, double x, double y, double z)
        {
            // Standard conversion to ZYX Euler angles in right-handed system
            double roll, pitch, yaw;

            // Roll (x-axis)
            double sinr_cosp = 2 * (w * x + y * z);
            double cosr_cosp = 1 - 2 * (x * x + y * y);
            roll = Math.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (y-axis)
            double sinp = 2 * (w * y - z * x);
            if (Math.Abs(sinp) >= 1)
                pitch = Math.CopySign(Math.PI / 2, sinp);
            else
                pitch = Math.Asin(sinp);

            // Yaw (z-axis)
            double siny_cosp = 2 * (w * z + x * y);
            double cosy_cosp = 1 - 2 * (y * y + z * z);
            yaw = Math.Atan2(siny_cosp, cosy_cosp);

            // Convert to degrees
            roll = roll * 180.0 / Math.PI;
            pitch = pitch * 180.0 / Math.PI;
            yaw = yaw * 180.0 / Math.PI;

            // ABB RobotStudio uses left-handed system → invert signs
            double rx = -roll;
            double ry = -pitch;
            double rz = -yaw;

            return new double[] { rx, ry, rz };
        }

    }
}
