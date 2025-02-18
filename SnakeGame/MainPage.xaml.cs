using Microsoft.Maui.Controls.Shapes;
using System.Timers;

namespace SnakeGame {
	public partial class MainPage : ContentPage {
		//the cells contains:
		//0 if the chell is empty; 1 if the cell is occupied; 2 if the cell contains the snake's head; 3 if the cell contains the food
		private byte[,] gridGame;
		private int numRows, numCols;
		private Direction currentDirection;
		private bool run = false;
		private System.Timers.Timer runningGame;
		private Image food;

		//will contains the cell that the snake occupies
		LinkedList<CellSnake> snake;

		//will contains the rectangle used to raffigurate the snake in UI, used to remove the element from grid 
		LinkedList<Rectangle> snakePieces;

		public MainPage() {
			food = new Image() { Source = "food.png" };
			snake = new LinkedList<CellSnake>();
			snakePieces = new LinkedList<Rectangle>();
			InitializeComponent();
			InsideGrid.BackgroundColor = Colors.WhiteSmoke;

			numRows = 734 / 12;
			numCols = (1366 - 300) / 12;
			gridGame = new byte[numCols, numRows];

			//Adding rows and columns to the grid
			for (int i = 0; i < numCols; i++) {
				InsideGrid.AddColumnDefinition(new ColumnDefinition() { Width = new GridLength(12) });
			}

			for (int i = 0; i < numRows; i++) {
				InsideGrid.AddRowDefinition(new RowDefinition() { Height = new GridLength(12) });
			}


			currentDirection = (Direction)new Random().Next(4);
			GenerateInitialSnake(3, currentDirection, new CellSnake() { X = numCols / 2 - 1, Y = numRows / 2 - 1 });

			GenerateFood();
		}

		private void GenerateFood() {
			Random rnd = new Random();
			bool created = false;
			int row, col;

			while (!created) {
				row = rnd.Next(numRows);
				col = rnd.Next(numCols);

				if (gridGame[col, row] == 0) {
					InsideGrid.Add(food, col, row);
					gridGame[col, row] = 3;
					created = true;
				}
			}
		}
		private void GenerateInitialSnake(int length, Direction direction, CellSnake initialPosition) {
			Rectangle rectangle = new Rectangle() { BackgroundColor = Colors.Red };
			InsideGrid.Add(rectangle, initialPosition.X, initialPosition.Y);
			gridGame[initialPosition.X, initialPosition.Y] = 2;
			snakePieces.AddFirst(rectangle);
			snake.AddFirst(initialPosition);

			CellSnake cellToAdd = direction switch {
				Direction.Up => new CellSnake { X = 0, Y = 1 },
				Direction.Down => new CellSnake { X = 0, Y = -1 },
				Direction.Right => new CellSnake { X = -1, Y = 0 },
				Direction.Left => new CellSnake { X = 1, Y = 0 },
				_ => throw new NotImplementedException(),

			};

			for (int i = 1; i < length; i++) {
				CellSnake lastCell = snake.Last();
				CellSnake newCell = new CellSnake() { X = lastCell.X + cellToAdd.X, Y = lastCell.Y + cellToAdd.Y, Direction = direction };
				Rectangle rect = new Rectangle() { BackgroundColor = Colors.Black };
				InsideGrid.Add(rect, newCell.X, newCell.Y);
				gridGame[newCell.X, newCell.Y] = 1;
				snakePieces.AddLast(rect);
				snake.AddLast(newCell);
			}
		}

		void RunGame(object sender, EventArgs args) {
			run = true;
			runningGame = new System.Timers.Timer(200);
			runningGame.Elapsed += (object source, ElapsedEventArgs e) => {
				MainThread.BeginInvokeOnMainThread(() => {
					Run();
				});
			};
			runningGame.AutoReset = true;
			runningGame.Enabled = true;
		}

		void Run() {
			if (!run) //to prevent another execution if the game is already started
				return;

			CellSnake nextCell, newHeadCell, currentHeadCell, cellToEliminate;

			nextCell = currentDirection switch {
				Direction.Up => new CellSnake { X = 0, Y = -1 },
				Direction.Down => new CellSnake { X = 0, Y = 1 },
				Direction.Right => new CellSnake { X = 1, Y = 0 },
				Direction.Left => new CellSnake { X = -1, Y = 0 },
				_ => throw new NotImplementedException(),
			};

			currentHeadCell = snake.First();
			newHeadCell = new CellSnake() { X = (currentHeadCell.X + nextCell.X) % numCols, Y = (currentHeadCell.Y + nextCell.Y) % numRows, Direction = currentDirection };

			if (newHeadCell.X < 0)
				newHeadCell.X = numCols - 1;

			if (newHeadCell.Y < 0)
				newHeadCell.Y = numRows - 1;

			//Update collections
			snake.AddFirst(newHeadCell);
			cellToEliminate = snake.Last();
			snake.RemoveLast();




			//Update UI
			Rectangle rectToRemove = snakePieces.First();
			snakePieces.RemoveFirst();
			InsideGrid.Remove(rectToRemove);

			Rectangle rect1 = new Rectangle() { BackgroundColor = Colors.Black };
			snakePieces.AddFirst(rect1);
			InsideGrid.Add(rect1, currentHeadCell.X, currentHeadCell.Y);

			Rectangle rect2 = new Rectangle() { BackgroundColor = Colors.Red };
			snakePieces.AddFirst(rect2);
			InsideGrid.Add(rect2, newHeadCell.X, newHeadCell.Y);

			//if the snake doesn't reach the food will continue normally, otherwise the last cell of snake it's not deleted and it will generate new food on the grid
			if (gridGame[newHeadCell.X, newHeadCell.Y] != 3) {
				gridGame[cellToEliminate.X, cellToEliminate.Y] = 0;
				rectToRemove = snakePieces.Last();
				snakePieces.RemoveLast();
				InsideGrid.Remove(rectToRemove);
			} else {
				InsideGrid.Remove(food);
				GenerateFood();
			}

			gridGame[newHeadCell.X, newHeadCell.Y] = 2;
			gridGame[currentHeadCell.X, currentHeadCell.Y] = 1;
		}

		private void AddSnakePiece(CellSnake cell) {
			Rectangle rectangle = new Rectangle() { BackgroundColor = Colors.Black };

			snakePieces.AddLast(rectangle);
			InsideGrid.Add(rectangle, cell.X, cell.Y);
			snake.AddLast(cell);

		}

		void StopGame(object sender, EventArgs e) {
			if (!run)
				return;

			run = false;
			runningGame.Stop();
			runningGame.Dispose();
		}

		void ChangeDirection(object sender, EventArgs e) {
			Button button = (Button)sender;
			string? parameter = button.CommandParameter.ToString();

			currentDirection = parameter switch {
				"UP" => Direction.Up,
				"DOWN" => Direction.Down,
				"LEFT" => Direction.Left,
				"RIGHT" => Direction.Right,
				_ => throw new NotImplementedException()
			};

		}
	}

	struct CellSnake {
		public int X { get; set; }
		public int Y { get; set; }
		public Direction Direction { get; set; }
	}

	enum Direction {
		Left, Right, Up, Down
	}
}


