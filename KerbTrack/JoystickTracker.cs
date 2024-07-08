using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbTrack
{
    public class JoystickTracker : ITracker
    {
		public JoystickTracker()
        {
            Debug.Log("[KerbTrack] Initialising Joystick tracker...");
            string[] joysticks = Input.GetJoystickNames();
            Debug.Log("Joysticks available: ");
            for (int i = 0; i < joysticks.Length; i++)
            {
                Debug.Log(i + " - " + joysticks[i]);
            }
        }

        public void GetData(ref Vector3 rot, ref Vector3 pos)
        {
            KerbTrack kerbTrack = KerbTrack.Instance;

            if (kerbTrack.joyPitchAxisId != -1)
            {
                string pitchAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyPitchAxisId;
                rot.x = Input.GetAxis(pitchAxis) * 200;
                if (kerbTrack.joyPitchInverted)
                    rot.x *= -1;
            }
            if (kerbTrack.joyYawAxisId != -1)
            {
                string yawAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyYawAxisId;
                rot.y = Input.GetAxis(yawAxis) * 200;
                if (kerbTrack.joyYawInverted)
                    rot.y *= -1;
            }
            if (kerbTrack.joyRollAxisId != -1)
            {
                string rollAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyRollAxisId;
                rot.z = Input.GetAxis(rollAxis) * 200;
                if (kerbTrack.joyRollInverted)
                    rot.z *= -1;
            }
            if (kerbTrack.joyXAxisId != -1)
            {
                string xAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyXAxisId;
                pos.x = Input.GetAxis(xAxis);
                if (kerbTrack.joyXInverted)
                    pos.x *= -1;
            }
            if (kerbTrack.joyYAxisId != -1)
            {
                string yAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyYAxisId;
                pos.y = Input.GetAxis(yAxis);
                if (kerbTrack.joyYInverted)
                    pos.y *= -1;
            }
            if (kerbTrack.joyZAxisId != -1)
            {
                string zAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyZAxisId;
                pos.z = Input.GetAxis(zAxis);
                if (kerbTrack.joyZInverted)
                    pos.z *= -1;
            }
        }

        public void GetFlightCamData(ref Vector2 rot)
        {
			KerbTrack kerbTrack = KerbTrack.Instance;

			if (kerbTrack.joyCamPitchAxisId != -1)
            {
                string pitchAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyCamPitchAxisId;
                rot.x = Deadzone(Input.GetAxis(pitchAxis)) * 5;

                if (kerbTrack.joyCamPitchInverted)
                    rot.x *= -1;
            }
            if (kerbTrack.joyCamOrbitAxisId != -1)
            {
                string orbitAxis = "joy" + kerbTrack.joystickId + "." + kerbTrack.joyCamOrbitAxisId;
                rot.y = Deadzone(Input.GetAxis(orbitAxis)) * 5;
                if (kerbTrack.joyCamOrbitInverted)
                    rot.y *= -1;
            }
        }

        private float Deadzone(float val, float deadzone = 0.2f)
        {
            if (val > -deadzone && val < deadzone)
            {
                return 0f;
            }
            return val;
        }

        public void ResetOrientation() { }
        public void Stop() { }
    }
}
