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

    enum Direction
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
        Restarting
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        Snake snake = new Snake();
        Timer timer;
        public int matrixSize;
        public static CellType[,] matrix;
        public static bool GenFood;
        public static GameState State { get; set; }

        private Direction lastAcceptedInputDirection;

        private void Form1_Load(object sender, EventArgs e)
        {
            Initalize();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            GameLogic();
            if(State == GameState.Stopped)
                timer.Stop();
            DrawMatrix();
        }

        private void DrawMatrix(bool endGame = false)
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics graphics = Graphics.FromImage(bitmap);

            if (State == GameState.Stopped)
            {
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
            }
            else if (State == GameState.Playing)
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
                                color = Brushes.White;
                                break;
                            case CellType.obstical:
                                color = Brushes.Blue;
                                break;
                            case CellType.head:
                                color = Brushes.Black;
                                break;
                            case CellType.body:
                                color = Brushes.Brown;
                                break;
                            case CellType.food:
                                color = Brushes.Red;
                                break;
                            default:
                                color = Brushes.White;
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
            //snake set new direction
            if (GenFood)
                PlaceFood();
            snake.ChangeDirection(lastAcceptedInputDirection);
            snake.Move();
            snake.UpdateMatrix(matrix);
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


            snake = new Snake();
            GenFood = false;
            State = GameState.Playing;
            timer = new Timer();
            timer.Interval = 90;
            timer.Start();
            timer.Tick += Timer_Tick;
            matrixSize = 25;
            matrix = new CellType[matrixSize, matrixSize];
            //make snake
            int midpoint = (int)Math.Round((float)matrixSize / 2);

            List<int[,]> points = new List<int[,]>();
            points.Add(new int[midpoint, midpoint]);
            for (int i = 1; i < 7; i++)
            {
                points.Add(new int[midpoint, midpoint + i]);
            }
            snake.segments= points;
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

    class Snake
    {
        public List<int[,]> segments = new List<int[,]>();
        public Direction currentDirection;

        public Snake()
        {
            currentDirection = Direction.Up;
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
            var oldHeadX = segments.First().GetLength(0);
            var oldHeadY = segments.First().GetLength(1);

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

            if (newHeadPositon.X < 0 || newHeadPositon.X > Form1.matrix.GetLength(0) - 1 || newHeadPositon.Y < 0 || newHeadPositon.Y > Form1.matrix.GetLength(0) - 1)
            {
                Form1.State = GameState.Stopped;
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
                        Form1.State = GameState.Stopped;
                        return;
                    case CellType.food:
                        grow = true;
                        Form1.GenFood = true;
                        segments.Insert(0, new int[newHeadPositon.X, newHeadPositon.Y]);
                        break;
                    case CellType.empty:
                        segments.Insert(0, new int[newHeadPositon.X, newHeadPositon.Y]);
                        break;
                }
            }

            //move tail
            var last = segments.Last();
            Form1.ChangeIndividualCellState(new int[last.GetLength(0), last.GetLength(1)], CellType.empty);
            if(!grow)
                segments.Remove(last);

        }

        public void UpdateMatrix(CellType[,] matrix)
        {
            foreach (int[,] segment in segments)
                matrix[segment.GetLength(0), segment.GetLength(1)] = CellType.body;
            matrix[segments.First().GetLength(0), segments.First().GetLength(1)] = CellType.head;
        }
    }
}
