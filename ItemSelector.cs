using Godot;
using System;

public partial class ItemSelector : Node2D
{
	private bool moving = false;

	[Export]
	public bool unlocked = true;

	[Export]
	public Area2D item_shift_area;
	[Export]
	public Area2D item_e_area;

	[Export]
	public RichTextLabel item_info_label;

	[Export(PropertyHint.File)]
	public String item_scene;

	[Export]
	public String item_name;
	[Export]
	public String item_description;
	[Export]
	public Texture2D item_image;
	
	[Export]
	public Sprite2D item_lock_overlay;

	enum ItemPlaceState
	{
		OverItemPlacement,
		NotOverItemPlacement,
		NotSelected
	}

	private ItemPlaceState shift_item_place_state = ItemPlaceState.NotSelected;
	private ItemPlaceState e_item_place_state = ItemPlaceState.NotSelected;

	private static ItemSelector current_shift_item = null;
	private static ItemSelector current_e_item = null;

	private Vector2 initial_position;
	private Vector2 position_difference = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Sprite2D>("Sprite2D").Texture = item_image;
		initial_position = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustReleased("ui_lmb"))
		{
			moving = false;

			if (shift_item_place_state == ItemPlaceState.OverItemPlacement) {
				GD.Print("item was placed shift!");
				
				if (current_shift_item != null)
				{
					if (current_shift_item == this) {
						current_shift_item = this;
						select_this_item(true);
					} 
					else {
						current_shift_item.reset_item_selector();
					}
				}

				if (current_e_item == this)
				{
					current_e_item = null;
				}

				current_shift_item = this;
				select_this_item(true);
				// if (current_shift_item == null) {
				// 	if (unlocked == false) {
				// 		reset_item_selector();
				// 		return;
				// 	}
				// 	current_shift_item = this;
				// 	select_this_item(true);
				// }
				// else if (current_e_item == this)
				// {
				// 	current_e_item = null;
				// 	current_shift_item = this;
				// 	select_this_item(true);
				// }
				// else if (current_shift_item == this)
				// {
				// 	select_this_item(true);
				// }
				// else {
				// 	// move it away i guess
				// 	GD.Print("Cannot place at shift");
				// 	reset_item_selector();
				// 	return;
				// }
				
			}
			else if (e_item_place_state == ItemPlaceState.OverItemPlacement) {
				GD.Print("item placed on E");

				if (current_e_item != null)
				{
					if (current_e_item == this)
					{
						current_e_item = this;
						select_this_item(false);
					} 
					else {
						current_e_item.reset_item_selector();
					}
				}

				if (current_shift_item == this)
				{
					current_shift_item = null;
				}

				current_e_item = this;
				select_this_item(false);
				// if (current_e_item == null) {
				// 	if (unlocked == false) {
				// 		reset_item_selector();
				// 		return;
				// 	}
				// 	current_e_item = this;
				// 	select_this_item(false);
				// } 
				// else if (current_shift_item == this)
				// {
				// 	current_shift_item = null;
				// 	current_e_item = this;
				// 	select_this_item(false);
				// }
				// else if (current_e_item == this)
				// {
				// 	select_this_item(false);
				// }
				// else {
				// 	GD.Print("Cannot place at e");
				// 	reset_item_selector();
				// 	return;
				// }
				
			}
			else {
				GD.Print("item not placed");
				if (current_e_item == this) {
					current_e_item = null;
				}
				
				if (current_shift_item == this) {
					current_shift_item = null;
				}
			}
		}

		if (moving) {
			GlobalPosition = GetGlobalMousePosition() + position_difference;
		}
	}

	private void reset_item_selector()
	{
		Tween move_tween = GetTree().CreateTween();
		move_tween.SetEase(Tween.EaseType.Out);
		move_tween.SetTrans(Tween.TransitionType.Bounce);
		move_tween.TweenProperty(this, "global_position", initial_position, 1.2);
		shift_item_place_state = ItemPlaceState.NotSelected;
		e_item_place_state = ItemPlaceState.NotSelected;
	}

	private void select_this_item(bool is_shift)
	{
		if (unlocked == false) {
			reset_item_selector();
			return;
		}

		Tween move_tween = GetTree().CreateTween();
		move_tween.SetEase(Tween.EaseType.Out);
		move_tween.SetTrans(Tween.TransitionType.Expo);

		if (is_shift) {
			RectangleShape2D area_shape = (RectangleShape2D) item_shift_area.GetNode<CollisionShape2D>("CollisionShape2D").Shape;
			move_tween.TweenProperty(this, "global_position", item_shift_area.GlobalPosition, 0.2);
			GetNode<GlobalInfo>("/root/GlobalInfo").selected_shift_item = this;
		}
		else {
			RectangleShape2D area_shape = (RectangleShape2D) item_e_area.GetNode<CollisionShape2D>("CollisionShape2D").Shape;
			move_tween.TweenProperty(this, "global_position", item_e_area.GlobalPosition, 0.2);
			GetNode<GlobalInfo>("/root/GlobalInfo").selected_e_item = this;
		}
	}

	private void _on_area_2d_input_event(Node viewport, InputEvent @event, long shape_idx)
	{
		if (@event.IsActionPressed("ui_lmb"))
		{
			item_info_label.Text = String.Format("{0}\n\n {1}", item_name, item_description);
			position_difference = GlobalPosition - GetGlobalMousePosition();
			moving = true;
		}
	}
	
	private void _on_area_2d_area_entered(Area2D area)
	{
		if (area == item_shift_area) {
			GD.Print("item over shift area");
			shift_item_place_state = ItemPlaceState.OverItemPlacement;
		}
		if (area == item_e_area) {
			GD.Print("item over e area");
			e_item_place_state = ItemPlaceState.OverItemPlacement;
		}
	}

	private void _on_area_2d_area_exited(Area2D area)
	{
		if (area == item_shift_area) {
			GD.Print("item exit shift area");
			shift_item_place_state = ItemPlaceState.NotOverItemPlacement;
		}
		if (area == item_e_area) {
			GD.Print("item exit e area");
			e_item_place_state = ItemPlaceState.NotOverItemPlacement;
		}
	}
}
