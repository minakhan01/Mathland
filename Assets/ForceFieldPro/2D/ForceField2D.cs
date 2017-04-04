using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// This is the force field 2D
/// </summary>
[AddComponentMenu("Physics 2D/Force Field 2D")]
public class ForceField2D : MonoBehaviour
{
    #region Properties
    [FFToolTip("Should this force field send messages to those who interacting with it?")]
    public bool sendMessage = true;

    [FFToolTip("Set this to false to disable all the tooltips.")]
    public bool showTooltips = true;

    [FFToolTip("The layers that this force field will influence.")]
    public LayerMask layerMask = -1;

    [FFToolTip("The rigidbodies in this list will always be ignored.")]
    public List<Rigidbody2D> alwaysIgnoredList;

    [FFToolTip("This multiplier will scale the output of the whole field.")]
    public float generalMultiplier = 1;

    #region Privates
    Transform thisTransform;

    [SerializeField]
    List<SelectionMethod> selectionMethods = new List<SelectionMethod>();

    [SerializeField]
    List<FieldFunction> fieldFunctions = new List<FieldFunction>();

    #region for targeting system
    Dictionary<int, Rigidbody2D> ignoredDict = new Dictionary<int, Rigidbody2D>();
    Dictionary<int, Rigidbody2D> targetDict1 = new Dictionary<int, Rigidbody2D>();
    Dictionary<int, Rigidbody2D> targetDict2 = new Dictionary<int, Rigidbody2D>();
    bool dictFlag = true;
    Dictionary<int, Rigidbody2D> currDict;
    Dictionary<int, Rigidbody2D> historyDict;
    #endregion
    #endregion
    #endregion

    #region Interfaces
    /// <summary>
    /// This interface is what need to be implemented by the custom field function class.
    /// The GetForce function will return the force vector that will later applied on the target.
    /// Note if the Force Field has UseLocalCoordination on, the position passed in will be in the force field object's local coordination. 
    /// </summary>
    public interface IFieldFunction2D
    {
        /// <summary>
        /// Return a vector based on the input rigidbody on input position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        Vector2 GetForce(Vector2 position, Rigidbody2D target);
    }
    #endregion

    #region Classes
    #region SelectionMethod
    /// <summary>
    /// This class contains the parameters of how should the ForceField2D select its targets.
    /// The final targets will be the union of all the enabled selection methods, no target will be select twice.
    /// </summary>
    [Serializable]
    public class SelectionMethod
    {
        /// <summary>
        /// The ForceField2D this selection method belongs.
        /// </summary>
        [HideInInspector]
        public ForceField2D forceField;

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
        public List<Rigidbody2D> targetsList = new List<Rigidbody2D>();

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
                case ETargetingMode.Collider2D:
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
        /// Collider2D: use the collider that attached to the gameobject
        /// Raycast: use raycast
        /// Manual: let the user edit the target list directly
        /// </summary>
        public enum ETargetingMode
        {
            Collider2D,
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
        /// The ForceField2D that this field function belongs to.
        /// </summary>
        [HideInInspector]
        public ForceField2D forceField;

        /// <summary>
        /// Is this field function enabled?
        /// </summary>
        [FFToolTip("Is this field function enabled?")]
        public bool enabled = true;

        /// <summary>
        /// Is this field function respect to the local coordination of the ForceField2D object?
        /// </summary>
        [FFToolTip("Is this field function respect to the local coordination of the ForceField2D object?")]
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
        /// The custom field function options.
        /// </summary>
        public FFCustomOption customFieldOption;

        //the current field function
        IFieldFunction2D currField;

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
        public Vector2 GetForce(Vector2 position, Rigidbody2D target)
        {
            if (currField == null)
            {
                return Vector2.zero;
            }
            if (useLocalCoordination)
            {
                position = forceField.thisTransform.InverseTransformPoint(position);
            }
            if (useLocalCoordination)
            {
                return forceField.thisTransform.TransformDirection(currField.GetForce(position, target));
            }
            return currField.GetForce(position - Vec3to2(forceField.thisTransform.position), target);
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
        /// </summary>
        public enum ESimpleFieldType
        {
            ConstantForce,
            CentripetalForce,
            AxipetalForce
        }
        #endregion
    }
    #endregion

