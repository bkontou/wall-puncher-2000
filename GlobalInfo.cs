using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalInfo : Node
{
	public ItemSelector selected_shift_item = null;
	public ItemSelector selected_e_item = null;

	public String[] unlocked_items = {"Leap", "Kick", "Wall Phase", "Fury Punch", "Mind Punch"};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void saveGame()
	{
		var saveNodes = GetTree().GetNodesInGroup("Persist");
		foreach (Node saveNode in saveNodes)
		{
			saveNode.Call("Save");
		}
	}

	public Godot.Collections.Dictionary<string, Variant> Save()
	{
		return new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "Filename", SceneFilePath },
			{ "Parent", GetParent().GetPath() },
			{ "selected_shift_item", selected_shift_item }
		};
	}
}
