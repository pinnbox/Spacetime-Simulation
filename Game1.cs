using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using GameUtility;
using System.Collections.Generic;

namespace MonoProgram;

static class Spacetime
{
	public const int PLANE_WIDTH = 1800;
	public const int PLANE_HEIGHT = 1125;

	public const int AXIS_SCALE = 25;

	public const int X_WIDTH = PLANE_WIDTH / AXIS_SCALE;
	public const int T_HEIGHT = PLANE_HEIGHT / AXIS_SCALE;
}

public struct Event
{
	public float x;
	public float t;

	public Event(float x, float t)
	{
		this.x = x;
		this.t = t;
	}
}

public class Worldline
{
	public Func<float, float> XofT;

	public Color color;

	public float tStart;
	public float xStart;
}

public class Observer
{
	public float v;
	public float gamma;

	public Worldline worldline;

	public Observer(float velocity, float startPosition, float startTime, Color worldlineColor)
	{
		v = velocity;

		gamma = 1f / MathF.Sqrt(1 - v * v);

		worldline = new Worldline()
		{
			xStart = startPosition,
			tStart = startTime,
			color = worldlineColor
		};
	}
}

public class Game1 : Game
{
	const int SCREEN_WIDTH = 1920;
	const int SCREEN_HEIGHT = 1200;

	static Random rng = new Random();

	KeyboardState kb;
	KeyboardState prevKb;

	MouseState mouse;
	MouseState prevMouse;

	SpriteFont smallFont;
	SpriteFont medFont;
	SpriteFont largeFont;
	SpriteFont americanFont;

	Texture2D pixel;

	List<Observer> observers = new List<Observer>();
	List<Event> events = new List<Event>();

