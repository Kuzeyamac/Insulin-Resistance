using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace KuzeyAMAC;

public class Game1 : Game
{
    private const int StatePlaying = 0;
    private const int StatePointScored = 1;
    private const int StateGameOver = 2;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D pixelTexture;
    private Texture2D donutTexture;

    private const int WindowWidth = 1000;
    private const int WindowHeight = 600;
    private const int CeilingY = 20;
    private const int FloorY = WindowHeight - 20;
    private const int WinningScore = 3;

    private Paddle paddle1;
    private Paddle paddle2;
    private Ball ball;

    private int player1Score = 0;
    private int player2Score = 0;
    private int currentGameState = StatePointScored;
    private bool isPaused = false;
    private KeyboardState previousKeyboardState;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        graphics.PreferredBackBufferWidth = WindowWidth;
        graphics.PreferredBackBufferHeight = WindowHeight;
        graphics.ApplyChanges();

        paddle1 = new Paddle(WindowHeight, CeilingY, FloorY, 30, true);
        paddle2 = new Paddle(WindowHeight, CeilingY, FloorY, WindowWidth - 30 - Paddle.PaddleWidth, false);

        previousKeyboardState = Keyboard.GetState();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
        donutTexture = Content.Load<Texture2D>("donut");
        
        ball = new Ball(WindowWidth, WindowHeight, CeilingY, FloorY, donutTexture);
    }
    protected override void Update(GameTime gameTime)
    {
        KeyboardState currentKeyboardState = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || currentKeyboardState.IsKeyDown(Keys.Escape))
            Exit();

        HandlePauseToggle(currentKeyboardState);

        if (!isPaused)
        {
            switch (currentGameState)
            {
                case StatePlaying:
                    UpdatePlaying(currentKeyboardState, gameTime);
                    break;
                case StatePointScored:
                    UpdatePointScored(currentKeyboardState);
                    break;
                case StateGameOver:
                    UpdateGameOver(currentKeyboardState);
                    break;
            }
        }

        previousKeyboardState = currentKeyboardState;
        base.Update(gameTime);
    }

    private void HandlePauseToggle(KeyboardState currentKeyboardState)
    {
        bool ctrlPressed = currentKeyboardState.IsKeyDown(Keys.LeftControl) || currentKeyboardState.IsKeyDown(Keys.RightControl);
        bool ctrlWasPressed = previousKeyboardState.IsKeyDown(Keys.LeftControl) || previousKeyboardState.IsKeyDown(Keys.RightControl);

        if (ctrlPressed && !ctrlWasPressed)
        {
            isPaused = !isPaused;
        }
    }

    private void UpdatePlaying(KeyboardState keyboardState, GameTime gameTime)
    {
        HandlePlayer1Input(keyboardState);
        HandlePlayer2Input(keyboardState);

        if (ball.State == Ball.StateHeld)
        {
            UpdateBallWithPaddle();
        }
        else if (ball.State == Ball.StateMoving)
        {
            ball.UpdatePosition();
            CheckCollisions(keyboardState);
            CheckScoringBoundaries();
        }

        UpdateWindowTitle();
    }

    private void HandlePlayer1Input(KeyboardState keyboardState)
    {
        paddle1.Update(keyboardState);

        bool isHoldingSpace = keyboardState.IsKeyDown(Keys.Space);
        bool wasHoldingSpace = previousKeyboardState.IsKeyDown(Keys.Space);

        if (!isHoldingSpace && wasHoldingSpace)
        {
            if (ball.State == Ball.StateHeld && ball.IsPlayer1Holding)
            {
                ball.Throw(true, keyboardState);
            }
        }
    }

    private void HandlePlayer2Input(KeyboardState keyboardState)
    {
        paddle2.Update(keyboardState);

        bool isHoldingEnter = keyboardState.IsKeyDown(Keys.Enter);
        bool wasHoldingEnter = previousKeyboardState.IsKeyDown(Keys.Enter);

        if (!isHoldingEnter && wasHoldingEnter)
        {
            if (ball.State == Ball.StateHeld && ball.IsPlayer2Holding)
            {
                ball.Throw(false, keyboardState);
            }
        }
    }

    private void UpdateBallWithPaddle()
    {
        if (ball.IsPlayer1Holding)
        {
            ball.Position = new Vector2(
                paddle1.Bounds.X + paddle1.Bounds.Width + 5,
                paddle1.Bounds.Y + paddle1.Bounds.Height / 2f - 4
            );
        }
        else if (ball.IsPlayer2Holding)
        {
            ball.Position = new Vector2(
                paddle2.Bounds.X - 5 - 8,
                paddle2.Bounds.Y + paddle2.Bounds.Height / 2f - 4
            );
        }
    }

    private void UpdatePointScored(KeyboardState keyboardState)
    {
        paddle1.Update(keyboardState);
        paddle2.Update(keyboardState);

        if (keyboardState.IsKeyDown(Keys.P) && !previousKeyboardState.IsKeyDown(Keys.P))
        {
            ball.Serve();
            currentGameState = StatePlaying;
        }

        UpdateWindowTitle();
    }

    private void UpdateGameOver(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.R) && !previousKeyboardState.IsKeyDown(Keys.R))
        {
            RestartGame();
        }
    }

    private void CheckCollisions(KeyboardState keyboardState)
    {
        if (ball.State != Ball.StateHeld)
        {
            if (ball.IsCollidingWith(paddle1.Bounds) && ball.Velocity.X < 0)
            {
                ball.Position = new Vector2(paddle1.Bounds.X + paddle1.Bounds.Width + 2, ball.Position.Y);

                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    ball.Catch(true);
                }
                else
                {
                    ball.BounceOffPaddle(true);
                }
            }

            if (ball.IsCollidingWith(paddle2.Bounds) && ball.Velocity.X > 0)
            {
                ball.Position = new Vector2(paddle2.Bounds.X - 8 - 2, ball.Position.Y);

                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    ball.Catch(false);
                }
                else
                {
                    ball.BounceOffPaddle(false);
                }
            }
        }
    }

    private void CheckScoringBoundaries()
    {
        if (ball.IsOutOfBounds(WindowWidth))
        {
            if (ball.ScoredByPlayer2(WindowWidth))
            {
                player2Score++;
            }
            else
            {
                player1Score++;
            }

            ball.Reset();
            ProcessPointEnd();
        }
    }

    private void ProcessPointEnd()
    {
        if (player1Score >= WinningScore || player2Score >= WinningScore)
        {
            currentGameState = StateGameOver;
        }
        else
        {
            currentGameState = StatePointScored;
        }
    }

    private void RestartGame()
    {
        player1Score = 0;
        player2Score = 0;
        currentGameState = StatePointScored;
        ball.Reset();
    }

    private void UpdateWindowTitle()
    {
        if (currentGameState == StateGameOver)
        {
            string winner = player1Score >= WinningScore ? "P1 WINS!" : "P2 WINS!";
            Window.Title = $"Team Insulin Resistance - Gravity Tennis - {winner} - Press R to Restart";
        }
        else if (currentGameState == StatePointScored)
        {
            Window.Title = $"Team Insulin Resistance - Gravity Tennis - P1: {player1Score} - P2: {player2Score} - Press P to Serve";
        }
        else
        {
            string stateInfo = ball.State == Ball.StateHeld ? " [CAUGHT]" : "";
            Window.Title = $"Team Insulin Resistance - Gravity Tennis - P1: {player1Score} - P2: {player2Score}{stateInfo}";
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        DrawCourtLines();
        paddle1.Draw(spriteBatch, pixelTexture);
        paddle2.Draw(spriteBatch, pixelTexture);
        ball.Draw(spriteBatch);

        if (isPaused)
        {
            DrawPauseOverlay();
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawCourtLines()
    {
        DrawLine(0, CeilingY, WindowWidth, CeilingY, 3, Color.Lime);
        DrawLine(0, FloorY, WindowWidth, FloorY, 3, Color.Lime);
        DrawLine(WindowWidth / 2, CeilingY, WindowWidth / 2, FloorY, 2, Color.Lime);
    }

    private void DrawPauseOverlay()
    {
        Rectangle overlay = new Rectangle(0, 0, WindowWidth, WindowHeight);
        spriteBatch.Draw(pixelTexture, overlay, Color.Black * 0.5f);

        int centerX = WindowWidth / 2;
        int centerY = WindowHeight / 2;
        int barWidth = 15;
        int barHeight = 60;
        int barSpacing = 25;

        Rectangle leftBar = new Rectangle(centerX - barSpacing - barWidth / 2, centerY - barHeight / 2, barWidth, barHeight);
        Rectangle rightBar = new Rectangle(centerX + barSpacing - barWidth / 2, centerY - barHeight / 2, barWidth, barHeight);

        spriteBatch.Draw(pixelTexture, leftBar, Color.Yellow);
        spriteBatch.Draw(pixelTexture, rightBar, Color.Yellow);
    }

    private void DrawLine(int startX, int startY, int endX, int endY, int thickness, Color color)
    {
        if (startX == endX)
        {
            int height = Math.Abs(endY - startY);
            int y = Math.Min(startY, endY);
            spriteBatch.Draw(pixelTexture, new Rectangle(startX, y, thickness, height), color);
        }
        else if (startY == endY)
        {
            int width = Math.Abs(endX - startX);
            int x = Math.Min(startX, endX);
            spriteBatch.Draw(pixelTexture, new Rectangle(x, startY, width, thickness), color);
        }
    }
}
