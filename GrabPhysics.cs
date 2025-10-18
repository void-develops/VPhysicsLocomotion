using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using easyInputs;

public class GrabPhysics : MonoBehaviour
{
    [Header("Grab Settings")]
    public float GrabRadius = 0.065f;
    public LayerMask GrabbableLayer;
    public XRNode HandNode = XRNode.RightHand;
    public EasyHand Hand = EasyHand.RightHand;


    private FixedJoint fixedJoint;
    private bool isGrabbing = false;
    private Transform grabbedObject;

    void Update()
    {
        bool grabbing = GetGrip(HandNode);

        if (grabbing && !isGrabbing)
        {
            AttemptGrab();
        }
        else if (!grabbing && isGrabbing)
        {
            Release();
        }
    }

    private bool GetGrip(XRNode node)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue))
            return gripValue;
        return false;
    }

    private void AttemptGrab()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, GrabRadius, GrabbableLayer);

        if (nearby.Length == 0) return;

        StartCoroutine(EasyInputs.Vibration(Hand, 0.1f, 0.05f));

        grabbedObject = nearby[0].transform;
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

        if (rb == null)
        {
            // Add kinematic Rigidbody if missing (good for climbing static objects)
            rb = grabbedObject.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Attach FixedJoint to hand
        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.autoConfigureConnectedAnchor = false;
        fixedJoint.connectedBody = rb;
        fixedJoint.connectedAnchor = rb.transform.InverseTransformPoint(transform.position);

        isGrabbing = true;
    }

    private void Release()
    {
        if (fixedJoint != null)
            Destroy(fixedJoint);

        grabbedObject = null;
        isGrabbing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GrabRadius);
    }
}
