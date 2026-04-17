using Projekt.Services;
using Projekt.ViewModels;

namespace Projekt
{
    public partial class AddLeavePage : ContentPage
    {
        public AddLeavePage(DatabaseService dbService)
        {
            InitializeComponent();
            BindingContext = new AddLeaveViewModel(dbService);
        }
    }
}