    #region SimpleForceField2Ds
    /// <summary>
    /// This field will return the same vector at any position.
    /// </summary>
    [Serializable]
    public class FFConstantField : IFieldFunction2D
    {
        /// <summary>
        /// The magnitude of the force.
        /// </summary>
        [FFToolTip("The magnitude of the force.")]
        public float force = 1;

        [SerializeField]
        [FFToolTip("The direction of the force.\nIt need to be normalized.")]
        Vector2 _direction = Vector2.up;

        /// <summary>
        /// The direction of the force.
        /// Set it via code will automatically normalize it.
        /// </summary>
        public Vector2 direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// Return the force vector at given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector2 GetForce(Vector2 position, Rigidbody2D target)
        {
            return force * direction;
        }
    }

    /// <summary>
    /// This field returns a vector that always point to the reference point.
    /// Note that positive force is outward.
    /// </summary>
    [Serializable]
    public class FFCentripetalField : IFieldFunction2D
    {
        /// <summary>
        /// The reference point of the field.
        /// </summary>
        [FFToolTip("The reference point of the field.")]
        public Vector2 referencePoint = Vector2.zero;

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
        public Vector2 GetForce(Vector2 position, Rigidbody2D target)
        {
            Vector2 x = position - referencePoint;
            return force * distanceModifier.getModifier(x.magnitude) * x.normalized;
        }
    }

    /// <summary>
    /// This field returns a vector that always perpendicular and point to the reference line.
    /// Note that positive force is outward.
    /// </summary>
    [Serializable]
    public class FFAxipetalField : IFieldFunction2D
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
        public Vector2 referencePoint = Vector2.zero;

        [SerializeField]
        [FFToolTip("The direction of the reference line.\nIt need to be normalized.")]
        Vector2 _direction = Vector2.up;

        /// <summary>
        /// The direction of the reference line.
        /// Set it via code will automatically normalize it.
        /// </summary>
        public Vector2 direction
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
        public Vector2 GetForce(Vector2 position, Rigidbody2D target)
        {
            Vector2 x = position - (referencePoint + Vector2.Dot(direction, position - referencePoint) * direction);
            return force * distanceModifier.getModifier(x.magnitude) * x.normalized;
        }
    }
    #endregion

    #region CustomForceField2D
    /// <summary>
    /// This is a wrapper of the custom field functions.
    /// </summary>
    [Serializable]
    public class FFCustomOption
    {
        /// <summary>
        /// The actually custom field function.
        /// </summary>
        public CustomFieldFunction2D fieldFunction;
    }

    /// <summary>
    /// This is the class that a custom field function class should extend.
    /// </summary>
    public abstract class CustomFieldFunction2D : MonoBehaviour, IFieldFunction2D
    {
        /// <summary>
        /// This is the method that need to implement.
        /// It will return a vector based on the input rigidbody on input position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rigidbody"></param>
        /// <returns></returns>
        public abstract Vector2 GetForce(Vector2 position, Rigidbody2D rigidbody);
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
        Vector2 _direction;

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
        public Vector2 direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// The center of the over lap sphere.
        /// </summary>
        [FFToolTip("The center of the over lap sphere.")]
        public Vector2 center = Vector2.zero;

        /// <summary>
        /// First point for overlap area.
        /// </summary>
        [FFToolTip("Diagonal corner 1.")]
        public Vector2 point1;

        /// <summary>
        /// Second point for overlap area.
        /// </summary>
        [FFToolTip("Diagonal corner 2.")]
        public Vector2 point2;

        /// <summary>
        /// The minimum depth that this method will select.
        /// </summary>
        [FFToolTip("The minimum depth that this method will select.")]
        public float minDepth = -10000;

        /// <summary>
        /// The maximum depth that this method will select.
        /// </summary>
        [FFToolTip("The maximum depth that this method will select.")]
        public float maxDepth = 10000;

