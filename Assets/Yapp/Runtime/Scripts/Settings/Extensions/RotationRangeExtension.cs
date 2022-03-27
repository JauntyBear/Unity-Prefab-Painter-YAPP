using System;

namespace Rowlan.Yapp
{
    public static class RotationRangeExtension
    {
        public static float GetMinimum(this PrefabSettings.RotationRange rotationRange)
        {
            return rotationRange switch
            {
                PrefabSettings.RotationRange.Base_360 => 0f,
                PrefabSettings.RotationRange.Base_180 => -180f,
                _ => throw new Exception("Unsupported enum " + rotationRange),
            };
        }
        public static float GetMaximum(this PrefabSettings.RotationRange rotationRange)
        {
            return rotationRange switch
            {
                PrefabSettings.RotationRange.Base_360 => 360f,
                PrefabSettings.RotationRange.Base_180 => 180f,
                _ => throw new Exception("Unsupported enum " + rotationRange),
            };
        }

        public static PrefabSettings.RotationRange GetNext(this PrefabSettings.RotationRange rotationRange)
        {
            return rotationRange switch
            {
                PrefabSettings.RotationRange.Base_360 => PrefabSettings.RotationRange.Base_180,
                PrefabSettings.RotationRange.Base_180 => PrefabSettings.RotationRange.Base_360,
                _ => throw new Exception("Unsupported enum " + rotationRange),
            };
        }

        public static string GetDisplayName(this PrefabSettings.RotationRange rotationRange)
        {
            return rotationRange switch
            {
                PrefabSettings.RotationRange.Base_360 => "0..360",
                PrefabSettings.RotationRange.Base_180 => "-180..180",
                _ => throw new Exception("Unsupported enum " + rotationRange),
            };
        }
    }
}
