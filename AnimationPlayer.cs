using Godot;
using System;
using System.Collections;

public class AnimationPlayer : Godot.AnimationPlayer
{
    public class AnimationState {
        public string name;
        public float speed;
        public AnimationState next;
        public AnimationState[] connections;

        public AnimationState(string name, float speed) {
            this.name = name;
            this.speed = speed;
        }

        public bool ConnectedTo(AnimationState otherState) {
            return System.Array.IndexOf(connections, otherState) > -1;
        }

        public override bool Equals(object obj)
        {   
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            return this.name.Equals(((AnimationState) obj).name);
        }
        
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }

    public static AnimationState idleUnarmed = new AnimationState("Idle_unarmed", 1f);

    public static AnimationState pistolEquip = new AnimationState("Pistol_equip", 1.4f);
    public static AnimationState pistolFire = new AnimationState("Pistol_fire", 1.8f);
    public static AnimationState pistolIdle = new AnimationState("Pistol_idle", 1f);
    public static AnimationState pistolReload = new AnimationState("Pistol_reload", 1f);
    public static AnimationState pistolUnequip = new AnimationState("Pistol_unequip", 1.4f);

    public static AnimationState rifleEquip = new AnimationState("Rifle_equip", 2f);
    public static AnimationState rifleFire = new AnimationState("Rifle_fire", 1.8f);
    public static AnimationState rifleIdle = new AnimationState("Rifle_idle", 1f);
    public static AnimationState rifleReload = new AnimationState("Rifle_reload", 1.45f);
    public static AnimationState rifleUnequip = new AnimationState("Rifle_unequip", 2f);

    public static AnimationState knifeEquip = new AnimationState("Knife_equip", 1f);
    public static AnimationState knifeFire = new AnimationState("Knife_fire", 1.35f);
    public static AnimationState knifeIdle = new AnimationState("Knife_idle", 1f);
    public static AnimationState knifeUnequip = new AnimationState("Knife_unequip", 1f);

    AnimationState currentState;

    static AnimationPlayer() {
        idleUnarmed.connections = new AnimationState[]{pistolIdle};
        
        pistolEquip.connections = new AnimationState[]{pistolIdle};
        pistolFire.connections = new AnimationState[]{pistolIdle};
        pistolReload.connections = new AnimationState[]{pistolIdle};
        pistolUnequip.connections = new AnimationState[]{idleUnarmed};
        pistolIdle.connections = new AnimationState[]{pistolFire, pistolReload, pistolUnequip, pistolIdle};
        pistolEquip.next = pistolFire.next = pistolReload.next = pistolUnequip.next = pistolIdle;

        rifleEquip.connections = new AnimationState[]{rifleIdle};
        rifleFire.connections = new AnimationState[]{rifleIdle};
        rifleReload.connections = new AnimationState[]{rifleIdle};
        rifleUnequip.connections = new AnimationState[]{idleUnarmed};
        rifleIdle.connections = new AnimationState[]{rifleFire, rifleReload, rifleUnequip, rifleIdle};
        rifleEquip.next = rifleFire.next = rifleReload.next = rifleUnequip.next = rifleIdle;
        
        knifeEquip.connections = new AnimationState[]{knifeIdle};
        knifeFire.connections = new AnimationState[]{knifeIdle};
        knifeUnequip.connections = new AnimationState[]{idleUnarmed};
        knifeIdle.connections = new AnimationState[]{knifeFire, knifeUnequip, knifeIdle};
        knifeEquip.next = knifeFire.next = knifeUnequip.next = knifeIdle;
    }

    public override void _Ready()
    {
        SetAnimation(idleUnarmed);
        Connect("animation_finished", this, "AnimationEnded");
    }

    public bool SetAnimation(AnimationState nextState) {
        if (nextState == currentState) {
            GD.PrintErr("Animation state already set to "+nextState.name);
            return true;
        }

        if (!HasAnimation(nextState.name)) {
            GD.PrintErr("Animation not found: "+nextState.name);
            return false;
        }

        if (currentState == null || currentState.ConnectedTo(nextState)) {
            currentState = nextState;
            Play(nextState.name, -1, nextState.speed);
            return true;
        }

        GD.PrintErr(string.Format("Animation {0} does not connect to {1}", currentState.name, nextState.name));
        return false;
    }

    public void AnimationEnded(string animationName) {
        if(currentState.next != null) {
            SetAnimation(currentState.next);
        }
    }

    public void FirePistolRound() {
        GD.Print("Firing pistol round!");
    }

    public void FireRifleRound() {
        GD.Print("Firing rifle round!");
    }

    public void FireKnife() {
        GD.Print("Firing knife!");
    }

    public AnimationState CurrentAnimationState() {
        return currentState;
    }


}
