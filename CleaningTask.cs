using System.Collections;
using UnityEngine;

/**
This class handles the logic of the cleaning task

You can make changes in this file
*/
public class CleaningTask : MonoBehaviour
{   
    [Header("You can change this file, just not these pre-set parameters")]
    [Header("Drag colliders here")]
    public Collider[] targets;       // Drag grid colliders manually

    [Header("Filter (assign the sponge's Rigidbody)")]
    public Rigidbody spongeRigidbody; 

    [Header("State")]
    public bool cleaningTask;        // True when all zones touched
    public bool IsComplete { get { return cleaningTask; } }

    // Internal fields
    private bool[] touched;
    private int touchedCount;

    public Color stainColor = new Color(0.45f, 0.28f, 0.12f); // brown
    public float stainSize = 0.18f;      
    public float fadeDuration = 0.5f;    

    private GameObject[] stains;

    // awake runs before start create one visible stain on each cleaning zone
    void Awake()
    {
        int n = (targets != null) ? targets.Length : 0;
        stains = new GameObject[n];
        for (int i = 0; i < n; i++)
        {
            if (targets[i] != null) stains[i] = CreateStain(targets[i]);
        }
    }

    GameObject CreateStain(Collider zone)
    {
        // Find the desk surface under the (invisible) zone
        Bounds b = zone.bounds;
        Vector3 pos = new Vector3(b.center.x, b.min.y, b.center.z);
        Vector3 rayStart = new Vector3(b.center.x, b.max.y, b.center.z);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 1f, ~0, QueryTriggerInteraction.Ignore))
            pos = hit.point;

        // brown disc lying on the desk
        GameObject stain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stain.name = zone.name + " Stain";
        Destroy(stain.GetComponent<Collider>()); // must not block the sponge
        stain.transform.position = pos + Vector3.up * 0.002f;
        stain.transform.localScale = new Vector3(stainSize, 0.001f, stainSize);
        stain.GetComponent<Renderer>().material.color = stainColor;
        return stain;
    }

    // the shrinking animation, hiden when it's fully faded
    IEnumerator FadeStain(GameObject stain)
    {
        Vector3 start = stain.transform.localScale;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / fadeDuration);
            stain.transform.localScale = new Vector3(start.x * k, start.y, start.z * k);
            yield return null;
        }
        stain.SetActive(false);
    }


    // DO NOT CHANGE THIS METHOD
    void Start()
    {

        // initialize array keeping track of progress
        int n = (targets != null) ? targets.Length : 0;
        touched = new bool[n];
        touchedCount = 0;

        // no zones means already complete
        cleaningTask = (n == 0); 
    }

    // This method is called when a trigger collider is touched
    void OnTriggerEnter(Collider other)
    {
        if (cleaningTask || targets == null) return;

        // Only count when the assigned sponge Rigidbody touches the zone
        if (spongeRigidbody != null && other.attachedRigidbody != spongeRigidbody)
            return;

        // Loop over the targets, to see if this collision is a new one
        for (int i = 0; i < targets.Length; i++)
        {
            if (!touched[i] && other == targets[i])
            {
                touched[i] = true;
                touchedCount++;
                if (stains != null && stains[i] != null) StartCoroutine(FadeStain(stains[i]));
                Debug.Log("Touched a cleaning spot");

                if (touchedCount == targets.Length)
                {
                    cleaningTask = true;
                    Debug.Log("Cleaning task COMPLETE: all zones touched.");
                }
                break;
            }
        }
    }

}
