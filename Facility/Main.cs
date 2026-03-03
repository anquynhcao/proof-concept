using Godot;

public partial class Main : Node2D
{
	[Export] public int TileSize = 16;

	private bool _roadMode = false;
	private bool _dragging = false;
	private Vector2I _lastTile;
	private Node2D _roadsRoot;

	public override void _Ready()
	{
		_roadsRoot = GetNode<Node2D>("Road");
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Interact"))
		{
			_roadMode = !_roadMode;
			GD.Print(_roadMode ? "Road mode ON" : "Road mode OFF");
		}

		// Paint tiles while dragging
		if (_roadMode && _dragging)
		{
			var tile = WorldToTile(GetGlobalMousePosition());
			if (tile != _lastTile)
			{
				PlaceRoad(tile);
				_lastTile = tile;
			}
		}
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (!_roadMode) return;

		if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
		{
			if (mb.Pressed)
			{
				_dragging = true;
				_lastTile = WorldToTile(GetGlobalMousePosition());
				PlaceRoad(_lastTile);
			}
			else
			{
				_dragging = false;
			}
		}
	}

	private void PlaceRoad(Vector2I tile)
	{
		// Don't place duplicate on same tile
		foreach (var child in _roadsRoot.GetChildren())
		{
			if (child is ColorRect r && r.Position == TileToWorld(tile))
				return;
		}

		var rect = new ColorRect();
		rect.Color = new Color(0.6f, 0.4f, 0.1f);
		rect.Size = new Vector2(TileSize, TileSize);
		rect.Position = TileToWorld(tile);
		_roadsRoot.AddChild(rect);

		// TODO: RoadPathfinder.AddRoad(tile)
	}

	private Vector2I WorldToTile(Vector2 world)
	{
		return new Vector2I(
			Mathf.FloorToInt(world.X / TileSize),
			Mathf.FloorToInt(world.Y / TileSize)
		);
	}

	private Vector2 TileToWorld(Vector2I tile)
	{
		return new Vector2(tile.X * TileSize, tile.Y * TileSize);
	}
}
