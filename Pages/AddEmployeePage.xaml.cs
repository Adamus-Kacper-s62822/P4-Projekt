using Projekt.Services;
using Projekt.ViewModels;

namespace Projekt
{
    public partial class AddEmployeePage : ContentPage
    {
        public AddEmployeePage(DatabaseService dbService)
        {
            InitializeComponent();
            BindingContext = new AddEmployeeViewModel(dbService);
        }
    }
}
