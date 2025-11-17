using CameraDemoApp.Views;

namespace CameraDemoApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var mainPage = this.serviceProvider.GetRequiredService<MainPage>();
            return new Window(new NavigationPage(mainPage));
        }
    }
}
