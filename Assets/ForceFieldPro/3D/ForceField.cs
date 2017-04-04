using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// This is the force field 3D
/// </summary>
[AddComponentMenu("Physics/Force Field")]
public class ForceField : MonoBehaviour
{
    #region Properties
    [FFToolTip("Should this force field send messages to those who interacting with it?")]
    public bool sendMessage = true;

    [FFToolTip("If true then the force will ignore mass, which means the field will apply acceleration rather than force on the targets.")]
    public bool ignoreMass = false;

    [FFToolTip("Set this to false to disable all the tooltips.")]
    public bool showTooltips = true;

    [FFToolTip("The layers that this force field will influence.")]
    public LayerMask layerMask = -1;

    [FFToolTip("The rigidbodies in this list will always be ignored.")]
    public List<Rigidbody> alwaysIgnoredList;

    [FFToolTip("This multiplier will scale the output of the whole field.")]
    public float generalMultiplier = 1;

    [FFToolTip("If true, the force will be applied on the mass center rather than transform center.")]
    public bool useMassCenter = false;
    #region Privates
    Transform thisTransform;

    [SerializeField]
    List<SelectionMethod> selectionMethods = new List<SelectionMethod>();

    [SerializeField]
    List<FieldFunction> fieldFunctions = new List<FieldFunction>();

    #region for targeting system
    Dictionary<int, Rigidbody> ignoredDict = new Dictionary<int, Rigidbody>();
    Dictionary<int, Rigidbody> targetDict1 = new Dictionary<int, Rigidbody>();
    Dictionary<int, Rigidbody> targetDict2 = new Dictionary<int, Rigidbody>();
    bool dictFlag = true;
    Dictionary<int, Rigidbody> currDict;
    Dictionary<int, Rigidbody> historyDict;
    #endregion
    #endregion
    #endregion

    #region Interfaces
    /// <summary>
    /// This interface is what need to be implemented by the custom field function class.
    /// The GetForce function will return the force vector that will later applied on the target.
    /// Note if the Force Field has UseLocalCoordination on, the position passed in will be in the force field object's local coordination. 
    /// </summary>
    public interface IFieldFunction
    {
        /// <summary>
        /// Return a vector based on the input rigidbody on input position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        Vector3 GetForce(Vector3 position, Rigidbody target);
    }
    #endregion

    #region Classes
    #region SelectionMethod
    /// <summary>
    /// This class contains the parameters of how should the ForceField select its targets.
    /// The final targets will be the union of all the enabled selection methods, no target will be select twice.
    /// </summary>
    [Serializable]
    public class SelectionMethod
    {
        /// <summary>
        /// The ForceField this selection method belongs.
        /// </summary>
        [HideInInspector]
        public ForceField forceField;

        /// <summary>
        /// Is this selection method enabled?
        /// </summary>
        [FFToolTip("Is this selection method enabled?")]
        public bool enabled = true;

        /// <summary>
        /// Which mode should this selection method use to select targets?
        /// </summary>
        [FFToolTip("Which mode should this selection method use to select targets?")]
        public ETargetingMode targetingMode;

        /// <summary>
        /// The list that contains the targets of this selection method.
        /// </summary>
        [FFToolTip("This list contains the targets of this selection method.")]
        public List<Rigidbody> targetsList = new List<Rigidbody>();

        /// <summary>
        /// Contains the options of raycast parameters. 
        /// </summary>
        public RaycastOption rayCastOption;

        /// <summary>
        /// Clear the target list based on the setting.
        /// This is for internal use.
        /// </summary>
        public void refreshTargets()
        {
            switch (targetingMode)
            {
                case ETargetingMode.Collider:
                    targetsList.Clear();
                    break;
                case ETargetingMode.Raycast:
                    targetsList.Clear();
                    targetsList.AddRange(rayCastOption.GetRaycastResult(forceField));
                    break;
                case ETargetingMode.Manual:
                    break;
            }
        }

        /// <summary>
        /// The method that the field use to choose its targets:
        /// Collider: use the collider that attached to the gameobject
        /// Raycast: use raycast
        /// Manual: let the user edit the target list directly
        /// </summary>
        public enum ETargetingMode
        {
            Collider,
            Raycast,
            Manual
        }
    }
    #endregion

    #region FieldFunction
    /// <summary>
    /// This class contains the parameters of a vector field.
    /// The final resulting force of the force field will be the sum of all the enabled field functions. 
    /// </summary>
    [Serializable]
    public class FieldFunction
    {
        /// <summary>
        /// The ForceField that this field function belongs to.
        /// </summary>
        [HideInInspector]
        public ForceField forceField;

        /// <summary>
        /// Is this field function enabled?
        /// </summary>
        [FFToolTip("Is this field function enabled?")]
        public bool enabled = true;

        /// <summary>
        /// Is this field function respect to the local coordination of the ForceField object?
        /// </summary>
        [FFToolTip("Is this field function respect to the local coordination of the ForceField object?")]
        public bool useLocalCoordination = true;

        /// <summary>
        /// The field function type of this field function.
        /// </summary>
        [FFToolTip("The field function type of this field function.")]
        public EFieldFunctionType fieldFunctionType;