	float keyPressMult = 1f;
	float frameVelocity = 0f;
	float frameOriginX = 0f;
	float frameOriginT = 0f;
	float currentTime = 0f;

	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
	}

	protected override void Initialize()
	{
		_graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
		_graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
		_graphics.SynchronizeWithVerticalRetrace = false;
		IsFixedTimeStep = true;

		_graphics.ApplyChanges();
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		pixel = Content.Load<Texture2D>("Images/Sprites/pixel");

		smallFont = Content.Load<SpriteFont>("Fonts/SmallFont");
		medFont = Content.Load<SpriteFont>("Fonts/MedFont");
		largeFont = Content.Load<SpriteFont>("Fonts/LargeFont");
		americanFont = Content.Load<SpriteFont>("Fonts/AmericanFont");

		Observer restObserver = new Observer(0, 0, 0, Color.Red);
		Observer slowObserver = new Observer(0f, -10, 5, Color.Cyan);
		Observer fastObserver = new Observer(0.8f, 0, 0, Color.Green);
		Observer photon1 = new Observer(1, 0, 0, Color.Yellow);
		Observer photon2 = new Observer(-1, 0, 0, Color.Yellow);

		observers.Add(restObserver);
		observers.Add(slowObserver);
		observers.Add(fastObserver);
		observers.Add(photon1);
		observers.Add(photon2);

		Event e = new Event(5, 10);

		events.Add(e);
	}

	protected override void Update(GameTime gameTime)
	{
		prevKb = kb;
		kb = Keyboard.GetState();

		prevMouse = mouse;
		mouse = Mouse.GetState();

		if (kb.IsKeyDown(Keys.Z) && !prevKb.IsKeyDown(Keys.Z))
		{
			if (keyPressMult < 32)
			{
				keyPressMult *= 2;
			}
		}

		if (kb.IsKeyDown(Keys.X) && !prevKb.IsKeyDown(Keys.X))
		{
			if (keyPressMult > 0.03125)
			{
				keyPressMult /= 2;
			}
		}

		if (kb.IsKeyDown(Keys.E))
		{
			frameVelocity += 0.002f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.Q))
		{
			frameVelocity -= 0.002f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.D))
		{
			frameOriginX += 0.05f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.A))
		{
			frameOriginX -= 0.05f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.W))
		{
			frameOriginT += 0.05f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.S))
		{
			frameOriginT -= 0.05f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.F))
		{
			currentTime += 0.05f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.G))
		{
			currentTime -= 0.05f * keyPressMult;
		}
		if (kb.IsKeyDown(Keys.R))
		{
			frameVelocity = 0;
			frameOriginX = 0;
			frameOriginT = 0;
			currentTime = 0;
		}

		frameVelocity = Math.Clamp(frameVelocity, -0.99f, 0.99f);
		frameOriginX = Math.Clamp(frameOriginX, -Spacetime.X_WIDTH / 2, Spacetime.X_WIDTH / 2);
		frameOriginT = Math.Clamp(frameOriginT, -Spacetime.T_HEIGHT / 2, Spacetime.T_HEIGHT / 2);

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);

		_spriteBatch.Begin();

		for (int x = -Spacetime.X_WIDTH / 2; x <= Spacetime.X_WIDTH / 2; x++)
		{
			DrawLine(WorldToScreenUI(x, -2), WorldToScreenUI(x, Spacetime.T_HEIGHT), 1, Color.DimGray);
		}

		for (int t = -1; t <= Spacetime.T_HEIGHT; t++)
		{
			DrawLine(WorldToScreenUI(-Spacetime.X_WIDTH / 2, t), WorldToScreenUI(Spacetime.X_WIDTH / 2, t), 1, Color.DimGray);
		}

		DrawLine(WorldToScreenUI(-Spacetime.X_WIDTH / 2f, 0), WorldToScreenUI(Spacetime.X_WIDTH / 2f, 0), 2, Color.White);
		DrawLine(WorldToScreenUI(0, 0), WorldToScreenUI(0, Spacetime.T_HEIGHT), 2, Color.White);

		for (int i = -Spacetime.X_WIDTH / 2; i < Spacetime.X_WIDTH / 2; i++)
		{
			if (i % 5 == 0)
			{
				_spriteBatch.DrawString(smallFont, i.ToString(), WorldToScreenUI(i + 0.1f, 0), Color.White);
			}
		}

		for (int i = 0; i < Spacetime.T_HEIGHT; i++)
		{
			if (i % 5 == 0)
			{
				_spriteBatch.DrawString(smallFont, i.ToString(), WorldToScreenUI(0.1f, i), Color.White);
			}
		}

		for (int i = 0; i < events.Count; i++)
		{
			DrawEvent(events[i], Color.Pink);
		}

		for (int i = 0; i < observers.Count; i++)
		{
			DrawSimultaneity(observers[i], currentTime);
			DrawTicks(observers[i], observers[i].worldline.color);
			DrawWorldline(observers[i]);
		}

		_spriteBatch.DrawString(medFont, "Mult: " + keyPressMult, new Vector2(20, SCREEN_HEIGHT - 280), Color.White);
		_spriteBatch.DrawString(medFont, "V Relative to Rest Frame: " + Math.Round(frameVelocity, 2), new Vector2(20, SCREEN_HEIGHT - 240), Color.White);
		_spriteBatch.DrawString(medFont, "X Relative to Rest Frame: " + Math.Round(frameOriginX, 2), new Vector2(20, SCREEN_HEIGHT - 200), Color.White);
		_spriteBatch.DrawString(medFont, "T Relative to Rest Frame: " + Math.Round(frameOriginT, 2), new Vector2(20, SCREEN_HEIGHT - 160), Color.White);
		_spriteBatch.DrawString(medFont, "Observers' Proper Time: " + Math.Round(currentTime, 2), new Vector2(20, SCREEN_HEIGHT - 120), Color.White);

		_spriteBatch.End();

		base.Draw(gameTime);
	}

	void DrawLine(Vector2 start, Vector2 end, float thickness, Color color)
	{
		Vector2 edge = end - start;
		float angle = (float)Math.Atan2(edge.Y, edge.X);
		float length = edge.Length();

		_spriteBatch.Draw(pixel, start, null, color, angle, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
	}

	void DrawTicks(Observer obs, Color color)
	{
		float properStep = 1f;
		float tickLength = 10f;

		float tauMax = Spacetime.T_HEIGHT * 5f / obs.gamma;

		float x0 = obs.worldline.xStart;
		float t0 = obs.worldline.tStart;

		if (obs.worldline.tStart < 0)
		{
			t0 = -2 * Spacetime.AXIS_SCALE;
		}

		int tickIndex = 0;

		for (float tau = 0f; tau <= tauMax; tau += properStep)
		{
			float t = t0 + obs.gamma * tau;
			float x = x0 + obs.v * (t - t0);

			Vector2 p1 = WorldToScreen(x, t);
			Vector2 p2 = WorldToScreen(x + obs.v, t + 1f);

			Vector2 tangent = p2 - p1;
			tangent.Normalize();

			Vector2 normal = new Vector2(-tangent.Y, tangent.X);

			Vector2 start = p1 - normal * tickLength * 0.5f;
			Vector2 end = p1 + normal * tickLength * 0.5f;

			if (end.X < Spacetime.PLANE_WIDTH && start.X >= 0 && end.Y < Spacetime.PLANE_HEIGHT && start.Y < Spacetime.PLANE_HEIGHT)
			{
				DrawLine(start, end, 2f, color);

				if (tickIndex % 5 == 0)
				{
					string label;

					if (obs.worldline.tStart >= 0)
					{
						label = tau.ToString();
					}
					else
					{
						label = (tau - 50).ToString();
					}

					Vector2 textOffset = tangent * 8f + normal * 8f + new Vector2(-5, -5);
					Vector2 textPos = p1 + textOffset;

					_spriteBatch.DrawString(smallFont, label, textPos, Color.White);
					_spriteBatch.DrawString(smallFont, label, textPos, color);
				}
			}

			tickIndex++;
		}
	}

	void DrawEvent(Event e, Color color)
	{
		Vector2 p = WorldToScreen(e.x, e.t);

		if (e.x < Spacetime.PLANE_WIDTH)
		{
			_spriteBatch.Draw(pixel, new Rectangle((int)p.X - 3, (int)p.Y - 3, 6, 6), color);
		}
	}

	void DrawWorldline(Observer obs)
	{
		int worldLineLength = 5;

		if (Math.Abs(obs.v) == 1)
		{
			worldLineLength = 20;
		}

		float tStart = obs.worldline.tStart;

		if (tStart < 0)
		{
			tStart = -2 * Spacetime.AXIS_SCALE;
		}
		else
		{
			tStart = obs.worldline.tStart;
		}

		for (float t = tStart; t < Spacetime.T_HEIGHT * worldLineLength; t += 0.1f)
		{
			float x1 = obs.worldline.xStart + obs.v * (t - obs.worldline.tStart);
			float x2 = obs.worldline.xStart + obs.v * (t + 0.1f - obs.worldline.tStart);

			Vector2 p1 = WorldToScreen(x1, t);
			Vector2 p2 = WorldToScreen(x2, t + 0.1f);

			if (t == tStart && tStart >= 0 && WorldToScreen(obs.worldline.xStart, obs.worldline.tStart).Y <= Spacetime.PLANE_HEIGHT)
			{
				DrawEvent(new Event(obs.worldline.xStart, obs.worldline.tStart), obs.worldline.color);
			}

			if (p1.Y > Spacetime.PLANE_HEIGHT && p2.Y < Spacetime.PLANE_HEIGHT)
			{
				float percent = (Spacetime.PLANE_HEIGHT - p2.Y) / (p1.Y - p2.Y);

				DrawLine(new Vector2(p2.X - (p2.X - p1.X) * percent, Spacetime.PLANE_HEIGHT), p2, 2f, obs.worldline.color);
			}
			else if (p1.Y <= Spacetime.PLANE_HEIGHT && p2.Y <= Spacetime.PLANE_HEIGHT && p1.X <= Spacetime.PLANE_WIDTH && p2.X <= Spacetime.PLANE_WIDTH)
			{
				DrawLine(p1, p2, 2f, obs.worldline.color);
			}
		}
	}

	public static float TransformVelocity(float v, float frameV)
	{
		return (v - frameV) / (1 - v * frameV);
	}

	Vector2 WorldToScreenUI(double x, double t)
	{
		float screenX = Spacetime.PLANE_WIDTH / 2f + (float)x * Spacetime.AXIS_SCALE;
		float screenY = Spacetime.PLANE_HEIGHT - (float)t * Spacetime.AXIS_SCALE;

		return new Vector2(screenX, screenY);
	}

	Vector2 WorldToScreen(float x, float t)
	{
		Event e = LorentzTransform(new Event(x, t));

		float screenX = e.x * Spacetime.AXIS_SCALE + Spacetime.PLANE_WIDTH / 2;
		float screenY = Spacetime.PLANE_HEIGHT - e.t * Spacetime.AXIS_SCALE;

		return new Vector2(screenX, screenY);
	}

	Event LorentzTransform(Event e)
	{
		float gamma = 1f / MathF.Sqrt(1 - frameVelocity * frameVelocity);

		float dx = e.x - frameOriginX;
		float dt = e.t - frameOriginT;

		float xPrime = gamma * (dx - frameVelocity * dt);
		float tPrime = gamma * (dt - frameVelocity * dx);

		return new Event(xPrime, tPrime);
	}

	void DrawDashedLine(Vector2 start, Vector2 end, float thickness, Color color, float dashLength = 10f, float gapLength = 6f)
	{
		Vector2 dir = end - start;
		float length = dir.Length();
		dir.Normalize();

		float traveled = 0f;

		while (traveled < length)
		{
			float segmentStart = traveled;
			float segmentEnd = MathF.Min(traveled + dashLength, length);

			Vector2 a = start + dir * segmentStart;
			Vector2 b = start + dir * segmentEnd;

			DrawLine(a, b, thickness, color);

			traveled += dashLength + gapLength;
		}
	}

	void DrawSimultaneity(Observer obs, float currentTime)
	{
		float v = obs.v;
		float gamma = obs.gamma;

		float tau = currentTime;

		float xMin = -Spacetime.PLANE_WIDTH / Spacetime.AXIS_SCALE * 2;
		float xMax = Spacetime.PLANE_WIDTH / Spacetime.AXIS_SCALE * 2;

		float step = 0.25f;

		Vector2? prev = null;
		bool drawSegment = false;

		for (float x = xMin; x <= xMax; x += step)
		{
			float t = v * x + tau / gamma + obs.worldline.tStart;

			Vector2 p = WorldToScreen(x, t);

			if (prev != null && drawSegment && p.X < Spacetime.PLANE_WIDTH && p.Y < Spacetime.PLANE_HEIGHT && Math.Abs(obs.v) != 1)
			{
				DrawLine(prev.Value, p, 2f, obs.worldline.color);
			}

			drawSegment = !drawSegment;
			prev = p;
		}
	}
}
