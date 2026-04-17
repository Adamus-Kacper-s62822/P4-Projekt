using Projekt.Models;
using Projekt.Services;
using Projekt.ViewModels;

namespace Projekt
{
    public partial class MainPage : ContentPage
    {
        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            BindingContext = new EmployeeViewModel(dbService);
        }

        private async void OnAddEmployeeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEmployeePage(
                App.Current.Handler.MauiContext.Services.GetService<DatabaseService>()));
        }

        private async void OnEmployeeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Employee selectedEmployee)
            {
                await Shell.Current.GoToAsync("EmployeeDetailPage", new Dictionary<string, object>
        {
            { "Employee", selectedEmployee }
        });
            }
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
