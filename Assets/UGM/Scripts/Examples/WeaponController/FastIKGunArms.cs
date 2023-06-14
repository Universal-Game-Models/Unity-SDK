#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class FastIKGunArms : FastIKFabric
{
    // Custom Initialize function
    public void Initialize(int chainLength, Transform target, Transform pole)
    {
        ChainLength = chainLength;
        Target = target;
        Pole = pole;
        Init();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        Vector3 targetDirection = Target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Target.rotation = targetRotation;
    }

}
