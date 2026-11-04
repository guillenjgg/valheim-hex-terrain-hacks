using UnityEngine;

namespace HexTerrainHacks.Core
{
    internal sealed class FlatGroundPreviewComponent : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;
        private Material _material;
        private Texture2D _texture;
        private float _radius;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var particleRenderer = GetComponent<ParticleSystemRenderer>();
            _material = new Material(particleRenderer.sharedMaterial);
            _material.SetColor("_Color", Color.white);

            const int size = 64;
            var pixels = new Color32[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var border = x < 2 || y < 2 || x >= size - 2 || y >= size - 2;
                    pixels[y * size + x] = new Color32(255, 255, 255, border ? (byte)255 : (byte)0);
                }
            }

            _texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.SetPixels32(pixels);
            _texture.Apply();
            _material.mainTexture = _texture;
            _material.SetTexture("_EmissionMap", _texture);
            particleRenderer.sharedMaterial = _material;

            var main = _particleSystem.main;
            main.startColor = new Color(0f, 1f, 1f, 1f);
            main.startRotation = 0f;
            // Keep the outline centered and at the configured size throughout its lifetime.
            var shape = _particleSystem.shape;
            shape.enabled = false;
            var sizeOverLifetime = _particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = false;
            _particles = new ParticleSystem.Particle[main.maxParticles];
            Update();
        }

        private void Update()
        {
            // Update the pulse every frame, even when the radius has not changed.
            var alpha = Mathf.Lerp(0.25f, 1f, (Mathf.Sin(Time.time * Mathf.PI * 2f) + 1f) * 0.5f);
            _material.SetColor("_Color", new Color(1f, 1f, 1f, alpha));

            var radius = Plugin.LevelRadius.Value;

            if (_radius == radius)
            {
                return;
            }

            _radius = radius;
            var main = _particleSystem.main;
            main.startSize = radius * 2f;

            // Resize particles already visible when the configuration changes.
            var count = _particleSystem.GetParticles(_particles);

            for (var i = 0; i < count; i++)
            {
                _particles[i].startSize = radius * 2f;
            }

            _particleSystem.SetParticles(_particles, count);
        }

        private void OnDestroy()
        {
            Destroy(_material);
            Destroy(_texture);
        }
    }
}
