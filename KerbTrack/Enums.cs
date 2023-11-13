using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbTrack
{
   public enum Trackers
    {
        //FreeTrack,
        TrackIR,
        //OculusRift,
        Joystick,
        OpentrackUdp
    }

    public enum TrackerScene
    {
        Flight,
        IVA,
        Map,
        KSC,
        Editor,
        MainMenu
    }
}
