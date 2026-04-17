using Projekt.Services;
using Projekt.ViewModels;

namespace Projekt
{
    public partial class EmployeeDetailPage : ContentPage
    {
        public EmployeeDetailPage(DatabaseService dbService)
        {
            InitializeComponent();
            BindingContext = new EmployeeDetailViewModel(dbService);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is EmployeeDetailViewModel viewModel)
            {
                viewModel.LoadDetails();
            }
        }
    }
}
