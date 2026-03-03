using Godot;

public partial class Road : Node2D
{
	private Line2D _line;

	public override void _Ready()
	{
		_line = new Line2D();
		_line.Width = 6;
		_line.DefaultColor = Colors.SandyBrown;
		AddChild(_line);
	}

	public void SetPoints(Vector2 a, Vector2 b)
	{
		_line.ClearPoints();
		_line.AddPoint(ToLocal(a));
		_line.AddPoint(ToLocal(b));
	}
}
