using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	private float Sens = 0.005f;

	[Export]
	private Node3D _neck;
	[Export]
	private Camera3D _camera;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready() {
		_neck = GetNode<Node3D>("Neck");
		_neck = GetNode<Camera3D>("Neck/Camera3D");
	}

	// capturar inputsEvents do mouse
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
		// se o player clicou na janela do jogo
		if(@event is InputEventMouseButton ){
			// captura o mouse
			Input.MouseMode = Input.MouseModeEnum.Captured;
			// se player dar esc
		}else if(@event.IsActionPressed("ui_cancel")){
			// solta o mouse
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		// se o mouse foi capturado
		if(Input.MouseMode == Input.MouseModeEnum.Captured){
			// se o evento for o mouse mexendo
			if(@event is InputEventMouseMotion){
				InputEventMouseMotion motion = (InputEventMouseMotion)@event;

				// pegar o angulo atual do neck utilizando euler, ordem importa 
				float neckAngle = _neck.Transform.Basis.GetEuler(EulerOrder.Yxz).X;
				// clampar para a neck nao dar loop
				float finalRotation = Mathf.Clamp(neckAngle + (-motion.Relative.Y * Sens) , Mathf.DegToRad(-89f), Mathf.DegToRad (89f));

				float neckVerticalRotation = finalRotation - neckAngle;

				float bodyHorizontalRotation = -motion.Relative.X * Sens;
				
				// The mouse position relative to the previous position (position at the last frame).
				// diminuimos para o quao longe movimentou a esquerda quando o mouse movimentou
				// o valor eh multiplicado por 0.01 pois eh em radianos (2pi radianos eh uma volta)

				RotateY(bodyHorizontalRotation);

				_neck.RotateX(neckVerticalRotation);

				// pra nao alterar a escala pq o godot eh frango
				Transform = Transform.Orthonormalized();
				_neck.Transform = _neck.Transform.Orthonormalized();
			}
		}

    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
							// pega a direcao em que o pescoco esta olhando
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
