using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class FaceMeshManager : MonoBehaviour
{

    private UnityARSessionNativeInterface m_session;
    Dictionary<string, float> currentBlendShapes;

    [SerializeField]
    private GameObject model;
    [SerializeField]
    private Transform headJoint;
    [SerializeField]
    private SkinnedMeshRenderer ref_SMR_EYE_DEF;
    [SerializeField]
    private SkinnedMeshRenderer ref_SMR_EL_DEF;
    [SerializeField]
    private SkinnedMeshRenderer ref_SMR_MTH_DEF;
    private float offsetY = 0f;

    private Vector3 prevRotation = Vector3.zero;
    private Quaternion defaultRotation;


    [SerializeField]
    Transform locator;

    void Awake()
    {
        offsetY = model.transform.position.y;
        Debug.Log("offset:" + offsetY);
    }

    void Start()
	{
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

        Application.targetFrameRate = 100;
        ARKitFaceTrackingConfiguration config = new ARKitFaceTrackingConfiguration();
        config.alignment = UnityARAlignment.UnityARAlignmentGravity;
        config.enableLightEstimation = true;

        if (config.IsSupported)
        {

            m_session.RunWithConfig(config);

            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
            UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;

        }
        defaultRotation = headJoint.localRotation;

#if UNITY_EDITOR
        locator.gameObject.SetActive(true);
#endif

    }

    void FaceAdded(ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
        model.transform.localPosition = UnityARMatrixOps.GetPosition(anchorData.transform);
        model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y + offsetY, model.transform.localPosition.z);
    }

    void FaceUpdated(ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
        model.transform.localPosition = UnityARMatrixOps.GetPosition(anchorData.transform);
        model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y + offsetY, model.transform.localPosition.z);
        var rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        var rot = new Vector3(rotation.eulerAngles.y - prevRotation.y, rotation.eulerAngles.z - prevRotation.z, rotation.eulerAngles.x - prevRotation.x);
        rot = new Vector3(rot.x * -1f, rot.y * -1f, rot.z * 1f);
        headJoint.transform.Rotate(rot);
        prevRotation = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);

        foreach (KeyValuePair<string, float> kvp in currentBlendShapes)
        {
            if (kvp.Key == ARBlendShapeLocation.EyeBlinkLeft)
            {
                var leftEyeValue = kvp.Value * 100f;
                // 眼を完全に閉じても値が1.0にならないので補正
                if (leftEyeValue > 70f)
                {
                    leftEyeValue *= 1.2f;
                }
                ref_SMR_EYE_DEF.SetBlendShapeWeight(6, leftEyeValue);
                ref_SMR_EL_DEF.SetBlendShapeWeight(6, leftEyeValue);
            }
            else if (kvp.Key == ARBlendShapeLocation.JawOpen)
            {
                ref_SMR_MTH_DEF.SetBlendShapeWeight(6, kvp.Value * 100f);
            }
        }

    }

    void FaceRemoved(ARFaceAnchor anchorData)
    {
        headJoint.localRotation = defaultRotation;
        prevRotation = Vector3.zero;
    }
#if UNITY_EDITOR
    void Update()
    {
        if (locator == null)
        {
            return;
        }
        var rotation = locator.localRotation;
        var rot = new Vector3(rotation.eulerAngles.y - prevRotation.y, rotation.eulerAngles.z - prevRotation.z, rotation.eulerAngles.x - prevRotation.x);
        rot = new Vector3(rot.x * -1f, rot.y * -1f, rot.z * 1f);
        headJoint.transform.Rotate(rot);
        prevRotation = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);
    }
#endif


}