namespace Projekt
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("EmployeeDetailPage", typeof(EmployeeDetailPage));
            Routing.RegisterRoute("AddLeavePage", typeof(AddLeavePage));
        }
    }
}
