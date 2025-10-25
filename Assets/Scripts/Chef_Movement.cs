using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Chef_NavMover : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string spotTag = "InteractableSpot";
    public float waitTime = 2f;

    [Header("Navigation Settings")]
    public float stoppingDistance = 1.0f;
    public float repickAfterSeconds = 10f;

    [Header("NavMesh Sampling")]
    public float sampleRadius = 3.0f;
    public float approachRingRadius = 1.25f;
    public int approachSamples = 12;

    [Header("Separation Settings (optional)")]
    public string chefTag = "Chef";
    public float separationRadius = 0.9f;
    public float separationStrength = 0.75f;
    public float verticalTolerance = 0.75f;

    // State flags
    private bool isMoving = false;
    private bool isWaiting = false;

    // Internal references
    private NavMeshAgent agent;
    private InteractableSpot currentSpot;

    // Stall monitor guard
    private Coroutine stallRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 70); // helps reduce traffic jams
    }

    void OnEnable()
    {
        StartCoroutine(MainLoop());
    }

    void OnDisable()
    {
        // Ensure we release our spot if we are turned off/destroyed
        ReleaseCurrentSpot();
        if (stallRoutine != null) StopCoroutine(stallRoutine);
        stallRoutine = null;
    }

    void Update()
    {
        // If we somehow lost ownership (e.g., spot disabled/force released), bail and repick.
        if (currentSpot != null && !(currentSpot.isInUse && currentSpot.currentChef == gameObject))
        {
            currentSpot = null;
            isMoving = false;
            if (agent.hasPath) agent.ResetPath();
        }

        // Local avoidance between chefs (lightweight extra push)
        ApplySeparationFromOtherChefs();
    }

    IEnumerator MainLoop()
    {
        while (true)
        {
            if (currentSpot == null)
            {
                // Find and reserve a new spot first (ownership before pathing)
                currentSpot = PickRandomFreeSpot();
                if (currentSpot != null)
                {
                    if (TryFindReachableDestination(currentSpot.transform.position, out var dest))
                    {
                        agent.SetDestination(dest);
                        isMoving = true;

                        if (stallRoutine != null) StopCoroutine(stallRoutine);
                        stallRoutine = StartCoroutine(GiveUpIfStalled());
                    }
                    else
                    {
                        // No path -> drop reservation and try again next frame
                        ReleaseCurrentSpot();
                        currentSpot = null;
                    }
                }
            }
            else
            {
                // We have a target spot. If we reached it, "use" it briefly, then release.
                if (ReachedDestination())
                {
                    isMoving = false;
                    isWaiting = true;

                    // We ALREADY reserved earlier. Just wait (simulate work).
                    yield return new WaitForSeconds(waitTime);

                    // Done "using" -> release and look for another
                    ReleaseCurrentSpot();
                    currentSpot = null;
                    isWaiting = false;

                    if (stallRoutine != null) StopCoroutine(stallRoutine);
                    stallRoutine = null;
                }
            }

            yield return null;
        }
    }

    bool ReachedDestination()
    {
        if (agent.pathPending) return false;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return false;
        if (agent.remainingDistance <= agent.stoppingDistance)
            return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
        return false;
    }

    InteractableSpot PickRandomFreeSpot()
    {
        var objs = GameObject.FindGameObjectsWithTag(spotTag);
        if (objs.Length == 0) return null;

        // Try up to N attempts to avoid bias and busy spots
        const int maxTries = 12;
        for (int i = 0; i < maxTries; i++)
        {
            var go = objs[Random.Range(0, objs.Length)];
            var spot = go.GetComponent<InteractableSpot>();
            if (spot == null) continue;

            // Try to reserve (idempotent for owner)
            if (spot.TryReserve(gameObject))
                return spot;
        }

        // Fallback: linear scan if random attempts failed
        foreach (var go in objs)
        {
            var spot = go.GetComponent<InteractableSpot>();
            if (spot != null && spot.TryReserve(gameObject))
                return spot;
        }

        return null;
    }

    bool TryFindReachableDestination(Vector3 spotPos, out Vector3 dest)
    {
        // 1) Try direct sample near the interactable
        if (NavMesh.SamplePosition(spotPos, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            dest = hit.position;
            return true;
        }

        // 2) Try a ring of approach points around the spot
        Vector3 best = Vector3.zero;
        float bestScore = float.NegativeInfinity;
        Vector3 from = transform.position;
        float step = 360f / Mathf.Max(3, approachSamples);

        for (int i = 0; i < approachSamples; i++)
        {
            float angle = i * step;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * approachRingRadius;
            Vector3 candidate = spotPos + offset;

            if (NavMesh.SamplePosition(candidate, out var h2, sampleRadius, NavMesh.AllAreas))
            {
                float score = -Vector3.Distance(from, h2.position);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = h2.position;
                }
            }
        }

        if (bestScore > float.NegativeInfinity)
        {
            dest = best;
            return true;
        }

        dest = Vector3.zero;
        return false;
    }

    IEnumerator GiveUpIfStalled()
    {
        float t = 0f;
        float lastDist = float.PositiveInfinity;

        while (t < repickAfterSeconds)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid) break;
            if (ReachedDestination()) yield break;

            // Progress check
            float rd = agent.remainingDistance;
            if (!float.IsNaN(rd) && rd < lastDist - 0.05f)
                lastDist = rd;

            t += Time.deltaTime;
            yield return null;
        }

        // Give up on unreachable target: drop our reservation and reset state
        ReleaseCurrentSpot();
        currentSpot = null;
        isMoving = false;
        if (agent.hasPath) agent.ResetPath();
    }

    void ReleaseCurrentSpot()
    {
        if (currentSpot == null) return;
        // Only release if we’re the owner (guard against someone else owning it)
        currentSpot.ReleaseBy(gameObject);
    }

    // --- Simple local push to avoid other chefs ---
    void ApplySeparationFromOtherChefs()
    {
        if (!agent.isOnNavMesh) return;
        var others = GameObject.FindGameObjectsWithTag(chefTag);
        Vector3 push = Vector3.zero;
        Vector3 myPos = transform.position;

        foreach (var go in others)
        {
            if (go == gameObject) continue;
            Vector3 oPos = go.transform.position;
            if (Mathf.Abs(oPos.y - myPos.y) > verticalTolerance) continue;

            Vector3 toMe = myPos - oPos;
            float dist = toMe.magnitude;
            if (dist <= 0.001f || dist > separationRadius) continue;

            float weight = 1f - (dist / separationRadius);
            push += toMe.normalized * weight;
        }

        if (push.sqrMagnitude > 0f)
            agent.Move(push.normalized * separationStrength * Time.deltaTime);
    }
}
