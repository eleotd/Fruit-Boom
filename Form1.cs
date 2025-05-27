using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Fruit_Bomb_Game
{
    public partial class Form1 : Form
    {
        // Игровые объекты
        private Rectangle basket;
        private List<FallingObject> fallingObjects;
        private Random random;

        // Игровые параметры
        private int score;
        private int lives;
        private int gameSpeed;
        private bool gameOver;

        private int gameTime;
        private float speedMultiplier = 1.0f;
        private const float SPEED_INCREASE_RATE = 0.01f;

        // Управление
        private bool leftPressed;
        private bool rightPressed;

        // Графика
        private Brush basketBrush;
        private Dictionary<string, Brush> objectBrushes;
        private Font gameFont;

        public Form1()
        {
            InitializeComponent();

            // Явная установка размеров
            this.ClientSize = new Size(800, 600);
            this.Text = "Фруктовая корзинка";
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // Обработчики событий
            this.Paint += MainForm_Paint;
            this.KeyDown += MainForm_KeyDown;
            this.KeyUp += MainForm_KeyUp;
            this.Shown += (s, e) => this.Invalidate(); // Первая отрисовка

            gameTimer.Tick += GameTimer_Tick;
            objectTimer.Tick += ObjectTimer_Tick;

            InitializeGame();
        }

        private void InitializeGame()
        {
            // Настройка формы
            this.Text = "Фруктовая корзинка";
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // Инициализация 
            basket = new Rectangle(350, 550, 100, 30);
            fallingObjects = new List<FallingObject>();
            random = new Random();

            score = 0;
            lives = 3;
            gameSpeed = 5;
            gameOver = false;

            // Сброс состояния клавиш
            leftPressed = false;
            rightPressed = false;

            // Настройка цветов
            basketBrush = new SolidBrush(Color.Brown);

            objectBrushes = new Dictionary<string, Brush>
            {
                { "Apple", new SolidBrush(Color.Red) },
                { "Banana", new SolidBrush(Color.Yellow) },
                { "Grape", new SolidBrush(Color.Purple) },
                { "Bomb", new SolidBrush(Color.Black) }
            };

            gameFont = new Font("Arial", 14);

            // Запуск игрового таймера
            gameTimer.Interval = 20;
            gameTimer.Start();

            // Таймер для создания объектов
            objectTimer.Interval = 1000;
            objectTimer.Start();
            this.Invalidate();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Отрисовка корзинки
            g.FillRectangle(basketBrush, basket);

            // Отрисовка падающих объектов
            foreach (var obj in fallingObjects)
            {
                g.FillEllipse(objectBrushes[obj.Type], obj.Bounds);
            }

            // Отрисовка счета и жизней
            g.DrawString($"Счет: {score}", gameFont, Brushes.Black, 10, 10);
            g.DrawString($"Жизни: {lives}", gameFont, Brushes.Black, 10, 40);

            // Экран завершения игры
            if (gameOver)
            {
                string gameOverText = "Игра окончена! Нажмите R для рестарта";
                SizeF textSize = g.MeasureString(gameOverText, gameFont);
                g.DrawString(gameOverText, gameFont, Brushes.Red,
                    (this.ClientSize.Width - textSize.Width) / 2,
                    (this.ClientSize.Height - textSize.Height) / 2);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!gameOver)
            {
                gameTime++;
                if (gameTime % 100 == 0) // Увеличение скорости
                {
                    speedMultiplier += SPEED_INCREASE_RATE;
                }

                // Обработка движения корзинки
                int step = 15;
                if (leftPressed && basket.X > 0)
                {
                    basket.X -= step;
                }
                if (rightPressed && basket.Right < this.ClientSize.Width)
                {
                    basket.X += step;
                }

                // Перемещение падающих объектов
                for (int i = fallingObjects.Count - 1; i >= 0; i--)
                {
                    FallingObject obj = fallingObjects[i];
                    var bounds = obj.Bounds;
                    bounds.Y += obj.Speed;
                    obj.Bounds = bounds;

                    // Проверка столкновения с корзинкой
                    if (obj.Bounds.IntersectsWith(basket))
                    {
                        if (obj.Type == "Bomb")
                        {
                            lives--;
                            if (lives <= 0)
                            {
                                gameOver = true;
                            }
                        }
                        else
                        {
                            switch (obj.Type)
                            {
                                case "Apple": score += 10; break;
                                case "Banana": score += 20; break;
                                case "Grape": score += 30; break;
                            }
                        }

                        fallingObjects.RemoveAt(i);
                        continue;
                    }

                    // Удаление объектов, упавших за пределы экрана
                    if (obj.Bounds.Y > this.ClientSize.Height)
                    {
                        fallingObjects.RemoveAt(i);
                    }
                }

                // Постепенное увеличение скорости
                if (score > 0 && score % 500 == 0)
                {
                    gameSpeed++;
                }
            }

            this.Invalidate();
        }

        private void ObjectTimer_Tick(object sender, EventArgs e)
        {
            if (!gameOver)
            {
                // Создание нового падающего объекта
                int x = random.Next(50, this.ClientSize.Width - 50);
                int size = random.Next(20, 40);
                Rectangle bounds = new Rectangle(x, -size, size, size);

                // Выбор типа объекта 
                string type;
                if (random.Next(100) < 25)
                {
                    type = "Bomb";
                }
                else
                {
                    // Выбор типа фрукта
                    int fruitType = random.Next(3);
                    switch (fruitType)
                    {
                        case 0: type = "Apple"; break;
                        case 1: type = "Banana"; break;
                        case 2: type = "Grape"; break;
                        default: type = "Apple"; break;
                    }
                }

                int baseSpeed = random.Next(3, 3 + gameSpeed / 2);
                int speed = (int)(baseSpeed * speedMultiplier);
                fallingObjects.Add(new FallingObject(bounds, type, speed));
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameOver)
            {
                // Установка флагов для клавиш
                if (e.KeyCode == Keys.Left)
                {
                    leftPressed = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    rightPressed = true;
                }
            }
            else if (e.KeyCode == Keys.R)
            {
                // Рестарт игры
                InitializeGame();
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            // Сброс флагов при отпускании клавиш
            if (e.KeyCode == Keys.Left)
            {
                leftPressed = false;
            }
            else if (e.KeyCode == Keys.Right)
            {
                rightPressed = false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            gameTimer?.Stop();
            objectTimer?.Stop();
            basketBrush?.Dispose();
            if (objectBrushes != null)
            {
                foreach (var brush in objectBrushes.Values)
                {
                    brush?.Dispose();
                }
            }
            gameFont?.Dispose();
        }
    }

    public class FallingObject
    {
        private Rectangle bounds;
        public Rectangle Bounds
        {
            get => bounds;
            set => bounds = value;
        }
        public string Type { get; }
        public int Speed { get; }

        public FallingObject(Rectangle bounds, string type, int speed)
        {
            this.bounds = bounds;
            Type = type;
            Speed = speed;
        }

        public void MoveDown()
        {
            bounds.Y += Speed;
        }
    }
}
