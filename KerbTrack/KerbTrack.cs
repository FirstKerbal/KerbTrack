/*
 * InternalCamera:
 * Rotation X+ = rotate head down to feet.
 * Rotation Y+ = rotate head right
 * Rotation Z+ = rotate head anti-clockwise
 * Translation X+ = Right
 * Translation Y+ = Up
 * Translation Z+ = Away
 * 
 * FlightCamera: 
 * Pitch: Looking down in positive, looking up is negative
 * Heading: From above, rotating the craft anti-clockwise is positive, clockwise is negative.
 */

using System;
using System.Reflection;
using UnityEngine;

namespace KerbTrack
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public partial class KerbTrack : MonoBehaviour
    {
        public static KerbTrack Instance { get; private set; }

        public static bool trackerEnabled = true;
        public static ITracker tracker;

        // [...]GameData\KerbTrack\Plugins\PluginData\KerbTrack\settings.cfg
        private string savePath = System.IO.Path.Combine(
            AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)), "settings.cfg");

        public static void ChangeTracker(Enums.Trackers t)
        {
            try
            {
                if(tracker != null)
                    tracker.Stop();

                switch (t)
                {
                    /*case Enums.Trackers.FreeTrack:
                        {
                            Debug.Log("[KerbTrack] Using FreeTrack");
                            tracker = new FreeTrackTracker();
                            break;
                        }*/
                    case Enums.Trackers.TrackIR:
                        {
                            Debug.Log("[KerbTrack] Using TrackIR");
                            tracker = new TrackIRTracker();
                            break;
                        }
                    /*case Enums.Trackers.OculusRift:
                        {
                            Debug.Log("[KerbTrack] Using Oculus Rift");
                            tracker = new OVRTracker();
                            break;
                        }*/
                    case Enums.Trackers.Joystick:
                        {
                            Debug.Log("KerbTrack: Using Joystick");
                            tracker = new JoystickTracker();
                            break;
                        }
                    case Enums.Trackers.OpentrackUdp:
                        {
                            Debug.Log("KerbTrack: Using OpentrackUdp");
                            tracker = new OpentrackUdpTracker();
                            break;
                        }
                }

                trackerEnabled = true;
            }
            catch (Exception)
            {
                trackerEnabled = false;
                throw;
            }
        }

        void Start()
        {
            Instance = this;

            Debug.Log("[KerbTrack] Starting");
            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnPause);
            LoadSettings();
            ChangeTracker(activeTracker);
        }

        public void OnDestroy()
        {
            Instance = null;

            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnPause);
            SaveSettings();
        }

#region GUI

        public void OnPause()
        {
            guiVisible = true;
        }

        public void OnUnPause()
        {
            guiVisible = false;
        }

        public void OnGUI()
        {
            OnGUI(GetInstanceID());
        }

#endregion GUI

