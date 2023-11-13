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
    public class AdjustmentSettings
    {
        public AdjustmentSettings(Vector3 scale, Vector3 offset, Vector3 min, Vector3 max)
        {
            Scale = scale;
            Offset = offset;
            Min = min;
            Max = max;
        }

        [Persistent] public Vector3 Scale;
        [Persistent] public Vector3 Offset;
        [Persistent] public Vector3 Min;
        [Persistent] public Vector3 Max;
    }

    public class AdjustmentProfile
    {
        static AdjustmentSettings MakeDefaultRotation()
        {
            // pitch, yaw roll
            return new AdjustmentSettings(
                new Vector3(1.0f, 1.0f, 0.0f),  // scale
                Vector3.zero,                   // offset
                new Vector3(-90f, -135f, -90f), // min limit
                new Vector3(90f, 135f, 90f));   // max limit
        }

        static AdjustmentSettings MakeDefaultTranslation()
        {
            // horizontal, vertical, fore/back
            return new AdjustmentSettings(
                new Vector3(1.0f, 1.0f, 1.0f), // scale
                Vector3.zero, // offset
                -0.2f * Vector3.one, // min limit
                0.2f * Vector3.one); // max limit
        }

		[Persistent] public AdjustmentSettings Rotation = MakeDefaultRotation();
        [Persistent] public AdjustmentSettings Translation = MakeDefaultTranslation();
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public partial class KerbTrack : MonoBehaviour
    {
        public static KerbTrack Instance { get; private set; }

        public static bool trackerEnabled = true;
        public static ITracker tracker;

        // [...]GameData\KerbTrack\Plugins\PluginData\KerbTrack\settings.cfg
        private string savePath = System.IO.Path.Combine(
            AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)), "settings.cfg");

        public static void ChangeTracker(Trackers t)
        {
            try
            {
                if(tracker != null)
                    tracker.Stop();

                switch (t)
                {
                    /*case Trackers.FreeTrack:
                        {
                            Debug.Log("[KerbTrack] Using FreeTrack");
                            tracker = new FreeTrackTracker();
                            break;
                        }*/
                    case Trackers.TrackIR:
                        {
                            Debug.Log("[KerbTrack] Using TrackIR");
                            tracker = new TrackIRTracker();
                            break;
                        }
                    /*case Trackers.OculusRift:
                        {
                            Debug.Log("[KerbTrack] Using Oculus Rift");
                            tracker = new OVRTracker();
                            break;
                        }*/
                    case Trackers.Joystick:
                        {
                            Debug.Log("KerbTrack: Using Joystick");
                            tracker = new JoystickTracker();
                            break;
                        }
                    case Trackers.OpentrackUdp:
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

        [Persistent] public Trackers activeTracker = Trackers.TrackIR;

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

        [Persistent] public AdjustmentProfile IVA = new AdjustmentProfile();
        [Persistent] public AdjustmentProfile Flight = new AdjustmentProfile();
        [Persistent] public AdjustmentProfile Map = new AdjustmentProfile();
        [Persistent] public AdjustmentProfile KSC = new AdjustmentProfile();
        [Persistent] public AdjustmentProfile Editor = new AdjustmentProfile();
        [Persistent] public AdjustmentProfile MainMenu = new AdjustmentProfile();

        #endregion Persistence

        // raw values from the tracker
        public Vector3 InputRotation;
        public Vector3 InputTranslation;
		// Values after applying the adjustment settings
		public Vector3 OutputRotation;
        public Vector3 OutputTranslation;

        void Update()
        {
            if (Input.GetKeyDown(toggleEnabledKey))
                trackerEnabled = !trackerEnabled;
            if (Input.GetKeyDown(resetOrientationKey))
                tracker.ResetOrientation();
        }

		void LateUpdate()
		{
			if (!trackerEnabled)
				return;

			if (tracker != null)
			{
				InputRotation = Vector3.zero;
				InputTranslation = Vector3.zero;
				try
				{
					tracker.GetData(ref InputRotation, ref InputTranslation);
				}
				catch (Exception e)
				{
					Debug.Log("[KerbTrack] " + activeTracker + " error: " + e.Message + "\n" + e.StackTrace);
					trackerEnabled = false;
					return;
				}

				var currentScene = GetCurrentScene();
				var currentProfile = GetProfile(currentScene);

				OutputRotation = ApplyAdjustments(InputRotation, currentProfile.Rotation);
				OutputTranslation = ApplyAdjustments(InputTranslation, currentProfile.Translation);

				// should this be a delegate table or something..?  It's unlikely that we'll ever add new ones...
				switch (currentScene)
				{
					case TrackerScene.Flight: ApplyFlight(); break;
					case TrackerScene.IVA: ApplyIVA(); break;
					case TrackerScene.Map: ApplyMap(); break;
					case TrackerScene.KSC: ApplyKSC(); break;
					case TrackerScene.Editor: ApplyEditor(); break;
					case TrackerScene.MainMenu: ApplyMainMenu(); break;
				}
			}
		}

		static TrackerScene GetCurrentScene()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.EDITOR: return TrackerScene.Editor;
                case GameScenes.MAINMENU: return TrackerScene.MainMenu;
                case GameScenes.SPACECENTER: return TrackerScene.KSC;
            }

            switch (CameraManager.Instance.currentCameraMode)
            {
                case CameraManager.CameraMode.Map: return TrackerScene.Map;
                case CameraManager.CameraMode.IVA:
                case CameraManager.CameraMode.Internal: return TrackerScene.IVA;
                default:
                    return TrackerScene.Flight;
            }
        }

        AdjustmentProfile GetProfile(TrackerScene scene)
        {
            switch (scene)
            {
                case TrackerScene.Flight: return Flight;
                case TrackerScene.IVA: return IVA;
                case TrackerScene.Map: return Map;
                case TrackerScene.KSC: return KSC;
                case TrackerScene.Editor: return Editor;
                case TrackerScene.MainMenu: return MainMenu;
                default: return Flight;
            }
        }

		#region Application functions

		void ApplyFlight()
        {
			if (!externalTrackingEnabled) return;

			if (activeTracker == Trackers.Joystick)
			{
				Vector2 joyCamPos = new Vector3(0, 0);
				((JoystickTracker)tracker).GetFlightCamData(ref joyCamPos);
				bool relative = true;
				if (relative)
				{
					FlightCamera.fetch.camPitch += -joyCamPos.x * Flight.Rotation.Scale.x * Time.deltaTime;
					FlightCamera.fetch.camHdg += -joyCamPos.y * Flight.Rotation.Scale.y * Time.deltaTime;
				}
				else
				{
					FlightCamera.fetch.camPitch = -joyCamPos.x * Flight.Rotation.Scale.x;
					FlightCamera.fetch.camHdg = -joyCamPos.y * Flight.Rotation.Scale.y;
				}
			}
			else
			{
				bool freeLook = true;
				if (freeLook)
				{
					FlightCamera.fetch.transform.localEulerAngles = OutputRotation;
				}
				else
				{
                    // Orbit around the vessel in the same way as the stock camera.
                    FlightCamera.fetch.camPitch = OutputRotation.x;
                    FlightCamera.fetch.camHdg = OutputRotation.y;
				}
			}
		}

        void ApplyIVA()
        {
			InternalCamera.Instance.transform.localEulerAngles = OutputRotation;
            InternalCamera.Instance.transform.localPosition = OutputTranslation;

			// Without setting the flight camera transform, the pod rotates about without changing the background.
			FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
			FlightCamera.fetch.transform.position = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.position);
		}

        void ApplyMap()
        {
			if (!mapTrackingEnabled) return;
            PlanetariumCamera.fetch.camPitch = OutputRotation.x;
            PlanetariumCamera.fetch.camHdg = OutputRotation.y;
		}

        void ApplyKSC()
        {

        }

        void ApplyEditor()
        {

        }

        void ApplyMainMenu()
        {

        }

        Vector3 ApplyAdjustments(Vector3 input, AdjustmentSettings settings)
        {
            Vector3 adjusted = Mult(input, settings.Scale) + settings.Offset;
            return Clamp(adjusted, settings.Min, settings.Max);
        }

        #endregion

		static Vector3 Mult(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		static Vector3 Clamp(Vector3 input, Vector3 min, Vector3 max)
		{
			return new Vector3(
				Mathf.Clamp(input.x, min.x, max.x),
				Mathf.Clamp(input.y, min.y, max.y),
				Mathf.Clamp(input.z, min.z, max.z));
		}
	}
}
