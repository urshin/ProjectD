using UnityEngine;
using UnityEditor;

namespace AutoMoverPro
{
    [CustomEditor(typeof(AutoMover)), CanEditMultipleObjects]
    public class MoverEditor : Editor
    {

        private static GUIContent moveUpButtonContent = new GUIContent("\u25b2", "move up");
        private static GUIContent moveDownButtonContent = new GUIContent("\u25bc", "move down");
        private static GUIContent duplicateButtonContent = new GUIContent("+", "duplicate");
        private static GUIContent deleteButtonContent = new GUIContent("-", "delete");
        private AutoMover mover;
        private bool expandedNoise;
        private bool expandedNoisePos;
        private bool expandedNoiseRot;
        private bool expandedNoiseScale;
        private bool expandedCurve;
        private bool expandedAnchor;
        private bool coreThingChanged;
        private bool pointSpaceChanged;
        private int oldAPSpace;
        SerializedProperty waypointPos;
        SerializedProperty waypointRot;
        SerializedProperty waypointScl;
        SerializedProperty length;
        SerializedProperty steps;
        SerializedProperty weight;
        SerializedProperty lpStyle;
        SerializedProperty ipStyle;
        SerializedProperty rlStyle;
        SerializedProperty wsStyle;
        SerializedProperty ntStyle;
        SerializedProperty rntStyle;
        SerializedProperty sntStyle;
        SerializedProperty noiseAmplitude;
        SerializedProperty noiseFrequency;
        SerializedProperty noiseAmplitudeR;
        SerializedProperty noiseFrequencyR;
        SerializedProperty noiseAmplitudeS;
        SerializedProperty noiseFrequencyS;
        SerializedProperty sineOffset;
        SerializedProperty sineOffsetR;
        SerializedProperty sineOffsetS;
        SerializedProperty runOnStart;
        SerializedProperty delayStartMin;
        SerializedProperty delayStartMax;
        SerializedProperty delayMin;
        SerializedProperty delayMax;
        SerializedProperty stopAfter;
        SerializedProperty stopEveryXSeconds;
        SerializedProperty stopForXSeconds;
        SerializedProperty slowOnCurves;
        SerializedProperty turnRate;
        SerializedProperty decelerationTime;
        SerializedProperty movableGizmos;
        SerializedProperty drawGizmos;
        SerializedProperty instantChanges;
        SerializedProperty populateWithMesh;
        SerializedProperty moving;
        SerializedProperty faceForward;
        SerializedProperty dynamicUp;
        SerializedProperty anchorExpanded;
        SerializedProperty curveExpanded;
        SerializedProperty noiseExpanded;
        SerializedProperty noisePosExpanded;
        SerializedProperty noiseRotExpanded;
        SerializedProperty noiseScaleExpanded;
        SerializedProperty newChanges;

        //This is executed always when the object is selected
        void OnEnable()
        {
            waypointPos = serializedObject.FindProperty("pos");
            waypointRot = serializedObject.FindProperty("rot");
            waypointScl = serializedObject.FindProperty("scl");
            length = serializedObject.FindProperty("length");
            steps = serializedObject.FindProperty("steps");
            weight = serializedObject.FindProperty("curveWeight");
            lpStyle = serializedObject.FindProperty("loopingStyle");
            ipStyle = serializedObject.FindProperty("curveStyle");
            rlStyle = serializedObject.FindProperty("rotationMethod");
            wsStyle = serializedObject.FindProperty("anchorPointSpace");
            ntStyle = serializedObject.FindProperty("positionNoiseType");
            rntStyle = serializedObject.FindProperty("rotationNoiseType");
            sntStyle = serializedObject.FindProperty("scaleNoiseType");
            noiseAmplitude = serializedObject.FindProperty("positionNoiseAmplitude");
            noiseFrequency = serializedObject.FindProperty("positionNoiseFrequency");
            noiseAmplitudeR = serializedObject.FindProperty("rotationNoiseAmplitude");
            noiseFrequencyR = serializedObject.FindProperty("rotationNoiseFrequency");
            noiseAmplitudeS = serializedObject.FindProperty("scaleNoiseAmplitude");
            noiseFrequencyS = serializedObject.FindProperty("scaleNoiseFrequency");
            sineOffset = serializedObject.FindProperty("positionSineOffset");
            sineOffsetR = serializedObject.FindProperty("rotationSineOffset");
            sineOffsetS = serializedObject.FindProperty("scaleSineOffset");
            moving = serializedObject.FindProperty("moving");
            runOnStart = serializedObject.FindProperty("runOnStart");
            delayStartMin = serializedObject.FindProperty("delayStartMin");
            delayStartMax = serializedObject.FindProperty("delayStartMax");
            delayMin = serializedObject.FindProperty("delayMin");
            delayMax = serializedObject.FindProperty("delayMax");
            stopAfter = serializedObject.FindProperty("stopAfter");
            stopEveryXSeconds = serializedObject.FindProperty("stopEveryXSeconds");
            stopForXSeconds = serializedObject.FindProperty("stopForXSeconds");
            slowOnCurves = serializedObject.FindProperty("slowOnCurves");
            turnRate = serializedObject.FindProperty("turnRate");
            decelerationTime = serializedObject.FindProperty("decelerationTime");
            movableGizmos = serializedObject.FindProperty("movableGizmos");
            drawGizmos = serializedObject.FindProperty("drawGizmos");
            instantChanges = serializedObject.FindProperty("instantRuntimeChanges");
            populateWithMesh = serializedObject.FindProperty("populateWithMesh");
            faceForward = serializedObject.FindProperty("faceForward");
            dynamicUp = serializedObject.FindProperty("dynamicUpVector");
            anchorExpanded = serializedObject.FindProperty("anchorExpanded");
            curveExpanded = serializedObject.FindProperty("curveExpanded");
            noiseExpanded = serializedObject.FindProperty("noiseExpanded");
            noisePosExpanded = serializedObject.FindProperty("noisePosExpanded");
            noiseRotExpanded = serializedObject.FindProperty("noiseRotExpanded");
            noiseScaleExpanded = serializedObject.FindProperty("noiseScaleExpanded");
            newChanges = serializedObject.FindProperty("newChanges");
            expandedAnchor = anchorExpanded.boolValue;
            expandedNoise = noiseExpanded.boolValue;
            expandedCurve = curveExpanded.boolValue;
            expandedNoisePos = noisePosExpanded.boolValue;
            expandedNoiseRot = noiseRotExpanded.boolValue;
            expandedNoiseScale = noiseScaleExpanded.boolValue;
            mover = (AutoMover)target;
        }

