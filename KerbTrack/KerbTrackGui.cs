using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbTrack
{
    partial class KerbTrack
    {
        public bool guiVisible = false;
        private Rect windowPos = new Rect(Screen.width / 4, Screen.height / 4, 10f, 10f);
        private string[] trackerNames = Enum.GetNames(typeof(Trackers));
        public const int MaxAxisNum = 19;

        private void MainGUI(int windowID)
        {
            GUILayout.BeginVertical();

            string statusText = (trackerEnabled ? "Enabled" : "Disabled") +
                " (" + toggleEnabledKey + ")";
            GUILayout.Label(statusText);

            //if (activeTracker == (int)Trackers.Joystick)

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("IVA config"))
            {
                _showIvaGui = true;
                _showFlightGui = false;
                _showJoystickGui = false;
                _showSettingsGui = false;
            }
            if (GUILayout.Button("Flight config"))
            {
                _showIvaGui = false;
                _showFlightGui = true;
                _showJoystickGui = false;
                _showSettingsGui = false;
            }
            if (GUILayout.Button("Joystick config"))
            {
                _showIvaGui = false;
                _showJoystickGui = true;
                _showFlightGui = false;
                _showSettingsGui = false;
            }
            if (GUILayout.Button("Settings"))
            {
                _showIvaGui = false;
                _showFlightGui = false;
                _showJoystickGui = false;
                _showSettingsGui = true;
            }
            GUILayout.EndHorizontal();

            if (_showJoystickGui)
                JoystickGui();
            if (_showIvaGui)
                IvaGui();
            if (_showFlightGui)
                FlightGui();
            if (_showSettingsGui)
                SettingsGui();

            /*if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal ||
                CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)*/

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void OnGUI(int instanceId)
        {
            if (guiVisible)
            {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(instanceId, windowPos, MainGUI, "KerbTrack", GUILayout.Width(500), GUILayout.Height(50));
            }
        }

        private bool _showIvaGui = false;
        private void IvaGui()
        {
            GuiUtils.LabelValue("IVA Pitch", pv);
            GuiUtils.LabelValue("IVA Yaw", yv);
            GuiUtils.LabelValue("IVA Roll", rv);

            GUILayout.Label("<b>Scale</b>");
            GuiUtils.SliderScale("IVA Pitch", ref pitchScaleIVA);
            GuiUtils.SliderScale("IVA Yaw", ref yawScaleIVA);
            GuiUtils.SliderScale("IVA Roll", ref rollScaleIVA);

            GUILayout.Label("<b>Offset</b>");
            GuiUtils.SliderOffset("IVA Pitch", ref pitchOffsetIVA);
            GuiUtils.SliderOffset("IVA Yaw", ref yawOffsetIVA);
            GuiUtils.SliderOffset("IVA Roll", ref rollOffsetIVA);

            GuiUtils.LabelValue("IVA Left-Right", xp);
            GuiUtils.LabelValue("IVA Up-Down", yp);
            GuiUtils.LabelValue("IVA In-Out", zp);

            GUILayout.Label("<b>Scale</b>");
            GuiUtils.SliderScale("Left/Right (X)", ref xScale);
            GuiUtils.SliderScale("Up/Down (Y)", ref yScale);
            GuiUtils.SliderScale("In/Out (Z)", ref zScale);

            GUILayout.Label("<b>Offset</b>");
            GuiUtils.Slider("Left/Right (X)", ref xOffset, xMinIVA, xMaxIVA);
            GuiUtils.Slider("Up/Down (Y)", ref yOffset, yMinIVA, yMaxIVA);
            GuiUtils.Slider("In/Out (Z)", ref zOffset, zMinIVA, zMaxIVA);
        }

        private bool _showFlightGui = false;
        private void FlightGui()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Flight Pitch");
            GUILayout.Label(pv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(pitchScaleFlight.ToString());
            pitchScaleFlight = GUILayout.HorizontalSlider(pitchScaleFlight, 0, 1);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Flight Yaw");
            GUILayout.Label(yv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(yawScaleFlight.ToString());
            yawScaleFlight = GUILayout.HorizontalSlider(yawScaleFlight, 0, 1);
        }

        private bool _showMapGui = false;
        private void MapGui()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Map Pitch");
            GUILayout.Label(pv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(pitchScaleMap.ToString());
            pitchScaleMap = GUILayout.HorizontalSlider(pitchScaleMap, 0, 1);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Map Yaw");
            GUILayout.Label(yv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(yawScaleMap.ToString());
            yawScaleMap = GUILayout.HorizontalSlider(yawScaleMap, 0, 1);
        }

        private bool _showJoystickGui = false;
        private void JoystickGui()
        {
            string[] joysticks = Input.GetJoystickNames();
            if (joysticks.Length == 0)
            {
                GUILayout.Label("<b>No joysticks detected!</b>");
                return;
            }

            // Joystick selection.
            if (joystickId >= joysticks.Length)
                joystickId = 0;
            GUILayout.Label("Active joystick");
            GUILayout.Label(joystickId + " - " + joysticks[joystickId]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous joystick"))
                joystickId--;
            if (GUILayout.Button("Next joystick"))
                joystickId++;
            GUILayout.EndHorizontal();
            if (joystickId >= joysticks.Length)
                joystickId = 0;
            if (joystickId < 0)
                joystickId = joysticks.Length - 1;
            GUILayout.Space(10);

            SelectAxis(ref joyYawAxisId, ref joyYawInverted, "Yaw");
            SelectAxis(ref joyPitchAxisId, ref joyPitchInverted, "Pitch");
            SelectAxis(ref joyRollAxisId, ref joyRollInverted, "Roll");
            SelectAxis(ref joyXAxisId, ref joyXInverted, "X");
            SelectAxis(ref joyYAxisId, ref joyYInverted, "Y");
            SelectAxis(ref joyZAxisId, ref joyZInverted, "Z");
            SelectAxis(ref joyCamOrbitAxisId, ref joyCamOrbitInverted, "Flight Camera Orbit");
            SelectAxis(ref joyCamPitchAxisId, ref joyCamPitchInverted, "Flight Camera Pitch");
        }

        private void SelectAxis(ref int axisId, ref bool axisInverted, string axisName)
        {
            string label = axisId == -1 ? "Disabled" : axisId.ToString();
            GuiUtils.LabelValue(axisName + " axis", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous " + axisName + " Axis"))
                axisId--;
            if (GUILayout.Button("Next " + axisName + " Axis"))
                axisId++;
            axisInverted = GUILayout.Toggle(axisInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (axisId > MaxAxisNum)
                axisId = 0;
            if (axisId < -1)
                axisId = MaxAxisNum;
        }

        private bool _showSettingsGui = false;
        private void SettingsGui()
        {
            mapTrackingEnabled = GUILayout.Toggle(mapTrackingEnabled, "Enabled in map view");
            externalTrackingEnabled = GUILayout.Toggle(externalTrackingEnabled, "Enabled in external view");

            Trackers oldTracker = activeTracker;
            activeTracker = (Trackers)GuiUtils.RadioButton(trackerNames, (int)activeTracker);
            if (oldTracker != activeTracker)
                ChangeTracker(activeTracker);

            switch (activeTracker)
            {
                /*case Trackers.FreeTrack:
                    GUILayout.Label("<b>FreeTrack</b>\r\nThis is used for FaceTrackNoIR. Freetrackclient.dll must be placed next to KSP.exe, and must be a 64-bit version if 64-bit KSP is used.");
                    break;*/
                case Trackers.TrackIR:
                    GUILayout.Label("<b>TrackIR</b>\r\nSupports TrackIR and other systems which emulate it, such as opentrack.\r\n" +
                        "<b>opentrack</b>\r\nWhen using opentrack, select the Input tracker appripriate to your hardware setup, and select \"freetrack 2.0 Enhanced\" as the Output.\r\n" +
                        "In the Output settings, ensure \"Use TrackIR\" or \"Enable both\" is selected.");
                    break;
                /*case Trackers.OculusRift:
                    GUILayout.Label("<b>Oculus Rift</b>\r\nRequires an older version of the Oculus Rift runtime (2015), and only 64-bit is supported.\r\n" + 
                        "It's recommended to select \"TrackIR\" as your tracker and use opentrack instead.\r\n" +
                        "Place \"Oculus OVR PosRotWrapper 64-bit.dll\" next to KSP.exe.");
                    break;*/
                case Trackers.Joystick:
                    GUILayout.Label("<b>Joystick</b>\r\nUse your joystick axes as input. Good for assigning to a spare axis on a joystick if you don't have a head tracker.\r\n" +
                        "If you have a head tracker that isn't supported, try setting it to output as a joystick and using this setting to receive it in KerbTrack.");
                    break;
                case Trackers.OpentrackUdp:
                    GUILayout.Label("<b>Opentrack Udp</b>\r\n Supports opentrack's udp protocol, listening on port 4242.");
                    break;
            }
        }
    }
}
