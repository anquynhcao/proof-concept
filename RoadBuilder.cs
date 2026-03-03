using Godot;

public partial class RoadBuilder : Node2D
{
	[Export] public NodePath FacilitiesRootPath;
	[Export] public NodePath RoadsRootPath;
	[Export] public NodePath PreviewLinePath;

	private Node2D _facilitiesRoot = default!;
	private Node2D _roadsRoot = default!;
	private Line2D _preview = default!;

	private Facility? _startFacility;
	private bool _dragging;

	public override void _Ready()
	{
		_facilitiesRoot = GetNode<Node2D>(FacilitiesRootPath);
		_roadsRoot = GetNode<Node2D>(RoadsRootPath);
		_preview = GetNode<Line2D>(PreviewLinePath);

		_preview.ClearPoints();

		foreach (var child in _facilitiesRoot.GetChildren())
		{
			if (child is Facility f)
				f.PressOnFacility += OnPressOnFacility;
		}
	}

	private void OnPressOnFacility(Facility facility)
	{
		_startFacility = facility;
		_dragging = true;

		// Start preview line from facility center
		_preview.ClearPoints();
		_preview.AddPoint(_preview.ToLocal(_startFacility.GlobalPosition));
		_preview.AddPoint(_preview.ToLocal(GetGlobalMousePosition()));
	}

	public override void _Process(double delta)
	{
		if (!_dragging || _startFacility == null) return;

		// Update preview end to mouse position
		_preview.SetPointPosition(1, _preview.ToLocal(GetGlobalMousePosition()));
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (!_dragging || _startFacility == null) return;

		// Release finishes attempt
		if (e is InputEventMouseButton mb &&
			mb.ButtonIndex == MouseButton.Left &&
			!mb.Pressed)
		{
			var target = GetFacilityUnderMouse();

			if (target != null && target != _startFacility)
			{
				// Optional: connect to facility edge instead of center (better visuals)
				var a =  GetRectEdgeAnchor(_startFacility, target.GlobalPosition);
				var b = GetRectEdgeAnchor(target, _startFacility!.GlobalPosition);

				CreateRoad(a, b);
			}

			CancelBuild();
		}

		// Esc cancels
		if (e is InputEventKey k && k.Pressed && k.Keycode == Key.Escape)
		{
			CancelBuild();
		}
	}

	private void CancelBuild()
	{
		_dragging = false;
		_startFacility = null;
		_preview.ClearPoints();
	}

	private Facility? GetFacilityUnderMouse()
	{
		var mousePos = GetGlobalMousePosition();
		var spaceState = GetWorld2D().DirectSpaceState;

		var query = new PhysicsPointQueryParameters2D
		{
			Position = mousePos,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var hits = spaceState.IntersectPoint(query, maxResults: 16);

		foreach (var hit in hits)
		{
			var collider = (Node)hit["collider"];

			if (collider is Facility f)
				return f;

			if (collider.GetParent() is Facility parentFacility)
				return parentFacility;
		}

		return null;
	}

	private void CreateRoad(Vector2 aGlobal, Vector2 bGlobal)
	{
		var road = new Line2D
		{
			Width = 6,
			DefaultColor = Colors.SandyBrown,
			Antialiased = true
		};

		_roadsRoot.AddChild(road);

		// Convert global points into the road's local space
		road.AddPoint(road.ToLocal(aGlobal));
		road.AddPoint(road.ToLocal(bGlobal));
	}
	private Vector2 GetRectEdgeAnchor(Facility from, Vector2 towardGlobal)
{
	var cs = from.GetNodeOrNull<CollisionShape2D>("CollisionFacility");
	if (cs?.Shape is not RectangleShape2D rect)
		return from.GlobalPosition; // fallback

	// Rectangle in LOCAL space is centered at (0,0)
	Vector2 half = rect.Size * 0.5f;

	// Direction to target in GLOBAL space
	Vector2 dirGlobal = towardGlobal - from.GlobalPosition;
	if (dirGlobal.Length() < 0.001f)
		return from.GlobalPosition;

	// Convert direction into LOCAL space (so we can intersect with local rectangle)
	Vector2 dirLocal = from.GlobalTransform.BasisXformInv(dirGlobal).Normalized();

	// Ray from origin to rectangle boundary: choose smallest t that hits an edge
	float tx = Mathf.Abs(dirLocal.X) < 1e-6f ? float.PositiveInfinity : half.X / Mathf.Abs(dirLocal.X);
	float ty = Mathf.Abs(dirLocal.Y) < 1e-6f ? float.PositiveInfinity : half.Y / Mathf.Abs(dirLocal.Y);
	float t = Mathf.Min(tx, ty);

	Vector2 hitLocal = dirLocal * t;

	// Convert boundary point back to GLOBAL
	return from.GlobalTransform * hitLocal;
}
}
