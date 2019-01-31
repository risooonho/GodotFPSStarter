using Godot;
using System;

public class Player : KinematicBody
{
    public class Weapon {
        public string name;
        private string nodePath;
        public int number;
        private WeaponPoint instance;
        public AnimationPlayer.AnimationState idleAnim;
        public AnimationPlayer.AnimationState fireAnim;

        public Weapon(string name, int number, string nodePath, AnimationPlayer.AnimationState idleAnim, AnimationPlayer.AnimationState fireAnim) {
            this.name = name;
            this.nodePath = nodePath;
            this.number = number;
            this.idleAnim = idleAnim;
            this.fireAnim = fireAnim;
        }

        public WeaponPoint Instance(Player player) {
            if (nodePath.Empty()) {
                return null;
            }

            if (instance == null) {
                this.instance = player.GetNode<WeaponPoint>(nodePath);
            }

            return instance;
        }
    }

    private const float gravity = -24.8f;
    private const float maxSpeed = 20f;
    private const float jumpSpeed = 18f;
    private const float acceleration = 4.5f;
    private const float deceleration = 16f;
    private const float maxSlopeAngle = 40f;
    private const float mouseSensitivity = 0.05f;
    private const float maxSprintSpeed = 30f;
    private const float sprintAccel = 18f;
    
    private bool isSprinting = false;
    private SpotLight flashLight;
    public Camera camera;
    public Spatial rotationHelper;

    private Vector3 direction = new Vector3();
    private Vector3 velocity = new Vector3();
    private AnimationPlayer animationPlayer;

    public static Weapon unarmed = new Weapon("UNARMED", 0, "", AnimationPlayer.idleUnarmed, null);
    // public static Weapon knife = new Weapon("Knife", 1, "Rotation_Helper/Gun_Fire_Points/Knife_Point", AnimationPlayer.knifeIdle, AnimationPlayer.knifeFire);
    public static Weapon pistol = new Weapon("PISTOL", 2, "Rotation_Helper/Gun_Fire_Points/Pistol_Point", AnimationPlayer.pistolIdle, AnimationPlayer.pistolFire);
    // public static Weapon rifle = new Weapon("Rifle", 3, "Rotation_Helper/Gun_Fire_Points/Rifle_Point", AnimationPlayer.rifleIdle, AnimationPlayer.rifleFire);

    public static Weapon[] allWeapons = new Weapon[]{unarmed, pistol};

    private Weapon currentWeapon = unarmed;
    private bool changingWeapon = false;
    private Weapon changingWeaponType = unarmed;
    private int health = 100;
    Label uiStatusLabel;

    public override void _Ready()
    {
        camera = GetNode<Camera>("Rotation_Helper/Camera");
        rotationHelper = GetNode<Spatial>("Rotation_Helper");
        flashLight = GetNode<SpotLight>("Rotation_Helper/Flashlight");
        GetAnimationPlayer();
        uiStatusLabel = GetNode<Label>("HUD/Panel/Gun_label");

        if (animationPlayer == null) {
            GD.Print("animation player null!");
        } else {
            GD.Print("animation player not null");
        }

        Input.SetMouseMode(Input.MouseMode.Captured);

        var gunAimPoint = GetNode<Spatial>("Rotation_Helper/Gun_Aim_Point").GlobalTransform.origin;

        foreach(Weapon weapon in allWeapons) {
            var instance = weapon.Instance(this);
            if (instance == null) {
                continue;
            }

            instance.LookAt(gunAimPoint, new Vector3(0, 1, 0));
            instance.RotateObjectLocal(new Vector3(0, 1, 0), Mathf.Deg2Rad(180f));
        }
    }

    public override void _PhysicsProcess(float delta) {
        ProcessInput(delta);
        ProcessMovement(delta);
        ProcessChangingWeapons(delta);
    }

