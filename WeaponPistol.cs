using Godot;
using System;

public class WeaponPistol : Spatial
{
    private const float damage = 15;

    private bool weaponEnabled = false;
    private PackedScene bulletScene =  ResourceLoader.Load("Bullet_Scene.tscn") as PackedScene;

    public AnimationPlayer animationPlayer;

    public override void _Ready() {
        animationPlayer = GetTree().Root.GetNode<Player>("Player").GetAnimationPlayer();
    }

    public void FireWeapon() {
        var clone = bulletScene.Instance() as Bullet;
        var sceneRoot = GetTree().Root.GetChildren()[0] as Spatial;
        sceneRoot.AddChild(clone);

        clone.GlobalTransform = this.GlobalTransform;
        clone.Scale = new Vector3(4, 4, 4);
        clone.bulletDamage = damage;
    }

    public bool EquipWeapon() {
        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.pistolIdle) {
            weaponEnabled = true;
            return true;
        }

        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.idleUnarmed) {
            animationPlayer.SetAnimation(AnimationPlayer.pistolEquip);
        }

        return false;
    }

    public bool UnequipWeapon() {
        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.pistolIdle) {
            animationPlayer.SetAnimation(AnimationPlayer.pistolUnequip);
        }

        if (animationPlayer.CurrentAnimationState() == AnimationPlayer.idleUnarmed) {
            weaponEnabled = false;
            return true;
        } else {
            return false;
        }
    }

}
