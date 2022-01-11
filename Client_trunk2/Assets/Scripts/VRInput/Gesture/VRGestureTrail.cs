using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRGestureTrail : MonoBehaviour
{
    VRCaptureHand registeredHand;
    int lengthOfLineRenderer = 50;
    List<Vector3> displayLine;

    public LineRenderer currentRenderer { get; set; }

    public bool listening = false;

    bool currentlyInUse = false;

    // Use this for initialization
    void Start()
    {
        currentlyInUse = true;
        displayLine = new List<Vector3>();
    }

    void OnEnable()
    {
        if (registeredHand != null)
        {
            SubscribeToEvents();
        }
    }

    void SubscribeToEvents()
    {
        registeredHand.StartCaptureEvent += StartTrail;
        registeredHand.ContinueCaptureEvent += CapturePoint;
        registeredHand.StopCaptureEvent += StopTrail;
    }

    void OnDisable()
    {
        if (registeredHand != null)
        {
            UnsubscribeFromEvents();
        }
    }

    void UnsubscribeFromEvents()
    {
        registeredHand.StartCaptureEvent -= StartTrail;
        registeredHand.ContinueCaptureEvent -= CapturePoint;
        registeredHand.StopCaptureEvent -= StopTrail;
    }

    void UnsubscribeAll()
    {
    }

    void OnDestroy()
    {
        currentlyInUse = false;
    }

    public void RenderTrail(LineRenderer lineRenderer, List<Vector3> capturedLine)
    {
        if (capturedLine.Count == lengthOfLineRenderer)
        {
            lineRenderer.SetVertexCount(lengthOfLineRenderer);
            lineRenderer.SetPositions(capturedLine.ToArray());
        }
    }

    public void StartTrail()
    {
        //currentRenderer.SetColors(Color.magenta, Color.magenta);
        displayLine.Clear();
        listening = true;
    }

    public void CapturePoint(Vector3 rightHandPoint)
    {
        displayLine.Add(rightHandPoint);
        currentRenderer.SetVertexCount(displayLine.Count);
        currentRenderer.SetPositions(displayLine.ToArray());
    }

    public void CapturePoint(Vector3 myVector, List<Vector3> capturedLine, int maxLineLength)
    {
        if (capturedLine.Count >= maxLineLength)
        {
            capturedLine.RemoveAt(0);
        }
        capturedLine.Add(myVector);
    }

    public void StopTrail()
    {
        //currentRenderer.SetColors(Color.blue, Color.cyan);
        listening = false;
    }

    public bool UseCheck()
    {
        return currentlyInUse;
    }

    public void AssignHand(VRCaptureHand captureHand)
    {
        currentlyInUse = true;
        registeredHand = captureHand;
        SubscribeToEvents();
    }

    public void ClearTrail(bool immediate = false)
    {
        if (!immediate)
            StartCoroutine(_ClearTrail());
        currentRenderer.SetVertexCount(0);
    }

    private IEnumerator _ClearTrail()
    {
        GameObject go = Instantiate(currentRenderer.gameObject, currentRenderer.transform.parent) as GameObject;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        LineRenderer line = go.GetComponent<LineRenderer>();

        Color color = Color.white;
        float alpha = 1.0f;
        while (0 < alpha)
        {
            //Debug.Log(alpha);
            color.a = alpha;
            alpha -= 0.1f;
            line.SetColors(color, color);
            yield return new WaitForEndOfFrame();
        }
        Destroy(go);
    }
}