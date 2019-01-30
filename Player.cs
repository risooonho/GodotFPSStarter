using Godot;
using System;

public class Player : KinematicBody
{
    private const float gravity = -24.8f;
    private const float maxSpeed = 20f;
    private const float jumpSpeed = 18f;
    private const float acceleration = 4.5f;
    private const float deceleration = 16f;
    private const float maxSlopeAngle = 40f;
    private const float mouseSensitivity = 0.05f;

    [Export]
    public Camera camera;
    [Export]
    public Spatial rotationHelper;

    private Vector3 direction = new Vector3();
    private Vector3 velocity = new Vector3();

    public override void _Ready()
    {
        camera = GetNode<Camera>("Rotation_Helper/Camera");
        rotationHelper = GetNode<Spatial>("Rotation_Helper");

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _PhysicsProcess(float delta) {
        ProcessInput(delta);
        ProcessMovement(delta);
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
    }

    private void ProcessMovement(float delta) {
        direction.y = 0;
        direction = direction.Normalized();

        velocity.y = delta * gravity;

        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        var targetVelocity = new Vector3(direction.x, direction.y, direction.z);
        targetVelocity *= maxSpeed;

        var accel = direction.Dot(horizontalVelocity) > 0 ? acceleration : deceleration;

        horizontalVelocity = horizontalVelocity.LinearInterpolate(targetVelocity, accel * delta);
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
        velocity = MoveAndSlide(velocity, new Vector3(0,1,0), 0.05f, 4, Mathf.Deg2Rad(maxSlopeAngle));
    }

    public override void _InputEvent(Godot.Object camera, InputEvent @event, Vector3 clickPosition, Vector3 clickNormal, int shapeIdx) {
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

        var mouseEvent = (InputEventMouseMotion) @event;

    }
}