        /// <summary>
        /// This returns the result rigidbodies of the raycast.
        /// </summary>
        /// <param name="ff"></param>
        /// <returns></returns>
        public Rigidbody2D[] GetRaycastResult(ForceField2D ff)
        {
            Rigidbody2D[] targets = new Rigidbody2D[0];
            Collider2D[] overlap = new Collider2D[0];
            List<Rigidbody2D> rbList = new List<Rigidbody2D>();
            //raycast
            switch (raycastType)
            {
                case ERayCastMode.RayCast:
                    RaycastHit2D[] hits = new RaycastHit2D[0];
                    float d;
                    Vector2 dir;
                    //adjust direction and distance
                    if (useAnchor)
                    {
                        if (anchor == null)
                        {
                            return targets;
                        }
                        dir = Vec3to2(anchor.position - ff.thisTransform.position);
                        d = dir.magnitude;
                    }
                    else
                    {
                        if (distance == 0 || direction == Vector2.zero)
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
                    hits = Physics2D.RaycastAll(ff.thisTransform.position, dir, d, ff.layerMask, minDepth, maxDepth);
                    //select targets
                    if (numberLimit < 0)
                    {
                        List<Rigidbody2D> rblist = new List<Rigidbody2D>();
                        for (int i = 0; i < hits.Length; i++)
                        {
                            Rigidbody2D rb = hits[i].rigidbody;
                            if (rb != null && !ff.ignoredDict.ContainsKey(rb.GetInstanceID()))
                            {
                                rblist.Add(hits[i].rigidbody);
                            }
                        }
                        targets = rblist.ToArray();
                    }
                    else
                    {
                        Dictionary<float, Rigidbody2D> dict = new Dictionary<float, Rigidbody2D>();
                        for (int i = 0; i < hits.Length; i++)
                        {
                            Rigidbody2D rb = hits[i].rigidbody;
                            if (rb != null && !ff.ignoredDict.ContainsKey(rb.GetInstanceID()))
                            {
                                dict.Add((rb.transform.position - ff.thisTransform.position).sqrMagnitude, rb);
                            }
                        }
                        float[] dis = new float[dict.Keys.Count];
                        dict.Keys.CopyTo(dis, 0);
                        Array.Sort(dis);
                        int num = (dis.Length < numberLimit) ? dis.Length : numberLimit;
                        targets = new Rigidbody2D[num];
                        for (int i = 0; i < num; i++)
                        {
                            targets[i] = dict[dis[i]];
                        }
                    }
                    break;
                case ERayCastMode.OverLapArea:
                    if (useLocalDirection)
                    {
                        overlap = Physics2D.OverlapAreaAll(point1 + Vec3to2(ff.thisTransform.position), point2 + Vec3to2(ff.thisTransform.position), ff.layerMask, minDepth, maxDepth);
                    }
                    else
                    {
                        overlap = Physics2D.OverlapAreaAll(point1, point2, ff.layerMask, minDepth, maxDepth);
                    }
                    for (int i = 0; i < overlap.Length; i++)
                    {
                        if (overlap[i].attachedRigidbody)
                        {
                            rbList.Add(overlap[i].attachedRigidbody);
                        }
                    }
                    targets = rbList.ToArray();
                    break;
                case ERayCastMode.OverLapCircle:
                    if (useAnchor)
                    {
                        if (anchor == null)
                        {
                            return targets;
                        }
                        overlap = Physics2D.OverlapCircleAll(ForceField2D.Vec3to2(anchor.transform.position), radius, ff.layerMask, minDepth, maxDepth);
                    }
                    else if (useLocalDirection)
                    {
                        overlap = Physics2D.OverlapCircleAll(ff.thisTransform.TransformDirection(center) + ff.thisTransform.position, radius, ff.layerMask, minDepth, maxDepth);
                    }
                    else
                    {
                        overlap = Physics2D.OverlapCircleAll(center, radius, ff.layerMask, minDepth, maxDepth);
                    }
                    for (int i = 0; i < overlap.Length; i++)
                    {
                        if (overlap[i].attachedRigidbody)
                        {
                            rbList.Add(overlap[i].attachedRigidbody);
                        }
                    }
                    targets = rbList.ToArray();
                    break;
                case ERayCastMode.OverLapPoint:
                    if (useAnchor)
                    {
                        if (anchor == null)
                        {
                            return targets;
                        }
                        overlap = Physics2D.OverlapPointAll(ForceField2D.Vec3to2(anchor.transform.position), ff.layerMask, minDepth, maxDepth);
                    }
                    else if (useLocalDirection)
                    {
                        overlap = Physics2D.OverlapPointAll(ff.thisTransform.TransformDirection(center) + ff.thisTransform.position, ff.layerMask, minDepth, maxDepth);
                    }
                    else
                    {
                        overlap = Physics2D.OverlapPointAll(center, ff.layerMask, minDepth, maxDepth);
                    }
                    for (int i = 0; i < overlap.Length; i++)
                    {
                        if (overlap[i].attachedRigidbody)
                        {
                            rbList.Add(overlap[i].attachedRigidbody);
                        }
                    }
                    targets = rbList.ToArray();
                    break;
            }
            return targets;
        }

        /// <summary>
        /// The mode of the raycast.
        /// See unity script reference section Physics2D for details. 
        /// </summary>
        public enum ERayCastMode
        {
            RayCast,
            OverLapPoint,
            OverLapCircle,
            OverLapArea
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
                    historyDict[key].SendMessage("OnForceField2DExit", this, SendMessageOptions.DontRequireReceiver);
                    this.SendMessage("OnForceField2DExited", historyDict[key], SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.attachedRigidbody)
        {
            Rigidbody2D rb = other.attachedRigidbody;
            for (int i = 0; i < selectionMethods.Count; i++)
            {
                SelectionMethod sm = selectionMethods[i];
                if (sm != null && sm.targetingMode == SelectionMethod.ETargetingMode.Collider2D)
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
        List<Rigidbody2D> rbList = sm.targetsList;
        //targets list loop
        for (int i_rb = 0; i_rb < rbList.Count; i_rb++)
        {
            Rigidbody2D rb = rbList[i_rb];
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
                        rb.SendMessage("OnForceField2DStay", this, SendMessageOptions.DontRequireReceiver);
                        this.SendMessage("OnForceField2DStayed", rb, SendMessageOptions.DontRequireReceiver);
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
                            rb.SendMessage("OnForceField2DEnter", this, SendMessageOptions.DontRequireReceiver);
                            this.SendMessage("OnForceField2DEntered", rb, SendMessageOptions.DontRequireReceiver);
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
    void ApplyForce(Rigidbody2D target)
    {
        if (target == null)
        {
            return;
        }
        Vector2 vec = GetForce((Vector2)target.transform.position, target);
        target.AddForce(vec);
    }

    //copy the ignored list to a dictionary for easy look up
    void UpdateIgnoredDictionary()
    {
        ignoredDict.Clear();
        Rigidbody2D self = GetComponent<Rigidbody2D>();
        if (self != null)
        {
            ignoredDict.Add(self.GetInstanceID(), self);
        }
        for (int i = 0; i < alwaysIgnoredList.Count; i++)
        {
            Rigidbody2D rb = alwaysIgnoredList[i];
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

    //transfer vector3 to vector2, omit z
    static Vector2 Vec3to2(Vector3 vec3)
    {
        return new Vector2(vec3.x, vec3.y);
    }

    //transfer vector2 to vector3, z is 0 
    static Vector3 Vec2to3(Vector2 vec2)
    {
        return new Vector3(vec2.x, vec2.y, 0);
    }

    //return a vector that z is 0
    static Vector3 omitZ(Vector3 vec3)
    {
        return new Vector3(vec3.x, vec3.y, 0);
    }
    #endregion

    #region PublicFunctions
    /// <summary>
    /// It returns the force vector of the force field at the input world position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 GetForce(Vector2 position, Rigidbody2D target = null)
    {
        Vector2 force = Vector2.zero;
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
    public Rigidbody2D[] GetCurrentTargets()
    {
        if (currDict == null)
        {
            return new Rigidbody2D[0];
        }
        Rigidbody2D[] ret = new Rigidbody2D[currDict.Count];
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
    public void AddTarget(Rigidbody2D rb, SelectionMethod sm = null)
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
    public void RemoveTarget(Rigidbody2D rb, SelectionMethod sm = null)
    {
        if (rb == null)
        {
            return;
        }
        if (sm != null)
        {
            if (sm.targetingMode == SelectionMethod.ETargetingMode.Manual)
            {
                sm.targetsList.RemoveAll(delegate(Rigidbody2D _rb) { return _rb == rb; });
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
                _sm.targetsList.RemoveAll(delegate(Rigidbody2D _rb) { return _rb == rb; });
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
    public void Ignore(Rigidbody2D rb)
    {
        alwaysIgnoredList.Add(rb);
    }

    /// <summary>
    /// Remove input rigidbody from always ignored list.
    /// </summary>
    /// <param name="rb"></param>
    public void StopIgnore(Rigidbody2D rb)
    {
        alwaysIgnoredList.RemoveAll(delegate(Rigidbody2D _rb) { return _rb == rb; });
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
    Rigidbody2D testObject = null;

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
        Rigidbody2D[] targets = GetCurrentTargets();
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                Gizmos.DrawLine(thisTransform.position, targets[i].transform.position);
            }
        }
        Gizmos.color = defaultColor;
    }

    void DrawFieldPointer()
    {
        if (pointerXCount <= 0 || pointerYCount <= 0)
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
                Vector3 pos;
                if (useLocalSpace)
                {
                    pos = transform.position + thisTransform.TransformDirection(Vector3.Scale(new Vector3(i, j), thisTransform.lossyScale) * pointerSpace);
                }
                else
                {
                    pos = transform.position + new Vector3(i, j) * pointerSpace;
                }
                Vector3 pointer = GetForce(pos, testObject) * pointerLength / 1000;
                Gizmos.color = Color.Lerp(weakPointerColor, strongPointerColor, (pointer.magnitude - weakThreshold) / colorInterval);
                FFGizmos.DrawArrow(pos, pointer);
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
                    case RaycastOption.ERayCastMode.OverLapArea:
                        Vector2 point1 = sm.rayCastOption.point1;
                        Vector2 point2 = sm.rayCastOption.point2;
                        if (sm.rayCastOption.useLocalDirection)
                        {
                            Vector3 vec1 = Vec2to3(point1) + thisTransform.position;
                            Vector3 vec2 = new Vector3(point1.x, point2.y, 0) + thisTransform.position;
                            Vector3 vec3 = Vec2to3(point2) + thisTransform.position;
                            Vector3 vec4 = new Vector3(point2.x, point1.y) + thisTransform.position;
                            FFGizmos.DrawPolygon(vec1, vec2, vec3, vec4);
                        }
                        else
                        {
                            FFGizmos.DrawPolygon(point1, new Vector3(point1.x, point2.y, 0), point2, new Vector3(point2.x, point1.y));
                        }
                        break;
                    case RaycastOption.ERayCastMode.OverLapCircle:
                        float rr = sm.rayCastOption.radius;
                        Vector2 center = sm.rayCastOption.center;
                        if (sm.rayCastOption.useAnchor)
                        {
                            if (sm.rayCastOption.anchor == null)
                            {
                                continue;
                            }
                            center = sm.rayCastOption.anchor.transform.position;
                            FFGizmos.DrawCircle(center, Vector3.forward, rr);
                        }
                        else
                        {
                            center = sm.rayCastOption.center;
                            if (sm.rayCastOption.useLocalDirection)
                            {
                                center = transform.TransformDirection(center) + transform.position;
                                FFGizmos.DrawCircle(center, Vector3.forward, rr);
                            }
                            else
                            {
                                FFGizmos.DrawCircle(center, Vector3.forward, rr);
                            }
                        }
                        break;
                    case RaycastOption.ERayCastMode.OverLapPoint:
                        Vector2 center2 = sm.rayCastOption.center;
                        if (sm.rayCastOption.useAnchor)
                        {
                            if (sm.rayCastOption.anchor == null)
                            {
                                continue;
                            }
                            center2 = sm.rayCastOption.anchor.transform.position;
                            Gizmos.DrawSphere(center2, 0.1f);
                        }
                        else
                        {
                            if (sm.rayCastOption.useLocalDirection)
                            {
                                center2 = transform.TransformDirection(center2) + transform.position;
                                Gizmos.DrawSphere(center2, 0.1f);
                            }
                            else
                            {
                                Gizmos.DrawSphere(center2, 0.1f);
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


