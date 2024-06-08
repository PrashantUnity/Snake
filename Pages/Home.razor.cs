using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Snake.Model; 

namespace Snake.Pages
{
    public partial class Home
    {
        int row = 10;
        int column = 25;
        List<List<Node>> ls = [];
        Node head;
        Random random = new();
        char PreviousKey = 'O';
        int sleepTimer = 500;
        int snakeLength = 0;

        bool GameOfover = false;
        bool IsPlaying = false;

        #region Color Variable
        readonly string headColor = $"background:{Colors.Teal.Default};";
        readonly string bodyColor = $"background:{Colors.Teal.Lighten3};";
        readonly string foodColor = $"background:{Colors.LightBlue.Darken1};";
        readonly string playGroundColor = $"background:{Colors.LightBlue.Lighten5};";
        #endregion

        Queue<Node> snake = new();

        #region Dialog Related
        private bool visible;
        private void OpenDialog()
        {
            GameOfover = true;
            IsPlaying = false;
        }
        void Submit()
        {
            GameOfover = false;
            while (snake.Count > 1)
            {
                var tail = snake.Dequeue();
                tail.NodeColor = playGroundColor;
                tail.IsBody = false;
                tail.IsfoodPosition = false;
            }
            snakeLength = snake.Count;
            StartGame();
        }

        DialogOptions dialogOptions =
        new()
        {
            FullWidth = true,
            DisableBackdropClick = true
        };

        #endregion

        protected override Task OnInitializedAsync()
        {
            ls = new();

            for (var i = 0; i < row; i++)
            {
                var temp = new List<Node>();
                for (var j = 0; j < column; j++)
                {
                    temp.Add(new Node(i, j));
                }
                ls.Add(temp);
            }
            ls[5][5].IsBody = true;
            head = ls[5][5];
            head.NodeColor = headColor;

            snake.Enqueue(head);
            snakeLength++;

            return base.OnInitializedAsync();
        }

        //food
        async Task StartGame()
        {
            int prevX = -1;
            int prevY = -1;
            int x = random.Next(0, row), y = random.Next(0, row);

            if (IsPlaying) return;

            while (!GameOfover)
            {
                prevX = x;
                prevY = y;

                x = random.Next(0, row);
                y = random.Next(0, column);

                while (ls[x][y].IsBody)
                {
                    x = random.Next(0, row);
                    y = random.Next(0, column);
                }
                ls[x][y].IsfoodPosition = true;
                ls[x][y].NodeColor = foodColor;

                if (x != prevX && y != prevY && !ls[prevX][prevY].IsBody)
                {
                    ls[prevX][prevY].IsfoodPosition = false;
                    ls[prevX][prevY].NodeColor = playGroundColor;
                }
                IsPlaying = true;
                StateHasChanged();
                await Task.Delay(6000);
            }
            IsPlaying = false;
        }

        #region JavaScript InterOps Key Binding
        private HashSet<string> left  = ["ARROWLEFT","L"];
        private HashSet<string> right = ["ARROWRIGHT","D"];
        private HashSet<string> up    = ["ARROWUP","W"];
        private HashSet<string> down  = ["ARROWDOWN","S"];
        WindowSize? windowSize;
        readonly int gridDimentions = 64;
        readonly int inputDelayInMilliSecond = 400;


        #region Experimental Time
        
        //private string lastInput;
        //private bool canTakeInput = true;
        //private System.Timers.Timer _inputKeyDelay;
        //private System.Timers.Timer _snakeySpeed;

        //protected override void OnInitialized()
        //{
        //    _inputKeyDelay = new System.Timers.Timer(100);
        //    _inputKeyDelay.Elapsed += OnTimerElapsed;
        //    _inputKeyDelay.AutoReset = false; // Ensures the timer runs only once per trigger


        //    // for this feature wiil be implemented later.
        //    _snakeySpeed = new System.Timers.Timer(100);
        //    _snakeySpeed.Elapsed += OnTimerElapsed;
        //    _snakeySpeed.AutoReset = false; // Ensures the timer runs only once per trigger
        //}
         

        //private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{ 
        //    InvokeAsync(() =>
        //    {
        //        canTakeInput = true;
        //        _inputKeyDelay.Stop(); // Stop the timer until the next input event
        //    });
        //}
        //public void Dispose()
        //{
        //    _inputKeyDelay.Dispose();
        //}

