using UnityEngine;

public interface ITracker
{
    void GetData(ref Vector3 rot, ref Vector3 pos);

    void ResetOrientation();

    void Stop();
}
