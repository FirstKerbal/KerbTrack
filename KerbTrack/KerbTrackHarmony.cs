using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbTrack
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class KerbTrackHarmony : MonoBehaviour
	{
		void Awake()
		{
			var harmony = new Harmony("KerbTrack");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(FlightCamera), nameof(FlightCamera.UpdateCameraTransform))]
	static class FlightCamera_UpdateCameraTransform_Patch
	{
		static void Postfix(FlightCamera __instance)
		{
			KerbTrack kerbTrack = KerbTrack.Instance;

			if (!kerbTrack.externalTrackingEnabled) return;

			// for some reason this leads to pretty extreme jittering, even if you just replace the tracked position with a constant offset.  There's probably some kind of feedback loop somewhere.
			//__instance.transform.localPosition += kerbTrack.OutputTranslation;
			

			__instance.transform.Rotate(Vector3.up, kerbTrack.OutputRotation.y, Space.Self);
			__instance.transform.Rotate(Vector3.right, kerbTrack.OutputRotation.x, Space.Self);
			__instance.transform.Rotate(Vector3.forward, kerbTrack.OutputRotation.z, Space.Self);

			
		}
	}
}
