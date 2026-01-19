using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Paddle
{
    public Rectangle Bounds { get; set; }

    private int windowHeight;
    private int windowTopBound;
    private int windowBottomBound;
    private float paddleSpeed;
    private bool isPlayer1;

    public const int PaddleWidth = 15;
    public const int PaddleHeight = 100;

    public Paddle(int windowHeight, int windowTopBound, int windowBottomBound, int xPosition, bool isPlayer1)
    {
        this.windowHeight = windowHeight;
        this.windowTopBound = windowTopBound;
        this.windowBottomBound = windowBottomBound;
        this.paddleSpeed = 6f;
        this.isPlayer1 = isPlayer1;

        Bounds = new Rectangle(xPosition, windowHeight / 2 - PaddleHeight / 2, PaddleWidth, PaddleHeight);
    }

    public void Update(KeyboardState keyboardState)
    {
        if (isPlayer1)
        {
            UpdatePlayer1Input(keyboardState);
        }
        else
        {
            UpdatePlayer2Input(keyboardState);
        }
    }

    private void UpdatePlayer1Input(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.W))
        {
            Bounds = new Rectangle(Bounds.X,
                (int)MathHelper.Max(windowTopBound, Bounds.Y - paddleSpeed),
                Bounds.Width, Bounds.Height);
        }

        if (keyboardState.IsKeyDown(Keys.X))
        {
            Bounds = new Rectangle(Bounds.X,
                (int)MathHelper.Min(windowBottomBound - PaddleHeight, Bounds.Y + paddleSpeed),
                Bounds.Width, Bounds.Height);
        }
    }

    private void UpdatePlayer2Input(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up))
        {
            Bounds = new Rectangle(Bounds.X,
                (int)MathHelper.Max(windowTopBound, Bounds.Y - paddleSpeed),
                Bounds.Width, Bounds.Height);
        }

        if (keyboardState.IsKeyDown(Keys.Down))
        {
            Bounds = new Rectangle(Bounds.X,
                (int)MathHelper.Min(windowBottomBound - PaddleHeight, Bounds.Y + paddleSpeed),
                Bounds.Width, Bounds.Height);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        spriteBatch.Draw(pixelTexture, Bounds, Color.Lime);
    }
}