#region Persistence

        public void SaveSettings()
        {
            Debug.Log("[KerbTrack] Saving settings to " + savePath);
            var node = new ConfigNode();
            
            node.name = "KERBTRACK_SETTINGS";

            ConfigNode.CreateConfigFromObject(this, node);

            node.Save(savePath);
        }

        public void LoadSettings()
        {
            Debug.Log("KerbTrack: Loading settings from " + savePath);
            var node = ConfigNode.Load(savePath);

            if (node != null)
            {
                ConfigNode.LoadObjectFromConfig(this, node);
            }
        }

        [Persistent] public Enums.Trackers activeTracker = Enums.Trackers.TrackIR;

        [Persistent] public KeyCode toggleEnabledKey = KeyCode.ScrollLock;
        [Persistent] public KeyCode resetOrientationKey = KeyCode.Home;
        [Persistent] public bool externalTrackingEnabled = true;
        [Persistent] public bool mapTrackingEnabled = true;

        [Persistent] public int joystickId = 0;
        [Persistent] public int joyYawAxisId = 0;
        [Persistent] public bool joyYawInverted = true;
        [Persistent] public int joyPitchAxisId = 1;
        [Persistent] public bool joyPitchInverted = true;
        [Persistent] public int joyRollAxisId = -1;
        [Persistent] public bool joyRollInverted = false;
        [Persistent] public int joyXAxisId = 4;
        [Persistent] public bool joyXInverted = false;
        [Persistent] public int joyYAxisId = 4;
        [Persistent] public bool joyYInverted = true;
        [Persistent] public int joyZAxisId = 2;
        [Persistent] public bool joyZInverted = false;
        [Persistent] public int joyCamPitchAxisId = -1;
        [Persistent] public bool joyCamPitchInverted = false;
        [Persistent] public int joyCamOrbitAxisId = -1;
        [Persistent] public bool joyCamOrbitInverted = false;

        [Persistent] public float pitchScaleIVA = 0.3f;
        [Persistent] public float pitchOffsetIVA = 0.0f;
        [Persistent] public float yawScaleIVA = 0.3f;
        [Persistent] public float yawOffsetIVA = 0.0f;
        [Persistent] public float rollScaleIVA = 0.15f;
        [Persistent] public float rollOffsetIVA = 0.0f;
        [Persistent] public float xScale = 0.1f;
        [Persistent] public float xOffset = 0.0f;
        [Persistent] public float yScale = 0.1f;
        [Persistent] public float yOffset = 0.0f;
        [Persistent] public float zScale = 0.1f;
        [Persistent] public float zOffset = 0.0f;
        [Persistent] public float pitchScaleFlight = 0.01f;
        [Persistent] public float yawScaleFlight = 0.01f;
        [Persistent] public float pitchScaleMap = 0.01f;
        [Persistent] public float yawScaleMap = 0.01f;

        // Ignore the built-in max/min values.

        [Persistent] public float pitchMaxIVA = 120f;
        [Persistent] public float pitchMinIVA = -90f;
        [Persistent] public float yawMaxIVA = 135f;
        [Persistent] public float yawMinIVA = -135f;
        [Persistent] public float rollMaxIVA = 90f;
        [Persistent] public float rollMinIVA = -90f;
        [Persistent] public float xMaxIVA = 0.15f;
        [Persistent] public float xMinIVA = -0.15f;
        [Persistent] public float yMaxIVA = 0.1f;
        [Persistent] public float yMinIVA = -0.1f;
        [Persistent] public float zMaxIVA = 0.1f;
        [Persistent] public float zMinIVA = -0.15f;

    #endregion Persistence

        // Values after scaling.
        public static float pv = 0f;
        public static float yv = 0f;
        public static float rv = 0f;
        public static float xp = 0f;
        public static float yp = 0f;
        public static float zp = 0f;

        Quaternion lastRotation = Quaternion.identity;

        void Update()
        {
            if (Input.GetKeyDown(toggleEnabledKey))
                trackerEnabled = !trackerEnabled;
            if (Input.GetKeyDown(resetOrientationKey))
                tracker.ResetOrientation();

            if (!trackerEnabled)
                return;

            if (tracker != null)
            {
                Vector3 rot = new Vector3(0, 0, 0);
                Vector3 pos = new Vector3(0, 0, 0);
                try
                {
                    tracker.GetData(ref rot, ref pos);
                }
                catch (Exception e)
                {
                    Debug.Log("[KerbTrack] " + activeTracker + " error: " + e.Message + "\n" + e.StackTrace);
                    trackerEnabled = false;
                    return;
                }
                float pitch = (float)rot.x;
                float yaw = (float)rot.y;
                float roll = (float)rot.z;
                float x = pos.x;
                float y = pos.y;
                float z = pos.z;

                switch (CameraManager.Instance.currentCameraMode)
                {
                    case CameraManager.CameraMode.External:
                        {
                            break;
                        }
                    case CameraManager.CameraMode.Flight:
                        {
                            if (!externalTrackingEnabled) return;

                            if (activeTracker == Enums.Trackers.Joystick)
                            {
                                Vector2 joyCamPos = new Vector3(0, 0);
                                ((JoystickTracker)tracker).GetFlightCamData(ref joyCamPos);
                                bool relative = true;
                                if (relative)
                                {
                                    FlightCamera.fetch.camPitch += -joyCamPos.x * pitchScaleFlight * Time.deltaTime;
                                    FlightCamera.fetch.camHdg += -joyCamPos.y * yawScaleFlight * Time.deltaTime;
                                }
                                else
                                {
                                    FlightCamera.fetch.camPitch = -joyCamPos.x * pitchScaleFlight;
                                    FlightCamera.fetch.camHdg = -joyCamPos.y * yawScaleFlight;
                                }
                            }
                            else
                            {
                                bool freeLook = true;
                                if (freeLook)
                                {
                                    pv = pitch * pitchScaleIVA + pitchOffsetIVA;
                                    yv = yaw * yawScaleIVA + yawOffsetIVA;
                                    rv = roll * rollScaleIVA + rollOffsetIVA;
                                    xp = x * xScale + xOffset;
                                    yp = y * yScale + yOffset;
                                    zp = z * -zScale + zOffset;
                                    FlightCamera.fetch.transform.localEulerAngles = new Vector3(-pv, -yv, rv);
									break;
                                }
                                else
                                {
                                    // Orbit around the vessel in the same way as the stock camera.
                                    FlightCamera.fetch.camPitch = -pitch * pitchScaleFlight;
                                    FlightCamera.fetch.camHdg = -yaw * yawScaleFlight;
                                }
                            }
                            pv = pitch * pitchScaleFlight;
                            yv = yaw * yawScaleFlight;
                            break;
                        }
                    case CameraManager.CameraMode.Internal: // Window zoom cameras
                    case CameraManager.CameraMode.IVA: // Main IVA cameras
                        {
                            pv = pitch * pitchScaleIVA + pitchOffsetIVA;
                            yv = yaw * yawScaleIVA + yawOffsetIVA;
                            rv = roll * rollScaleIVA + rollOffsetIVA;
                            xp = x * xScale + xOffset;
                            yp = y * yScale + yOffset;
                            zp = z * -zScale + zOffset;
                            InternalCamera.Instance.transform.localEulerAngles = new Vector3(
                                -Mathf.Clamp(pv, pitchMinIVA, pitchMaxIVA),
                                -Mathf.Clamp(yv, yawMinIVA, yawMaxIVA),
                                Mathf.Clamp(rv, rollMinIVA, rollMaxIVA));

                            InternalCamera.Instance.transform.localPosition = new Vector3(
                                Mathf.Clamp(xp, xMinIVA, xMaxIVA),
                                Mathf.Clamp(yp, yMinIVA, yMaxIVA),
                                Mathf.Clamp(zp, zMinIVA, zMaxIVA));
                            // Without setting the flight camera transform, the pod rotates about without changing the background.
                            FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
                            FlightCamera.fetch.transform.position = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.position);
                            break;
                        }
                    case CameraManager.CameraMode.Map:
                        {
                            if (!mapTrackingEnabled) return;
                            PlanetariumCamera.fetch.camPitch = -pitch * pitchScaleMap;
                            PlanetariumCamera.fetch.camHdg = -yaw * yawScaleMap;
                            pv = pitch * pitchScaleMap;
                            yv = yaw * yawScaleMap;
                            break;
                        }
                }
            }
        }
    }
}
