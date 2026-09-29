using UnityEngine;

namespace SwipeClean.Cleaning
{
    public enum BrushOperation
    {
        Clean,
        AddMoisture,
        Absorb,
        Smear,
        Push,
        Reveal
    }

    public readonly struct BrushCommand
    {
        public int LayerIndex { get; }
        public BrushOperation Operation { get; }
        public Vector2 CenterUv { get; }
        public Vector2 DirectionUv { get; }
        public float RadiusUv { get; }
        public float Hardness01 { get; }
        public float StrengthPerSecond { get; }
        public float MoistureDeltaPerSecond { get; }
        public float SideEffectStrength01 { get; }
        public float DeltaTime { get; }
        public uint Sequence { get; }

        public BrushCommand(int layerIndex, BrushOperation operation, Vector2 centerUv, Vector2 directionUv,
            float radiusUv, float hardness01, float strengthPerSecond, float moistureDeltaPerSecond,
            float sideEffectStrength01, float deltaTime, uint sequence)
        {
            LayerIndex = Mathf.Max(0, layerIndex);
            Operation = operation;
            CenterUv = new Vector2(Mathf.Clamp01(centerUv.x), Mathf.Clamp01(centerUv.y));
            DirectionUv = directionUv.sqrMagnitude > 0.000001f ? directionUv.normalized : Vector2.zero;
            RadiusUv = Mathf.Clamp(radiusUv, 0.001f, 0.5f);
            Hardness01 = Mathf.Clamp01(hardness01);
            StrengthPerSecond = Mathf.Max(0f, strengthPerSecond);
            MoistureDeltaPerSecond = Mathf.Clamp(moistureDeltaPerSecond, -10f, 10f);
            SideEffectStrength01 = Mathf.Clamp01(sideEffectStrength01);
            DeltaTime = Mathf.Clamp(deltaTime, 0f, 1f / 15f);
            Sequence = sequence;
        }
    }
}

