using CameraDemoApp.Views;

namespace CameraDemoApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();

            var mainPage = serviceProvider.GetRequiredService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);
        }
    }
}
