using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.LightSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class LightReceiver : MonoBehaviour, ILightReceiver
{
    [SerializeField] private GameObject beamPrefab;

    [Space] [SerializeField] private bool beamForward;
    [SerializeField] private bool beamBackward;
    [SerializeField] private bool beamLeft;
    [SerializeField] private bool beamRight;

    [Space] [SerializeField] private GameObject forward;
    [SerializeField] private GameObject backward;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;

    private List<ILightComponent> sendingTo = new List<ILightComponent>();
    private List<ILightComponent> receivingFrom = new List<ILightComponent>();
    public void LightReceive(ILightComponent lightComponent)
    {
        Vector3 dir = (lightComponent.GetGameObject().transform.position- transform.position).normalized;
        if (dir.Equals(Vector3.right) && beamRight)
            FixReceiverBeams(right);
        if (dir.Equals(Vector3.forward) && beamForward)
            FixReceiverBeams(forward);
        if (dir.Equals(Vector3.back) && beamBackward)
            FixReceiverBeams(backward);
        if (dir.Equals(Vector3.left) && beamLeft)
            FixReceiverBeams(left);
        
        if (!receivingFrom.Contains(lightComponent)) receivingFrom.Add(lightComponent);
        
        foreach (var component in sendingTo)
        {
            try
            {
                ILightReceiver receiver = (ILightReceiver) component;
                receiver.LightReceive(this);
            }
            catch (InvalidCastException e)
            {
            }
        }
    }

    public void FixReceiverBeams(GameObject receivedFromBeam)
    {
        Debug.Log(receivedFromBeam.gameObject.transform.position);
        if (left != null && receivedFromBeam != left)
            FixBeamSize(left);

        if (right != null && receivedFromBeam != right)
            FixBeamSize(right);

        if (forward != null && receivedFromBeam != forward)
            FixBeamSize(forward);

        if (backward != null && receivedFromBeam != backward)
            FixBeamSize(backward);
        
    }

    public void LightDisconnect(ILightComponent lightComponent)
    {
        Vector3 beamScale = beamPrefab.transform.localScale;
        if (forward) forward.transform.localScale = beamScale;
        if (backward) backward.transform.localScale = beamScale;
        if (left) left.transform.localScale = beamScale;
        if (right) right.transform.localScale = beamScale;

        if (receivingFrom.Contains(lightComponent)) receivingFrom.Remove(lightComponent);
        
        foreach (var component in sendingTo)
        {
            try
            {
                ILightReceiver receiver = (ILightReceiver) component;
                receiver.LightDisconnect(this);
            }
            catch (InvalidCastException e)
            {
            }
        }
        sendingTo.Clear();
    }

    public Vector3 GetClosestBeamTarget(GameObject beam)
    {
        RaycastHit[] hits = Physics.RaycastAll(beam.transform.position + -beam.transform.up, -beam.transform.up, 100);

        if (hits.Length == 0) return Vector3.zero;

        RaycastHit closest = hits[0];
        foreach (RaycastHit hit in hits)
        {
            if (Vector3.Distance(hit.point, beam.transform.position) < Vector3.Distance(beam.transform.position, closest.point))
            {
                closest = hit;
            }
        }

        if (closest.collider.gameObject.TryGetComponent(out ILightComponent lightComponent))
        {
            if (!sendingTo.Contains(lightComponent)) sendingTo.Add(lightComponent);
        }
        
        return closest.point;
    }

    public void FixBeamSize(GameObject beam)
    {
        Vector3 closestPoint = GetClosestBeamTarget(beam);
        Debug.Log(closestPoint);
        if (closestPoint == Vector3.zero) return;
        float distance = Vector3.Distance(closestPoint, beam.transform.position);
        Vector3 currentScale = beam.transform.localScale;
        currentScale.y = distance / transform.localScale.z / 2;
        beam.transform.localScale = currentScale;
    }


    #region Editor methodes

    public void InstantiateBeams()
    {
        Debug.Log("generating beams");
        DestroyImmediate(forward);
        DestroyImmediate(backward);
        DestroyImmediate(left);
        DestroyImmediate(right);

        if (beamForward)
            forward = SpawnBeam(new Vector3(270, 0, 0));
        if (beamBackward)
            backward = SpawnBeam(new Vector3(90, 0, 0));
        if (beamLeft)
            left = SpawnBeam(new Vector3(0, 0, 270));
        if (beamRight)
            right = SpawnBeam(new Vector3(0, 0, 90));
    }

    private GameObject SpawnBeam(Vector3 rotation)
    {
        GameObject newObject = (GameObject) PrefabUtility.InstantiatePrefab(beamPrefab as Object);
        newObject.transform.parent = transform;
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.Euler(rotation);
        newObject.transform.localScale = beamPrefab.transform.localScale;
        return newObject;
    }    

    #endregion

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}