        /// <summary>
        /// The simple field type that this field function is using.
        /// </summary>
        public ESimpleFieldType simpleFieldType;

        /// <summary>
        /// The constant field options.
        /// </summary>
        public FFConstantField constantField;

        /// <summary>
        /// The centripetal field options.
        /// </summary>
        public FFCentripetalField centripetalField;

        /// <summary>
        /// The axipetal field options.
        /// </summary>
        public FFAxipetalField axipetalField;

        /// <summary>
        /// The perpendicular field options.
        /// </summary>
        public FFPerpendicularField perpendicularField;

        /// <summary>
        /// The custom field function options.
        /// </summary>
        public FFCustomOption customFieldOption;

        //the current field function
        IFieldFunction currField;

        /// <summary>
        /// Update the field setting.
        /// This is for internal use.
        /// </summary>
        public void UpdateFieldFunction()
        {
            switch (fieldFunctionType)
            {
                case EFieldFunctionType.Simple:
                    switch (simpleFieldType)
                    {
                        case ESimpleFieldType.ConstantForce:
                            currField = constantField;
                            break;
                        case ESimpleFieldType.CentripetalForce:
                            currField = centripetalField;
                            break;
                        case ESimpleFieldType.AxipetalForce:
                            currField = axipetalField;
                            break;
                        case ESimpleFieldType.PerpendicularForce:
                            currField = perpendicularField;
                            break;
                    }
                    break;
                case EFieldFunctionType.Custom:

                    if (customFieldOption.fieldFunction != null)
                    {
                        currField = customFieldOption.fieldFunction;
                    }
                    else
                    {
                        currField = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// Get the force of this field function based on the input rigidbody on the input position.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector3 GetForce(Vector3 position, Rigidbody target)
        {
            if (currField == null)
            {
                return Vector3.zero;
            }
            if (useLocalCoordination)
            {
                position = forceField.thisTransform.InverseTransformPoint(position);
            }
            if (useLocalCoordination)
            {
                return forceField.thisTransform.TransformDirection(currField.GetForce(position, target));
            }
            return currField.GetForce(position - forceField.thisTransform.position, target);
        }

        #region Enums
        /// <summary>
        /// The type of the field function will be used
        /// Simple: user can select one of the prepared field functions, which can satisfy most situations.
        /// Custom: a class that implemented IFieldFunction will be used.
        /// </summary>
        public enum EFieldFunctionType
        {
            Simple,
            Custom
        }

        /// <summary>
        /// The type of simple fields.
        /// Constant: always return the same vector.
        /// Centripetal: the result vector will always points to the reference point.
        /// Axipetal: the result vector will always points to the reference line.
        /// Perpendicular: the result vector will always points to the reference plane.
        /// </summary>
        public enum ESimpleFieldType
        {
            ConstantForce,
            CentripetalForce,
            AxipetalForce,
            PerpendicularForce
        }
        #endregion
    }
    #endregion

    #region SimpleForceFields
    /// <summary>
    /// This field will return the same vector at any position.
    /// </summary>
    [Serializable]
    public class FFConstantField : IFieldFunction
    {
        /// <summary>
        /// The magnitude of the force.
        /// </summary>
        [FFToolTip("The magnitude of the force.")]
        public float force = 1;

        [SerializeField]
        [FFToolTip("The direction of the force.\nIt need to be normalized.")]
        Vector3 _direction = Vector3.up;

        /// <summary>
        /// The direction of the force.
        /// Set it via code will automatically normalize it.
        /// </summary>
        public Vector3 direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// Return the force vector at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetForce(Vector3 position, Rigidbody target)
        {
            return force * direction;
        }
    }

    /// <summary>
    /// This field returns a vector that always point to the reference point.
    /// Note that positive force is outward.
    /// </summary>
    [Serializable]
    public class FFCentripetalField : IFieldFunction
    {
        /// <summary>
        /// The reference point of the field.
        /// </summary>
        [FFToolTip("The reference point of the field.")]
        public Vector3 referencePoint = Vector3.zero;

        /// <summary>
        /// The magnitude of the force.
        /// Note positive value points outward.
        /// </summary>
        [FFToolTip("The magnitude of the force.\nNote positive value points outward.")]
        public float force = 1;

        /// <summary>
        /// The options of how should this field change with distance.
        /// </summary>
        public DistanceModifier distanceModifier;

        /// <summary>
        /// Return the force vector at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetForce(Vector3 position, Rigidbody target)
        {
            Vector3 x = position - referencePoint;
            return force * distanceModifier.getModifier(x.magnitude) * x.normalized;
        }
    }

    /// <summary>
    /// This field returns a vector that always perpendicular and point to the reference line.
    /// Note that positive force is outward.
    /// </summary>
    [Serializable]
    public class FFAxipetalField : IFieldFunction
    {
        /// <summary>
        /// The magnitude of the force.
        /// Note positive value points outward.
        /// </summary>
        [FFToolTip("The magnitude of the force.\nNote positive value points outward.")]
        public float force = 1;

        /// <summary>
        /// The point that the reference line will pass.
        /// </summary>
        [FFToolTip("The point that the reference line will pass.")]
        public Vector3 referencePoint = Vector3.zero;

        [SerializeField]
        [FFToolTip("The direction of the reference line.\nIt need to be normalized.")]
        Vector3 _direction = Vector3.up;

        /// <summary>
        /// The direction of the reference line.
        /// Set it via code will automatically normalize it.
        /// </summary>
        public Vector3 direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// The options of how should this field change with distance.
        /// </summary>
        public DistanceModifier distanceModifier;

        /// <summary>
        /// Return the force vector at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetForce(Vector3 position, Rigidbody target)
        {
            Vector3 x = position - (referencePoint + Vector3.Dot(direction, position - referencePoint) * direction);
            return force * distanceModifier.getModifier(x.magnitude) * x.normalized;
        }
    }

    /// <summary>
    /// This field returns a vector that always perpendicular and point the reference plane.
    /// Note that positive force is outward.
    /// </summary>
    [Serializable]
    public class FFPerpendicularField : IFieldFunction
    {
        /// <summary>
        /// The magnitude of the force.
        /// Note positive value points outward.
        /// </summary>
        [FFToolTip("The magnitude of the force.\nNote positive value points outward.")]
        public float force = 1;

        /// <summary>
        /// The point that the reference plane will pass.
        /// </summary>
        [FFToolTip("The point that the reference plane will pass.")]
        public Vector3 referencePoint = Vector3.zero;

        [SerializeField]
        [FFToolTip("The normal vector of the reference plane.\nIt need to be normalized.")]
        Vector3 _direction = Vector3.up;

        /// <summary>
        /// The direction of the reference line.
        /// Set it via code will automatically normalize it.
        /// </summary>
        public Vector3 direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// The options of how should this field change with distance.
        /// </summary>
        public DistanceModifier distanceModifier;

        Plane referencePlane = new Plane();

        /// <summary>
        /// Return the force vector at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetForce(Vector3 position, Rigidbody target)
        {
            referencePlane.SetNormalAndPosition(_direction, referencePoint);
            float signedDistance = referencePlane.GetDistanceToPoint(position);
            float sign = Mathf.Sign(signedDistance);
            return force * distanceModifier.getModifier(Mathf.Abs(signedDistance)) * sign * referencePlane.normal;
        }
    }

    #endregion

    #region CustomForceField
    /// <summary>
    /// This is a wrapper of the custom field functions.
    /// </summary>
    [Serializable]
    public class FFCustomOption
    {
        /// <summary>
        /// The actually custom field function.
        /// </summary>
        public CustomFieldFunction fieldFunction;
    }

    /// <summary>
    /// This is the class that a custom field function class should extend.
    /// </summary>
    public abstract class CustomFieldFunction : MonoBehaviour, IFieldFunction
    {
        /// <summary>
        /// This is the method that need to implement.
        /// It will return a vector based on the input rigidbody on input position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rigidbody"></param>
        /// <returns></returns>
        public abstract Vector3 GetForce(Vector3 position, Rigidbody rigidbody);
    }
    #endregion

    #region ForceDecayFunction
    /// <summary>
    /// This class contains the options of how the force decay with the distance.
    /// </summary>
    [Serializable]
    public class DistanceModifier
    {
        [FFToolTip("The function type.\nConstant mode will always return 1.")]
        public EDistanceModifier modifierType;

        [FFToolTip("The size of the animation curve.\n(how long is 1 unit in the curve?)")]
        public float curveSize = 20;

        [FFToolTip("The animation curve that represent the distance modifier")]
        public AnimationCurve curve;

        [FFToolTip("If checked, the modifier will not below zero.")]
        public bool boundAtZero = true;

        public float a = 1;
        public float b = 0;
        public float n = 2;

        /// <summary>
        /// This function returns a float modifier.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public float getModifier(float d)
        {
            float result = 1;
            switch (modifierType)
            {
                case EDistanceModifier.Exponential:
                    //make it faster for when n is integer
                    if (n % 1 == 0)
                    {
                        int _n = (int)n;
                        float _x = d * a + b;
                        if (n >= 0)
                        {
                            for (int i = 0; i < _n; i++)
                            {
                                result *= _x;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < -_n; i++)
                            {
                                result /= _x;
                            }
                        }
                    }
                    else
                    {
                        result = Mathf.Pow(d * a + b, n);
                    }
                    break;
                case EDistanceModifier.AnimationCurve:
                    result = curve.Evaluate(d / curveSize);
                    break;
            }
            if (boundAtZero && result < 0)
            {
                result = 0;
            }
            return result;
        }

        /// <summary>
        /// The types of distance modifier.
        /// Constant: always 1.
        /// Exponential: the modifier changes with distance exponentially.
        /// AnimationCurve: use AnimationCurve.
        /// </summary>
        public enum EDistanceModifier
        {
            Constant,
            Exponential,
            AnimationCurve
        }
    }
    #endregion

    #region Raycast
    /// <summary>
    /// This class contains the options of raycast selection method.
    /// </summary>
    [Serializable]
    public class RaycastOption
    {
        /// <summary>
        /// If true, then you can set a transform to be the anchor.
        /// The direction of the ray will always point to the anchor,
        /// And the distance of the ray will always equal to the distance between force field object and the anchor.
        /// </summary>
        [FFToolTip("If true, then you can set a transform to be the anchor.\nThe direction of the ray will always point to the anchor,and the distance of the ray will always equal to the distance between force field object and the anchor.")]
        public bool useAnchor;

        /// <summary>
        /// The anchor transform.
        /// </summary>
        [FFToolTip("The anchor transform.")]
        public Transform anchor;

        /// <summary>
        /// Is this direction in force field object's local coordination?
        /// </summary>
        [FFToolTip("Is this direction in force field object's local coordination?")]
        public bool useLocalDirection;

        [SerializeField]
        [FFToolTip("The direction of the ray.")]
        Vector3 _direction;

        /// <summary>
        /// The mode of the raycast.
        /// </summary>
        [FFToolTip("The mode of the raycast.")]
        public ERayCastMode raycastType;

        /// <summary>
        /// Radius of the sphere.
        /// </summary>
        [FFToolTip("Radius of the sphere.")]
        public float radius;

        /// <summary>
        /// How much closest targets can this ray choose at max?
        /// Negative number is treated as infinity.
        /// </summary>
        [FFToolTip("How much closest targets can this ray choose at max?\nNegative number is treated as infinity.")]
        public int numberLimit = -1;

        /// <summary>
        /// How long is this ray?\nNegative number is treated as infinity.
        /// </summary>
        [FFToolTip("How long is this ray?\nNegative number is treated as infinity.")]
        public float distance = -1;

        /// <summary>
        /// The direction of the ray.
        /// Set it via code will automatically normalize it.
        /// </summary>
        public Vector3 direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// The center of the over lap sphere.
        /// </summary>
        [FFToolTip("The center of the over lap sphere.")]
        public Vector3 sphereCenter = Vector3.zero;

        /// <summary>
        /// This returns the result rigidbodies of the raycast.
        /// </summary>
        /// <param name="ff"></param>
        /// <returns></returns>
        public Rigidbody[] GetRaycastResult(ForceField ff)
        {
            Rigidbody[] targets = new Rigidbody[0];
            RaycastHit[] hits = new RaycastHit[0];
            float d = 0;
            Vector3 dir = Vector3.zero;
            //adjust direction and distance
            if (useAnchor)
            {
                if (anchor == null)
                {
                    return targets;
                }
                dir = anchor.position - ff.thisTransform.position;
                d = dir.magnitude;
            }
            else
            {
                if (raycastType != ERayCastMode.OverLapSphere)
                {
                    if (distance == 0 || direction == Vector3.zero)
                    {
                        return targets;
                    }
                    d = distance < 0 ? Mathf.Infinity : distance;
                    if (useLocalDirection)
                    {
                        dir = ff.thisTransform.TransformDirection(direction);
                    }
                    else
                    {
                        dir = direction;
                    }
                }
            }
            //raycast
            switch (raycastType)
            {
                case ERayCastMode.RayCast:
                    hits = Physics.RaycastAll(ff.thisTransform.position, dir, d, ff.layerMask);
                    break;
                case ERayCastMode.SphereCast:
                    hits = Physics.SphereCastAll(ff.thisTransform.position, radius, dir, d, ff.layerMask);
                    break;
                case ERayCastMode.OverLapSphere:
                    Collider[] overlap;
                    if (useAnchor)
                    {
                        overlap = Physics.OverlapSphere(anchor.transform.position, radius, ff.layerMask);
                    }
                    else if (useLocalDirection)
                    {
                        overlap = Physics.OverlapSphere(ff.thisTransform.TransformDirection(sphereCenter) + ff.thisTransform.position, radius, ff.layerMask);
                    }
                    else
                    {
                        overlap = Physics.OverlapSphere(sphereCenter, radius, ff.layerMask);
                    }
                    List<Rigidbody> rbList = new List<Rigidbody>();
                    for (int i = 0; i < overlap.Length; i++)
                    {
                        if (overlap[i].attachedRigidbody)
                        {
                            rbList.Add(overlap[i].attachedRigidbody);
                        }
                    }
                    targets = rbList.ToArray();
                    return targets;
            }
            //select targets
            if (numberLimit < 0)
            {
                List<Rigidbody> rblist = new List<Rigidbody>();
                for (int i = 0; i < hits.Length; i++)
                {
                    Rigidbody rb = hits[i].rigidbody;
                    if (rb != null && !ff.ignoredDict.ContainsKey(rb.GetInstanceID()))
                    {
                        rblist.Add(hits[i].rigidbody);
                    }
                }
                targets = rblist.ToArray();
            }
            else
            {
                Dictionary<float, Rigidbody> dict = new Dictionary<float, Rigidbody>();
                for (int i = 0; i < hits.Length; i++)
                {
                    Rigidbody rb = hits[i].rigidbody;
                    if (rb != null && !ff.ignoredDict.ContainsKey(rb.GetInstanceID()))
                    {
                        dict.Add((rb.position - ff.thisTransform.position).sqrMagnitude, rb);
                    }
                }
                float[] dis = new float[dict.Keys.Count];
                dict.Keys.CopyTo(dis, 0);
                Array.Sort(dis);
                int num = (dis.Length < numberLimit) ? dis.Length : numberLimit;
                targets = new Rigidbody[num];
                for (int i = 0; i < num; i++)
                {
                    targets[i] = dict[dis[i]];
                }
            }
            return targets;
        }

        /// <summary>
        /// The mode of the raycast.
        /// See unity script reference section Physics for details. 
        /// </summary>
        public enum ERayCastMode
        {
            RayCast,
            SphereCast,
            OverLapSphere
        }
    }
    #endregion
    #endregion

    #region UnityEvents
    // Use this for initialization
    void Awake()
    {
        Initialize();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        UpdateIgnoredDictionary();
        UpdateBufferPointer();
        UpdateFieldFunction();
        //selection method loop
        for (int i_sm = 0; i_sm < selectionMethods.Count; i_sm++)
        {
            SelectionMethod sm = selectionMethods[i_sm];
            if (sm.enabled)
            {
                ProcessSelectionMethod(sm);
                sm.refreshTargets();
            }
        }
        //send message to those who left inside history dictionary
        if (sendMessage)
        {
            foreach (int key in historyDict.Keys)
            {
                if (historyDict[key] != null)
                {
                    historyDict[key].SendMessage("OnForceFieldExit", this, SendMessageOptions.DontRequireReceiver);
                    this.SendMessage("OnForceFieldExited", historyDict[key], SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            for (int i = 0; i < selectionMethods.Count; i++)
            {
                SelectionMethod sm = selectionMethods[i];
                if (sm != null && sm.targetingMode == SelectionMethod.ETargetingMode.Collider)
                {
                    sm.targetsList.Add(rb);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (gizmosMode == EGizmosMode.DrawGizmosSelected)
        {
            DrawGizmos();
        }
    }

    void OnDrawGizmos()
    {
        if (gizmosMode == EGizmosMode.DrawGizmosAlways)
        {
            DrawGizmos();
        }
    }
    #endregion

    #region Function
    #region PrivateFunctions
    //initialize
    void Initialize()
    {
        thisTransform = transform;
        foreach (SelectionMethod sm in selectionMethods)
        {
            sm.forceField = this;
        }
        foreach (FieldFunction f in fieldFunctions)
        {
            f.forceField = this;
        }
    }

    //pick out the unprocessed targets, apply force, and send message
    void ProcessSelectionMethod(SelectionMethod sm)
    {
        if (sm == null)
        {
            return;
        }
        List<Rigidbody> rbList = sm.targetsList;
        //targets list loop
        for (int i_rb = 0; i_rb < rbList.Count; i_rb++)
        {
            Rigidbody rb = rbList[i_rb];
            if (rb == null)
            {
                continue;
            }
            int id = rb.GetInstanceID();
            //check processed or not
            if (!currDict.ContainsKey(id))
            {
                //check layer and black list
                if ((!ignoredDict.ContainsKey(id)) && LayerCheck(rb.gameObject.layer))
                {
                    currDict.Add(id, rb);
                    ApplyForce(rb);
                    if (sendMessage)
                    {
                        rb.SendMessage("OnForceFieldStay", this, SendMessageOptions.DontRequireReceiver);
                        this.SendMessage("OnForceFieldStayed", rb, SendMessageOptions.DontRequireReceiver);
                    }
                    //check processed or not last turn
                    if (historyDict.ContainsKey(id))
                    {
                        historyDict.Remove(id);
                    }
                    else
                    {
                        //since no record in the history dictionary, it is just entered
                        if (sendMessage)
                        {
                            rb.SendMessage("OnForceFieldEnter", this, SendMessageOptions.DontRequireReceiver);
                            this.SendMessage("OnForceFieldEntered", rb, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
            }
        }
    }

    //reverse the pointer of current targets and history targets
    void UpdateBufferPointer()
    {
        if (dictFlag)
        {
            currDict = targetDict1;
            historyDict = targetDict2;
        }
        else
        {
            currDict = targetDict2;
            historyDict = targetDict1;
        }
        currDict.Clear();
        dictFlag = !dictFlag;
    }

    //update the FieldFunction to the current setting
    void UpdateFieldFunction()
    {
        for (int i = 0; i < fieldFunctions.Count; i++)
        {
            FieldFunction ff = fieldFunctions[i];
            if (ff != null)
            {
                ff.UpdateFieldFunction();
            }
        }
    }

    //apply the force to the target rigidbody base on the current setting
    void ApplyForce(Rigidbody target)
    {
        if (target == null)
        {
            return;
        }
        Vector3 vec = GetForce(target.position, target);
        if (ignoreMass)
        {
            if (useMassCenter)
            {
                target.AddForceAtPosition(vec, target.worldCenterOfMass, ForceMode.Acceleration);
            }
            else
            {
                target.AddForce(vec, ForceMode.Acceleration);
            }
        }
        else
        {
            if (useMassCenter)
            {
                target.AddForceAtPosition(vec, target.worldCenterOfMass);
            }
            else
            {
                target.AddForce(vec);
            }
        }
    }

    //copy the ignored list to a dictionary for easy look up
    void UpdateIgnoredDictionary()
    {
        ignoredDict.Clear();
        Rigidbody self = GetComponent<Rigidbody>();
        if (self != null)
        {
            ignoredDict.Add(self.GetInstanceID(), self);
        }
        for (int i = 0; i < alwaysIgnoredList.Count; i++)
        {
            Rigidbody rb = alwaysIgnoredList[i];
            if (rb == null)
            {
                continue;
            }
            int id = rb.GetInstanceID();
            ignoredDict.Add(id, rb);
        }
    }

    //return true is the input layer is covered by the layer mask
    bool LayerCheck(int layer)
    {
        return EnumMaskCheck(layer, layerMask.value);
    }

    //check if the value is covered by the mask or not
    bool EnumMaskCheck(int value, int mask)
    {
        bool result = false;
        if (mask == -1)
        {
            result = true;
        }
        else if (mask != 0)
        {
            result = ((mask >> value) & 1) == 1;
        }
        return result;
    }

    //weak up sleeping targets
    void WeakUpTargets()
    {
        Collider[] nearbyTargets = Physics.OverlapSphere(thisTransform.position, GetComponent<Collider>().bounds.extents.magnitude);
        for (int i = 0; i < nearbyTargets.Length; i++)
        {
            Collider c = nearbyTargets[i];
            if (c == null)
            {
                continue;
            }
            if (c.attachedRigidbody != null)
            {
                c.attachedRigidbody.WakeUp();
            }
        }
    }
    #endregion

    #region PublicFunctions
    /// <summary>
    /// It returns the force vector of the force field at the input world position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 GetForce(Vector3 position, Rigidbody target = null)
    {
        Vector3 force = Vector3.zero;
        for (int i = 0; i < fieldFunctions.Count; i++)
        {
            FieldFunction ff = fieldFunctions[i];
            if (ff != null && ff.enabled)
            {
                force += ff.GetForce(position, target);
            }
        }
        return force * generalMultiplier;
    }

    /// <summary>
    /// It returns a rigidbody array that contains all the current targets of the force field.
    /// </summary>
    /// <returns></returns>
    public Rigidbody[] GetCurrentTargets()
    {
        if (currDict == null)
        {
            return new Rigidbody[0];
        }
        Rigidbody[] ret = new Rigidbody[currDict.Count];
        currDict.Values.CopyTo(ret, 0);
        return ret;
    }

    /// <summary>
    /// Add and return a new SelectionMethod.
    /// </summary>
    /// <returns></returns>
    public SelectionMethod AddSelectionMethod()
    {
        SelectionMethod sm = new SelectionMethod();
        sm.forceField = this;
        selectionMethods.Add(sm);
        return sm;
    }

    /// <summary>
    /// Remove the SelectionMethod with input index, default will be the last one.
    /// </summary>
    /// <param name="index"></param>
    public void removeSelectionMethod(int index = -1)
    {
        int count = selectionMethods.Count;
        if (count == 0)
        {
            return;
        }
        if (index == -1)
        {
            selectionMethods.RemoveAt(count - 1);
        }
        else if (index >= 0 && index < count)
        {
            selectionMethods.RemoveAt(index);
        }
    }

    /// <summary>
    /// Get the selection method at input index.
    /// Default will return the index 0.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public SelectionMethod GetSelectionMethod(int index = 0)
    {
        int count = selectionMethods.Count;
        if (count == 0)
        {
            return null;
        }
        if (index < 0 || index >= count)
        {
            return null;
        }
        return selectionMethods[index];
    }

    /// <summary>
    /// Return the index of the input selection method.
    /// </summary>
    /// <param name="sm"></param>
    /// <returns></returns>
    public int GetSelectionMethodIndex(SelectionMethod sm)
    {
        return selectionMethods.IndexOf(sm);
    }

    /// <summary>
    /// Add and return a new FieldFunction.
    /// </summary>
    /// <returns></returns>
    public FieldFunction AddFieldFunction()
    {
        FieldFunction f = new FieldFunction();
        f.forceField = this;
        fieldFunctions.Add(f);
        return f;
    }

    /// <summary>
    /// Remove the FieldFunction with input index, default will be the last one.
    /// </summary>
    /// <param name="index"></param>
    public void removeFieldFunction(int index = -1)
    {
        int count = fieldFunctions.Count;
        if (count == 0)
        {
            return;
        }
        if (index == -1)
        {
            fieldFunctions.RemoveAt(count - 1);
        }
        else if (index >= 0 && index < count)
        {
            fieldFunctions.RemoveAt(index);
        }
    }

    /// <summary>
    /// Get the field function at input index.
    /// Default will return the index 0.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public FieldFunction GetFieldFunction(int index = 0)
    {
        int count = fieldFunctions.Count;
        if (count == 0)
        {
            return null;
        }
        if (index < 0 || index >= count)
        {
            return null;
        }
        return fieldFunctions[index];
    }

    /// <summary>
    /// Return the index of the input field function.
    /// </summary>
    /// <param name="sm"></param>
    /// <returns></returns>
    public int GetFieldFunctionIndex(FieldFunction f)
    {
        return fieldFunctions.IndexOf(f);
    }

    /// <summary>
    /// This function will add the target to the input Manual selection method.
    /// If no selection method is specified, then it will add the target to the first Manual Selection Method.
    /// If cannot find one Manual Selection Method, then one will be created.
    /// </summary>
    /// <param name="rb"></param>
    public void AddTarget(Rigidbody rb, SelectionMethod sm = null)
    {
        if (rb == null)
        {
            return;
        }
        if (sm != null)
        {
            if (sm.targetingMode == SelectionMethod.ETargetingMode.Manual)
            {
                sm.targetsList.Add(rb);
            }
            return;
        }
        else
        {
            for (int i = 0; i < selectionMethods.Count; i++)
            {
                SelectionMethod _sm = selectionMethods[i];
                if (_sm != null && _sm.targetingMode == SelectionMethod.ETargetingMode.Manual)
                {
                    _sm.targetsList.Add(rb);
                    return;
                }
            }
            SelectionMethod smn = AddSelectionMethod();
            smn.targetingMode = SelectionMethod.ETargetingMode.Manual;
            smn.targetsList.Add(rb);
        }
    }

    /// <summary>
    /// This function will remove the input rigidbody from the input selection method, if the input selection method is under Manual mode.
    /// If the selection method is not specified, then the rigidbody will be removed from all the Manual selection methods.
    /// </summary>
    /// <param name="rb"></param>
    public void RemoveTarget(Rigidbody rb, SelectionMethod sm = null)
    {
        if (rb == null)
        {
            return;
        }
        if (sm != null)
        {
            if (sm.targetingMode == SelectionMethod.ETargetingMode.Manual)
            {
                sm.targetsList.RemoveAll(delegate(Rigidbody _rb) { return _rb == rb; });
            }
            return;
        }
        else
        {
            for (int i = 0; i < selectionMethods.Count; i++)
            {
                SelectionMethod _sm = selectionMethods[i];
                if (_sm == null || _sm.targetingMode != SelectionMethod.ETargetingMode.Manual)
                {
                    continue;
                }
                _sm.targetsList.RemoveAll(delegate(Rigidbody _rb) { return _rb == rb; });
            }
        }
    }

    /// <summary>
    /// Remove all the selection methods.
    /// </summary>
    public void ClearSelectionMethods()
    {
        selectionMethods.Clear();
    }

    /// <summary>
    /// Remove all the field functions.
    /// </summary>
    public void ClearFieldFunctions()
    {
        fieldFunctions.Clear();
    }

    /// <summary>
    /// Add input rigidbody to the always ignored list
    /// </summary>
    /// <param name="rb"></param>
    public void Ignore(Rigidbody rb)
    {
        alwaysIgnoredList.Add(rb);
    }

    /// <summary>
    /// Remove input rigidbody from always ignored list.
    /// </summary>
    /// <param name="rb"></param>
    public void StopIgnore(Rigidbody rb)
    {
        alwaysIgnoredList.RemoveAll(delegate(Rigidbody _rb) { return _rb == rb; });
    }
    #endregion
    #endregion

    #region Visualize
    #region Properties
    [SerializeField]
    [FFToolTip("When should the force field draw all those gizmos?")]
    EGizmosMode gizmosMode;

    [SerializeField]
    [FFToolTip("Draw lines between field object and targets.")]
    bool drawTargetsConnection = true;

    [SerializeField]
    [FFToolTip("Draw arrows in the space to indicate the force of the field.")]
    bool drawFieldPointers = true;

    [SerializeField]
    [FFToolTip("Draw the raycast ray.")]
    bool drawRaycastArea = true;

    [SerializeField]
    [FFToolTip("The color of the connection lines.")]
    Color targetsConnectionColor = Color.blue;

    [SerializeField]
    [FFToolTip("If your custom field use other informations that attached on the gameobject, you can put a test object here to see what turns out.")]
    Rigidbody testObject = null;

    [SerializeField]
    [FFToolTip("Is the field pointers drawn in the local space?")]
    bool useLocalSpace = true;

    [SerializeField]
    [FFToolTip("This controls the length of the pointers.\n1000 is the unit length.")]
    float pointerLength = 1000;

    [SerializeField]
    [FFToolTip("Space between the pointers.")]
    float pointerSpace = 1;

    [SerializeField]
    [FFToolTip("Number of pointers in x direction.")]
    int pointerXCount = 11;

    [SerializeField]
    [FFToolTip("Number of pointers in y direction.")]
    int pointerYCount = 11;

    [SerializeField]
    [FFToolTip("Number of pointers in z direction.")]
    int pointerZCount = 11;

    [SerializeField]
    [FFToolTip("The pointers that longer than this value will use the strong pointer color.")]
    float strongThreshold = 2;

    [SerializeField]
    [FFToolTip("The pointers that shorter than this value will use the weak pointer color.")]
    float weakThreshold = 0.1f;

    [SerializeField]
    [FFToolTip("The color of field pointers.")]
    Color strongPointerColor = Color.red;

    [SerializeField]
    [FFToolTip("The color of field pointers.")]
    Color weakPointerColor = Color.green;

    [SerializeField]
    [FFToolTip("The color of the ray.")]
    Color raycastColor = Color.cyan;
    #endregion

    #region Functions
    void DrawGizmos()
    {
        if (drawTargetsConnection)
        {
            DrawTargetsConnection();
        }
        if (drawFieldPointers)
        {
            DrawFieldPointer();
        }
        if (drawRaycastArea)
        {
            DrawRaycastArea();
        }
    }

    void DrawTargetsConnection()
    {
        Color defaultColor = Gizmos.color;
        Gizmos.color = targetsConnectionColor;
        Rigidbody[] targets = GetCurrentTargets();
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                Gizmos.DrawLine(thisTransform.position, targets[i].position);
            }
        }
        Gizmos.color = defaultColor;
    }

    void DrawFieldPointer()
    {
        if (pointerXCount <= 0 || pointerYCount <= 0 || pointerZCount <= 0)
        {
            return;
        }
        Initialize();
        UpdateFieldFunction();
        Color defaultColor = Gizmos.color;
        float colorInterval = strongThreshold - weakThreshold;
        for (float i = -(pointerXCount - 1) / 2f; i <= (pointerXCount - 1) / 2f; i++)
        {
            for (float j = -(pointerYCount - 1) / 2f; j <= (pointerYCount - 1) / 2f; j++)
            {
                for (float k = -(pointerZCount - 1) / 2f; k <= (pointerZCount - 1) / 2f; k++)
                {
                    Vector3 pos;
                    if (useLocalSpace)
                    {
                        pos = transform.position + thisTransform.TransformDirection(Vector3.Scale(new Vector3(i, j, k), thisTransform.lossyScale) * pointerSpace);
                    }
                    else
                    {
                        pos = transform.position + new Vector3(i, j, k) * pointerSpace;
                    }
                    Vector3 pointer = GetForce(pos, testObject) * pointerLength / 1000;
                    Gizmos.color = Color.Lerp(weakPointerColor, strongPointerColor, (pointer.magnitude - weakThreshold) / colorInterval);
                    FFGizmos.DrawArrow(pos, pointer);
                }
            }
        }
        Gizmos.color = defaultColor;
    }

    void DrawRaycastArea()
    {
        Color defaultColor = Gizmos.color;
        Gizmos.color = raycastColor;
        foreach (SelectionMethod sm in selectionMethods)
        {
            if (sm != null && sm.enabled && sm.targetingMode == SelectionMethod.ETargetingMode.Raycast)
            {
                switch (sm.rayCastOption.raycastType)
                {
                    case RaycastOption.ERayCastMode.RayCast:
                        if (sm.rayCastOption.useAnchor)
                        {
                            if (sm.rayCastOption.anchor == null)
                            {
                                continue;
                            }
                            Gizmos.DrawLine(transform.position, sm.rayCastOption.anchor.transform.position);
                        }
                        else
                        {
                            float distance = sm.rayCastOption.distance;
                            if (distance < 0)
                            {
                                distance = 1000000;
                            }
                            if (sm.rayCastOption.useLocalDirection)
                            {
                                Gizmos.DrawRay(transform.position, transform.TransformDirection(sm.rayCastOption.direction) * distance);
                            }
                            else
                            {
                                Gizmos.DrawRay(transform.position, sm.rayCastOption.direction * distance);
                            }
                        }
                        break;
                    case RaycastOption.ERayCastMode.SphereCast:
                        float r = sm.rayCastOption.radius;
                        if (sm.rayCastOption.useAnchor)
                        {
                            if (sm.rayCastOption.anchor == null)
                            {
                                continue;
                            }
                            Vector3 point1 = transform.position;
                            Vector3 point2 = sm.rayCastOption.anchor.transform.position;
                            FFGizmos.DrawCapsule(point1, point2, r);
                        }
                        else
                        {
                            float distance = sm.rayCastOption.distance;
                            if (distance < 0)
                            {
                                distance = 1000000;
                            }
                            if (sm.rayCastOption.useLocalDirection)
                            {
                                FFGizmos.DrawCapsuleByHeight(transform.position, transform.TransformDirection(sm.rayCastOption.direction) * distance, r);
                            }
                            else
                            {
                                FFGizmos.DrawCapsuleByHeight(transform.position, sm.rayCastOption.direction * distance, r);
                            }
                        }
                        break;
                    case RaycastOption.ERayCastMode.OverLapSphere:
                        float rr = sm.rayCastOption.radius;
                        Vector3 center;
                        if (sm.rayCastOption.useAnchor)
                        {
                            if (sm.rayCastOption.anchor == null)
                            {
                                continue;
                            }
                            center = sm.rayCastOption.anchor.transform.position;
                            FFGizmos.DrawSphere(center, rr);
                        }
                        else
                        {
                            center = sm.rayCastOption.sphereCenter;
                            if (sm.rayCastOption.useLocalDirection)
                            {
                                center = transform.TransformDirection(center) + transform.position;
                                FFGizmos.DrawSphere(center, rr);
                            }
                            else
                            {
                                FFGizmos.DrawSphere(center, rr);
                            }
                        }
                        break;
                }
            }
        }
        Gizmos.color = defaultColor;
    }

    public enum EGizmosMode
    {
        DrawGizmosSelected,
        DrawGizmosAlways,
        DoNotDraw
    }
    #endregion

    #endregion
}

