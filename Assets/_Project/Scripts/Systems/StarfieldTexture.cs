using System;
using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Builds a seamless, tileable starfield texture at runtime: a faint
    /// nebula from periodic value noise plus stars drawn with wrap-around,
    /// so the texture can repeat and scroll without visible seams.
    /// The same seed always produces the same texture.
    /// </summary>
    public static class StarfieldTexture
    {
        [Serializable]
        public struct Settings
        {
            public int size;
            public int seed;
            public int starCount;
            [Range(0f, 1f)] public float bigStarChance;
            [Range(0f, 1f)] public float nebulaStrength;
            public Color spaceColor;
            public Color nebulaColorA;
            public Color nebulaColorB;

            public static Settings Default => new Settings
            {
                size = 512,
                seed = 1980,
                starCount = 300,
                bigStarChance = 0.05f,
                nebulaStrength = 0.35f,
                spaceColor = new Color(0.010f, 0.012f, 0.035f),
                nebulaColorA = new Color(0.10f, 0.05f, 0.22f),
                nebulaColorB = new Color(0.02f, 0.10f, 0.20f)
            };
        }

        private const int NebulaBaseCells = 4;
        private const int NebulaOctaves = 4;

        public static Texture2D Generate(Settings settings)
        {
            int size = Mathf.Max(16, settings.size);
            var random = new System.Random(settings.seed);
            var pixels = new Color[size * size];

            PaintNebula(pixels, size, settings);
            for (int i = 0; i < settings.starCount; i++)
            {
                PaintStar(pixels, size, random, settings.bigStarChance);
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"Starfield_{settings.seed}",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static void PaintNebula(Color[] pixels, int size, Settings settings)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // Two decorrelated noise fields: one for density, one for hue.
                    float density = PeriodicFbm(u, v, settings.seed);
                    float hue = PeriodicFbm(u, v, settings.seed + 7919);

                    // Keep only the denser regions so the nebula reads as clouds, not fog.
                    float cloud = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 0.85f, density));
                    Color nebula = Color.Lerp(settings.nebulaColorA, settings.nebulaColorB, hue);
                    Color color = settings.spaceColor + nebula * (cloud * settings.nebulaStrength);
                    color.a = 1f;
                    pixels[y * size + x] = color;
                }
            }
        }

        private static void PaintStar(Color[] pixels, int size, System.Random random, float bigStarChance)
        {
            float cx = (float)random.NextDouble() * size;
            float cy = (float)random.NextDouble() * size;
            bool big = random.NextDouble() < bigStarChance;

            // Cubing the roll keeps most stars dim and only a few bright ones.
            float roll = (float)random.NextDouble();
            float brightness = big ? 0.8f + 0.2f * roll : 0.25f + 0.75f * roll * roll * roll;
            float radius = big ? 1.4f + (float)random.NextDouble() : 0.55f + 0.35f * (float)random.NextDouble();

            float tint = (float)random.NextDouble();
            Color starColor = tint < 0.15f ? new Color(1f, 0.85f, 0.7f)
                            : tint < 0.35f ? new Color(0.75f, 0.85f, 1f)
                            : Color.white;

            int reach = Mathf.CeilToInt(radius * 3f);
            float invTwoSigmaSq = 1f / (2f * radius * radius);
            int minX = Mathf.FloorToInt(cx) - reach;
            int minY = Mathf.FloorToInt(cy) - reach;

            for (int py = minY; py <= minY + reach * 2; py++)
            {
                for (int px = minX; px <= minX + reach * 2; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float falloff = Mathf.Exp(-(dx * dx + dy * dy) * invTwoSigmaSq);
                    if (falloff < 0.01f) continue;

                    // Wrap so stars on the edges continue on the opposite side.
                    int index = Wrap(py, size) * size + Wrap(px, size);
                    Color c = pixels[index] + starColor * (brightness * falloff);
                    c.a = 1f;
                    pixels[index] = c;
                }
            }
        }

        private static float PeriodicFbm(float u, float v, int seed)
        {
            float sum = 0f;
            float amplitude = 0.5f;
            float total = 0f;
            int cells = NebulaBaseCells;

            for (int octave = 0; octave < NebulaOctaves; octave++)
            {
                sum += amplitude * PeriodicValueNoise(u * cells, v * cells, cells, seed + octave * 101);
                total += amplitude;
                amplitude *= 0.5f;
                cells *= 2;
            }

            return sum / total;
        }

        // Value noise on a lattice that repeats every 'period' cells, which is what makes the texture tile.
        private static float PeriodicValueNoise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = SmoothCurve(x - x0);
            float ty = SmoothCurve(y - y0);

            float a = Hash(Wrap(x0, period), Wrap(y0, period), seed);
            float b = Hash(Wrap(x0 + 1, period), Wrap(y0, period), seed);
            float c = Hash(Wrap(x0, period), Wrap(y0 + 1, period), seed);
            float d = Hash(Wrap(x0 + 1, period), Wrap(y0 + 1, period), seed);

            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static float SmoothCurve(float t) => t * t * (3f - 2f * t);

        private static int Wrap(int value, int period) => ((value % period) + period) % period;

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }
    }
}
