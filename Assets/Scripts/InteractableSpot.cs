using UnityEngine;

[DisallowMultipleComponent]
public class InteractableSpot : MonoBehaviour
{
    [Header("Usage Lock")]
    public bool isInUse = false;          // true = occupied by a Chef
    public GameObject currentChef = null; // who owns it

    /// <summary>
    /// Attempt to reserve this spot for a specific chef.
    /// - If free -> reserve & return true
    /// - If already owned by the same chef -> return true (idempotent)
    /// - Otherwise -> return false
    /// </summary>
    public bool TryReserve(GameObject chef)
    {
        // Clean stale state if owner object vanished
        if (isInUse && currentChef == null)
        {
            isInUse = false;
        }

        if (!isInUse)
        {
            isInUse = true;
            currentChef = chef;
            return true;
        }

        // Idempotent: already owned by this chef
        if (currentChef == chef)
            return true;

        return false;
    }

    /// <summary>
    /// Release only if called by the current owner.
    /// Returns true if a release happened.
    /// </summary>
    public bool ReleaseBy(GameObject chef)
    {
        if (isInUse && currentChef == chef)
        {
            isInUse = false;
            currentChef = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Force release (use sparingly—primarily for editor/testing).
    /// </summary>
    public void ForceRelease()
    {
        isInUse = false;
        currentChef = null;
    }

    void OnDisable()
    {
        // If the spot is disabled (e.g., object turned off), clear lock.
        // This prevents deadlocks if the station is removed.
        isInUse = false;
        currentChef = null;
    }
}
