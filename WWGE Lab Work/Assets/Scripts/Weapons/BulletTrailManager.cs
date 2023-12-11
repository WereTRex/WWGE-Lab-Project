using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

// A script used by each entity that uses a GameObject with the Gun.cs script in order to create bullet trails.
public class BulletTrailManager : MonoBehaviour
{
    [SerializeField] private TrailConfigSO _baseTrailConfig;
    private Dictionary<TrailConfigSO, TrailPool> _trailPool;


    private void Awake() => _trailPool = new Dictionary<TrailConfigSO, TrailPool>();
    


    public void SpawnTrail(Vector3 startPoint, Vector3 endPoint, TrailConfigSO trailConfig)
    {
        // If there is not a valid ObjectPool currently in the scene, then create one.
        if (!_trailPool.ContainsKey(trailConfig))
        {
            TrailPool pool = new TrailPool(trailConfig);
            _trailPool.Add(trailConfig, pool);
        }

        StartCoroutine(PlayTrail(startPoint, endPoint, trailConfig));
    }
    public IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, TrailConfigSO trailConfig)
    {
        // Get a TrailRenderer from the pool.
        TrailRenderer instance = _trailPool[trailConfig].Pool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;

        // Avoid a potential position carry-over from the last frame if the instance was reused.
        yield return null;

        instance.emitting = true;

        // Move the TrailRenderer from the startPoint to the endPoint.
        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= trailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        // Note: If we wish, we could trigger the logic for Hitscan collision to here to simulate the bullet travelling, passing in a Function to allow for this.
        // (We would need a RaycastHit and Method Reference).


        // Allow for the trail die.
        yield return new WaitForSeconds(trailConfig.Duration);
        yield return null;

        // Disable the trail and pass it back to the object pool.
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        _trailPool[trailConfig].Pool.Release(instance);
    }
    
    public void AttachBulletTrail(TrailConfigSO trailConfig, Transform bullet)
    {
        // If there is not a valid ObjectPool currently in the scene, then create one.
        if (!_trailPool.ContainsKey(trailConfig))
        {
            TrailPool pool = new TrailPool(trailConfig);
            _trailPool.Add(trailConfig, pool);
        }

        // Get a trail from the object pool.
        TrailRenderer trail = _trailPool[trailConfig].Pool.Get();
        if (trail != null)
        {
            // Set Trail Information.
            trail.transform.SetParent(bullet.transform, false);
            trail.transform.localPosition = Vector3.zero;
            trail.emitting = true;
            trail.gameObject.SetActive(true);
        }
    }

    public void ReleaseTrail(TrailConfigSO trailConfig, TrailRenderer trail)
    {
        // Release a trail back into the ObjectPull.
        if (_trailPool.ContainsKey(trailConfig))
        {
            trail.transform.SetParent(null, true);
            StartCoroutine(DelayedDisableTrail(trailConfig, trail));
        }
    }
    // Allow the trail to end before we disable it.
    private IEnumerator DelayedDisableTrail(TrailConfigSO trailConfig, TrailRenderer trail)
    {
        yield return new WaitForSeconds(trailConfig.Duration);
        yield return null;

        trail.emitting = false;
        trail.gameObject.SetActive(false);
        _trailPool[trailConfig].Pool.Release(trail);
    }


    /// <summary> A class representing an ObjectPool of TrailRenderers created from a TrailConfigSO.</summary>
    private class TrailPool
    {
        public ObjectPool<TrailRenderer> Pool;
        public TrailConfigSO Config;

        public TrailPool(TrailConfigSO config)
        {
            this.Config = config;
            this.Pool = new ObjectPool<TrailRenderer>(CreateTrail);
        }

        // Create a new trail from the TrailConfigSO.
        public TrailRenderer CreateTrail()
        {
            GameObject instance = new GameObject("Bullet Trail");
            TrailRenderer trail = instance.AddComponent<TrailRenderer>();

            trail.colorGradient = Config.Colour;
            trail.material = Config.Material;
            trail.widthCurve = Config.WidthCurve;
            trail.time = Config.Duration;
            trail.minVertexDistance = Config.MinVertexDistance;

            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return trail;
        }
    }
}
