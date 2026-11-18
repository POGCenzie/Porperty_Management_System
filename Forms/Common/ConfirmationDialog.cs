namespace PropertyManagementSystem;

internal partial class ConfirmationDialog : Form
{
    internal ConfirmationDialog(string title, string message, string confirmText)
    {
        ConfigureWindow(title);
        BuildInterface(title, message, confirmText);
    }
}
