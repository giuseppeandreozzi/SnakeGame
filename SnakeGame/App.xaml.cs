namespace SnakeGame
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell()) {
                Width = 1366,
                Height = 768,
                MinimumHeight = 768,
                MinimumWidth = 1366,
                MaximumHeight = 768,
                MaximumWidth = 1366
            };
        }
    }
}