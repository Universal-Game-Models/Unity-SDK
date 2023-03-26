using UnityEngine;

public class SetFollowCamera : MonoBehaviour
{
    public Cinemachine.CinemachineVirtualCamera vc;
    public void SetCamera(object obj)
    {
        GameObject avatar = (GameObject)obj;
        vc.Follow = avatar.transform.GetChild(0);
    }
}
