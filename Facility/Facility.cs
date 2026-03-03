using Godot;

public partial class Facility : Area2D
{
	[Export] public string FacilityName = "Facility";

	[Signal]
	public delegate void PressOnFacilityEventHandler(Facility facility);

	public override void _Ready()
	{
		InputEvent += OnInputEvent;
	}

	private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mb &&
			mb.ButtonIndex == MouseButton.Left &&
			mb.Pressed)
		{
			EmitSignal(SignalName.PressOnFacility, this);
		}
	}
}
