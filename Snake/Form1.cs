using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snake
{

    public enum CellType
    {
        empty,
        obstical,
        head,
        body,
        food
    }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public enum GameState
    {
        Playing,
        Stopped,
        Restarting,
        Replay,
        Death
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Snake snake { get; set; }

        
        public int matrixSize;
        public static CellType[,] matrix;
        public static bool GenFood;
        public static GameState State { get; set; }
        public Timer gameTimer { get; set; }
        private Direction lastAcceptedInputDirection;
        public Graphics graphics { get; set; }
        public Bitmap bitmap { get; set; }
        private void Form1_Load(object sender, EventArgs e)
        {
            gameTimer = new Timer();
            gameTimer.Interval = 90;
            gameTimer.Start();
            gameTimer.Tick += Timer_Tick;

            Initalize();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            GameLogic();
            DrawMatrix();
        }

        private void DrawMatrix(bool endGame = false)
        {
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(bitmap);

            if (State == GameState.Playing || State == GameState.Replay)
            {
                graphics.FillRectangle(Brushes.Gray, 0, 0, pictureBox1.Width, pictureBox1.Height);


                SizeF sizeCell = new SizeF((float)pictureBox1.Width / matrixSize, (float)pictureBox1.Height / matrixSize);

                for (int x = 0; x < matrixSize; x++)
                {
                    for (int y = 0; y < matrixSize; y++)
                    {
                        Brush color;

                        switch (matrix[x, y])
                        {
                            case CellType.empty:
                                color = State != GameState.Replay ? Brushes.White : Brushes.Black;
                                break;
                            case CellType.obstical:
                                color = State != GameState.Replay ? Brushes.Blue : Brushes.Purple;
                                break;
                            case CellType.head:
                                color = State != GameState.Replay ? Brushes.Black : Brushes.LightGray;
                                break;
                            case CellType.body:
                                color = State != GameState.Replay ? Brushes.Black : Brushes.White;
                                break;
                            case CellType.food:
                                color = State != GameState.Replay ? Brushes.Red : Brushes.Black;
                                break;
                            default:
                                color = State != GameState.Replay ? Brushes.White : Brushes.Black;
                                break;
                        }

                        graphics.FillRectangle(color, x * (sizeCell.Width) + 1, y * (sizeCell.Height) + 1, (sizeCell.Width) - 2, (sizeCell.Height) - 2);
                    }
                }
            }
            pictureBox1.BackgroundImage = bitmap;
        }

        public static void ChangeIndividualCellState(int[,] position, CellType type)
        {
            matrix[position.GetLength(0), position.GetLength(1)] = type;
        }

        private void GameLogic()
        {

            switch (State)
            {
                case GameState.Playing:
                    if (GenFood)
                        PlaceFood();
                    snake.ChangeDirection(lastAcceptedInputDirection);
                    snake.Move();
                    snake.UpdateMatrix(matrix);
                    break;
                case GameState.Stopped:
                    graphics.FillRectangle(Brushes.Black, 0, 0, pictureBox1.Width, pictureBox1.Height);

                    Timer deathTimer = new Timer();
                    deathTimer.Interval = 3000;
                    deathTimer.Tick += (s, e) =>
                    {

                        Initalize();
                        deathTimer.Stop();
                    };
                    deathTimer.Start();
                    State = GameState.Restarting;
                    break;
                case GameState.Restarting:

                    break;
                case GameState.Replay:
                    snake.Move();
                    snake.UpdateMatrix(matrix);
                    break;
                case GameState.Death:
                    //replay
                    gameTimer.Interval = 50;
                    GenFood = false;

                    State = GameState.Replay;
                    //Timer = new Timer();
                    //Timer.Interval = 100;
                    //Timer.Start();
                    //Timer.Tick += Timer_Tick;
                    matrixSize = 25;
                    matrix = new CellType[matrixSize, matrixSize];
                    //make snake
                    int midpoint = (int)Math.Round((float)matrixSize / 2);
                    snake = new Snake(matrixSize, snake.StartingLength, snake.StartingDirection, snake.movementHistory);
                    snake.UpdateMatrix(matrix);
                    DrawMatrix();
                    break;
                default:
                    break;
            }

            //if(State == GameState.Playing)
            //{
            //    if (GenFood)
            //        PlaceFood();
            //    snake.ChangeDirection(lastAcceptedInputDirection);
            //    snake.Move();
            //    snake.UpdateMatrix(matrix);
            //}
            //else
            
            //{
            //    //replay
            //    GenFood = false;
                
            //    afterLifeTimer = new Timer();
            //    afterLifeTimer.Interval = 90;
            //    afterLifeTimer.Start();
            //    afterLifeTimer.Tick += Timer_Tick;
            //    matrixSize = 25;
            //    matrix = new CellType[matrixSize, matrixSize];
            //    //make snake
            //    int midpoint = (int)Math.Round((float)matrixSize / 2);
            //    snake = new Snake(matrixSize, 5, Direction.Up , snake.movementHistory);
            //    snake.UpdateMatrix(matrix);
            //    DrawMatrix();
            //}
        }

        private void PlaceFood()
        {
            Random random = new Random();
            Point foodPosition;

            do
            {
                foodPosition = new Point(random.Next() % matrixSize, random.Next() % matrixSize);
            } while (matrix[foodPosition.X, foodPosition.Y] != CellType.empty);

            matrix[foodPosition.X, foodPosition.Y] = CellType.food;
            GenFood = false;
        }

        private void Initalize()
        {
            gameTimer.Interval = 90;
            GenFood = false;
            State = GameState.Playing;


            matrixSize = 25;
            matrix = new CellType[matrixSize, matrixSize];
            //make snake
            int midpoint = (int)Math.Round((float)matrixSize / 2);
            snake = new Snake(matrixSize, 5, Direction.Up);
            snake.UpdateMatrix(matrix);
            PlaceFood();
            DrawMatrix();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //need to schedule this to be comepleted in the game loop
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if(snake.currentDirection != Direction.Down)
                        lastAcceptedInputDirection = Direction.Up;
                    break;
                case Keys.Right:
                    if(snake.currentDirection != Direction.Left)
                        lastAcceptedInputDirection = Direction.Right;
                    break;
                case Keys.Down:
                    if(snake.currentDirection != Direction.Up)
                        lastAcceptedInputDirection = Direction.Down;
                    break;
                case Keys.Left:
                    if(snake.currentDirection != Direction.Right)
                        lastAcceptedInputDirection = Direction.Left;
                    break;
            }
        }
    }









    public class Snake
    {
        public List<Point> segments = new List<Point>();
        public Direction currentDirection;

        public List<Point> movementHistory = new List<Point>();
        private List<Point> movementInstructions = new List<Point>();

        public int StartingLength { get; set; }
        public Direction StartingDirection { get; set; }

        public Snake(int matrixSize, int startingLength, Direction startingDirection)
        {
            StartingDirection = startingDirection;
            StartingLength = startingLength;
            currentDirection = startingDirection;
            int midpoint = (int)Math.Round((float)matrixSize / 2);
            List<Point> points = new List<Point>();
            points.Add(new Point(midpoint, midpoint));
            for (int i = 1; i < startingLength - 1; i++)
            {
                points.Add(new Point(midpoint, midpoint + i));
            }
            segments = points;
        }

        public Snake(int matrixSize, int startingLength, Direction startingDirection, List<Point> instructions)
        {
            StartingDirection = startingDirection;
            StartingLength = startingLength;
            int midpoint = (int)Math.Round((float)matrixSize / 2);
            List<Point> points = new List<Point>();
            points.Add(new Point(midpoint, midpoint));
            movementInstructions = instructions;
            for (int i = 1; i < startingLength - 1; i++)
            {
                points.Add(new Point(midpoint, midpoint + i));
            }
            segments = points;
        }

        public void ChangeDirection(Direction d)
        {
            switch (d)
            {
                case Direction.Up:
                    if (currentDirection != Direction.Down)
                        currentDirection = d;
                    return;
                case Direction.Right:
                    if(currentDirection != Direction.Left)
                        currentDirection = d;
                    return;
                case Direction.Down:
                    if(currentDirection != Direction.Up)
                        currentDirection = d;
                    return;
                case Direction.Left:
                    if(currentDirection != Direction.Right)
                        currentDirection = d;
                    return;
            }
        }
        public void Move()
        {
            var first = segments.First();

            bool grow = false;
            //move head
            var oldHeadX = segments.First().X;
            var oldHeadY = segments.First().Y;

            Point newHeadPositon = new Point(-1, -1);
            
            switch (currentDirection)
            {
                case Direction.Up:
                    newHeadPositon = new Point(oldHeadX, oldHeadY - 1);
                    break;
                case Direction.Right:
                    newHeadPositon = new Point(oldHeadX + 1, oldHeadY);
                    break;
                case Direction.Down:
                    newHeadPositon = new Point(oldHeadX, oldHeadY + 1);
                    break;
                case Direction.Left:
                    newHeadPositon = new Point(oldHeadX - 1, oldHeadY);
                    break;
            }

            if (Form1.State == GameState.Replay)
            {
                newHeadPositon = new Point(movementInstructions.First().X, movementInstructions.First().Y);
                movementInstructions.RemoveAt(0);
                if(movementInstructions.Count == 0)
                {
                    Form1.State = GameState.Stopped;
                    return;
                }
            }


            if (newHeadPositon.X < 0 || newHeadPositon.X > Form1.matrix.GetLength(0) - 1 || newHeadPositon.Y < 0 || newHeadPositon.Y > Form1.matrix.GetLength(0) - 1)
            {
                Form1.State = GameState.Death;
                return;
            }
            else
            {

                CellType mcell = Form1.matrix[newHeadPositon.X, newHeadPositon.Y];
                switch (mcell)
                {
                    case CellType.obstical:
                    case CellType.head:
                    case CellType.body:
                        if(Form1.State == GameState.Replay)
                        {
                            Form1.State = GameState.Stopped;
                        }
                        else
                        {
                            Form1.State = GameState.Death;
                        }
                        return;
                    case CellType.food:
                        grow = true;
                        Form1.GenFood = true;
                        segments.Insert(0, new Point(newHeadPositon.X, newHeadPositon.Y));
                        movementHistory.Add(new Point(newHeadPositon.X, newHeadPositon.Y));
                        break;
                    case CellType.empty:
                        segments.Insert(0, new Point(newHeadPositon.X, newHeadPositon.Y));
                        movementHistory.Add(new Point(newHeadPositon.X, newHeadPositon.Y));
                        break;
                }
            }

            //move tail
            var last = segments.Last();
            Form1.ChangeIndividualCellState(new int[last.X, last.Y], CellType.empty);
            if(!grow)
                segments.Remove(last);

        }

        public void UpdateMatrix(CellType[,] matrix)
        {
            foreach (Point segment in segments)

                matrix[segment.X, segment.Y] = CellType.body;
            matrix[segments.First().X, segments.First().Y] = CellType.head;
        }
    }
}