        #endregion
        [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("blazorKeyPressed", DotNetObjectReference.Create(this));
                await JSRuntime.InvokeVoidAsync("blazorGestureDetected", DotNetObjectReference.Create(this));
                await GetInnerDimensions();
            }
        }
        private async Task GetInnerDimensions()
        {
            windowSize = await JSRuntime.InvokeAsync<WindowSize>("getInnerDimensions");
            Resize();
        }
        void Resize()
        {
            int extra = windowSize?.Width > 1400 ? 3 : 0;
            column = (windowSize?.Width / gridDimentions) - extra ?? 10;
            row = (windowSize?.Height / gridDimentions) - 2 ?? 10;
            Reset();
            StateHasChanged();
        }
        void Reset()
        { 
            ls = new();

            for (var i = 0; i < row; i++)
            {
                var temp = new List<Node>();
                for (var j = 0; j < column; j++)
                {
                    temp.Add(new Node(i, j));
                }
                ls.Add(temp);
            }
            ls[5][5].IsBody = true;
            head = ls[5][5];
            head.NodeColor = headColor;

            snake.Enqueue(head);
            snakeLength++;
            StateHasChanged();
            StartGame();
        }

        [JSInvokable]
        public async Task OnArrowKeyPressed(string key)
        {
            //if (!canTakeInput) return;
            key = key.ToUpper();

            if (left.Contains(key))
            {
                await SnakeMovementControllingLoop('L');
            }
            if (right.Contains(key))
            {
                await SnakeMovementControllingLoop('R');
            }
            if (up.Contains(key))
            {
                await SnakeMovementControllingLoop('U');
            }
            if (down.Contains(key))
            {
                await SnakeMovementControllingLoop('D');
            }
            //canTakeInput = false;
            //_inputKeyDelay.Start();  
            
        }

        #endregion
        async Task SnakeMovementControllingLoop(char CurrentKey)
        { 
            if (CurrentKey == PreviousKey) return;
            if (CurrentKey == 'L' && PreviousKey == 'R') return;
            if (CurrentKey == 'R' && PreviousKey == 'L') return;

            if (CurrentKey == 'U' && PreviousKey == 'D') return;
            if (CurrentKey == 'D' && PreviousKey == 'U') return;

            PreviousKey = CurrentKey;


            while (PreviousKey == 'L')
            {
                var y = head.Y - 1;
                var x = head.X;
                if (y < 0) y = column - 1;

                if (ls[x][y].IsBody)
                {
                    GameOfover = true;
                    break;
                }

                head.NodeColor = bodyColor;

                head = ls[x][y];


                SnakeHelper(head);

                StateHasChanged();
                await Task.Delay(sleepTimer);
            }

            while (PreviousKey == 'R')
            {
                var y = head.Y + 1;
                var x = head.X;

                if (y >= column) y = 0;

                if (ls[x][y].IsBody)
                {
                    GameOfover = true;
                    break;
                }

                head.NodeColor = bodyColor;

                head = ls[x][y];

                SnakeHelper(head);

                StateHasChanged();
                await Task.Delay(sleepTimer);
            }
            while (PreviousKey == 'U')
            {
                var x = head.X - 1;
                var y = head.Y;
                if (x < 0) x = row - 1;


                if (ls[x][y].IsBody)
                {
                    GameOfover = true;
                    break;
                }

                head.NodeColor = bodyColor;
                head = ls[x][y];

                SnakeHelper(head);

                StateHasChanged();
                await Task.Delay(sleepTimer);
            }
            while (PreviousKey == 'D')
            {
                var x = head.X + 1;
                var y = head.Y;
                if (x >= row) x = 0;

                if (ls[x][y].IsBody)
                {
                    GameOfover = true;
                    break;
                }

                head.NodeColor = bodyColor;
                head = ls[x][head.Y];

                SnakeHelper(head);

                StateHasChanged();
                await Task.Delay(sleepTimer);
            }
            if (GameOfover) OpenDialog();

        }
        void SnakeHelper(Node head)
        {
            head.NodeColor = headColor;
            head.IsBody = true;
            snake.Enqueue(head);

            if (head.IsfoodPosition) snakeLength++;

            if (snakeLength < snake.Count)
            {
                var tail = snake.Dequeue();
                tail.NodeColor = playGroundColor;
                tail.IsBody = false;
                tail.IsfoodPosition = false;
            }
        }
    }
}
