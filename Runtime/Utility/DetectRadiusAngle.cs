using System.Collections.Generic;
using UnityEngine;

namespace m4k {
/// <summary>
/// Check hits by checking distance and angle from transform and transform forward, respectively. Iterates through a list of externally managed type T to process hits into local recycle list named hits.
/// </summary>
/// <typeparam name="T">Should be castable to Component; interfaces on MonoBehaviours should work</typeparam>
public class DetectRadiusAngle<T> where T : class {
    const float HitsStaleThreshold = 1f;

    public bool detectSelf { get; set; }

    public Transform self { get; set; }

    public IList<T> others { get; private set; }

    public IList<T> hits { get {
        CheckIfHitsStale();
        return _hits;
    }}

    public bool hasHit { get {
        CheckIfHitsStale();
        return _hits.Count > 0; 
    }}

    System.Predicate<T> query;
    IList<T> _hits;
    HashSet<T> _inRange;
    
    float _lastCheckTime;
    float _viewAngles;
    float _maxSquaredRange;
    float _closestDistance;
    T _closest;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="self">Caller/reference point point transform to calculate distance and forward for angle</param>
    /// <param name="others">Should be reference to externally managed list</param>
    /// <param name="maxSquaredRange">Max squared radius from self</param>
    /// <param name="viewAngles">Angle from transform forward for hits; leave empty or 0f to only check radius</param>
    /// <param name="query"></param>
    public DetectRadiusAngle(Transform self, IList<T> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<T> query = null) {
        this.self = self;
        this.others = others;
        this._viewAngles = viewAngles;
        this._maxSquaredRange = maxSquaredRange;
        this._hits = new List<T>();
        this._inRange = new HashSet<T>();
        this.query = query;
        this.detectSelf = false;
    }

    public bool CheckIfHitsStale() {
        bool stale = (Time.time - _lastCheckTime) > HitsStaleThreshold;
        if(stale) {
            Debug.LogWarning("Stale hit cache");
        }
        return stale;
    }

    public T GetCachedClosest() {
        CheckIfHitsStale();
        return _closest;
    }

    public bool CheckInRangeCached(T target) {
        CheckIfHitsStale();
        return _inRange.Contains(target);
    }

    public bool UpdateHits() 
    {
        _hits.Clear();
        _inRange.Clear();
        _closest = null;
        _closestDistance = Mathf.Infinity;

        foreach(var t in others) {
            if(( (query != null && query.Invoke(t)) || query == null)
            && t != null && IsValid(t)
            ) {
                _hits.Add(t);
                _inRange.Add(t);
            }
        }

        _lastCheckTime = Time.time;
        return hasHit;
    }

    bool IsValid(T other) {
        Component component = (other as Component);
        if(!component) {
            Debug.LogError("Invalid T; failed cast to Component");
            return false;
        }

        Transform otherTransform = component.transform;
        if(!detectSelf && otherTransform == self)
            return false;

        Vector3 direction = otherTransform.position - self.position;
        float sqrMagnitude = direction.sqrMagnitude;

        if(sqrMagnitude > _maxSquaredRange)
            return false;

        if(_viewAngles != 0f) {
            // direction -= self.up * Vector3.Dot(self.up, direction);
            direction.y = 0f;

            if(Vector3.Angle(self.forward, direction) > _viewAngles * 0.5f) 
                return false;

            // var dir = self.InverseTransformDirection(otherDirection);
            // var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // if(Mathf.Abs(angle) > _viewAngles * 0.5f)
            //     return false;
        }

        if(sqrMagnitude < _closestDistance) {
            _closestDistance = sqrMagnitude;
            _closest = other;
        }

        return true;
    }
}
}