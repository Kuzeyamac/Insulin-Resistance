using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

public class Ball
{
    public const int StateMoving = 0;
    public const int StateHeld = 1;
    public const int StateWaitingForServe = 2;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public int State { get; set; }
    public bool IsPlayer1Holding { get; set; }
    public bool IsPlayer2Holding { get; set; }

    private const int BallSize = 24;
    private const float MaxBallSpeed = 12f;
    private const float BounceMultiplier = 1.0f;
    private const float ThrowSpeed = 10f;

    private int windowWidth;
    private int windowHeight;
    private int ceilingY;
    private int floorY;
    private Texture2D texture;

    public Ball(int windowWidth, int windowHeight, int ceilingY, int floorY, Texture2D texture)
    {
        this.windowWidth = windowWidth;
        this.windowHeight = windowHeight;
        this.ceilingY = ceilingY;
        this.floorY = floorY;
        this.texture = texture;

        Position = new Vector2(windowWidth / 2f, windowHeight / 2f);
        Velocity = Vector2.Zero;
        State = StateWaitingForServe;
        IsPlayer1Holding = false;
        IsPlayer2Holding = false;
    }

    public void UpdatePosition()
    {
        if (State == StateMoving)
        {
            ClampSpeed();
            Position += Velocity;
            BounceOffBoundaries();
        }
    }

    private void ClampSpeed()
    {
        if (Math.Abs(Velocity.X) > MaxBallSpeed)
        {
            Velocity = new Vector2(MaxBallSpeed * Math.Sign(Velocity.X), Velocity.Y);
        }

        if (Math.Abs(Velocity.Y) > MaxBallSpeed)
        {
            Velocity = new Vector2(Velocity.X, MaxBallSpeed * Math.Sign(Velocity.Y));
        }
    }

    private void BounceOffBoundaries()
    {
        if (Position.Y + BallSize >= floorY)
        {
            Position = new Vector2(Position.X, floorY - BallSize);
            Velocity = new Vector2(Velocity.X, Velocity.Y * -BounceMultiplier);
        }

        if (Position.Y <= ceilingY)
        {
            Position = new Vector2(Position.X, ceilingY);
            Velocity = new Vector2(Velocity.X, Velocity.Y * -BounceMultiplier);
        }
    }

    public void Catch(bool isPlayer1)
    {
        State = StateHeld;
        IsPlayer1Holding = isPlayer1;
        IsPlayer2Holding = !isPlayer1;
        Velocity = Vector2.Zero;
    }

    public void Throw(bool isPlayer1, KeyboardState keyboardState)
    {
        State = StateMoving;
        IsPlayer1Holding = false;
        IsPlayer2Holding = false;

        float throwSpeedX;
        float throwSpeedY = 0f;

        bool holdingUp = isPlayer1 ? keyboardState.IsKeyDown(Keys.W) : keyboardState.IsKeyDown(Keys.Up);
        bool holdingDown = isPlayer1 ? keyboardState.IsKeyDown(Keys.X) : keyboardState.IsKeyDown(Keys.Down);

        if (holdingUp)
        {
            throwSpeedY = -6f;
            throwSpeedX = isPlayer1 ? ThrowSpeed : -ThrowSpeed;
        }
        else if (holdingDown)
        {
            throwSpeedY = 6f;
            throwSpeedX = isPlayer1 ? ThrowSpeed : -ThrowSpeed;
        }
        else
        {
            throwSpeedY = 0f;
            throwSpeedX = isPlayer1 ? ThrowSpeed : -ThrowSpeed;
        }

        Velocity = new Vector2(throwSpeedX, throwSpeedY);
    }

    public void Serve()
    {
        Random random = new Random();
        float direction = random.Next(0, 2) == 0 ? -1f : 1f;

        float serveSpeedX = direction * (6f + (float)random.NextDouble() * 3f);
        float serveSpeedY = (float)(random.NextDouble() * 8f - 4f);

        Position = new Vector2(windowWidth / 2f, windowHeight / 2f);
        Velocity = new Vector2(serveSpeedX, serveSpeedY);
        IsPlayer1Holding = false;
        IsPlayer2Holding = false;
        State = StateMoving;
    }

    public void Reset()
    {
        Position = new Vector2(windowWidth / 2f, windowHeight / 2f);
        Velocity = Vector2.Zero;
        State = StateWaitingForServe;
        IsPlayer1Holding = false;
        IsPlayer2Holding = false;
    }

    public bool IsCollidingWith(Rectangle paddle)
    {
        Rectangle ballRect = new Rectangle((int)Position.X, (int)Position.Y, BallSize, BallSize);
        return ballRect.Intersects(paddle);
    }

    public void BounceOffPaddle(bool isPlayer1Paddle)
    {
        Velocity = new Vector2(Velocity.X * -BounceMultiplier, Velocity.Y);
    }

    public bool IsOutOfBounds(int windowWidth)
    {
        return Position.X < 0 || Position.X > windowWidth;
    }

    public bool ScoredByPlayer2(int windowWidth)
    {
        return Position.X < 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle ballRect = new Rectangle((int)Position.X, (int)Position.Y, BallSize, BallSize);
        Color ballColor = State == StateHeld ? Color.Yellow : Color.White;
        spriteBatch.Draw(texture, ballRect, ballColor);
    }
}
