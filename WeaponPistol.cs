using Godot;
using System;

public class WeaponPistol : WeaponPoint
{
    private const float damage = 15;
    private PackedScene bulletScene =  ResourceLoader.Load("Bullet_Scene.tscn") as PackedScene;

    public AnimationPlayer animationPlayer;

    public override void _Ready() {
        animationPlayer = GetNode<Player>("../../../").GetAnimationPlayer();

        if (animationPlayer is AnimationPlayer) {
            GD.Print("animation player not null in equip weapon");
        } else {
            if (animationPlayer != null) {
                GD.Print("animation player is not instance in equip weapon: " + animationPlayer.GetType());
            } else {
                GD.Print("animation player is null in equip");
            
            }
        }
    }

    public override void FireWeapon() {
        var clone = bulletScene.Instance() as Bullet;
        var sceneRoot = GetTree().Root.GetChildren()[0] as Spatial;
        sceneRoot.AddChild(clone);

        clone.GlobalTransform = this.GlobalTransform;
        clone.Scale = new Vector3(4, 4, 4);
        clone.bulletDamage = damage;
    }

    public override bool EquipWeapon() {
        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.pistolIdle) {
            isEnabled = true;
            return true;
        }

        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.idleUnarmed) {
            animationPlayer.SetAnimation(AnimationPlayer.pistolEquip);
        }

        return false;
    }

    public override bool UnequipWeapon() {
        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.pistolIdle) {
            animationPlayer.SetAnimation(AnimationPlayer.pistolUnequip);
        }

        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.idleUnarmed) {
            isEnabled = false;
            return true;
        } else {
            return false;
        }
    }

}

public abstract class WeaponPoint : Spatial {
    public bool isEnabled = false;

    public abstract bool UnequipWeapon();
    public abstract bool EquipWeapon();
    public abstract void FireWeapon();
}
