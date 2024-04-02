using System;
using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CoordinateSystemHandles : MonoBehaviour
{
    [Serializable]
    private class HandleSet
    {
        public Vector3 direction;
        public BoundsHandleInteractable[] handles = new BoundsHandleInteractable[6];

        public void Set(bool enable)
        {
            foreach (var handle in handles)
                handle.IsOccluded = !enable;
        }
    }

    [SerializeField]
    private HandleSet[] sets;

    private HandleSet activeSet = null;
    private BoundsControl boundsControl;
    private BoundsHandleInteractable[] handles = null;
    private bool isManipulating = false;


    private void Awake()
    {
        boundsControl = GetComponentInParent<BoundsControl>();
        handles = GetComponentsInChildren<BoundsHandleInteractable>();

        boundsControl.ManipulationStarted.AddListener(OnManipulationStarted);
        boundsControl.ManipulationEnded.AddListener(OnManipulationEnded);
    }

    private void OnDestroy()
    {
        boundsControl.ManipulationStarted.RemoveListener(OnManipulationStarted);
        boundsControl.ManipulationEnded.RemoveListener(OnManipulationEnded);
    }

    private void Start()
    {
        for (int i = 0; i < sets.Length; i++)
            sets[i].Set(false);
    }

    private void Update()
    {
        if (!isManipulating)
        {
            Vector3 lookDirection = (transform.position - Camera.main.transform.position).normalized;

            Vector3 direction = transform.TransformDirection(sets[0].direction);
            float minDot = Vector3.Dot(direction, lookDirection);
            HandleSet newActiveSet = sets[0];

            for (int i = 1; i < sets.Length; i++)
            {
                direction = transform.TransformDirection(sets[i].direction);
                float dot = Vector3.Dot(direction, lookDirection);
                if (dot < minDot)
                {
                    minDot = dot;
                    newActiveSet = sets[i];
                }
            }

            if (newActiveSet != activeSet)
            {
                activeSet?.Set(false);
                activeSet = newActiveSet;
                activeSet.Set(true);
            }
        }

        foreach (var handle in handles)
        {
            // Only show the handle if the type of the handle is one we want to show.
            handle.enabled = boundsControl.HandlesActive && ((boundsControl.EnabledHandles & handle.HandleType) == handle.HandleType);
        }
    }

    private void OnManipulationStarted(SelectEnterEventArgs args)
    {
        isManipulating = true;
        foreach (var handle in handles)
            handle.IsOccluded = !handle.isSelected;
    }

    private void OnManipulationEnded(SelectExitEventArgs args)
    {
        isManipulating = false;
        foreach (var handle in handles)
            handle.IsOccluded = true;
        activeSet?.Set(true);
    }

}
