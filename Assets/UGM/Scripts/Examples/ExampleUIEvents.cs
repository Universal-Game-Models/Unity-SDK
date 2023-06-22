using UGM.Core;
using UGM.Examples.WeaponController;
using UnityEngine.Events;

public abstract class ExampleUIEvents
{
    public static UnityEvent<bool> OnShowCursor = new UnityEvent<bool>();
    public static UnityEvent<WeaponType> OnWeaponDeterminedType = new UnityEvent<WeaponType>();
    public static UnityEvent<UGMDataTypes.TokenInfo> OnChangeEquipment = new UnityEvent<UGMDataTypes.TokenInfo>();
}
