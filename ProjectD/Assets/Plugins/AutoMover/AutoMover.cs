/*  Created by Two Units, May 15 2020 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoMoverPro
{

    /// <summary>
    /// Holds the necessary information for an anchor point (position and rotation).
    /// </summary>
    public struct AnchorPoint
    {
        /// <summary>
        /// Position of the anchor point.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Rotation of the anchor point as an euler angle.
        /// </summary>
        public Vector3 rotation;
        /// <summary>
        /// Scale of the anchor point as an euler angle.
        /// </summary>
        public Vector3 scale;
    }

    // Enumerations for curve, looping, rotation, point space and noise options

    /// <summary>
    /// Curve option enumerations
    /// </summary>
    public enum AutoMoverCurve
    {
        /// <summary>
        /// Straight lines between the anchor points.
        /// </summary>
        Linear,

        /// <summary>
        /// Form a single bezier curve from the anchor points.
        /// </summary>
        Curve,

        /// <summary>
        /// Form as many curves as there are anchor points - 1. The curves are connected with C2 continuity.
        /// </summary>
        Spline,


        /// <summary>
        /// Form as many curves as there are anchor points - 1. The curves are connected with C2 continuity.
        /// Stretches the spline to go through (or very close to) the anchor points.
        /// </summary>
        SplineThroughPoints,


    }

    /// <summary>
    /// Looping style enumerations
    /// </summary>
    public enum AutoMoverLoopingStyle
    {

        /// <summary>
        /// Form a closed loop from the anchor points.
        /// </summary>
        loop,

        /// <summary>
        /// Simply start the path again after finishing.
        /// </summary>
        repeat,

        /// <summary>
        /// Travel the original path back to the start after reaching the end.
        /// </summary>
        bounce
    }

    /// <summary>
    /// Rotation method enumarations
    /// </summary>
    public enum AutoMoverRotationMethod
    {

        /// <summary>
        /// For example when going from 300 degrees to 10 degrees, the rotation will actually go to 370 degrees.
        /// </summary>
        spherical,
        /// <summary>
        /// Rotation will always approach exactly the given value, even with multiple 360 degree spins.
        /// </summary>
        linear
    }

    /// <summary>
    /// Anchor point space enumerations
    /// </summary>
    public enum AutoMoverAnchorPointSpace
    {
        /// <summary>
        /// The points are defined in World space. Moving the parent of the object will not move the anchor points.
        /// </summary>
        world,

        /// <summary>
        /// The points are defined in the object's local space. Moving the parent of the object also moves the anchor points.
        /// </summary>
        local,

        /// <summary>
        /// The points are defined in the object's 'child' space. Moving the object also moves the anchor points.
        /// </summary>
        child
    }

    /// <summary>
    /// Noise type enumerations
    /// </summary>
    public enum AutoMoverNoiseType
    {
        /// <summary>
        /// Generates random noise. Uses the specified noise amplitude and frequency.
        /// </summary>
        random,

        /// <summary>
        /// Sine noise. Uses specified amplitude, frequency and offset
        /// </summary>
        sine,

        /// <summary>
        /// Generates smooth random noise. Uses the specified noise amplitude and frequency.
        /// </summary>
        smoothRandom
    }

    /// <summary>
    /// Target enumerations. Used when generating noise.
    /// </summary>
    public enum AutoMoverTarget
    {
        position,

        rotation,

        scale
    }

    public class AutoMover : MonoBehaviour
    {
        [SerializeField]
        private List<Vector3> pos = new List<Vector3>();
        /// <summary>
        /// List of the anchor point positions.
        /// </summary>
        public List<Vector3> Pos
        {
            get { return pos; }
        }

        [SerializeField]
        private List<Vector3> rot = new List<Vector3>();
        /// <summary>
        /// List of the anchor point rotations.
        /// </summary>
        public List<Vector3> Rot
        {
            get { return rot; }
        }

        [SerializeField]
        private List<Vector3> scl = new List<Vector3>();
        /// <summary>
        /// List of the anchor point scales.
        /// </summary>
        public List<Vector3> Scl
        {
            get { return scl; }
        }

        [SerializeField]
        private float length = 5;
        /// <summary>
        /// The length of the curve in seconds. Minimum value of 0.5f.
        /// </summary>
        public float Length
        {
            get { return length; }
            set
            {
                bool moved = moving && instantRuntimeChanges;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moved)
                    StopMoving();

                if (value < 0.5f)
                    length = 0.5f;
                else
                    length = value;

                if (moved)
                    StartMoving();
            }
        }

        [SerializeField]
        private AutoMoverCurve curveStyle = AutoMoverCurve.Spline;
        /// <summary>
        /// The type of the curve.
        /// </summary>
        public AutoMoverCurve CurveStyle
        {
            get { return curveStyle; }
            set
            {
                if (curveStyle != value)
                {
                    bool moved = moving && instantRuntimeChanges;
                    if (moving && !instantRuntimeChanges)
                        newChanges = true;
                    if (moved)
                        StopMoving();

                    curveStyle = value;

                    if (moved)
                        StartMoving();
                }
            }
        }

        [SerializeField]
        private AutoMoverLoopingStyle loopingStyle = AutoMoverLoopingStyle.repeat;
        /// <summary>
        /// The looping style.
        /// </summary>
        public AutoMoverLoopingStyle LoopingStyle
        {
            get { return loopingStyle; }
            set
            {
                if (value != loopingStyle)
                {
                    bool moved = moving && instantRuntimeChanges;
                    if (moving && !instantRuntimeChanges)
                        newChanges = true;
                    if (moved)
                        StopMoving();

                    loopingStyle = value;

                    if (moved)
                        StartMoving();
                }
            }
        }

        [SerializeField]
        private AutoMoverRotationMethod rotationMethod = AutoMoverRotationMethod.linear;
        /// <summary>
        /// The way rotations are done.
        /// </summary>
        public AutoMoverRotationMethod RotationMethod
        {
            get { return rotationMethod; }
            set
            {
                if (rotationMethod != value)
                {
                    bool moved = moving && instantRuntimeChanges;
                    if (moving && !instantRuntimeChanges)
                        newChanges = true;
                    if (moved)
                        StopMoving();

                    rotationMethod = value;

                    if (moved)
                        StartMoving();
                }
            }
        }

        [SerializeField]
        private AutoMoverAnchorPointSpace anchorPointSpace = AutoMoverAnchorPointSpace.world;
        /// <summary>
        /// The space in which anchor points are defined.
        /// Converts existing anchor points when changed.
        /// </summary>
        public AutoMoverAnchorPointSpace AnchorPointSpace
        {
            get { return anchorPointSpace; }
            set
            {
                if (anchorPointSpace != value)
                {
                    if (transform.parent != null)
                    {
                        bool moved = moving && instantRuntimeChanges;
                        if (moving && !instantRuntimeChanges)
                            newChanges = true;
                        if (moved)
                            StopMoving();

                        AutoMoverAnchorPointSpace oldValue = anchorPointSpace;

                        anchorPointSpace = value;

                        //Convert the anchor points to world/local/child coordinates, depending on the variable
                        if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                        {
                            if (oldValue == AutoMoverAnchorPointSpace.local)
                            {
                                //from local to world
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    pos[i] = transform.parent.TransformPoint(pos[i]);
                                    rot[i] = (transform.parent.rotation * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(transform.parent.lossyScale.x * scl[i].x, transform.parent.lossyScale.y * scl[i].y, transform.parent.lossyScale.z * scl[i].z);
                                }
                            }
                            else
                            {
                                //from child to world
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    pos[i] = transform.TransformPoint(pos[i]);
                                    rot[i] = (transform.rotation * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(transform.lossyScale.x * scl[i].x, transform.lossyScale.y * scl[i].y, transform.lossyScale.z * scl[i].z);
                                }
                            }
                        }
                        else if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
                        {
                            if (oldValue == AutoMoverAnchorPointSpace.world)
                            {
                                //from world to local
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    pos[i] = transform.parent.InverseTransformPoint(pos[i]);
                                    rot[i] = (Quaternion.Inverse(transform.parent.rotation) * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(scl[i].x / transform.parent.lossyScale.x, scl[i].y / transform.parent.lossyScale.y, scl[i].z / transform.parent.lossyScale.z);
                                }
                            }
                            else
                            {
                                //from child to local (via world)
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    //child to world
                                    pos[i] = transform.TransformPoint(pos[i]);
                                    rot[i] = (transform.rotation * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(transform.lossyScale.x * scl[i].x, transform.lossyScale.y * scl[i].y, transform.lossyScale.z * scl[i].z);

                                    //world to local
                                    pos[i] = transform.parent.InverseTransformPoint(pos[i]);
                                    rot[i] = (Quaternion.Inverse(transform.parent.rotation) * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(scl[i].x / transform.parent.lossyScale.x, scl[i].y / transform.parent.lossyScale.y, scl[i].z / transform.parent.lossyScale.z);
                                }
                            }
                        }
                        else if (anchorPointSpace == AutoMoverAnchorPointSpace.child)
                        {
                            if (oldValue == AutoMoverAnchorPointSpace.world)
                            {
                                //from world to child
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    pos[i] = transform.InverseTransformPoint(pos[i]);
                                    rot[i] = (Quaternion.Inverse(transform.rotation) * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(scl[i].x / transform.lossyScale.x, scl[i].y / transform.lossyScale.y, scl[i].z / transform.lossyScale.z);
                                }
                            }
                            else
                            {
                                //from local to child (via world)
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    //local to world
                                    pos[i] = transform.parent.TransformPoint(pos[i]);
                                    rot[i] = (transform.parent.rotation * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(transform.parent.lossyScale.x * scl[i].x, transform.parent.lossyScale.y * scl[i].y, transform.parent.lossyScale.z * scl[i].z);

                                    //world to child
                                    pos[i] = transform.InverseTransformPoint(pos[i]);
                                    rot[i] = (Quaternion.Inverse(transform.rotation) * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(scl[i].x / transform.lossyScale.x, scl[i].y / transform.lossyScale.y, scl[i].z / transform.lossyScale.z);
                                }
                            }
                        }

                        if (moved)
                            StartMoving();
                    }
                    else
                    {
                        //object is directly in the scene aka. parent is null
                        AutoMoverAnchorPointSpace oldValue = anchorPointSpace;
                        anchorPointSpace = value;

                        if (anchorPointSpace == AutoMoverAnchorPointSpace.local || anchorPointSpace == AutoMoverAnchorPointSpace.world)
                        {
                            if (oldValue == AutoMoverAnchorPointSpace.child)
                            {
                                //from child to world/local
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    pos[i] = transform.TransformPoint(pos[i]);
                                    rot[i] = (transform.rotation * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(transform.lossyScale.x * scl[i].x, transform.lossyScale.y * scl[i].y, transform.lossyScale.z * scl[i].z);
                                }
                            }
                        }
                        else if (anchorPointSpace == AutoMoverAnchorPointSpace.child)
                        {
                            if (oldValue == AutoMoverAnchorPointSpace.local || oldValue == AutoMoverAnchorPointSpace.world)
                            {
                                //from world/local to child
                                for (int i = 0; i < pos.Count; ++i)
                                {
                                    pos[i] = transform.InverseTransformPoint(pos[i]);
                                    rot[i] = (Quaternion.Inverse(transform.rotation) * Quaternion.Euler(rot[i])).eulerAngles;
                                    scl[i] = new Vector3(scl[i].x / transform.lossyScale.x, scl[i].y / transform.lossyScale.y, scl[i].z / transform.lossyScale.z);
                                }
                            }
                        }
                    }
                }
            }
        }

        [SerializeField]
        private bool moving = false;
        /// <summary>
        /// True if the mover is moving the object.
        /// </summary>
        public bool Moving
        {
            get { return moving; }
        }

        [SerializeField]
        private AutoMoverNoiseType positionNoiseType = AutoMoverNoiseType.random;
        /// <summary>
        /// The type of the position noise.
        /// </summary>
        public AutoMoverNoiseType PositionNoiseType
        {
            get { return positionNoiseType; }
            set
            {
                if (positionNoiseType != value)
                {
                    positionNoiseType = value;
                }
            }
        }

        [SerializeField]
        private AutoMoverNoiseType rotationNoiseType = AutoMoverNoiseType.random;
        /// <summary>
        /// The type of the rotation noise.
        /// </summary>
        public AutoMoverNoiseType RotationNoiseType
        {
            get { return rotationNoiseType; }
            set
            {
                if (rotationNoiseType != value)
                {
                    rotationNoiseType = value;
                }
            }
        }

        [SerializeField]
        private AutoMoverNoiseType scaleNoiseType = AutoMoverNoiseType.random;
        /// <summary>
        /// The type of the scale noise.
        /// </summary>
        public AutoMoverNoiseType ScaleNoiseType
        {
            get { return scaleNoiseType; }
            set
            {
                if (scaleNoiseType != value)
                {
                    scaleNoiseType = value;
                }
            }
        }

        [SerializeField]
        private Vector3 positionNoiseAmplitude = new Vector3(0, 0, 0);
        /// <summary>
        /// The amplitude of position noise in each direction.
        /// </summary>
        public Vector3 PositionNoiseAmplitude
        {
            get { return positionNoiseAmplitude; }
            set
            {
                if (value.x < 0)
                    positionNoiseAmplitude.x = 0;
                else
                    positionNoiseAmplitude.x = value.x;

                if (value.y < 0)
                    positionNoiseAmplitude.y = 0;
                else
                    positionNoiseAmplitude.y = value.y;

                if (value.z < 0)
                    positionNoiseAmplitude.z = 0;
                else
                    positionNoiseAmplitude.z = value.z;
            }
        }

        [SerializeField]
        private Vector3 positionNoiseFrequency = new Vector3(1, 1, 1);
        /// <summary>
        /// The frequency of position noise in each direction.
        /// </summary>
        public Vector3 PositionNoiseFrequency
        {
            get { return positionNoiseAmplitude; }
            set
            {
                if (value.x < 0)
                    positionNoiseAmplitude.x = 0;
                else
                    positionNoiseAmplitude.x = value.x;

                if (value.y < 0)
                    positionNoiseAmplitude.y = 0;
                else
                    positionNoiseAmplitude.y = value.y;

                if (value.z < 0)
                    positionNoiseAmplitude.z = 0;
                else
                    positionNoiseAmplitude.z = value.z;
            }
        }

        [SerializeField]
        private Vector3 rotationNoiseAmplitude = new Vector3(0, 0, 0);
        /// <summary>
        /// The amplitude of rotation noise in each direction. (Degrees)
        /// </summary>
        public Vector3 RotationNoiseAmplitude
        {
            get { return rotationNoiseAmplitude; }
            set
            {
                if (value.x < 0)
                    rotationNoiseAmplitude.x = 0;
                else
                    rotationNoiseAmplitude.x = value.x;

                if (value.y < 0)
                    rotationNoiseAmplitude.y = 0;
                else
                    rotationNoiseAmplitude.y = value.y;

                if (value.z < 0)
                    rotationNoiseAmplitude.z = 0;
                else
                    rotationNoiseAmplitude.z = value.z;
            }
        }

        [SerializeField]
        private Vector3 scaleNoiseAmplitude = new Vector3(0, 0, 0);
        /// <summary>
        /// The amplitude of scale noise in each direction.
        /// </summary>
        public Vector3 ScaleNoiseAmplitude
        {
            get { return scaleNoiseAmplitude; }
            set
            {
                if (value.x < 0)
                    scaleNoiseAmplitude.x = 0;
                else
                    scaleNoiseAmplitude.x = value.x;

                if (value.y < 0)
                    scaleNoiseAmplitude.y = 0;
                else
                    scaleNoiseAmplitude.y = value.y;

                if (value.z < 0)
                    scaleNoiseAmplitude.z = 0;
                else
                    scaleNoiseAmplitude.z = value.z;
            }
        }

        [SerializeField]
        private Vector3 rotationNoiseFrequency = new Vector3(1, 1, 1);
        /// <summary>
        /// The frequency of rotation noise in each direction.
        /// <para>For random noise, this means the amount of half rotations (180 degrees) in a second.</para>
        /// <para>For sine noise, this means the frequency of the sine wave. With value of 1, the wave takes the time of 2*pi seconds.</para>
        /// </summary>
        public Vector3 RotationNoiseFrequency
        {
            get { return rotationNoiseFrequency; }
            set
            {
                if (value.x < 0)
                    rotationNoiseFrequency.x = 0;
                else
                    rotationNoiseFrequency.x = value.x;

                if (value.y < 0)
                    rotationNoiseFrequency.y = 0;
                else
                    rotationNoiseFrequency.y = value.y;

                if (value.z < 0)
                    rotationNoiseFrequency.z = 0;
                else
                    rotationNoiseFrequency.z = value.z;
            }
        }

        [SerializeField]
        private Vector3 scaleNoiseFrequency = new Vector3(1, 1, 1);
        /// <summary>
        /// The frequency of scale noise in each direction.
        /// </summary>
        public Vector3 ScaleNoiseFrequency
        {
            get { return scaleNoiseFrequency; }
            set
            {
                if (value.x < 0)
                    scaleNoiseFrequency.x = 0;
                else
                    scaleNoiseFrequency.x = value.x;

                if (value.y < 0)
                    scaleNoiseFrequency.y = 0;
                else
                    scaleNoiseFrequency.y = value.y;

                if (value.z < 0)
                    scaleNoiseFrequency.z = 0;
                else
                    scaleNoiseFrequency.z = value.z;
            }
        }

        [SerializeField]
        private Vector3 rotationSineOffset = new Vector3(0, 0, 0);
        /// <summary>
        /// The phase offset of rotation sine noise in each direction. Values equal with the remainder when divided by 2*pi.
        /// </summary>
        public Vector3 RotationSineOffset
        {
            get { return rotationSineOffset; }
            set
            {
                rotationSineOffset = value;
            }
        }

        [SerializeField]
        private Vector3 scaleSineOffset = new Vector3(0, 0, 0);
        /// <summary>
        /// The phase offset of scale sine noise in each direction. Values equal with the remainder when divided by 2*pi.
        /// </summary>
        public Vector3 ScaleSineOffset
        {
            get { return scaleSineOffset; }
            set
            {
                scaleSineOffset = value;
            }
        }

        [SerializeField]
        private Vector3 positionSineOffset = new Vector3(0, 0, 0);
        /// <summary>
        /// The phase offset of position sine noise in each direction. Values equal with the remainder when divided by 2*pi.
        /// </summary>
        public Vector3 PositionSineOffset
        {
            get { return positionSineOffset; }
            set
            {
                positionSineOffset = value;
            }
        }

        [SerializeField]
        private bool runOnStart = true;
        /// <summary>
        /// The movement is started automatically during Start. If set to false, the movement will have to be manually started.
        /// </summary>
        public bool RunOnStart
        {
            get { return runOnStart; }
            set { runOnStart = value; }
        }

        [SerializeField]
        private float delayStartMin = 0;
        /// <summary>
        /// Minimum delay on start in seconds. The actual delay is a random number between DelayStartMin and DelayStartMax.
        /// </summary>
        public float DelayStartMin
        {
            get { return delayStartMin; }
            set
            {
                if (value < 0)
                    delayStartMin = 0;
                else
                    delayStartMin = value;

                if (DelayStartMax < DelayStartMin)
                    DelayStartMax = DelayStartMin;
            }
        }

        [SerializeField]
        private float delayStartMax = 0;
        /// <summary>
        /// Maximum delay on start in seconds. The actual delay is a random number between DelayStartMin and DelayStartMax.
        /// </summary>
        public float DelayStartMax
        {
            get { return delayStartMax; }
            set
            {
                if (value < delayStartMin)
                    delayStartMax = delayStartMin;
                else
                    delayStartMax = value;
            }
        }

        [SerializeField]
        private float delayMin = 0;
        /// <summary>
        /// Minimum delay between loops in seconds. The actual delay is a random number between DelayMin and DelayMax.
        /// </summary>
        public float DelayMin
        {
            get { return delayMin; }
            set
            {
                if (value < 0)
                    delayMin = 0;
                else
                    delayMin = value;

                if (delayMax < delayMin)
                    delayMax = delayMin;
            }
        }

        [SerializeField]
        private float delayMax = 0;
        /// <summary>
        /// Maximum delay between loops in seconds. The actual delay is a random number between DelayMin and DelayMax.
        /// </summary>
        public float DelayMax
        {
            get { return delayMax; }
            set
            {
                if (value < delayMin)
                    delayMax = delayMin;
                else
                    delayMax = value;
            }
        }

        [SerializeField]
        private uint stopAfter = 0;
        /// <summary>
        /// How many times the object moves the path. 0 Means that the movement will run until stopped.
        /// </summary>
        public uint StopAfter
        {
            get { return stopAfter; }
            set
            {
                if (value < 0)
                    stopAfter = 0;
                else
                    stopAfter = value;
            }
        }

        [SerializeField]
        private float stopEveryXSeconds = 0;
        /// <summary>
        /// How often (seconds) should the movement stop.
        /// <para>The time the movement is stopped for is defined by StopForXSeconds.</para>
        /// </summary>
        public float StopEveryXSeconds
        {
            get { return stopEveryXSeconds; }
            set
            {
                if (value < 0)
                    stopEveryXSeconds = 0;
                else
                    stopEveryXSeconds = value;
            }
        }

        [SerializeField]
        private float stopForXSeconds = 0;
        /// <summary>
        /// For how long should the movement stop. Value of 0 means no stopping.
        /// <para>The interval for how often the movement is stopped is defined by StopEveryXSeconds.</para>
        /// </summary>
        public float StopForXSeconds
        {
            get { return stopForXSeconds; }
            set
            {
                if (value < 0)
                    stopForXSeconds = 0;
                else
                    stopForXSeconds = value;
            }
        }

        [SerializeField]
        private bool slowOnCurves = false;
        /// <summary>
        /// Should the movement be slower for sharp curves and faster during straight paths.
        /// <para>Turn rate and deceleration can be adjusted.</para>
        /// </summary>
        public bool SlowOnCurves
        {
            get { return slowOnCurves; }
            set
            {
                if (value != slowOnCurves)
                {
                    bool moved = moving && instantRuntimeChanges;
                    if (moving && !instantRuntimeChanges)
                        newChanges = true;
                    if (moved)
                        StopMoving();

                    slowOnCurves = value;

                    if (moved)
                        StartMoving();

                }
            }
        }

        [SerializeField]
        private bool faceForward = false;
        /// <summary>
        /// Ignore rotations in anchor points after the first one and always face forward.
        /// <para>Default 'forward' is along the X-axis, so align the object in the first anchor point along that.</para>
        /// </summary>
        public bool FaceForward
        {
            get { return faceForward; }
            set
            {
                if (value != faceForward)
                {
                    bool moved = moving && instantRuntimeChanges;
                    if (moving && !instantRuntimeChanges)
                        newChanges = true;
                    if (moved)
                        StopMoving();

                    faceForward = value;

                    if (moved)
                        StartMoving();

                }
            }
        }

        [SerializeField]
        private bool dynamicUpVector = true;
        /// <summary>
        /// Allow the up-vector to be dynamically recalculated while facing forward to allow for the object to turn smoothly along the curve.
        /// </summary>
        public bool DynamicUpVector
        {
            get { return dynamicUpVector; }
            set
            {
                if (value != dynamicUpVector)
                {
                    bool moved = moving && instantRuntimeChanges;
                    if (moving && !instantRuntimeChanges)
                        newChanges = true;
                    if (moved)
                        StopMoving();

                    dynamicUpVector = value;

                    if (moved)
                        StartMoving();

                }
            }
        }

        [SerializeField]
        private bool drawGizmos = true;
        /// <summary>
        /// Should the path be visualized in the editor.
        /// </summary>
        public bool DrawGizmos
        {
            get { return drawGizmos; }
            set { drawGizmos = value; }
        }

        [SerializeField]
        private bool instantRuntimeChanges = true;
        /// <summary>
        /// Apply changes made during runtime instantly and restart the movement.
        /// <para>If false, changes are applied after the current loop.</para>
        /// </summary>
        public bool InstantRuntimeChanges
        {
            get { return instantRuntimeChanges; }
            set { instantRuntimeChanges = value; }
        }

        [SerializeField]
        private bool populateWithMesh = true;
        /// <summary>
        /// Populate the exported curve's gameobjects with the MeshRenderer or SpriteRenderer attached to the object.
        /// </summary>
        public bool PopulateWithMesh
        {
            get { return populateWithMesh; }
            set { populateWithMesh = value; }
        }

        [SerializeField]
        private List<bool> movableGizmos = new List<bool>();
        /// <summary>
        /// Should the gizmos in editor have position control.
        /// </summary>
        public List<bool> MovableGizmos
        {
            get { return movableGizmos; }
        }

        private bool isPaused = false;
        /// <summary>
        /// Tells if the movement is paused. False if the object is not moving, or if it is moving and is not paused.
        /// <para>Control with Pause() and Resume()</para>
        /// </summary>
        public bool IsPaused
        {
            get { return isPaused; }
        }

        [SerializeField]
        [Range(0, 1)]
        private float curveWeight = 0.666666f;
        /// <summary>
        /// The control point weighting used for the creation of curves and splines.
        /// <para>Values from 0 to 1 are allowed</para>
        /// </summary>
        public float CurveWeight
        {
            get { return curveWeight; }
            set
            {
                if (value < 0)
                    curveWeight = 0;
                else if (value > 1)
                    curveWeight = 1;
                else
                    curveWeight = value;
            }
        }

        [SerializeField]
        [Range(1f, 360f)]
        private float turnRate = 20f;
        /// <summary>
        /// The turn rate when SlowOnCurves is enabled.
        /// <para>Values from 1 to 360 are allowed.</para>
        /// </summary>
        public float TurnRate
        {
            get { return turnRate; }
            set
            {
                if (value < 1)
                    turnRate = 1;
                else if (value > 360)
                    turnRate = 360;
                else
                    turnRate = value;
            }
        }

        [SerializeField]
        private float decelerationTime = 0.5f;
        /// <summary>
        /// The time in seconds that is spent decelerating for a sharp turn when SlowOnCurves is enabled.
        /// <para>Values must be non-negative.</para>
        /// </summary>
        public float DecelerationTime
        {
            get { return decelerationTime; }
            set
            {
                if (value < 0)
                    decelerationTime = 0;
                else
                    decelerationTime = value;
            }
        }

        [SerializeField]
        private int steps = 300;
        /// <summary>
        /// The number of segments in the curve.
        /// <para>Value cannot be lower than the number of anchor points.</para>
        /// <para>Real step count may differ slightly in reality.</para>
        /// </summary>
        public int Steps
        {
            get { return steps; }
            set
            {
                int minSteps = Mathf.Min(pos.Count, 1);
                if (value < minSteps)
                    steps = minSteps;
                else
                    steps = value;
            }
        }

        //Holding these for the editor, because it forgets everything once some other object is selected
#pragma warning disable 0414
        [SerializeField]
        private bool anchorExpanded = false;
        [SerializeField]
        private bool curveExpanded = false;
        [SerializeField]
        private bool noiseExpanded = false;
        [SerializeField]
        private bool noisePosExpanded = false;
        [SerializeField]
        private bool noiseRotExpanded = false;
        [SerializeField]
        private bool noiseScaleExpanded = false;
        [SerializeField]
        private bool firstInspect = true;
        [SerializeField]
        private bool newChanges = false;
#pragma warning restore 0414

        // private variables
        private Vector3 posNoiseTarget;
        private Vector3 rotNoiseTarget;
        private Vector3 scaleNoiseTarget;
        private Vector3[] prevPosNoiseTargets = new Vector3[3];
        private Vector3[] prevRotNoiseTargets = new Vector3[3];
        private Vector3[] prevScaleNoiseTargets = new Vector3[3];
        private Vector3 posSmoothNoiseProgress;
        private Vector3 rotSmoothNoiseProgress;
        private Vector3 sclSmoothNoiseProgress;
        private Vector3 origLocalPos;
        private Vector3 origLocalRot;
        private Vector3 origLocalScl;
        private List<Vector3> origChildPosLocal;
        private List<Vector3> origChildRotLocal;
        private List<Vector3> origChildSclLocal;
        private float loopProgress = 0;
        private bool resetMovement = false;

        void Start()
        {
            if (runOnStart)
                StartMoving();
        }

        void Update()
        {
            if (resetMovement)
            {
                StopMoving();
                StartMoving();
                resetMovement = false;
            }
        }

        /// <summary>
        /// Adds the current position and rotation of the object as an anchor point.
        /// </summary>
        public void AddAnchorPoint()
        {
            if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
            {
                AddAnchorPoint(transform.position, transform.rotation.eulerAngles, transform.lossyScale);
            }
            else if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
            {
                AddAnchorPoint(transform.localPosition, transform.localRotation.eulerAngles, transform.localScale);
            }
            else
            {
                AddAnchorPoint(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            }

        }

        /// <summary>
        /// Adds the given position, rotation and scale as a anchor point at the end of the anchor point list. If the object is already moving, the new anchor point will be taken into account during the next lap.
        /// </summary>
        /// <param name="position">Position of the anchor point.</param>
        /// <param name="rotation">Rotation of the anchor point as an euler angle.</param>
        /// <param name="scale">Scale of the anchor point.</param>
        public void AddAnchorPoint(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            AnchorPointVersionCheck();

            bool moved = false;
            if (moving && !instantRuntimeChanges)
                newChanges = true;
            if (moving && instantRuntimeChanges)
            {
                moved = true;
                StopMoving();
            }

            pos.Add(position);
            rot.Add(rotation);
            scl.Add(scale);
            movableGizmos.Add(true);


            if (moved)
                StartMoving();
        }

        /// <summary>
        /// Adds the given position and rotation as a anchor point at the end of the anchor point list. Scale will be defaulted to the transform's current scale. If the object is already moving, the new anchor point will be taken into account during the next lap.
        /// </summary>
        /// <param name="position">Position of the anchor point.</param>
        /// <param name="rotation">Rotation of the anchor point as an euler angle.</param>
        public void AddAnchorPoint(Vector3 position, Vector3 rotation)
        {
            AnchorPointVersionCheck();

            bool moved = false;
            if (moving && !instantRuntimeChanges)
                newChanges = true;
            if (moving && instantRuntimeChanges)
            {
                moved = true;
                StopMoving();
            }

            pos.Add(position);
            rot.Add(rotation);
            if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                scl.Add(transform.lossyScale);
            else
                scl.Add(transform.localScale);
            movableGizmos.Add(true);


            if (moved)
                StartMoving();
        }

        /// <summary>
        /// Adds the given AnchorPoint at the end of the anchor point list. If the object is already moving, the new anchor point will be taken into account during the next lap.
        /// </summary>
        /// <param name="anchorPoint">Anchor point to be added.</param>
        public void AddAnchorPoint(AnchorPoint anchorPoint)
        {
            AddAnchorPoint(anchorPoint.position, anchorPoint.rotation, anchorPoint.scale);
        }

        /// <summary>
        /// Returns the AnchorPoint (position, rotation and scale) at the given index.
        /// </summary>
        /// <param name="index">Which element should be returned.</param>
        /// <returns>The anchor point at the given index.</returns>
        public AnchorPoint GetAnchorPoint(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                AnchorPoint ap = new AnchorPoint();
                ap.position = pos[index];
                ap.rotation = rot[index];
                ap.scale = scl[index];
                return ap;
            }

            throw new System.NullReferenceException("Given index is not in the range of anchor points.");
        }

        /// <summary>
        /// Returns the position of the anchor point at the given index.
        /// </summary>
        /// <param name="index">Which element should be returned.</param>
        /// <returns>The position of the anchor point at the given index.</returns>
        public Vector3 GetAnchorPointPosition(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                return pos[index];
            }

            throw new System.NullReferenceException("Given index is not in the range of anchor points.");
        }

        /// <summary>
        /// Returns the rotation of the anchor point at the given index.
        /// </summary>
        /// <param name="index">Which element should be returned.</param>
        /// <returns>The rotation of the anchor point at the given index.</returns>
        public Vector3 GetAnchorPointRotation(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < rot.Count)
            {
                return rot[index];
            }

            throw new System.NullReferenceException("Given index is not in the range of anchor points.");
        }

        /// <summary>
        /// Returns the scale of the anchor point at the given index.
        /// </summary>
        /// <param name="index">Which element should be returned.</param>
        /// <returns>The scale of the anchor point at the given index.</returns>
        public Vector3 GetAnchorPointScale(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < scl.Count)
            {
                return scl[index];
            }

            throw new System.NullReferenceException("Given index is not in the range of anchor points.");
        }

        /// <summary>
        /// Set a gizmo to be movable in the editor or not.
        /// </summary>
        /// <param name="index">Which element should be changed.</param>
        /// <param name="value">New value for the boolean.</param>
        /// <returns>The boolean value if an anchor point's gizmo is movable or not at the given index.</returns>
        public bool SetMovableGizmo(int index, bool value)
        {
            AnchorPointVersionCheck();

            if (index < movableGizmos.Count && index >= 0)
            {
                movableGizmos[index] = value;
                return movableGizmos[index];
            }

            throw new System.NullReferenceException("Given index is not in the range of anchor points.");
        }

        /// <summary>
        /// See if a gizmo is movable in the editor or not.
        /// </summary>
        /// <param name="index">Which element should be returned.</param>
        /// <returns>The boolean value if an anchor point's gizmo is movable or not at the given index.</returns>
        public bool GetMovableGizmo(int index)
        {
            AnchorPointVersionCheck();

            if (index < movableGizmos.Count && index >= 0)
            {
                return movableGizmos[index];
            }

            throw new System.NullReferenceException("Given index is not in the range of anchor points.");
        }

        /// <summary>
        /// Sets the anchor point (position, rotation and scale) at the given index.
        /// </summary>
        /// <param name="index">Which element should be set.</param>
        /// <param name="anchorPoint">The anchor point.</param>
        public void SetAnchorPoint(int index, AnchorPoint anchorPoint)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                pos[index] = anchorPoint.position;
                rot[index] = anchorPoint.rotation;
                scl[index] = anchorPoint.scale;

                if (moved)
                    StartMoving();
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Sets the anchor point (position, rotation and scale) at the given index.
        /// </summary>
        /// <param name="index">Which element should be set.</param>
        /// <param name="position">The position of the anchor point.</param>
        /// <param name="rotation">The rotation of the anchor point.</param>
        /// <param name="scale">The scale of the anchor point.</param>
        public void SetAnchorPoint(int index, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                pos[index] = position;
                rot[index] = rotation;
                scl[index] = scale;

                if (moved)
                    StartMoving();
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Sets the anchor point (position and rotation. scale is assumed) at the given index.
        /// </summary>
        /// <param name="index">Which element should be set.</param>
        /// <param name="position">The position of the anchor point.</param>
        /// <param name="rotation">The rotation of the anchor point.</param>
        public void SetAnchorPoint(int index, Vector3 position, Vector3 rotation)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                pos[index] = position;
                rot[index] = rotation;
                if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                    scl[index] = transform.lossyScale;
                else
                    scl[index] = transform.localScale;

                if (moved)
                    StartMoving();
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Sets the position of the anchor point at the given index.
        /// </summary>
        /// <param name="index">Which element should be set.</param>
        /// <param name="position">The position of the anchor point.</param>
        public void SetAnchorPointPosition(int index, Vector3 position)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                pos[index] = position;

                if (moved)
                    StartMoving();
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Sets the rotation of the anchor point at the given index.
        /// </summary>
        /// <param name="index">Which element should be set.</param>
        /// <param name="rotation">The rotation of the anchor point.</param>
        public void SetAnchorPointRotation(int index, Vector3 rotation)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < rot.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                rot[index] = rotation;

                if (moved)
                    StartMoving();
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Sets the scale of the anchor point at the given index.
        /// </summary>
        /// <param name="index">Which element should be set.</param>
        /// <param name="rotation">The rotation of the anchor point.</param>
        public void SetAnchorPointScale(int index, Vector3 scale)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < rot.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                scl[index] = scale;

                if (moved)
                    StartMoving();
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Inserts the given anchor point at the given index.
        /// </summary>
        /// <param name="index">The index where the anchor point should be inserted.</param>
        /// <param name="anchorPoint">The anchor point to be inserted.</param>
        public void InsertAnchorPoint(int index, AnchorPoint anchorPoint)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                DuplicateAnchorPoint(index);
                SetAnchorPoint(index, anchorPoint);
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Inserts the given position, rotation and scale as an anchor point at the given index.
        /// </summary>
        /// <param name="index">The index where the anchor point should be inserted.</param>
        /// <param name="position">The position of the anchor point to be inserted.</param>
        /// <param name="rotation">The rotation of the anchor point to be inserted.</param>
        /// <param name="scale">The scale of the anchor point to be inserted.</param>
        public void InsertAnchorPoint(int index, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                DuplicateAnchorPoint(index);
                SetAnchorPoint(index, position, rotation, scale);
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Inserts the given position and rotation as an anchor point at the given index. Scale is assumed
        /// </summary>
        /// <param name="index">The index where the anchor point should be inserted.</param>
        /// <param name="position">The position of the anchor point to be inserted.</param>
        /// <param name="rotation">The rotation of the anchor point to be inserted.</param>
        public void InsertAnchorPoint(int index, Vector3 position, Vector3 rotation)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                DuplicateAnchorPoint(index);
                SetAnchorPoint(index, position, rotation);
            }
            else
            {
                throw new System.NullReferenceException("Given index is not in the range of anchor points.");
            }
        }

        /// <summary>
        /// Duplicates the anchor point (position, rotation and scale) at given index.
        /// </summary>
        /// <param name="index">Which element should be duplicated. Does nothing if it is out of bounds.</param>
        public void DuplicateAnchorPoint(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                pos.Insert(index, pos[index]);
                rot.Insert(index, rot[index]);
                scl.Insert(index, scl[index]);
                movableGizmos.Insert(index, movableGizmos[index]);

                if (moved)
                    StartMoving();
            }
        }

        /// <summary>
        /// Removes the anchor point (position, rotation and scale) at given index.
        /// </summary>
        /// <param name="index">Which element should be removed. Does nothing if it is out of bounds.</param>
        public void RemoveAnchorPoint(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                pos.RemoveAt(index);
                rot.RemoveAt(index);
                scl.RemoveAt(index);
                movableGizmos.RemoveAt(index);

                if (moved)
                    StartMoving();
            }
        }

        /// <summary>
        /// Moves the anchor point at the given index up in the list (decreasing its index by one).
        /// </summary>
        /// <param name="index">Which element should be moved. Does nothing if it is out of bounds or 0.</param>
        public void MoveAnchorPointUp(int index)
        {
            AnchorPointVersionCheck();

            if (index > 0 && index < pos.Count)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                Vector3 position = pos[index];
                Vector3 rotation = rot[index];
                Vector3 scale = scl[index];
                bool movableGizmo = movableGizmos[index];
                pos.RemoveAt(index);
                rot.RemoveAt(index);
                scl.RemoveAt(index);
                movableGizmos.RemoveAt(index);
                pos.Insert(index - 1, position);
                rot.Insert(index - 1, rotation);
                scl.Insert(index - 1, scale);
                movableGizmos.Insert(index - 1, movableGizmo);

                if (moved)
                    StartMoving();
            }
        }

        /// <summary>
        /// Moves the anchor point at the given index down in the list (increasing its index by one).
        /// </summary>
        /// <param name="index">Which element should be moved. Does nothing if it is out of bounds or the last element in the list.</param>
        public void MoveAnchorPointDown(int index)
        {
            AnchorPointVersionCheck();

            if (index >= 0 && index < pos.Count - 1)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                Vector3 position = pos[index];
                Vector3 rotation = rot[index];
                Vector3 scale = scl[index];
                bool movableGizmo = MovableGizmos[index];
                pos.RemoveAt(index);
                rot.RemoveAt(index);
                scl.RemoveAt(index);
                MovableGizmos.RemoveAt(index);
                if (index == pos.Count - 1)
                {
                    pos.Add(position);
                    rot.Add(rotation);
                    scl.Add(scale);
                    movableGizmos.Add(movableGizmo);
                }
                else
                {
                    pos.Insert(index + 1, position);
                    rot.Insert(index + 1, rotation);
                    scl.Insert(index + 1, scale);
                    movableGizmos.Insert(index + 1, movableGizmo);
                }

                if (moved)
                    StartMoving();
            }
        }

        /// <summary>
        /// Moves the anchor point from index 'from' to index 'to'.
        /// </summary>
        /// <param name="from">Index of the anchor point that will be moved.</param>
        /// <param name="to">Index where the anchor point will be moved.</param>
        public void MoveAnchorPoint(int from, int to)
        {
            AnchorPointVersionCheck();

            if (from >= 0 && from < pos.Count && to >= 0 && to < pos.Count && from != to)
            {
                bool moved = false;
                if (moving && !instantRuntimeChanges)
                    newChanges = true;
                if (moving && instantRuntimeChanges)
                {
                    moved = true;
                    StopMoving();
                }

                Vector3 position = pos[from];
                Vector3 rotation = rot[from];
                Vector3 scale = scl[from];
                bool movableGizmo = movableGizmos[from];
                pos.RemoveAt(from);
                rot.RemoveAt(from);
                scl.RemoveAt(from);
                movableGizmos.RemoveAt(from);
                if (to == pos.Count - 1)
                {
                    pos.Add(position);
                    rot.Add(rotation);
                    scl.Add(scale);
                    movableGizmos.Add(movableGizmo);
                }
                else
                {
                    pos.Insert(to, position);
                    rot.Insert(to, rotation);
                    scl.Insert(to, scale);
                    movableGizmos.Insert(to, movableGizmo);
                }

                if (moved)
                    StartMoving();
            }
        }

        /// <summary>
        /// Get the progress of the current loop. 0 if not moving.
        /// </summary>
        /// <returns>progress as a float from 0 to 1.</returns>
        public float GetProgress()
        {
            return loopProgress;
        }

        /// <summary>
        /// Pauses the movement. Does nothing if the object is not moving.
        /// </summary>
        public void Pause()
        {
            if (moving)
                isPaused = true;
        }

        /// <summary>
        /// Resumes the object from a pause. Does nothing if IsPaused is false.
        /// </summary>
        public void Resume()
        {
            if (isPaused)
                isPaused = false;
        }


        /// <summary>
        /// Starts moving the object along the specified curve.
        /// </summary>
        public void StartMoving()
        {
            //starting the movement
            if (!moving)
            {
                StartCoroutine(Move());
            }
        }

        /// <summary>
        /// Stops moving the object. Starting the movement again will begin from the starting point of the curve.
        /// </summary>
        public void StopMoving()
        {
            StopAllCoroutines();

            transform.localPosition = origLocalPos;
            transform.localRotation = Quaternion.Euler(origLocalRot);
            transform.localScale = origLocalScl;

            moving = false;
        }

        /// <summary>
        /// Creates as many gameobjects in the current scene as there are steps in the path.
        /// <para>The gameobjects' transforms contain the position, rotation and scale information of that step.</para>
        /// </summary>
        /// <returns>The parent gameobject that contains the path.</returns>
        public GameObject ExportCurve()
        {
            if (moving)
                return null;
            GameObject curve = new GameObject(name + " curve");
            curve.transform.position = new Vector3(0, 0, 0);
            curve.transform.rotation = Quaternion.identity;
            curve.transform.localScale = new Vector3(1, 1, 1);

            List<Vector3> curvePos = CopyList(pos);
            List<Vector3> curveRot = CopyList(rot);
            List<Vector3> curveScl = CopyList(scl);
            //converting to world space. this functionality cannot be used while moving
            if (anchorPointSpace == AutoMoverAnchorPointSpace.child)
            {
                for (int i = 0; i < curvePos.Count; ++i)
                {
                    curvePos[i] = transform.TransformPoint(curvePos[i]);
                    curveRot[i] = (transform.rotation * Quaternion.Euler(curveRot[i])).eulerAngles;
                    curveScl[i] = new Vector3(transform.lossyScale.x * curveScl[i].x, transform.lossyScale.y * curveScl[i].y, transform.lossyScale.z * curveScl[i].z);
                }
            }
            else if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
            {
                if (transform.parent != null)
                {
                    for (int i = 0; i < curvePos.Count; ++i)
                    {
                        curvePos[i] = transform.parent.TransformPoint(curvePos[i]);
                        curveRot[i] = (transform.parent.rotation * Quaternion.Euler(curveRot[i])).eulerAngles;
                        curveScl[i] = new Vector3(transform.parent.lossyScale.x * curveScl[i].x, transform.parent.lossyScale.y * curveScl[i].y, transform.parent.lossyScale.z * curveScl[i].z);
                    }
                }
            }
            Vector3[] precomps = PrecomputedPath(curvePos, curveRot, curveScl, loopingStyle, rotationMethod, curveStyle, curveWeight, faceForward, dynamicUpVector, steps);

            Vector3[] precompPos = new Vector3[precomps.Length / 3];
            Vector3[] precompRot = new Vector3[precomps.Length / 3];
            Vector3[] precompScl = new Vector3[precomps.Length / 3];
            for (int i = 0; i < precomps.Length; ++i)
            {
                if (i < precomps.Length / 3)
                    precompPos[i] = precomps[i];
                else if (i < precomps.Length / 3 + precomps.Length / 3)
                {
                    precompRot[i - precomps.Length / 3] = precomps[i];
                }
                else
                {
                    precompScl[i - precomps.Length / 3 - precomps.Length / 3] = precomps[i];
                }
            }


            Vector3 averagePos = new Vector3(0, 0, 0);
            for (int i = 0; i < precompPos.Length; ++i)
            {
                averagePos += precompPos[i];
            }
            averagePos = averagePos / precompPos.Length;
            curve.transform.position = averagePos;

            MeshRenderer mr = null;
            if (GetComponent<MeshRenderer>() != null)
                mr = GetComponent<MeshRenderer>();
            SpriteRenderer sr = null;
            if (GetComponent<SpriteRenderer>() != null)
                sr = GetComponent<SpriteRenderer>();


            for (int i = 0; i < precompPos.Length; ++i)
            {
                GameObject point = null;
                if (populateWithMesh)
                {
                    point = Instantiate(gameObject);
                    DestroyImmediate(point.GetComponent<AutoMover>());
                }
                else
                {
                    point = new GameObject(name + " curve point " + (i + 1));
                }

                point.transform.parent = curve.transform;
                point.transform.position = precompPos[i];
                point.transform.rotation = Quaternion.Euler(precompRot[i]);
                point.transform.localScale = precompScl[i];

            }

            return curve;
        }

        /// <summary>
        /// Makes sure that the position, rotation etc. lists are coherent and there are no missing or extra values.
        /// <para>This is executed at the start of the movement.</para>
        /// </summary>
        public bool AnchorPointVersionCheck()
        {
            bool adjusted = false;

            if (pos == null)
            {
                pos = new List<Vector3>();
                adjusted = true;
            }
            if (rot == null)
            {
                rot = new List<Vector3>();
                adjusted = true;
            }
            if (scl == null)
            {
                scl = new List<Vector3>();
                adjusted = true;
            }
            if (movableGizmos == null)
            {
                movableGizmos = new List<bool>();
                adjusted = true;
            }

            //checking if the lists are equal in length
            for (int i = 0; i < pos.Count; ++i)
            {
                if (rot.Count <= i)
                {
                    if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                    {
                        rot.Add(transform.rotation.eulerAngles);
                        adjusted = true;
                    }
                    else if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
                    {
                        rot.Add(transform.localRotation.eulerAngles);
                        adjusted = true;
                    }
                    else
                    {
                        rot.Add(new Vector3(0, 0, 0));
                        adjusted = true;
                    }
                }
                if (scl.Count <= i)
                {
                    if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                    {
                        scl.Add(transform.lossyScale);
                        adjusted = true;
                    }
                    else if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
                    {
                        scl.Add(transform.localScale);
                        adjusted = true;
                    }
                    else
                    {
                        scl.Add(new Vector3(1, 1, 1));
                        adjusted = true;
                    }
                }

                if (movableGizmos.Count <= i)
                {
                    movableGizmos.Add(true);
                    adjusted = true;
                }
            }

            if (rot.Count > pos.Count)
            {
                rot.RemoveRange(pos.Count, rot.Count - pos.Count);
                adjusted = true;
            }

            if (scl.Count > pos.Count)
            {
                scl.RemoveRange(pos.Count, scl.Count - pos.Count);
                adjusted = true;
            }

            if (movableGizmos.Count > pos.Count)
            {
                movableGizmos.RemoveRange(pos.Count, movableGizmos.Count - pos.Count);
                adjusted = true;
            }

            return adjusted;
        }

        void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            // Draw a sphere on all anchor point positions
            Color startColor = Color.blue;
            Color endColor = Color.red;

            float gizmoSize = 0.1f;

            if (pos == null)
                return;

            for (int i = 0; i < pos.Count; ++i)
            {
                if (pos.Count > 1)
                    Gizmos.color = startColor * (1 - (i / (pos.Count - 1))) + endColor * i / (pos.Count - 1);
                else
                    Gizmos.color = startColor;

                if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
                {
                    Gizmos.DrawSphere(TransformPoint(transform.parent, pos[i]), gizmoSize);
                }
                else if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                {
                    Gizmos.DrawSphere(pos[i], gizmoSize);
                }
                else
                {

                    if (moving && origChildPosLocal.Count == pos.Count)
                    {
                        Gizmos.DrawSphere(TransformPoint(transform.parent, origChildPosLocal[i]), gizmoSize);
                    }
                    else
                        Gizmos.DrawSphere(TransformPoint(transform, pos[i]), gizmoSize);
                }

            }

            Vector3[] path = new Vector3[0];

            if (anchorPointSpace == AutoMoverAnchorPointSpace.child && moving && origChildPosLocal.Count == pos.Count)
            {
                path = PrecomputedPath(origChildPosLocal, null, null, loopingStyle, AutoMoverRotationMethod.linear, curveStyle, curveWeight, false, false, Mathf.Min(steps));
            }
            else
            {
                path = PrecomputedPath(pos, null, null, loopingStyle, AutoMoverRotationMethod.linear, curveStyle, curveWeight, false, false, Mathf.Min(steps));
            }
            for (int i = 0; i < path.Length - 1; ++i)
            {
                if (pos.Count > 1)
                    Gizmos.color = startColor * (1 - (i / (path.Length - 1))) + endColor * i / (path.Length - 1);
                else
                    Gizmos.color = startColor;

                if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
                {
                    Gizmos.DrawLine(TransformPoint(transform.parent, path[i]), TransformPoint(transform.parent, path[i + 1]));
                }
                else if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
                else
                {
                    if (anchorPointSpace == AutoMoverAnchorPointSpace.child && moving && origChildPosLocal.Count == pos.Count)
                    {
                        Gizmos.DrawLine(TransformPoint(transform.parent, path[i]), TransformPoint(transform.parent, path[i + 1]));
                    }
                    else
                    {
                        Gizmos.DrawLine(TransformPoint(transform, path[i]), TransformPoint(transform, path[i + 1]));
                    }
                }
            }
        }

        private static Vector3 GetRotationDifferenceMagnitude(Vector3 prev, Vector3 current)
        {
            Vector3 diff = current - prev;
            Vector3 magnitude = new Vector3(0, 0, 0);
            if (diff.x > 180)
            {
                magnitude.x = 1 + Mathf.FloorToInt((diff.x - 180f) / 360f);
            }
            else if (diff.x <= -180)
            {
                magnitude.x = -(1 + Mathf.FloorToInt((diff.x + 180f) / -360f));
            }
            if (diff.y > 180)
            {
                magnitude.y = 1 + Mathf.FloorToInt((diff.y - 180f) / 360f);
            }
            else if (diff.y <= -180)
            {
                magnitude.y = -(1 + Mathf.FloorToInt((diff.y + 180f) / -360f));
            }
            if (diff.z > 180)
            {
                magnitude.z = 1 + Mathf.FloorToInt((diff.z - 180f) / 360f);
            }
            else if (diff.z <= -180)
            {
                magnitude.z = -(1 + Mathf.FloorToInt((diff.z + 180f) / -360f));
            }

            return magnitude;
        }

        private static List<T> CopyList<T>(List<T> input)
        {
            List<T> output = new List<T>();
            for (int i = 0; i < input.Count; ++i)
            {
                output.Add(input[i]);
            }

            return output;
        }

        private static Vector3 TransformPoint(Transform transform, Vector3 point)
        {
            if (transform != null)
            {
                return transform.TransformPoint(point);
            }
            else
            {
                return point;
            }
        }

        private static List<Vector3> PathThroughPoints(List<Vector3> origPos, List<Vector3> pos, float curveWeight, bool loop, float precision = 0.01f, int maxIterations = 50)
        {
            float stepSize = 1f;
            bool newIteration = false;
            List<Vector3> newPos = CopyList(pos);
            int lastPoint = newPos.Count - 1;
            int firstPoint = 1;
            if (loop)
            {
                lastPoint = newPos.Count;
                firstPoint = 0;
            }

            for (int i = firstPoint; i < lastPoint; ++i)
            {
                Vector3 prevPoint;
                if (i == 0)
                    prevPoint = newPos[newPos.Count - 2];
                else
                    prevPoint = newPos[i - 1];
                Vector3 nextPoint;
                if (i == newPos.Count - 1)
                    nextPoint = newPos[1];
                else
                    nextPoint = newPos[i + 1];

                //checking if the point is already close enough
                Vector3 controlPoint1 = curveWeight * newPos[i] + (1 - curveWeight) * prevPoint;
                Vector3 controlPoint2 = curveWeight * newPos[i] + (1 - curveWeight) * nextPoint;
                Vector3 controlPointMid = controlPoint1 + controlPoint2;
                controlPointMid = controlPointMid / 2f;

                if (Vector3.Distance(controlPointMid, origPos[i]) > precision)
                {
                    newIteration = true;
                    Vector3 directionVec = -controlPointMid + origPos[i];

                    newPos[i] += directionVec * stepSize;
                }
            }

            if (newIteration && maxIterations > 1)
                return PathThroughPoints(origPos, newPos, curveWeight, loop, precision, maxIterations - 1);

            return newPos;
        }

        //essentially a copy of the moving function, but just outputs the resulting curve in a vector (using constant step size)
        private static Vector3[] PrecomputedPath(List<Vector3> anchors, List<Vector3> anchorsR, List<Vector3> anchorsS, AutoMoverLoopingStyle loopMode, AutoMoverRotationMethod rotMethod, AutoMoverCurve curveType, float curveWeight, bool faceForward, bool dynamicUp, int steps)
        {
            List<Vector3> path = new List<Vector3>();
            List<Vector3> pathR = new List<Vector3>();
            List<Vector3> pathS = new List<Vector3>();

            if (anchors == null || anchors.Count < 2)
            {
                return path.ToArray();
            }

            float step = 0.05f;

            bool looped = false;
            List<Vector3> curvePos = CopyList(anchors);
            List<Vector3> curveRot = null;
            if (anchorsR != null)
                curveRot = CopyList(anchorsR);
            List<Vector3> curveScl = null;
            if (anchorsS != null)
                curveScl = CopyList(anchorsS);

            //coordinate system for "face forward" option
            Vector3 origUp = new Vector3(0, 1f, 0);
            Vector3 up = origUp;
            Vector3 side;
            Vector3 originalRotation = new Vector3(0, 0, 0);
            Vector3 originalDirection = new Vector3(1, 0, 0);
            if (anchorsR != null && faceForward)
            {
                originalRotation = anchorsR[0];
                rotMethod = AutoMoverRotationMethod.spherical;
            }

            //If we are looping, it has to be considered before creating the curve
            if (loopMode == AutoMoverLoopingStyle.loop)
            {
                looped = true;
                curvePos.Add(curvePos[0]);

                if (curveRot != null)
                {
                    curveRot.Add(curveRot[0]);
                    if (rotMethod == AutoMoverRotationMethod.spherical)
                        curveRot[curveRot.Count - 1] -= GetRotationDifferenceMagnitude(curveRot[curveRot.Count - 2], curveRot[curveRot.Count - 1]) * 360f;
                }

                if (curveScl != null)
                {
                    curveScl.Add(curveScl[0]);
                }
            }

            if (curveType == AutoMoverCurve.Linear) // linear
            {
                float totalDist = 0;
                for (int i = 0; i < curvePos.Count - 1; ++i)
                {
                    totalDist += Vector3.Distance(curvePos[i], curvePos[i + 1]);
                }

                step = totalDist / steps;

                if (step > 0 && curvePos.Count > 1)
                {
                    float d = 0;
                    float dToi = 0;
                    for (int i = 0; i < curvePos.Count - 1; ++i)
                    {
                        float segDist = (-curvePos[i] + curvePos[i + 1]).magnitude;
                        float segProg = (d - dToi) / segDist;
                        if (segProg >= 1)
                        {
                            d = segDist + dToi;
                            dToi = d;
                            path.Add(curvePos[i + 1]);
                            if (curveRot != null)
                            {
                                if (faceForward)
                                {
                                    if (path.Count > 1)
                                    {
                                        Vector3 direction = (-path[path.Count - 2] + path[path.Count - 1]).normalized;

                                        if (dynamicUp)
                                        {
                                            side = Vector3.Cross(up, direction);
                                            up = Vector3.Cross(direction, side);
                                        }

                                        pathR.Add((Quaternion.LookRotation(direction, up) * Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)) * Quaternion.Euler(originalRotation)).eulerAngles);

                                        if (pathR.Count == 2)
                                            pathR[0] = pathR[1];
                                    }
                                    else
                                        pathR.Add(originalRotation);
                                }
                                else
                                {
                                    pathR.Add(curveRot[i] * (1 - segProg) + curveRot[i + 1] * segProg);
                                }
                            }
                            if (curveScl != null)
                            {
                                pathS.Add(curveScl[i + 1]);
                            }
                        }
                        else
                        {
                            path.Add(curvePos[i] * (1 - segProg) + curvePos[i + 1] * segProg);
                            if (curveRot != null)
                            {
                                if (faceForward)
                                {
                                    if (path.Count > 1)
                                    {
                                        Vector3 direction = (-path[path.Count - 2] + path[path.Count - 1]).normalized;

                                        if (dynamicUp)
                                        {
                                            side = Vector3.Cross(up, direction);
                                            up = Vector3.Cross(direction, side);
                                        }
                                        pathR.Add((Quaternion.LookRotation(direction, up) * Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)) * Quaternion.Euler(originalRotation)).eulerAngles);

                                        if (pathR.Count == 2)
                                            pathR[0] = pathR[1];
                                    }
                                    else
                                        pathR.Add(originalRotation);
                                }
                                else
                                {
                                    pathR.Add(curveRot[i] * (1 - segProg) + curveRot[i + 1] * segProg);
                                }
                            }
                            if (curveScl != null)
                                pathS.Add(curveScl[i] * (1 - segProg) + curveScl[i + 1] * segProg);
                            i--;
                        }
                        d += step;
                    }

                    //sanitizing the directions
                    if (faceForward)
                    {
                        for (int i = 1; i < pathR.Count; i++)
                        {
                            pathR[i] -= GetRotationDifferenceMagnitude(pathR[i - 1], pathR[i]) * 360f;
                        }
                    }
                }
                else
                {
                    path = CopyList(curvePos);
                    if (curveRot != null)
                        pathR = CopyList(curveRot);
                    if (curveScl != null)
                        pathS = CopyList(curveScl);
                }
            }
            else if (curveType == AutoMoverCurve.Curve) // curve
            {
                float totalDist = 0;
                for (int i = 0; i < curvePos.Count - 1; ++i)
                {
                    totalDist += Vector3.Distance(curvePos[i], curvePos[i + 1]);
                }

                step = totalDist / steps;

                if (step > 0 && curvePos.Count > 1)
                {
                    for (float d = 0; d <= totalDist; d += step)
                    {
                        if (d > totalDist)
                            d = totalDist;

                        float p = d / totalDist;
                        path.Add(BezierPoint(curvePos.ToArray(), p));
                        if (curveRot != null)
                        {

                            if (faceForward)
                            {
                                if (path.Count > 1)
                                {
                                    Vector3 direction = (-path[path.Count - 2] + path[path.Count - 1]).normalized;

                                    if (dynamicUp)
                                    {
                                        side = Vector3.Cross(up, direction);
                                        up = Vector3.Cross(direction, side);
                                    }
                                    pathR.Add((Quaternion.LookRotation(direction, up) * Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)) * Quaternion.Euler(originalRotation)).eulerAngles);

                                    if (pathR.Count == 2)
                                        pathR[0] = pathR[1];
                                }
                                else
                                    pathR.Add(originalRotation);
                            }
                            else
                            {
                                pathR.Add(BezierPoint(curveRot.ToArray(), p));
                            }
                        }
                        if (curveScl != null)
                            pathS.Add(BezierPoint(curveScl.ToArray(), p));
                    }

                    //sanitizing the directions
                    if (faceForward)
                    {
                        for (int i = 1; i < pathR.Count; i++)
                        {
                            pathR[i] -= GetRotationDifferenceMagnitude(pathR[i - 1], pathR[i]) * 360f;
                        }
                    }
                }
                else
                {
                    path = CopyList(curvePos);
                    if (curveRot != null)
                        pathR = CopyList(curveRot);
                    if (curveScl != null)
                        pathS = CopyList(curveScl);
                }
            }
            else if (curveType == AutoMoverCurve.Spline || curveType == AutoMoverCurve.SplineThroughPoints) //spline
            {
                if (curveType == AutoMoverCurve.SplineThroughPoints)
                {
                    curvePos = PathThroughPoints(curvePos, curvePos, curveWeight, looped);
                    if (curveRot != null)
                        curveRot = PathThroughPoints(curveRot, curveRot, curveWeight, looped);
                    if (curveScl != null)
                        curveScl = PathThroughPoints(curveScl, curveScl, curveWeight, looped);
                }

                float totalDist = 0;
                for (int i = 0; i < curvePos.Count - 1; ++i)
                {
                    totalDist += Vector3.Distance(curvePos[i], curvePos[i + 1]);
                }

                step = totalDist / steps;

                if (step > 0 && curvePos.Count > 1)
                {
                    //form anchor points
                    Vector3[] anchorPoints = curvePos.ToArray();
                    Vector3[] anchorPointsR = null;
                    if (curveRot != null)
                        anchorPointsR = curveRot.ToArray();
                    Vector3[] anchorPointsS = null;
                    if (curveScl != null)
                        anchorPointsS = curveScl.ToArray();

                    //form control points
                    Vector3[] controlPoints = new Vector3[2 * (curvePos.Count - 1)];
                    Vector3[] controlPointsR = null;
                    if (curveRot != null)
                        controlPointsR = new Vector3[2 * (curveRot.Count - 1)];
                    Vector3[] controlPointsS = null;
                    if (curveScl != null)
                        controlPointsS = new Vector3[2 * (curveScl.Count - 1)];

                    for (int i = 0; i < anchorPoints.Length - 1; ++i)
                    {
                        controlPoints[i * 2] = anchorPoints[i] * curveWeight + anchorPoints[i + 1] * (1 - curveWeight);
                        controlPoints[i * 2 + 1] = anchorPoints[i] * (1 - curveWeight) + anchorPoints[i + 1] * curveWeight;

                        if (curveRot != null)
                        {
                            controlPointsR[i * 2] = anchorPointsR[i] * curveWeight + anchorPointsR[i + 1] * (1 - curveWeight);
                            controlPointsR[i * 2 + 1] = anchorPointsR[i] * (1 - curveWeight) + anchorPointsR[i + 1] * curveWeight;
                        }

                        if (curveScl != null)
                        {
                            controlPointsS[i * 2] = anchorPointsS[i] * curveWeight + anchorPointsS[i + 1] * (1 - curveWeight);
                            controlPointsS[i * 2 + 1] = anchorPointsS[i] * (1 - curveWeight) + anchorPointsS[i + 1] * curveWeight;
                        }
                    }

                    //form the endpoints of all curves
                    Vector3[] endPoints = new Vector3[anchorPoints.Length];
                    Vector3[] endPointsR = null;
                    if (curveRot != null)
                        endPointsR = new Vector3[anchorPointsR.Length];
                    Vector3[] endPointsS = null;
                    if (curveScl != null)
                        endPointsS = new Vector3[anchorPointsS.Length];

                    for (int i = 1; i < controlPoints.Length - 2; i += 2)
                    {
                        endPoints[(i + 1) / 2] = 0.5f * controlPoints[i] + 0.5f * controlPoints[i + 1];

                        if (curveRot != null)
                            endPointsR[(i + 1) / 2] = 0.5f * controlPointsR[i] + 0.5f * controlPointsR[i + 1];

                        if (curveScl != null)
                            endPointsS[(i + 1) / 2] = 0.5f * controlPointsS[i] + 0.5f * controlPointsS[i + 1];
                    }

                    if (!looped)
                    {
                        endPoints[0] = anchorPoints[0];
                        endPoints[endPoints.Length - 1] = anchorPoints[anchorPoints.Length - 1];

                        if (curveRot != null)
                        {
                            endPointsR[0] = anchorPointsR[0];
                            endPointsR[endPointsR.Length - 1] = anchorPointsR[anchorPointsR.Length - 1];
                        }

                        if (curveScl != null)
                        {
                            endPointsS[0] = anchorPointsS[0];
                            endPointsS[endPointsS.Length - 1] = anchorPointsS[anchorPointsS.Length - 1];
                        }
                    }
                    else
                    {
                        //the first start point and final end point is the middlepoint between the first and last control points
                        Vector3 middlePoint = controlPoints[0] * 0.5f + controlPoints[controlPoints.Length - 1] * 0.5f;
                        endPoints[0] = middlePoint;
                        endPoints[endPoints.Length - 1] = middlePoint;

                        if (curveRot != null)
                        {
                            Vector3 tempControlPointR = controlPointsR[controlPointsR.Length - 1];
                            if (rotMethod == AutoMoverRotationMethod.spherical)
                                tempControlPointR -= GetRotationDifferenceMagnitude(controlPointsR[0], tempControlPointR) * 360;
                            Vector3 middlePointR = controlPointsR[0] * 0.5f + tempControlPointR * 0.5f;
                            endPointsR[0] = middlePointR;
                            endPointsR[endPointsR.Length - 1] = middlePointR;
                            if (rotMethod == AutoMoverRotationMethod.spherical)
                                endPointsR[endPointsR.Length - 1] -= GetRotationDifferenceMagnitude(endPointsR[endPointsR.Length - 2], endPointsR[endPointsR.Length - 1]) * 360f;
                        }

                        if (curveScl != null)
                        {
                            Vector3 tempControlPointS = controlPointsS[controlPointsS.Length - 1];
                            Vector3 middlePointS = controlPointsS[0] * 0.5f + tempControlPointS * 0.5f;
                            endPointsS[0] = middlePointS;
                            endPointsS[endPointsS.Length - 1] = middlePointS;
                        }
                    }

                    //recalculating step distance based on endpoints
                    totalDist = 0;
                    for (int i = 0; i < endPoints.Length - 1; ++i)
                    {
                        totalDist += Vector3.Distance(endPoints[i], endPoints[i + 1]);
                    }

                    step = totalDist / steps;

                    //make a bezier curve from anchor point to the next middle point ("through" the control points on the way)
                    //and repeat for all the curves
                    for (int c = 0; c < endPoints.Length - 1; ++c)
                    {
                        float dist = Vector3.Distance(curvePos[c], curvePos[c + 1]);
                        Vector3[] curvePoints = new Vector3[4];
                        curvePoints[0] = endPoints[c];
                        curvePoints[1] = controlPoints[c * 2];
                        curvePoints[2] = controlPoints[(c * 2) + 1];
                        curvePoints[3] = endPoints[c + 1];

                        Vector3[] curvePointsR = null;
                        if (curveRot != null)
                        {
                            curvePointsR = new Vector3[4];
                            curvePointsR[0] = endPointsR[c];
                            curvePointsR[1] = controlPointsR[c * 2];
                            curvePointsR[2] = controlPointsR[(c * 2) + 1];
                            curvePointsR[3] = endPointsR[c + 1];
                        }

                        Vector3[] curvePointsS = null;
                        if (curveScl != null)
                        {
                            curvePointsS = new Vector3[4];
                            curvePointsS[0] = endPointsS[c];
                            curvePointsS[1] = controlPointsS[c * 2];
                            curvePointsS[2] = controlPointsS[(c * 2) + 1];
                            curvePointsS[3] = endPointsS[c + 1];
                        }

                        for (float d = 0; d < dist; d += step)
                        {
                            float p = d / dist;
                            path.Add(BezierPoint(curvePoints, p));

                            if (curveRot != null)
                            {
                                if (faceForward)
                                {
                                    if (path.Count > 1)
                                    {
                                        Vector3 direction = (-path[path.Count - 2] + path[path.Count - 1]).normalized;

                                        if (dynamicUp)
                                        {
                                            side = Vector3.Cross(up, direction);
                                            up = Vector3.Cross(direction, side);
                                        }
                                        pathR.Add((Quaternion.LookRotation(direction, up) * Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)) * Quaternion.Euler(originalRotation)).eulerAngles);

                                        if (pathR.Count == 2)
                                            pathR[0] = pathR[1];
                                    }
                                    else
                                        pathR.Add(Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)).eulerAngles);
                                }
                                else
                                {
                                    pathR.Add(BezierPoint(curvePointsR, p));
                                }
                            }

                            if (curveScl != null)
                                pathS.Add(BezierPoint(curvePointsS, p));
                        }

                        if (c == endPoints.Length - 2)
                        {
                            path.Add(BezierPoint(curvePoints, 1));

                            if (curveRot != null)
                            {
                                if (faceForward)
                                {
                                    if (path.Count > 1)
                                    {
                                        Vector3 direction = (-path[path.Count - 2] + path[path.Count - 1]).normalized;

                                        if (dynamicUp)
                                        {
                                            side = Vector3.Cross(up, direction);
                                            up = Vector3.Cross(direction, side);
                                        }
                                        pathR.Add((Quaternion.LookRotation(direction, up) * Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)) * Quaternion.Euler(originalRotation)).eulerAngles);

                                        if (pathR.Count == 2)
                                            pathR[0] = pathR[1];
                                    }
                                    else
                                        pathR.Add(Quaternion.Inverse(Quaternion.LookRotation(originalDirection, origUp)).eulerAngles);
                                }
                                else
                                {
                                    pathR.Add(BezierPoint(curvePointsR, 1));
                                }
                            }

                            if (curveScl != null)
                                pathS.Add(BezierPoint(curvePointsS, 1));
                        }

                        //sanitizing the directions
                        if (faceForward)
                        {
                            for (int i = 1; i < pathR.Count; i++)
                            {
                                pathR[i] -= GetRotationDifferenceMagnitude(pathR[i - 1], pathR[i]) * 360f;
                            }
                        }
                    }

                }
                else
                {
                    path = CopyList(curvePos);
                    if (curveRot != null)
                        pathR = CopyList(curveRot);
                    if (curveScl != null)
                        pathS = CopyList(curveScl);
                }
            }

            if (anchorsR != null)
            {
                path.AddRange(pathR);
            }

            if (anchorsS != null)
            {
                path.AddRange(pathS);
            }

            return path.ToArray();
        }

        private Vector3 NewNoise(AutoMoverTarget target, Vector3 current, float startTime = 0)
        {
            float x = 0, y = 0, z = 0;
            AutoMoverNoiseType noiseType;
            Vector3 maxAmplitude;
            Vector3 frequency;
            Vector3 offset;
            if (target == AutoMoverTarget.position)
            {
                noiseType = positionNoiseType;
                maxAmplitude = positionNoiseAmplitude;
                frequency = positionNoiseFrequency;
                offset = positionSineOffset;
            }
            else if (target == AutoMoverTarget.rotation)
            {
                noiseType = rotationNoiseType;
                maxAmplitude = rotationNoiseAmplitude;
                frequency = rotationNoiseFrequency;
                if (noiseType != AutoMoverNoiseType.sine)
                    frequency *= 180f;
                offset = rotationSineOffset;
            }
            else
            {
                noiseType = scaleNoiseType;
                maxAmplitude = scaleNoiseAmplitude;
                frequency = scaleNoiseFrequency;
                offset = scaleSineOffset;
            }

            if (noiseType == AutoMoverNoiseType.random || noiseType == AutoMoverNoiseType.smoothRandom)
            {
                Vector3 targetVector = new Vector3(0, 0, 0);
                Vector3[] prevTargetVectors = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
                Vector3 progress = new Vector3(0, 0, 0);

                //possibly generate new target point
                if (target == AutoMoverTarget.position)
                {
                    //x
                    if (posNoiseTarget.x == current.x || posSmoothNoiseProgress.x >= 1)
                    {
                        posSmoothNoiseProgress.x = 0;
                        prevPosNoiseTargets[0].x = prevPosNoiseTargets[1].x;
                        prevPosNoiseTargets[1].x = prevPosNoiseTargets[2].x;
                        prevPosNoiseTargets[2].x = posNoiseTarget.x;
                        posNoiseTarget.x = Random.Range(-maxAmplitude.x, maxAmplitude.x);
                    }
                    //y
                    if (posNoiseTarget.y == current.y || posSmoothNoiseProgress.y >= 1)
                    {
                        posSmoothNoiseProgress.y = 0;
                        prevPosNoiseTargets[0].y = prevPosNoiseTargets[1].y;
                        prevPosNoiseTargets[1].y = prevPosNoiseTargets[2].y;
                        prevPosNoiseTargets[2].y = posNoiseTarget.y;
                        posNoiseTarget.y = Random.Range(-maxAmplitude.y, maxAmplitude.y);
                    }
                    //z
                    if (posNoiseTarget.z == current.z || posSmoothNoiseProgress.z >= 1)
                    {
                        posSmoothNoiseProgress.z = 0;
                        prevPosNoiseTargets[0].z = prevPosNoiseTargets[1].z;
                        prevPosNoiseTargets[1].z = prevPosNoiseTargets[2].z;
                        prevPosNoiseTargets[2].z = posNoiseTarget.z;
                        posNoiseTarget.z = Random.Range(-maxAmplitude.z, maxAmplitude.z);
                    }
                    targetVector = posNoiseTarget;
                    prevTargetVectors = prevPosNoiseTargets;
                    posSmoothNoiseProgress.x += Time.deltaTime * frequency.x;
                    posSmoothNoiseProgress.y += Time.deltaTime * frequency.y;
                    posSmoothNoiseProgress.z += Time.deltaTime * frequency.z;
                    progress.x = posSmoothNoiseProgress.x;
                    progress.y = posSmoothNoiseProgress.y;
                    progress.z = posSmoothNoiseProgress.z;
                }
                else if (target == AutoMoverTarget.rotation)
                {
                    //x
                    if (rotNoiseTarget.x == current.x || rotSmoothNoiseProgress.x >= 1)
                    {
                        rotSmoothNoiseProgress.x = 0;
                        prevRotNoiseTargets[0].x = prevRotNoiseTargets[1].x;
                        prevRotNoiseTargets[1].x = prevRotNoiseTargets[2].x;
                        prevRotNoiseTargets[2].x = rotNoiseTarget.x;
                        rotNoiseTarget.x = Random.Range(-maxAmplitude.x, maxAmplitude.x);
                    }
                    //y
                    if (rotNoiseTarget.y == current.y || rotSmoothNoiseProgress.y >= 1)
                    {
                        rotSmoothNoiseProgress.y = 0;
                        prevRotNoiseTargets[0].y = prevRotNoiseTargets[1].y;
                        prevRotNoiseTargets[1].y = prevRotNoiseTargets[2].y;
                        prevRotNoiseTargets[2].y = rotNoiseTarget.y;
                        rotNoiseTarget.y = Random.Range(-maxAmplitude.y, maxAmplitude.y);
                    }
                    //z
                    if (rotNoiseTarget.z == current.z || rotSmoothNoiseProgress.z >= 1)
                    {
                        rotSmoothNoiseProgress.z = 0;
                        prevRotNoiseTargets[0].z = prevRotNoiseTargets[1].z;
                        prevRotNoiseTargets[1].z = prevRotNoiseTargets[2].z;
                        prevRotNoiseTargets[2].z = rotNoiseTarget.z;
                        rotNoiseTarget.z = Random.Range(-maxAmplitude.z, maxAmplitude.z);
                    }
                    targetVector = rotNoiseTarget;
                    prevTargetVectors = prevRotNoiseTargets;
                    rotSmoothNoiseProgress.x += Time.deltaTime * frequency.x / 180f;
                    rotSmoothNoiseProgress.y += Time.deltaTime * frequency.y / 180f;
                    rotSmoothNoiseProgress.z += Time.deltaTime * frequency.z / 180f;
                    progress.x = rotSmoothNoiseProgress.x;
                    progress.y = rotSmoothNoiseProgress.y;
                    progress.z = rotSmoothNoiseProgress.z;
                }
                else
                {
                    //x
                    if (scaleNoiseTarget.x == current.x || sclSmoothNoiseProgress.x >= 1)
                    {
                        sclSmoothNoiseProgress.x = 0;
                        prevScaleNoiseTargets[0].x = prevScaleNoiseTargets[1].x;
                        prevScaleNoiseTargets[1].x = prevScaleNoiseTargets[2].x;
                        prevScaleNoiseTargets[2].x = scaleNoiseTarget.x;
                        scaleNoiseTarget.x = Random.Range(-maxAmplitude.x, maxAmplitude.x);
                    }
                    //y
                    if (scaleNoiseTarget.y == current.y || sclSmoothNoiseProgress.y >= 1)
                    {
                        sclSmoothNoiseProgress.y = 0;
                        prevScaleNoiseTargets[0].y = prevScaleNoiseTargets[1].y;
                        prevScaleNoiseTargets[1].y = prevScaleNoiseTargets[2].y;
                        prevScaleNoiseTargets[2].y = scaleNoiseTarget.y;
                        scaleNoiseTarget.y = Random.Range(-maxAmplitude.y, maxAmplitude.y);
                    }
                    //z
                    if (scaleNoiseTarget.z == current.z || sclSmoothNoiseProgress.z >= 1)
                    {
                        sclSmoothNoiseProgress.z = 0;
                        prevScaleNoiseTargets[0].z = prevScaleNoiseTargets[1].z;
                        prevScaleNoiseTargets[1].z = prevScaleNoiseTargets[2].z;
                        prevScaleNoiseTargets[2].z = scaleNoiseTarget.z;
                        scaleNoiseTarget.z = Random.Range(-maxAmplitude.z, maxAmplitude.z);
                    }
                    targetVector = scaleNoiseTarget;
                    prevTargetVectors = prevScaleNoiseTargets;
                    sclSmoothNoiseProgress.x += Time.deltaTime * frequency.x;
                    sclSmoothNoiseProgress.y += Time.deltaTime * frequency.y;
                    sclSmoothNoiseProgress.z += Time.deltaTime * frequency.z;
                    progress.x = sclSmoothNoiseProgress.x;
                    progress.y = sclSmoothNoiseProgress.y;
                    progress.z = sclSmoothNoiseProgress.z;
                }

                if (noiseType == AutoMoverNoiseType.random)
                {
                    x = Mathf.MoveTowards(current.x, targetVector.x, Time.deltaTime * frequency.x);
                    y = Mathf.MoveTowards(current.y, targetVector.y, Time.deltaTime * frequency.y);
                    z = Mathf.MoveTowards(current.z, targetVector.z, Time.deltaTime * frequency.z);
                }
                else
                {
                    bool startX = prevTargetVectors[0].x == 0;
                    bool startY = prevTargetVectors[0].y == 0;
                    bool startZ = prevTargetVectors[0].z == 0;
                    float nextPointX = OneDimensionalBezierNoise(new float[4] { prevTargetVectors[0].x, prevTargetVectors[1].x, prevTargetVectors[2].x, targetVector.x }, progress.x, startX);
                    float nextPointY = OneDimensionalBezierNoise(new float[4] { prevTargetVectors[0].y, prevTargetVectors[1].y, prevTargetVectors[2].y, targetVector.y }, progress.y, startY);
                    float nextPointZ = OneDimensionalBezierNoise(new float[4] { prevTargetVectors[0].z, prevTargetVectors[1].z, prevTargetVectors[2].z, targetVector.z }, progress.z, startZ);

                    if (prevTargetVectors[1].x != 0 && prevTargetVectors[2].x != 0 && targetVector.x != 0)
                    {
                        x = nextPointX;
                    }
                    else
                    {
                        if (target == AutoMoverTarget.position)
                            posSmoothNoiseProgress.x = 1;
                        else if (target == AutoMoverTarget.rotation)
                            rotSmoothNoiseProgress.x = 1;
                        else
                            sclSmoothNoiseProgress.x = 1;
                    }
                    if (prevTargetVectors[1].y != 0 && prevTargetVectors[2].y != 0 && targetVector.y != 0)
                    {
                        y = nextPointY;
                    }
                    else
                    {
                        if (target == AutoMoverTarget.position)
                            posSmoothNoiseProgress.y = 1;
                        else if (target == AutoMoverTarget.rotation)
                            rotSmoothNoiseProgress.y = 1;
                        else
                            sclSmoothNoiseProgress.y = 1;
                    }
                    if (prevTargetVectors[1].z != 0 && prevTargetVectors[1].z != 0 && targetVector.z != 0)
                    {
                        z = nextPointZ;
                    }
                    else
                    {
                        if (target == AutoMoverTarget.position)
                            posSmoothNoiseProgress.z = 1;
                        else if (target == AutoMoverTarget.rotation)
                            rotSmoothNoiseProgress.z = 1;
                        else
                            sclSmoothNoiseProgress.z = 1;
                    }


                }
            }
            else if (noiseType == AutoMoverNoiseType.sine)
            {
                x = maxAmplitude.x * Mathf.Sin((Time.time - startTime + offset.x) * frequency.x);
                y = maxAmplitude.y * Mathf.Sin((Time.time - startTime + offset.y) * frequency.y);
                z = maxAmplitude.z * Mathf.Sin((Time.time - startTime + offset.z) * frequency.z);
            }

            return new Vector3(x, y, z);
        }

        private float OneDimensionalBezierNoise(float[] anchors, float progress, bool start)
        {
            float[] controlPoints = new float[(anchors.Length - 1) * 2];
            for (int i = 0; i < anchors.Length - 1; ++i)
            {
                controlPoints[i * 2] = anchors[i] * 0.66666f + anchors[i + 1] * (1 - 0.66666f);
                controlPoints[i * 2 + 1] = anchors[i] * (1 - 0.66666f) + anchors[i + 1] * 0.66666f;
            }
            float[] endPoints = new float[anchors.Length];
            endPoints[0] = anchors[0];
            for (int i = 1; i < controlPoints.Length - 2; i += 2)
            {
                endPoints[(i + 1) / 2] = 0.5f * controlPoints[i] + 0.5f * controlPoints[i + 1];
            }
            endPoints[endPoints.Length - 1] = anchors[anchors.Length - 2];
            float[] curvePoints = new float[4];
            float nextPoint;
            if (start)
            {
                if (progress < 0.5f)
                {
                    curvePoints[0] = endPoints[0];
                    curvePoints[1] = controlPoints[0];
                    curvePoints[2] = controlPoints[1];
                    curvePoints[3] = endPoints[1];
                }
                else
                {
                    curvePoints[0] = endPoints[1];
                    curvePoints[1] = controlPoints[2];
                    curvePoints[2] = controlPoints[3];
                    curvePoints[3] = endPoints[2];
                }
                nextPoint = BezierPoint(curvePoints, (progress * 2) % 1);
            }
            else
            {
                curvePoints[0] = endPoints[1];
                curvePoints[1] = controlPoints[2];
                curvePoints[2] = controlPoints[3];
                curvePoints[3] = endPoints[2];
                nextPoint = BezierPoint(curvePoints, progress);
            }

            return nextPoint;
        }

        private IEnumerator Move()
        {
            //making sure anchor points are coherent and in the format of newest version
            AnchorPointVersionCheck();

            moving = true;
            origLocalPos = transform.localPosition;
            origLocalRot = transform.localRotation.eulerAngles;
            origLocalScl = transform.localScale;

            //saving the pos/rot/scl from the perspective of the original position. used in child anchor point space
            origChildPosLocal = new List<Vector3>();
            origChildRotLocal = new List<Vector3>();
            origChildSclLocal = new List<Vector3>();
            if (anchorPointSpace == AutoMoverAnchorPointSpace.child)
            {
                for (int i = 0; i < pos.Count; ++i)
                {
                    origChildPosLocal.Add(transform.TransformPoint(pos[i]));
                    if (transform.parent != null)
                        origChildPosLocal[i] = transform.parent.InverseTransformPoint(origChildPosLocal[i]);

                    origChildRotLocal.Add((transform.rotation * Quaternion.Euler(rot[i])).eulerAngles);
                    if (transform.parent != null)
                        origChildRotLocal[i] = (Quaternion.Inverse(transform.parent.rotation) * Quaternion.Euler(origChildRotLocal[i])).eulerAngles;

                    origChildSclLocal.Add(new Vector3(transform.lossyScale.x * scl[i].x, transform.lossyScale.y * scl[i].y, transform.lossyScale.z * scl[i].z));
                    if (transform.parent != null)
                        origChildSclLocal[i] = new Vector3(origChildSclLocal[i].x / transform.parent.lossyScale.x, origChildSclLocal[i].y / transform.parent.lossyScale.y, origChildSclLocal[i].z / transform.parent.lossyScale.z);
                }
            }

            Vector3 origGlobalPos = transform.position; //only used in a special case
            Vector3 origGlobalRot = transform.rotation.eulerAngles; //only used in a special case
            Vector3 origGlobalScl = transform.lossyScale; //only used in a special case
            int runs = 0;

            //initializing noise
            Vector3 curveNoise = new Vector3(0, 0, 0);
            Vector3 curveNoiseR = new Vector3(0, 0, 0);
            Vector3 curveNoiseS = new Vector3(0, 0, 0);
            float startTime = Time.time;

            Vector3[] precompPos = null;
            Vector3[] precompRot = null;
            Vector3[] precompScl = null;
            float precompTotalDist = 0;
            float[] precompDistSoFar = null;
            List<float> segmentTimes = new List<float>();
            float totalTime = 0;
            float lastPause = Time.time;

            if (pos != null && pos.Count > 1)
            {
                List<Vector3> curvePos;
                List<Vector3> curveRot;
                List<Vector3> curveScl;

                if (anchorPointSpace == AutoMoverAnchorPointSpace.child)
                {
                    curvePos = CopyList(origChildPosLocal);
                    curveRot = CopyList(origChildRotLocal);
                    curveScl = CopyList(origChildSclLocal);
                }
                else
                {
                    curvePos = CopyList(pos);
                    curveRot = CopyList(rot);
                    curveScl = CopyList(scl);
                }
                //modifying the rotations according to settings (absolute or shortest path)
                if (rotationMethod == AutoMoverRotationMethod.spherical)
                {
                    for (int i = 1; i < curveRot.Count; i++)
                    {
                        curveRot[i] -= GetRotationDifferenceMagnitude(curveRot[i - 1], curveRot[i]) * 360f;
                    }
                }

                Vector3[] precomps = PrecomputedPath(curvePos, curveRot, curveScl, loopingStyle, rotationMethod, curveStyle, curveWeight, faceForward, dynamicUpVector, steps);
                precompPos = new Vector3[precomps.Length / 3];
                precompRot = new Vector3[precomps.Length / 3];
                precompScl = new Vector3[precomps.Length / 3];
                for (int i = 0; i < precomps.Length; ++i)
                {
                    if (i < precomps.Length / 3)
                        precompPos[i] = precomps[i];
                    else if (i < precomps.Length / 3 + precomps.Length / 3)
                    {
                        precompRot[i - precomps.Length / 3] = precomps[i];
                    }
                    else
                    {
                        precompScl[i - precomps.Length / 3 - precomps.Length / 3] = precomps[i];
                    }
                }

                precompDistSoFar = new float[precompPos.Length];
                precompDistSoFar[0] = 0;
                for (int c = 0; c < precompPos.Length - 1; ++c)
                {
                    precompTotalDist += Vector3.Distance(precompPos[c], precompPos[c + 1]);
                    precompDistSoFar[c + 1] = precompTotalDist;
                }


                if (slowOnCurves)
                {
                    //figuring out the necessary speed for each segment when slowing down on curves/corners

                    float lookaheadDistance = Mathf.Min(precompTotalDist, (precompTotalDist / length) * decelerationTime);
                    for (int i = 0; i < precompPos.Length; ++i)
                    {
                        if (i == 0 || (i == 1 && loopingStyle != AutoMoverLoopingStyle.loop))
                        {
                            if (i == 0)
                                segmentTimes.Add(0f);
                            else
                            {

                                segmentTimes.Add((precompDistSoFar[i] - precompDistSoFar[i - 1]) / precompTotalDist);
                            }
                        }
                        else
                        {
                            float angleToTurn = 0;
                            float weight = 1;
                            float coveredDistance = 0;
                            //counting the weighted angle to turn during the lookahead period

                            //checking the lookahead distance backwards to avoid quick speedups (with lower weighting)
                            for (int k = i; k >= 1; --k)
                            {
                                if (k != i)
                                {
                                    Vector3 prevDirection;
                                    if (k > 1)
                                        prevDirection = -precompPos[k - 2] + precompPos[k - 1];
                                    else if (k == 1 && loopingStyle == AutoMoverLoopingStyle.loop)
                                        prevDirection = -precompPos[precompPos.Length - 2] + precompPos[precompPos.Length - 1];
                                    else
                                        continue;

                                    Vector3 nextDirection;
                                    if (k != 1 || loopingStyle == AutoMoverLoopingStyle.loop)
                                        nextDirection = -precompPos[k - 1] + precompPos[k];
                                    else
                                        continue;

                                    angleToTurn += weight * Vector3.Angle(prevDirection, nextDirection);
                                    weight *= 0.8f;
                                    coveredDistance += nextDirection.magnitude;
                                    if (coveredDistance > lookaheadDistance || weight < 0.01f)
                                    {
                                        break;
                                    }
                                }
                                if (loopingStyle == AutoMoverLoopingStyle.loop && k == 1)
                                {
                                    k = precompDistSoFar.Length;
                                }
                            }

                            weight = 1;
                            coveredDistance = 0;


                            //checking forward
                            for (int k = i; k < precompDistSoFar.Length; ++k)
                            {
                                Vector3 prevDirection;
                                if (k > 1)
                                    prevDirection = -precompPos[k - 2] + precompPos[k - 1];
                                else if (k == 1 && loopingStyle == AutoMoverLoopingStyle.loop)
                                    prevDirection = -precompPos[precompPos.Length - 2] + precompPos[precompPos.Length - 1];
                                else
                                    continue;

                                Vector3 nextDirection;
                                if (k != 1 || loopingStyle == AutoMoverLoopingStyle.loop)
                                    nextDirection = -precompPos[k - 1] + precompPos[k];
                                else
                                    continue;

                                angleToTurn += weight * Vector3.Angle(prevDirection, nextDirection);
                                weight *= 0.95f;
                                coveredDistance += nextDirection.magnitude;
                                if (coveredDistance > lookaheadDistance || weight < 0.01f)
                                {
                                    break;
                                }
                                if (loopingStyle == AutoMoverLoopingStyle.loop && k == precompDistSoFar.Length - 1)
                                {
                                    k = 0;
                                }
                            }
                            float segmentDist = (-precompPos[i - 1] + precompPos[i]).magnitude;
                            segmentTimes.Add(segmentDist / precompTotalDist * (1 + angleToTurn / turnRate));
                        }
                        totalTime += segmentTimes[i];
                    }
                }
            }


            //taking a local copy of certain parameters in case they are changed mid-movement and InstantRuntimeChanges is false
            AutoMoverRotationMethod tmpRotationMethod = rotationMethod;
            AutoMoverLoopingStyle tmpLoopingStyle = loopingStyle;
            AutoMoverAnchorPointSpace tmpAnchorPointSpace = anchorPointSpace;
            bool tmpSlowOnCurves = slowOnCurves;

            //applying the start delay
            if (delayStartMax > 0)
            {
                float t = Time.time;
                float d = Random.Range(delayStartMin, delayStartMax);

                yield return new WaitUntil(() => Time.time - t >= d);
            }

            do
            {
                if (pos == null || pos.Count < 2 || rot.Count < 2)
                {
                    if (stopEveryXSeconds > 0 && stopForXSeconds > 0 && lastPause + stopEveryXSeconds <= Time.time)
                    {
                        yield return new WaitForSeconds(stopForXSeconds);
                        lastPause = Time.time;
                        startTime += stopForXSeconds;
                    }

                    loopProgress = 1;
                    if (!isPaused)
                    {
                        if (pos == null || pos.Count == 0)
                        {
                            curveNoise = NewNoise(AutoMoverTarget.position, curveNoise);
                            curveNoiseR = NewNoise(AutoMoverTarget.rotation, curveNoiseR, startTime);
                            curveNoiseS = NewNoise(AutoMoverTarget.scale, curveNoiseS);
                            if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                            {
                                gameObject.transform.position = origGlobalPos + curveNoise;
                                gameObject.transform.rotation = Quaternion.Euler(origGlobalRot + curveNoiseR);
                                gameObject.transform.localScale = origGlobalScl + curveNoiseS;
                                if (gameObject.transform.parent != null)
                                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);
                            }
                            else
                            {
                                gameObject.transform.localPosition = origLocalPos + curveNoise;
                                gameObject.transform.localRotation = Quaternion.Euler(origLocalRot + curveNoiseR);
                                gameObject.transform.localScale = origLocalScl + curveNoiseS;
                            }
                        }
                        else if (pos.Count == 1)
                        {
                            curveNoise = NewNoise(AutoMoverTarget.position, curveNoise);
                            curveNoiseR = NewNoise(AutoMoverTarget.rotation, curveNoiseR, startTime);
                            curveNoiseS = NewNoise(AutoMoverTarget.scale, curveNoiseS);
                            if (anchorPointSpace == AutoMoverAnchorPointSpace.world)
                            {
                                gameObject.transform.position = pos[0] + curveNoise;
                                gameObject.transform.rotation = Quaternion.Euler(rot[0] + curveNoiseR);
                                gameObject.transform.localScale = scl[0] + curveNoiseS;
                                if (gameObject.transform.parent != null)
                                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);
                            }
                            else if (anchorPointSpace == AutoMoverAnchorPointSpace.local)
                            {
                                gameObject.transform.localPosition = pos[0] + curveNoise;
                                gameObject.transform.localRotation = Quaternion.Euler(rot[0] + curveNoiseR);
                                gameObject.transform.localScale = scl[0] + curveNoiseS;
                            }
                            else
                            {
                                gameObject.transform.localPosition = origChildPosLocal[0] + curveNoise;
                                gameObject.transform.localRotation = Quaternion.Euler(origChildRotLocal[0] + curveNoiseR);
                                gameObject.transform.localScale = origChildSclLocal[0] + curveNoiseS;
                            }
                        }
                    }
                    else
                    {
                        startTime += Time.deltaTime;
                    }

                    yield return null;
                    continue;
                }

                bool looped = false; //loop/bounce only once per round 

                float speed = precompTotalDist / length;

                float elapsed = 0;
                float travelled = 0;

                for (int i = 0; i < precompDistSoFar.Length; ++i)
                {
                    if (stopEveryXSeconds > 0 && stopForXSeconds > 0 && lastPause + stopEveryXSeconds <= Time.time)
                    {
                        yield return new WaitForSeconds(stopForXSeconds);
                        lastPause = Time.time;
                        startTime += stopForXSeconds;
                    }

                    if (precompDistSoFar[i] > travelled || precompTotalDist == 0)
                    {
                        if (isPaused)
                        {
                            --i;
                            startTime += Time.deltaTime;
                            yield return null;
                            continue;
                        }

                        float progress = 0;

                        //static object position but varying rotation/scale
                        if (precompTotalDist == 0)
                        {
                            if (isPaused)
                            {
                                --i;
                                startTime += Time.deltaTime;
                                yield return null;
                                continue;
                            }
                            elapsed += Time.deltaTime;
                            float totalProgress = elapsed / length;
                            if (totalProgress > 1)
                                totalProgress = 1;
                            loopProgress = totalProgress;
                            i = (int)((precompDistSoFar.Length - 1) * totalProgress) + 1;
                            if (i >= precompDistSoFar.Length) break;
                            float seg = 1.0f / (precompDistSoFar.Length - 1);
                            progress = (totalProgress - seg * (i - 1)) / seg;

                        }

                        //normal movement
                        else
                        {
                            loopProgress = Mathf.Min(1, travelled / precompTotalDist);

                            if (tmpSlowOnCurves && segmentTimes.Count > 0 && i > 0)
                                speed = (precompDistSoFar[i] - precompDistSoFar[i - 1]) / (length * segmentTimes[i] / totalTime);

                            elapsed += Time.deltaTime;
                            travelled += Time.deltaTime * speed;
                            progress = (travelled - precompDistSoFar[i - 1]) / (precompDistSoFar[i] - precompDistSoFar[i - 1]);
                            if (progress > 1)
                                progress = 1;
                        }

                        curveNoise = NewNoise(AutoMoverTarget.position, curveNoise);
                        curveNoiseR = NewNoise(AutoMoverTarget.rotation, curveNoiseR, startTime);
                        curveNoiseS = NewNoise(AutoMoverTarget.scale, curveNoiseS);

                        if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.world)
                        {
                            gameObject.transform.position = precompPos[i - 1] * (1 - progress) + precompPos[i] * progress + curveNoise;
                            if (tmpRotationMethod == AutoMoverRotationMethod.linear)
                                gameObject.transform.rotation = Quaternion.Euler(precompRot[i - 1] * (1 - progress) + precompRot[i] * progress + curveNoiseR);
                            else if (tmpRotationMethod == AutoMoverRotationMethod.spherical)
                                gameObject.transform.rotation = Quaternion.Euler(Quaternion.Slerp(Quaternion.Euler(precompRot[i - 1]), Quaternion.Euler(precompRot[i]), progress).eulerAngles + curveNoiseR);
                            gameObject.transform.localScale = precompScl[i - 1] * (1 - progress) + precompScl[i] * progress + curveNoiseS;
                            if (gameObject.transform.parent != null)
                                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);

                        }
                        else
                        {
                            gameObject.transform.localPosition = precompPos[i - 1] * (1 - progress) + precompPos[i] * progress + curveNoise;
                            if (tmpRotationMethod == AutoMoverRotationMethod.linear)
                                gameObject.transform.localRotation = Quaternion.Euler(precompRot[i - 1] * (1 - progress) + precompRot[i] * progress + curveNoiseR);
                            else if (tmpRotationMethod == AutoMoverRotationMethod.spherical)
                                gameObject.transform.localRotation = Quaternion.Euler(Quaternion.Slerp(Quaternion.Euler(precompRot[i - 1]), Quaternion.Euler(precompRot[i]), progress).eulerAngles + curveNoiseR);
                            gameObject.transform.localScale = precompScl[i - 1] * (1 - progress) + precompScl[i] * progress + curveNoiseS;
                        }

                        --i;
                        yield return null;
                    }
                }

                if (tmpLoopingStyle == AutoMoverLoopingStyle.bounce && !looped) //bounce
                {
                    elapsed = 0;
                    travelled = precompTotalDist;

                    for (int i = precompDistSoFar.Length - 1; i >= 0; --i)
                    {
                        if (stopEveryXSeconds > 0 && stopForXSeconds > 0 && lastPause + stopEveryXSeconds <= Time.time)
                        {
                            yield return new WaitForSeconds(stopForXSeconds);
                            lastPause = Time.time;
                            startTime += stopForXSeconds;
                        }

                        if (precompDistSoFar[i] < travelled)
                        {
                            if (isPaused)
                            {
                                --i;
                                startTime += Time.deltaTime;
                                yield return null;
                                continue;
                            }

                            loopProgress = Mathf.Min(1, travelled / precompTotalDist);

                            if (tmpSlowOnCurves && segmentTimes.Count > 0 && i > 0)
                                speed = (precompDistSoFar[i] - precompDistSoFar[i - 1]) / (length * segmentTimes[i] / totalTime);

                            elapsed += Time.deltaTime;
                            travelled -= Time.deltaTime * speed;

                            float progress = (travelled - precompDistSoFar[i + 1]) / (precompDistSoFar[i] - precompDistSoFar[i + 1]);
                            curveNoise = NewNoise(AutoMoverTarget.position, curveNoise);
                            curveNoiseR = NewNoise(AutoMoverTarget.rotation, curveNoiseR, startTime);
                            curveNoiseS = NewNoise(AutoMoverTarget.scale, curveNoiseS);
                            if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.world)
                            {
                                gameObject.transform.position = precompPos[i + 1] * (1 - progress) + precompPos[i] * progress + curveNoise;
                                if (tmpRotationMethod == AutoMoverRotationMethod.linear)
                                    gameObject.transform.rotation = Quaternion.Euler(precompRot[i + 1] * (1 - progress) + precompRot[i] * progress + curveNoiseR);
                                else if (tmpRotationMethod == AutoMoverRotationMethod.spherical)
                                    gameObject.transform.rotation = Quaternion.Euler(Quaternion.Slerp(Quaternion.Euler(precompRot[i + 1]), Quaternion.Euler(precompRot[i]), progress).eulerAngles + curveNoiseR);
                                gameObject.transform.localScale = precompScl[i + 1] * (1 - progress) + precompScl[i] * progress + curveNoiseS;
                                if (gameObject.transform.parent != null)
                                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);

                            }
                            else
                            {
                                gameObject.transform.localPosition = precompPos[i + 1] * (1 - progress) + precompPos[i] * progress + curveNoise;
                                if (tmpRotationMethod == AutoMoverRotationMethod.linear)
                                    gameObject.transform.localRotation = Quaternion.Euler(precompRot[i + 1] * (1 - progress) + precompRot[i] * progress + curveNoiseR);
                                else if (tmpRotationMethod == AutoMoverRotationMethod.spherical)
                                    gameObject.transform.localRotation = Quaternion.Euler(Quaternion.Slerp(Quaternion.Euler(precompRot[i + 1]), Quaternion.Euler(precompRot[i]), progress).eulerAngles + curveNoiseR);
                                gameObject.transform.localScale = precompScl[i + 1] * (1 - progress) + precompScl[i] * progress + curveNoiseS;
                            }
                            ++i;
                            yield return null;
                        }
                    }

                    //moving exactly to the end of the path
                    if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.world)
                    {
                        gameObject.transform.position = precompPos[0] + curveNoise;
                        gameObject.transform.rotation = Quaternion.Euler(precompRot[0] + curveNoiseR);
                        gameObject.transform.localScale = precompScl[0] + curveNoiseS;
                        if (gameObject.transform.parent != null)
                            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);

                    }
                    else
                    {
                        gameObject.transform.localPosition = precompPos[0] + curveNoise;
                        gameObject.transform.localRotation = Quaternion.Euler(precompRot[0] + curveNoiseR);
                        gameObject.transform.localScale = precompScl[0] + curveNoiseS;
                    }
                }
                else //moving exactly to the end of the path
                {
                    if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.world)
                    {
                        gameObject.transform.position = precompPos[precompPos.Length - 1] + curveNoise;
                        gameObject.transform.rotation = Quaternion.Euler(precompRot[precompRot.Length - 1] + curveNoiseR);
                        gameObject.transform.localScale = precompScl[precompScl.Length - 1] + curveNoiseS;
                        if (gameObject.transform.parent != null)
                            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);

                    }
                    else
                    {
                        gameObject.transform.localPosition = precompPos[precompPos.Length - 1] + curveNoise;
                        gameObject.transform.localRotation = Quaternion.Euler(precompRot[precompRot.Length - 1] + curveNoiseR);
                        gameObject.transform.localScale = precompScl[precompScl.Length - 1] + curveNoiseS;
                    }
                }

                yield return null;


                curveNoise = NewNoise(AutoMoverTarget.position, curveNoise);
                curveNoiseR = NewNoise(AutoMoverTarget.rotation, curveNoiseR, startTime);
                curveNoiseS = NewNoise(AutoMoverTarget.scale, curveNoiseS);

                //Moving the object back to starting point
                if (pos == null || pos.Count == 0)
                {
                    if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.world)
                    {
                        gameObject.transform.position = origGlobalPos + curveNoise;
                        gameObject.transform.rotation = Quaternion.Euler(origGlobalRot + curveNoiseR);
                        gameObject.transform.localScale = origGlobalScl + curveNoiseS;
                        if (gameObject.transform.parent != null)
                            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);
                    }
                    else
                    {
                        gameObject.transform.localPosition = origLocalPos + curveNoise;
                        gameObject.transform.localRotation = Quaternion.Euler(origLocalRot + curveNoiseR);
                        gameObject.transform.localScale = origLocalScl + curveNoiseS;
                    }
                }
                else
                {
                    if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.world)
                    {
                        gameObject.transform.position = precompPos[0] + curveNoise;
                        gameObject.transform.rotation = Quaternion.Euler(precompRot[0] + curveNoiseR);
                        gameObject.transform.localScale = precompScl[0] + curveNoiseS;
                        if (gameObject.transform.parent != null)
                            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x / gameObject.transform.parent.lossyScale.x, gameObject.transform.localScale.y / gameObject.transform.parent.lossyScale.y, gameObject.transform.localScale.z / gameObject.transform.parent.lossyScale.z);
                    }
                    else if (tmpAnchorPointSpace == AutoMoverAnchorPointSpace.local || tmpAnchorPointSpace == AutoMoverAnchorPointSpace.child)
                    {
                        gameObject.transform.localPosition = precompPos[0] + curveNoise;
                        gameObject.transform.localRotation = Quaternion.Euler(precompRot[0] + curveNoiseR);
                        gameObject.transform.localScale = precompScl[0] + curveNoiseS;
                    }
                }

                runs++;

                if (stopAfter > 0 && stopAfter <= runs)
                {
                    StopMoving();
                    break;
                }

                if (DelayMin > 0)
                    yield return new WaitForSeconds(Random.Range(delayMin, delayMax));
                else
                    yield return null;

                if (newChanges)
                {
                    newChanges = false;
                    resetMovement = true;
                }

            } while (true);

        }

        private static List<Vector3> BezierCurve(Vector3[] points, int steps)
        {
            List<Vector3> bezier = new List<Vector3>();
            for (int i = 0; i < steps; ++i)
            {
                bezier.Add(BezierPoint(points, ((float)i) / ((float)steps)));
            }

            return bezier;
        }

        private static Vector3 BezierPoint(Vector3[] points, float t)
        {
            if (points.Length == 1)
            {
                return points[0];
            }
            if (points.Length == 2)
            {
                return points[0] * (1f - t) + points[1] * t;
            }
            else if (points.Length > 2)
            {
                List<Vector3> newPoints = new List<Vector3>();
                for (int i = 0; i < points.Length - 1; ++i)
                {
                    newPoints.Add(points[i] * (1f - t) + points[i + 1] * t);
                }
                return BezierPoint(newPoints.ToArray(), t);
            }

            return new Vector3();
        }

        private static float BezierPoint(float[] points, float t)
        {
            if (points.Length == 1)
            {
                return points[0];
            }
            if (points.Length == 2)
            {
                return points[0] * (1f - t) + points[1] * t;
            }
            else if (points.Length > 2)
            {
                List<float> newPoints = new List<float>();
                for (int i = 0; i < points.Length - 1; ++i)
                {
                    newPoints.Add(points[i] * (1f - t) + points[i + 1] * t);
                }
                return BezierPoint(newPoints.ToArray(), t);
            }

            return 0;
        }
    }
}