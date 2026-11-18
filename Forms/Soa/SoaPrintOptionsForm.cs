using PropertyManagementSystem.Model;

namespace PropertyManagementSystem;

internal partial class SoaPrintOptionsForm : Form
{
    private readonly StatementOfAccount statement;

    internal SoaPrintOptionsForm(StatementOfAccount statement)
    {
        this.statement = statement;
        ConfigureWindow();
        BuildInterface();
    }

    internal string SelectedPaperSize =>
        paperSizeComboBox.SelectedItem?.ToString() ?? SoaPaperSizes.A4;
}
