using System;
using UnityEngine;

[Serializable]
public class pb_Color
{
	public float r;

	public float g;

	public float b;

	public float a;

	public pb_Color()
	{
		r = 0f;
		g = 0f;
		b = 0f;
		a = 0f;
	}

	public pb_Color(Color c)
	{
		r = c.r;
		g = c.g;
		b = c.b;
		a = c.a;
	}

	public pb_Color(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public static implicit operator Color(pb_Color c)
	{
		return new Color(c.r, c.g, c.b, c.a);
	}

	public static implicit operator pb_Color(Color c)
	{
		return new pb_Color(c);
	}
}