    private void ProcessInput(float delta) {
        direction = new Vector3();
        var camXForm = camera.GetGlobalTransform();
        var inputMovement = new Vector2();

        if (Input.IsActionPressed("movement_forward")) {
            inputMovement.y++;
        }
        if (Input.IsActionPressed("movement_backward")) {
            inputMovement.y--;
        }
        if (Input.IsActionPressed("movement_right")) {
            inputMovement.x++;
        }
        if (Input.IsActionPressed("movement_left")) {
            inputMovement.x--;
        }

        inputMovement = inputMovement.Normalized();

        direction += -camXForm.basis.z.Normalized() * inputMovement.y;
        direction += camXForm.basis.x.Normalized() * inputMovement.x;
    
        if(IsOnFloor() && Input.IsActionJustPressed("movement_jump")) {
            velocity.y = jumpSpeed;
        }

        if (Input.IsActionJustPressed("ui_cancel")) {
            if (Input.GetMouseMode() == Input.MouseMode.Visible) {
                Input.SetMouseMode(Input.MouseMode.Captured);
            } else {
                Input.SetMouseMode(Input.MouseMode.Visible);
            }
        }

        isSprinting = Input.IsActionPressed("movement_sprint");

        if (Input.IsActionJustPressed("flashlight")) {
            if (flashLight.IsVisibleInTree()) {
                flashLight.Hide();
            } else {
                flashLight.Show();
            }
        }

        int selectedWeapon = currentWeapon.number;

        if (Input.IsKeyPressed((int)KeyList.Key0)) {
            selectedWeapon = unarmed.number;
        // } else if (Input.IsKeyPressed((int)KeyList.Key1)) {
            // selectedWeapon = knife.number;
        } else if (Input.IsKeyPressed((int)KeyList.Key2)) {
            selectedWeapon = pistol.number;
        // } else if (Input.IsKeyPressed((int)KeyList.Key3)) {
            // selectedWeapon = rifle.number;
        } else if (Input.IsKeyPressed((int)KeyList.Plus)) {
            selectedWeapon++;
        } else if (Input.IsKeyPressed((int)KeyList.Minus)) {
            selectedWeapon--;
        }

        selectedWeapon = Mathf.Clamp(selectedWeapon, 0, allWeapons.Length-1);

        if (changingWeapon) {
            return;
        }
        
        if (currentWeapon.number != selectedWeapon) {
            changingWeaponType = allWeapons[selectedWeapon];
            changingWeapon = true;
        }

        if (Input.IsActionPressed("fire")) {
            Spatial weaponInstance = currentWeapon.Instance(this);
            if(weaponInstance != null) {
                if (animationPlayer.CurrentAnimationState() == currentWeapon.idleAnim) {
                    animationPlayer.SetAnimation(currentWeapon.fireAnim);
                }
            }
        }
    }

    private void ProcessMovement(float delta) {
        direction.y = 0;
        direction = direction.Normalized();

        velocity.y += delta * gravity;

        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        var targetVelocity = new Vector3(direction.x, direction.y, direction.z);
        targetVelocity *= isSprinting ? maxSprintSpeed : maxSpeed;

        var accel = direction.Dot(horizontalVelocity) > 0 ? (isSprinting ? sprintAccel : acceleration) : deceleration;

        horizontalVelocity = horizontalVelocity.LinearInterpolate(targetVelocity, accel * delta);
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
        velocity = MoveAndSlide(velocity, new Vector3(0,1,0), 0.05f, 4, Mathf.Deg2Rad(maxSlopeAngle));
    }

    public void ProcessChangingWeapons(float delta) {
        if (!changingWeapon) {
            return;
        }

        bool weaponUnequipped = false;

        WeaponPoint currentPoint = currentWeapon.Instance(this);
        if (currentPoint == null) {
            weaponUnequipped = true;
        } else {
            if (currentPoint.isEnabled) {
                weaponUnequipped = currentPoint.UnequipWeapon();
            } else {
                weaponUnequipped = true;
            }
        }

        if (weaponUnequipped) {
            bool weaponEquiped = false;

            WeaponPoint changingWeaponPoint = changingWeaponType.Instance(this);
            if (changingWeaponPoint == null) {
                weaponEquiped = true;
            } else {
                if (changingWeaponPoint.isEnabled) {
                    weaponEquiped = true;
                } else {
                    weaponEquiped = changingWeaponPoint.EquipWeapon();
                }
            }

            if(weaponEquiped) {
                changingWeapon = false;
                currentWeapon = changingWeaponType;
                changingWeaponType = null;
            }
        }

    }

    public override void _Input(InputEvent @event) {
        if (Input.GetMouseMode() != Input.MouseMode.Captured) {
            return;
        }

        if (@event is InputEventMouseMotion mouseEvent) {
            rotationHelper.RotateX(Mathf.Deg2Rad(mouseEvent.Relative.y * mouseSensitivity));

            RotateY(Mathf.Deg2Rad(mouseEvent.Relative.x * mouseSensitivity * -1f));

            var cameraRotation = rotationHelper.RotationDegrees;

            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -70f, 70f);
            rotationHelper.RotationDegrees = cameraRotation;
        }
    }

    public AnimationPlayer GetAnimationPlayer() {
        if (animationPlayer == null) {
            animationPlayer = GetNode<AnimationPlayer>("Rotation_Helper/Model/Animation_Player");
        }
        
        return animationPlayer;
    }

    public void FireWeapon() {
        if (changingWeapon) {
            return;
        }

        currentWeapon.Instance(this).FireWeapon();
    }
}
