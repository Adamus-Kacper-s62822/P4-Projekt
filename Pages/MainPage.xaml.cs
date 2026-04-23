using Projekt.Services;
using Projekt.ViewModels;

namespace Projekt
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            BindingContext = new EmployeeViewModel(dbService);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is EmployeeViewModel vm)
            {
                vm.LoadData();
            }
        }

        private async void OnAddEmployeeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEmployeePage(_dbService));
        }
    }
}