        public void OnSceneGUI()
        {
            Undo.RecordObject(mover, "Automover");
            if (mover.DrawGizmos)
            {
                for (int i = 0; i < mover.Pos.Count; ++i)
                {
                    if (mover.MovableGizmos.Count < mover.Pos.Count)
                    {
                        mover.AnchorPointVersionCheck();
                        break;
                    }
                    if (mover.MovableGizmos[i])
                    {
                        Vector3 origPos = mover.Pos[i];

                        if (mover.AnchorPointSpace == AutoMoverAnchorPointSpace.world)
                            mover.Pos[i] = Handles.PositionHandle(mover.Pos[i], Quaternion.identity);
                        else if (mover.AnchorPointSpace == AutoMoverAnchorPointSpace.local)
                        {
                            if (mover.transform.parent == null)
                                mover.Pos[i] = Handles.PositionHandle(mover.Pos[i], Quaternion.identity);
                            else if (mover.transform.parent != null && !mover.Moving)
                                mover.Pos[i] = mover.transform.parent.InverseTransformPoint(Handles.PositionHandle(mover.transform.parent.TransformPoint(mover.Pos[i]), Quaternion.identity));
                        }
                        else if (mover.AnchorPointSpace == AutoMoverAnchorPointSpace.child)
                        {
                            if (!mover.Moving)
                                mover.Pos[i] = mover.transform.InverseTransformPoint(Handles.PositionHandle(mover.transform.TransformPoint(mover.Pos[i]), Quaternion.identity));
                        }

                        if (origPos != mover.Pos[i])
                        {
                            bool moved = mover.Moving;
                            if (moved)
                            {
                                mover.StopMoving();
                                mover.StartMoving();
                            }
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            //making sure anchor points are coherent and in the format of newest version
            mover.AnchorPointVersionCheck();

            coreThingChanged = false;
            serializedObject.Update();

            //this might happen if the user copies the component when it is moving, and pastes it in edit mode
            if (!Application.isPlaying && moving.boolValue)
            {
                Vector3 origPosition = mover.transform.localPosition;
                Vector3 origRotation = mover.transform.localEulerAngles;
                Vector3 origScale = mover.transform.localScale;
                mover.StopMoving(); //this might move it to 0,0,0
                mover.transform.localPosition = origPosition;
                mover.transform.localEulerAngles = origRotation;
                mover.transform.localScale = origScale;
            }

            //start/pause button

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            string startButtonText = "Start";
            if (mover.IsPaused)
                startButtonText = "Resume";
            else if (mover.Moving)
                startButtonText = "Pause";


            if (GUILayout.Button(new GUIContent(startButtonText, "Start the mover manually in play mode."), GUILayout.Width(Screen.width * 0.5f)))
            {
                if (mover.IsPaused)
                    mover.Resume();
                else if (mover.Moving)
                    mover.Pause();
                else
                    mover.StartMoving();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            AnchorPointFoldout();

            CurveFoldout();

            NoiseFoldout();

            PropertyBounds();

            bool moved = false;
            if (coreThingChanged && moving.boolValue && instantChanges.boolValue)
            {
                moved = true;
                moving.boolValue = false;
                mover.StopMoving();
            }
            else if (coreThingChanged && moving.boolValue && !instantChanges.boolValue)
            {
                newChanges.boolValue = true;
            }
            if (pointSpaceChanged)
            {
                pointSpaceChanged = false;

                //Convert the anchor points to world/local/child coordinates, depending on the specified anchor point space
                if (wsStyle.intValue == 0)
                {
                    if (oldAPSpace == 1)
                    {
                        //from local to world
                        for (int i = 0; i < waypointPos.arraySize; ++i)
                        {
                            //if there is no parent, the points are the same
                            if (mover.transform.parent != null)
                            {
                                waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.parent.TransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                                waypointRot.GetArrayElementAtIndex(i).vector3Value = (mover.transform.parent.rotation * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                                waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(mover.transform.parent.lossyScale.x * waypointScl.GetArrayElementAtIndex(i).vector3Value.x, mover.transform.parent.lossyScale.y * waypointScl.GetArrayElementAtIndex(i).vector3Value.y, mover.transform.parent.lossyScale.z * waypointScl.GetArrayElementAtIndex(i).vector3Value.z);
                            }
                        }
                    }
                    else
                    {

                        //from child to world
                        for (int i = 0; i < waypointPos.arraySize; ++i)
                        {
                            waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.TransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                            waypointRot.GetArrayElementAtIndex(i).vector3Value = (mover.transform.rotation * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                            waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(mover.transform.lossyScale.x * waypointScl.GetArrayElementAtIndex(i).vector3Value.x, mover.transform.lossyScale.y * waypointScl.GetArrayElementAtIndex(i).vector3Value.y, mover.transform.lossyScale.z * waypointScl.GetArrayElementAtIndex(i).vector3Value.z);
                        }
                    }
                }
                else if (wsStyle.intValue == 1)
                {
                    if (oldAPSpace == 0)
                    {
                        //from world to local
                        for (int i = 0; i < waypointPos.arraySize; ++i)
                        {
                            //if there is no parent, the points are the same
                            if (mover.transform.parent != null)
                            {
                                waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.parent.InverseTransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                                waypointRot.GetArrayElementAtIndex(i).vector3Value = (Quaternion.Inverse(mover.transform.parent.rotation) * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                                waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(waypointScl.GetArrayElementAtIndex(i).vector3Value.x / mover.transform.parent.lossyScale.x, waypointScl.GetArrayElementAtIndex(i).vector3Value.y / mover.transform.parent.lossyScale.y, waypointScl.GetArrayElementAtIndex(i).vector3Value.z / mover.transform.parent.lossyScale.z);
                            }
                        }
                    }
                    else
                    {
                        //from child to local (via world)
                        for (int i = 0; i < waypointPos.arraySize; ++i)
                        {
                            //from child to world
                            waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.TransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                            waypointRot.GetArrayElementAtIndex(i).vector3Value = (mover.transform.rotation * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                            waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(mover.transform.lossyScale.x * waypointScl.GetArrayElementAtIndex(i).vector3Value.x, mover.transform.lossyScale.y * waypointScl.GetArrayElementAtIndex(i).vector3Value.y, mover.transform.lossyScale.z * waypointScl.GetArrayElementAtIndex(i).vector3Value.z);

                            //from world to local. if there is no parent, the points are the same
                            if (mover.transform.parent != null)
                            {
                                waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.parent.InverseTransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                                waypointRot.GetArrayElementAtIndex(i).vector3Value = (Quaternion.Inverse(mover.transform.parent.rotation) * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                                waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(waypointScl.GetArrayElementAtIndex(i).vector3Value.x / mover.transform.parent.lossyScale.x, waypointScl.GetArrayElementAtIndex(i).vector3Value.y / mover.transform.parent.lossyScale.y, waypointScl.GetArrayElementAtIndex(i).vector3Value.z / mover.transform.parent.lossyScale.z);
                            }
                        }
                    }
                }
                else
                {
                    if (oldAPSpace == 0)
                    {
                        //from world to child
                        for (int i = 0; i < waypointPos.arraySize; ++i)
                        {
                            waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.InverseTransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                            waypointRot.GetArrayElementAtIndex(i).vector3Value = (Quaternion.Inverse(mover.transform.rotation) * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                            waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(waypointScl.GetArrayElementAtIndex(i).vector3Value.x / mover.transform.lossyScale.x, waypointScl.GetArrayElementAtIndex(i).vector3Value.y / mover.transform.lossyScale.y, waypointScl.GetArrayElementAtIndex(i).vector3Value.z / mover.transform.lossyScale.z);
                        }
                    }
                    else
                    {
                        //from local to child
                        for (int i = 0; i < waypointPos.arraySize; ++i)
                        {
                            //from local to world. if there is no parent, the points are the same
                            if (mover.transform.parent != null)
                            {
                                waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.parent.TransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                                waypointRot.GetArrayElementAtIndex(i).vector3Value = (mover.transform.parent.rotation * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                                waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(mover.transform.parent.lossyScale.x * waypointScl.GetArrayElementAtIndex(i).vector3Value.x, mover.transform.parent.lossyScale.y * waypointScl.GetArrayElementAtIndex(i).vector3Value.y, mover.transform.parent.lossyScale.z * waypointScl.GetArrayElementAtIndex(i).vector3Value.z);
                            }

                            //from world to child
                            waypointPos.GetArrayElementAtIndex(i).vector3Value = mover.transform.InverseTransformPoint(waypointPos.GetArrayElementAtIndex(i).vector3Value);
                            waypointRot.GetArrayElementAtIndex(i).vector3Value = (Quaternion.Inverse(mover.transform.rotation) * Quaternion.Euler(waypointRot.GetArrayElementAtIndex(i).vector3Value)).eulerAngles;
                            waypointScl.GetArrayElementAtIndex(i).vector3Value = new Vector3(waypointScl.GetArrayElementAtIndex(i).vector3Value.x / mover.transform.lossyScale.x, waypointScl.GetArrayElementAtIndex(i).vector3Value.y / mover.transform.lossyScale.y, waypointScl.GetArrayElementAtIndex(i).vector3Value.z / mover.transform.lossyScale.z);
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (moved)
            {
                mover.StartMoving();
            }
        }

        private void AddWaypoint()
        {
            waypointPos.InsertArrayElementAtIndex(waypointPos.arraySize);

            //safety check in case of corrupted anchor point list
            while (waypointRot.arraySize >= waypointPos.arraySize)
            {
                waypointRot.DeleteArrayElementAtIndex(waypointRot.arraySize - 1);
            }
            waypointRot.InsertArrayElementAtIndex(waypointRot.arraySize);

            //safety check in case of corrupted anchor point list
            while (waypointScl.arraySize >= waypointPos.arraySize)
            {
                waypointScl.DeleteArrayElementAtIndex(waypointScl.arraySize - 1);
            }
            waypointScl.InsertArrayElementAtIndex(waypointScl.arraySize);

            //safety check in case of corrupted anchor point list
            while (movableGizmos.arraySize >= waypointPos.arraySize)
            {
                movableGizmos.DeleteArrayElementAtIndex(movableGizmos.arraySize - 1);
            }
            movableGizmos.InsertArrayElementAtIndex(movableGizmos.arraySize);
            movableGizmos.GetArrayElementAtIndex(movableGizmos.arraySize - 1).boolValue = true;

            if (wsStyle.intValue == 0)
            {
                waypointPos.GetArrayElementAtIndex(waypointPos.arraySize - 1).vector3Value = mover.gameObject.transform.position;
                waypointRot.GetArrayElementAtIndex(waypointRot.arraySize - 1).vector3Value = mover.gameObject.transform.rotation.eulerAngles;
                waypointScl.GetArrayElementAtIndex(waypointScl.arraySize - 1).vector3Value = mover.gameObject.transform.lossyScale;
            }
            else if (wsStyle.intValue == 1)
            {
                waypointPos.GetArrayElementAtIndex(waypointPos.arraySize - 1).vector3Value = mover.gameObject.transform.localPosition;
                waypointRot.GetArrayElementAtIndex(waypointRot.arraySize - 1).vector3Value = mover.gameObject.transform.localRotation.eulerAngles;
                waypointScl.GetArrayElementAtIndex(waypointScl.arraySize - 1).vector3Value = mover.gameObject.transform.localScale;
            }
            else
            {
                waypointPos.GetArrayElementAtIndex(waypointPos.arraySize - 1).vector3Value = new Vector3(0, 0, 0);
                waypointRot.GetArrayElementAtIndex(waypointRot.arraySize - 1).vector3Value = new Vector3(0, 0, 0);
                waypointScl.GetArrayElementAtIndex(waypointScl.arraySize - 1).vector3Value = new Vector3(1, 1, 1);
            }
        }

        private void AnchorPointFoldout()
        {
            expandedAnchor = EditorGUILayout.Foldout(expandedAnchor, "Anchor Points", true);

            anchorExpanded.boolValue = expandedAnchor;

            if (expandedAnchor)
            {
                EditorGUI.indentLevel += 1;

                int oldValuePointS = wsStyle.intValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Anchor point space", "The space in which the anchor points are defined relative to the object. Either 'World', 'Local' or 'Child'."));
                oldAPSpace = wsStyle.intValue;
                wsStyle.intValue = EditorGUILayout.Popup(wsStyle.intValue, new string[3] { "World", "Local", "Child" });
                EditorGUILayout.EndHorizontal();
                if (oldValuePointS != wsStyle.intValue)
                {
                    pointSpaceChanged = true;
                    coreThingChanged = true;
                }

                bool oldValueFaceForward = faceForward.boolValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Face forward", "Ignores the rotations of the anchor points after the first one and rotates the object along the path. 'Forward' for the first anchor point is along X-axis."));
                EditorGUILayout.PropertyField(faceForward, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (oldValueFaceForward != faceForward.boolValue)
                {
                    coreThingChanged = true;
                }

                EditorGUI.BeginDisabledGroup(!faceForward.boolValue);

                bool oldValueDynamicUp = dynamicUp.boolValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Dynamic up-vector", "Recalculates the up-vector each update, allowing for all kinds of smooth turns."));
                EditorGUILayout.PropertyField(dynamicUp, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (oldValueDynamicUp != dynamicUp.boolValue)
                {
                    coreThingChanged = true;
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Add anchor point", "Adds the current position, rotation and scale at the end of the anchor point list.")))
                {
                    AddWaypoint();
                    coreThingChanged = true;
                }

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(waypointPos.FindPropertyRelative("Array.size"));
                EditorGUI.EndDisabledGroup();

                DrawHorizontalLine();

                for (int i = 0; i < waypointPos.arraySize; i++)
                {
                    Vector3 posOldValue = waypointPos.GetArrayElementAtIndex(i).vector3Value;
                    Vector3 rotOldValue = waypointRot.GetArrayElementAtIndex(i).vector3Value;
                    Vector3 sclOldValue = waypointScl.GetArrayElementAtIndex(i).vector3Value;

                    float prevLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 200;
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("    Position ");
                    EditorGUILayout.PropertyField(waypointPos.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(i != 0 && faceForward.boolValue);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("    Rotation");
                    EditorGUILayout.PropertyField(waypointRot.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("    Scale     ");
                    EditorGUILayout.PropertyField(waypointScl.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = prevLabelWidth;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2.5f));
                    if (posOldValue != waypointPos.GetArrayElementAtIndex(i).vector3Value || rotOldValue != waypointRot.GetArrayElementAtIndex(i).vector3Value || sclOldValue != waypointScl.GetArrayElementAtIndex(i).vector3Value)
                        coreThingChanged = true;

                    if (GUILayout.Button(moveUpButtonContent))
                    {
                        if (i > 0)
                        {
                            waypointPos.MoveArrayElement(i, i - 1);
                            waypointRot.MoveArrayElement(i, i - 1);
                            waypointScl.MoveArrayElement(i, i - 1);
                            movableGizmos.MoveArrayElement(i, i - 1);
                            coreThingChanged = true;
                        }
                    }
                    if (GUILayout.Button(moveDownButtonContent))
                    {
                        if (i < waypointPos.arraySize - 1)
                        {
                            waypointPos.MoveArrayElement(i, i + 1);
                            waypointRot.MoveArrayElement(i, i + 1);
                            waypointScl.MoveArrayElement(i, i + 1);
                            movableGizmos.MoveArrayElement(i, i + 1);
                            coreThingChanged = true;
                        }
                    }

                    if (GUILayout.Button(duplicateButtonContent))
                    {
                        waypointPos.InsertArrayElementAtIndex(i);
                        waypointPos.GetArrayElementAtIndex(i).vector3Value = waypointPos.GetArrayElementAtIndex(i + 1).vector3Value;
                        waypointRot.InsertArrayElementAtIndex(i);
                        waypointRot.GetArrayElementAtIndex(i).vector3Value = waypointRot.GetArrayElementAtIndex(i + 1).vector3Value;
                        waypointScl.InsertArrayElementAtIndex(i);
                        waypointScl.GetArrayElementAtIndex(i).vector3Value = waypointScl.GetArrayElementAtIndex(i + 1).vector3Value;
                        movableGizmos.InsertArrayElementAtIndex(i);
                        movableGizmos.GetArrayElementAtIndex(i).boolValue = movableGizmos.GetArrayElementAtIndex(i + 1).boolValue;
                        coreThingChanged = true;
                    }
                    if (GUILayout.Button(deleteButtonContent))
                    {
                        int oldSize = waypointPos.arraySize;
                        waypointPos.DeleteArrayElementAtIndex(i);
                        if (waypointPos.arraySize == oldSize)
                        {
                            waypointPos.DeleteArrayElementAtIndex(i);
                        }
                        oldSize = waypointRot.arraySize;
                        waypointRot.DeleteArrayElementAtIndex(i);
                        if (waypointRot.arraySize == oldSize)
                        {
                            waypointRot.DeleteArrayElementAtIndex(i);
                        }
                        oldSize = waypointScl.arraySize;
                        waypointScl.DeleteArrayElementAtIndex(i);
                        if (waypointScl.arraySize == oldSize)
                        {
                            waypointScl.DeleteArrayElementAtIndex(i);
                        }
                        coreThingChanged = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(mover.Moving);
                    prevLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.LabelField(new GUIContent("Movable in editor", "Enables position control for the gizmo in the editor."));
                    EditorGUIUtility.labelWidth = prevLabelWidth;
                    EditorGUILayout.PropertyField(movableGizmos.GetArrayElementAtIndex(i), new GUIContent(""));
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    DrawHorizontalLine();
                }

                EditorGUI.indentLevel -= 1;
            }
        }

        private void CurveFoldout()
        {

            expandedCurve = EditorGUILayout.Foldout(expandedCurve, "Curve", true);

            curveExpanded.boolValue = expandedCurve;

            if (expandedCurve)
            {
                EditorGUI.indentLevel += 1;

                float oldValueLength = length.floatValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Length (seconds)", "The length of the curve in seconds. Minimum value of 0.5s."));
                EditorGUILayout.PropertyField(length, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (length.floatValue < 0.5f)
                    length.floatValue = 0.5f;
                if (oldValueLength != length.floatValue)
                    coreThingChanged = true;


                int oldValueSteps = steps.intValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Steps", "The amount of steps in the curve. May differ slightly in reality."));
                EditorGUILayout.PropertyField(steps, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (steps.intValue < waypointPos.arraySize)
                    steps.intValue = waypointPos.arraySize;
                if (steps.intValue < 1)
                    steps.intValue = 1;
                if (oldValueSteps != steps.intValue)
                    coreThingChanged = true;


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Delay on start (seconds)", "The delay before starting the movement in seconds. Actual delay is a random value between the Min and Max values."));
                GUILayout.Label(new GUIContent("    Min", "Minimum value for the delay."));
                EditorGUILayout.PropertyField(delayStartMin, GUIContent.none);
                GUILayout.Label(new GUIContent("Max", "Maximum value for the delay."));
                EditorGUILayout.PropertyField(delayStartMax, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Delay between loops (seconds)", "The Delay between each loop in seconds. Actual delay is a random value between the Min and Max values."));
                GUILayout.Label(new GUIContent("    Min", "Minimum value for the delay."));
                EditorGUILayout.PropertyField(delayMin, GUIContent.none);
                GUILayout.Label(new GUIContent("Max", "Maximum value for the delay."));
                EditorGUILayout.PropertyField(delayMax, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Run on Start", "Is the movement started during Start()-method. If the value is false, the movement will have to be manually started using the StartMoving() method."));
                EditorGUILayout.PropertyField(runOnStart, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Stop after (loops)", "How many loops the object moves before stopping. Value of 0 means that the object will move until the StopMoving() method is called."));
                EditorGUILayout.PropertyField(stopAfter, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel(new GUIContent("Stop every", "How often (seconds) should the movement stop and for how long (seconds)?"));
                EditorGUILayout.PropertyField(stopEveryXSeconds, new GUIContent(""));
                if (stopEveryXSeconds.floatValue < 0)
                    stopEveryXSeconds.floatValue = 0;

                float prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 90;
                EditorGUILayout.PrefixLabel(new GUIContent("seconds for", "How often (seconds) should the movement stop and for how long (seconds)?"));
                EditorGUILayout.PropertyField(stopForXSeconds, new GUIContent(""));
                if (stopForXSeconds.floatValue < 0)
                    stopForXSeconds.floatValue = 0;
                EditorGUILayout.LabelField(new GUIContent("seconds", "How often (seconds) should the movement stop and for how long (seconds)?"));
                EditorGUIUtility.labelWidth = prevLabelWidth;
                EditorGUILayout.EndHorizontal();

                bool oldValueSlow = slowOnCurves.boolValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Slow on curves", "Should the movement be slower when turning and faster on straight paths. Requires precomputing the path."));
                EditorGUILayout.PropertyField(slowOnCurves, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (oldValueSlow != slowOnCurves.boolValue)
                    coreThingChanged = true;

                EditorGUI.BeginDisabledGroup(!slowOnCurves.boolValue);

                float oldValueTurnRate = turnRate.floatValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Turn rate", "The rate at which the object turns."));
                EditorGUILayout.PropertyField(turnRate, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (oldValueTurnRate != turnRate.floatValue)
                    coreThingChanged = true;

                float oldValueDecelerationTime = decelerationTime.floatValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Deceleration time (seconds)", "The time spent slowing down for a turn."));
                EditorGUILayout.PropertyField(decelerationTime, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (oldValueDecelerationTime != decelerationTime.floatValue)
                    coreThingChanged = true;

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(faceForward.boolValue);

                int oldValueRotM = rlStyle.intValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Rotation method", "The logic that is used to rotate the objects. 'Spherical' will never rotate over 180 degrees to reach the target, ignoring any full rotations. 'Linear' will always rotate to the absolute specified value, even if it means multiple 360 degree spins."));
                rlStyle.intValue = EditorGUILayout.Popup(rlStyle.intValue, new string[] { "Spherical", "Linear" });
                EditorGUILayout.EndHorizontal();
                if (oldValueRotM != rlStyle.intValue)
                    coreThingChanged = true;

                EditorGUI.EndDisabledGroup();

                int oldValueLoopS = lpStyle.intValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Looping style", "The style that is used to loop the path. 'Loop' will create a closed loop from the end to start, 'Repeat' will start from the beginning after reaching the end, and 'Bounce' will backtrack the same route after reaching the end."));
                lpStyle.intValue = EditorGUILayout.Popup(lpStyle.intValue, new string[] { "Loop", "Repeat", "Bounce" });
                EditorGUILayout.EndHorizontal();
                if (oldValueLoopS != lpStyle.intValue)
                    coreThingChanged = true;

                int oldValueCurveS = ipStyle.intValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Curve style", "The style of the curve. 'Linear' is simply straight lines between the anchor points, 'Curve' forms a single Bezier curve using all of the anchor points, and 'Spline' forms multiple bezier curves that are joined with C2 continuity. 'Spline through points' creates a spline that tries to go as close to each anchor point as possible."));
                ipStyle.intValue = EditorGUILayout.Popup(ipStyle.intValue, new string[] { "Linear", "Curve", "Spline", "Spline Through Points" });
                EditorGUILayout.EndHorizontal();
                if (oldValueCurveS != ipStyle.intValue)
                    coreThingChanged = true;

                EditorGUI.BeginDisabledGroup(ipStyle.intValue != 3 && ipStyle.intValue != 2);

                float oldValueWeight = weight.floatValue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Spline weighting", "Weight of each anchor point for splines. Default is 0.666666f"));
                EditorGUILayout.PropertyField(weight, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (oldValueWeight != weight.floatValue)
                    coreThingChanged = true;

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Draw gizmos", "Is the path visualized in the editor with gizmos."));
                EditorGUILayout.PropertyField(drawGizmos, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Instant changes", "Changes made during runtime will be instantly applied and the mover is restarted. If false, the changes will take effect after the current loop."));
                EditorGUILayout.PropertyField(instantChanges, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(mover.Moving);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Include mesh in export", "Populate the exported curve's gameobjects with the MeshRenderer or SpriteRenderer attached to the object."));
                EditorGUILayout.PropertyField(populateWithMesh, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent("Export curve as GameObjects", "Creates a list of gameobjects that follow the curve.")))
                {
                    mover.ExportCurve();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel -= 1;
            }
        }

        private void NoiseFoldout()
        {
            expandedNoise = EditorGUILayout.Foldout(expandedNoise, "Noise", true);
            noiseExpanded.boolValue = expandedNoise;

            if (expandedNoise)
            {
                EditorGUI.indentLevel += 1;

                expandedNoisePos = EditorGUILayout.Foldout(expandedNoisePos, "Position", true);
                noisePosExpanded.boolValue = expandedNoisePos;

                if (expandedNoisePos)
                {
                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Type", "The type of the noise that is applied to the position of the object during movement."));
                    ntStyle.intValue = EditorGUILayout.Popup(ntStyle.intValue, new string[] { "Random", "Sine", "Smooth random" });
                    EditorGUILayout.EndHorizontal();

                    if (ntStyle.intValue == 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(new GUIContent("Offset", "The offset (delay) of the sine noise. Values equal with the remainder when divided by 2*pi."));
                        EditorGUILayout.PropertyField(sineOffset, new GUIContent(""));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Amplitude", "The amplitude (absolute of the maximum/minimum value) of the noise."));
                    EditorGUILayout.PropertyField(noiseAmplitude, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Frequency", "The frequency of the noise."));
                    EditorGUILayout.PropertyField(noiseFrequency, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel -= 1;
                }

                expandedNoiseRot = EditorGUILayout.Foldout(expandedNoiseRot, "Rotation", true);
                noiseRotExpanded.boolValue = expandedNoiseRot;

                if (expandedNoiseRot)
                {
                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Type", "The type of the noise that is applied to the rotation of the object during movement."));
                    rntStyle.intValue = EditorGUILayout.Popup(rntStyle.intValue, new string[] { "Random", "Sine", "Smooth random" });
                    EditorGUILayout.EndHorizontal();

                    if (rntStyle.intValue == 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(new GUIContent("Offset", "The offset (delay) of the sine noise. Values equal with the remainder when divided by 2*pi."));
                        EditorGUILayout.PropertyField(sineOffsetR, new GUIContent(""));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Amplitude", "The amplitude (absolute of the maximum/minimum value) of the noise in degrees."));
                    EditorGUILayout.PropertyField(noiseAmplitudeR, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Frequency", "The frequency of the noise."));
                    EditorGUILayout.PropertyField(noiseFrequencyR, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel -= 1;
                }

                expandedNoiseScale = EditorGUILayout.Foldout(expandedNoiseScale, "Scale", true);
                noiseScaleExpanded.boolValue = expandedNoiseScale;

                if (expandedNoiseScale)
                {
                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Type", "The type of the noise that is applied to the scale of the object during movement."));
                    sntStyle.intValue = EditorGUILayout.Popup(sntStyle.intValue, new string[] { "Random", "Sine", "Smooth random" });
                    EditorGUILayout.EndHorizontal();

                    if (sntStyle.intValue == 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(new GUIContent("Offset", "The offset (delay) of the sine noise. Values equal with the remainder when divided by 2*pi."));
                        EditorGUILayout.PropertyField(sineOffsetS, new GUIContent(""));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Amplitude", "The amplitude (absolute of the maximum/minimum value) of the noise in degrees."));
                    EditorGUILayout.PropertyField(noiseAmplitudeS, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Frequency", "The frequency of the noise."));
                    EditorGUILayout.PropertyField(noiseFrequencyS, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel -= 1;
                }

                EditorGUI.indentLevel -= 1;
            }
        }

        private void PropertyBounds()
        {
            //Checking the minimum/maximum value for some properties
            if (length.floatValue < 0.001f)
                length.floatValue = 0.001f;
            if (delayStartMin.floatValue < 0f)
                delayStartMin.floatValue = 0f;
            if (delayStartMax.floatValue < delayStartMin.floatValue)
                delayStartMax.floatValue = delayStartMin.floatValue;
            if (delayMin.floatValue < 0f)
                delayMin.floatValue = 0f;
            if (delayMax.floatValue < delayMin.floatValue)
                delayMax.floatValue = delayMin.floatValue;
            if (noiseAmplitude.vector3Value.x < 0)
                noiseAmplitude.vector3Value = new Vector3(0, noiseAmplitude.vector3Value.y, noiseAmplitude.vector3Value.z);
            if (noiseAmplitude.vector3Value.y < 0)
                noiseAmplitude.vector3Value = new Vector3(noiseAmplitude.vector3Value.x, 0, noiseAmplitude.vector3Value.z);
            if (noiseAmplitude.vector3Value.z < 0)
                noiseAmplitude.vector3Value = new Vector3(noiseAmplitude.vector3Value.x, noiseAmplitude.vector3Value.y, 0);
            if (noiseFrequency.vector3Value.x < 0)
                noiseFrequency.vector3Value = new Vector3(0, noiseFrequency.vector3Value.y, noiseFrequency.vector3Value.z);
            if (noiseFrequency.vector3Value.y < 0)
                noiseFrequency.vector3Value = new Vector3(noiseFrequency.vector3Value.x, 0, noiseFrequency.vector3Value.z);
            if (noiseFrequency.vector3Value.z < 0)
                noiseFrequency.vector3Value = new Vector3(noiseFrequency.vector3Value.x, noiseFrequency.vector3Value.y, 0);
            if (noiseAmplitudeR.vector3Value.x < 0)
                noiseAmplitudeR.vector3Value = new Vector3(0, noiseAmplitudeR.vector3Value.y, noiseAmplitudeR.vector3Value.z);
            if (noiseAmplitudeR.vector3Value.y < 0)
                noiseAmplitudeR.vector3Value = new Vector3(noiseAmplitudeR.vector3Value.x, 0, noiseAmplitudeR.vector3Value.z);
            if (noiseAmplitudeR.vector3Value.z < 0)
                noiseAmplitudeR.vector3Value = new Vector3(noiseAmplitudeR.vector3Value.x, noiseAmplitudeR.vector3Value.y, 0);
            if (noiseFrequencyR.vector3Value.x < 0)
                noiseFrequencyR.vector3Value = new Vector3(0, noiseFrequencyR.vector3Value.y, noiseFrequencyR.vector3Value.z);
            if (noiseFrequencyR.vector3Value.y < 0)
                noiseFrequencyR.vector3Value = new Vector3(noiseFrequencyR.vector3Value.x, 0, noiseFrequencyR.vector3Value.z);
            if (noiseFrequencyR.vector3Value.z < 0)
                noiseFrequencyR.vector3Value = new Vector3(noiseFrequencyR.vector3Value.x, noiseFrequencyR.vector3Value.y, 0);
            if (noiseAmplitudeS.vector3Value.x < 0)
                noiseAmplitudeS.vector3Value = new Vector3(0, noiseAmplitudeS.vector3Value.y, noiseAmplitudeS.vector3Value.z);
            if (noiseAmplitudeS.vector3Value.y < 0)
                noiseAmplitudeS.vector3Value = new Vector3(noiseAmplitudeS.vector3Value.x, 0, noiseAmplitudeS.vector3Value.z);
            if (noiseAmplitudeS.vector3Value.z < 0)
                noiseAmplitudeS.vector3Value = new Vector3(noiseAmplitudeS.vector3Value.x, noiseAmplitudeS.vector3Value.y, 0);
            if (noiseFrequencyS.vector3Value.x < 0)
                noiseFrequencyS.vector3Value = new Vector3(0, noiseFrequencyS.vector3Value.y, noiseFrequencyS.vector3Value.z);
            if (noiseFrequencyS.vector3Value.y < 0)
                noiseFrequencyS.vector3Value = new Vector3(noiseFrequencyS.vector3Value.x, 0, noiseFrequencyS.vector3Value.z);
            if (noiseFrequencyS.vector3Value.z < 0)
                noiseFrequencyS.vector3Value = new Vector3(noiseFrequencyS.vector3Value.x, noiseFrequencyS.vector3Value.y, 0);
        }

        private void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}