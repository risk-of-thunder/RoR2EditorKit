using EntityStates;
using UnityEngine;

namespace RoR2.Editor
{
    public class EntityStateConfigurationSerializationTest : EntityState
    {
        public static short shortValue;
        public static ushort ushortValue;
        public static int intValue;
        public static uint uIntValue;
        public static long longValue;
        public static ulong ulongValue;
        public static bool boolValue;
        public static float floatValue;
        public static double doubleValue;
        public static string stringValue;
        public static BoundsInt boundsIntValue;
        public static Quaternion quaternionValue;

        [SerializeField]
        public Color colorValue;
        [SerializeField]
        public LayerMask layerMaskValue;
        [SerializeField]
        public Vector2 vector2Value;
        [SerializeField]
        public Vector2Int vector2IntValue;
        [SerializeField]
        public Vector3 vector3Value;
        [SerializeField]
        public Vector3Int vector3IntValue;
        [SerializeField]
        public Vector4 vector4Value;
        [SerializeField]
        public Rect rectValue;
        [SerializeField]
        public RectInt rectIntValue;
        [SerializeField]
        public char charValue;
        [SerializeField]
        public Bounds boundsValue;
        [SerializeField]
        public AnimationCurve animationCurveValue;
        [SerializeField]
        public DamageType enumFlagsValue;
        [SerializeField]
        public HullClassification enumValue;
    }
}