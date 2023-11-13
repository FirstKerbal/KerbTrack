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
                ProfileGui(IVA);
            if (_showFlightGui)
                ProfileGui(Flight);
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

        private void ProfileGui(AdjustmentProfile profile)
        {
            AdjustmentGui(true, InputRotation, profile.Rotation, OutputRotation);
            AdjustmentGui(false, InputTranslation, profile.Translation, OutputTranslation);
        }

        private void AdjustmentGui(bool rotation, Vector3 input, AdjustmentSettings settings, Vector3 output)
        {
            string label = rotation ? "Rotation" : "Translation";
            string xLabel = rotation ? "Pitch" : "Left/Right";
            string yLabel = rotation ? "Yaw" : "Up/Down";
            string zLabel = rotation ? "Roll" : "Fore/Back";
            float offsetRange = rotation ? 90f : 2f;


            GUILayout.Label($"Input {label}");
            GuiUtils.LabelValue(xLabel, input.x);
            GuiUtils.LabelValue(yLabel, input.y);
            GuiUtils.LabelValue(zLabel, input.z);

            GUILayout.Label($"Output {label}");
            GuiUtils.LabelValue(xLabel, output.x);
            GuiUtils.LabelValue(yLabel, output.y);
            GuiUtils.LabelValue(zLabel, output.z);

            GUILayout.Label($"<b>{label} Scale</b>");
            GuiUtils.Slider(xLabel, ref settings.Scale.x, 0.0f, 5.0f);
            GuiUtils.Slider(yLabel, ref settings.Scale.y, 0.0f, 5.0f);
            GuiUtils.Slider(zLabel, ref settings.Scale.z, 0.0f, 5.0f);

            GUILayout.Label($"<b>{label} Offset</b>");
            GuiUtils.Slider(xLabel, ref settings.Offset.x, -offsetRange, offsetRange);
            GuiUtils.Slider(yLabel, ref settings.Offset.y, -offsetRange, offsetRange);
            GuiUtils.Slider(zLabel, ref settings.Offset.z, -offsetRange, offsetRange);

            GUILayout.Label($"<b>{label} Limits</b>");
            GuiUtils.Slider($"Min {xLabel}", ref settings.Min.x, -2 * offsetRange, 2 * offsetRange);
            GuiUtils.Slider($"Max {xLabel}", ref settings.Max.x, -2 * offsetRange, 2 * offsetRange);
            GuiUtils.Slider($"Min {yLabel}", ref settings.Min.y, -2 * offsetRange, 2 * offsetRange);
            GuiUtils.Slider($"Max {yLabel}", ref settings.Max.y, -2 * offsetRange, 2 * offsetRange);
            GuiUtils.Slider($"Min {zLabel}", ref settings.Min.z, -2 * offsetRange, 2 * offsetRange);
            GuiUtils.Slider($"Max {zLabel}", ref settings.Max.z, -2 * offsetRange, 2 * offsetRange);
        }

        private bool _showIvaGui = false;
        

        private bool _showFlightGui = false;

        private bool _showMapGui = false;

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
