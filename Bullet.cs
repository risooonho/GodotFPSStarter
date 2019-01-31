using Godot;
using System;

public class Bullet : Spatial
{
    private const float bulletSpeed = 70f;
    public float bulletDamage = 15f;
    private const float killTimer = 4;

    private float timer = 0;
    private bool hitSomething = false;

    public override void _Ready()
    {
        GetNode<Area>("Area").Connect("body_entered", this, "Collided");
    }

    public override void _PhysicsProcess(float delta) {
        var forward = GlobalTransform.basis.z.Normalized();
        GlobalTranslate(forward * bulletSpeed * delta);

        timer += delta;
        if (timer >= killTimer) {
            QueueFree();
        }
    }

    public void Collided(Godot.Object body) {
        if (!hitSomething) {
            body.EmitSignal("BulletHit", bulletDamage, GlobalTransform.origin);
            hitSomething = true;
        }

        QueueFree();
    }
}
