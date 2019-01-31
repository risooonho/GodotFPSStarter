using Godot;
using System;

public class WeaponKnife : WeaponPoint
{
    public override bool UnequipWeapon() {
        return false;
    }

    public override bool EquipWeapon() {
        return false;
    }

    public override void FireWeapon() {
        //
    }
}
