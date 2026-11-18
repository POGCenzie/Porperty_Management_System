using PropertyManagementSystem.Constants;
using PropertyManagementSystem.Model;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace PropertyManagementSystem;

internal static class SoaPaperSizes
{
    internal const string A4 = "A4";
    internal const string ShortBond = "Short Bond (8.5 x 11 in)";
    internal const string LongBond = "Long Bond (8.5 x 13 in)";

    internal static IReadOnlyList<string> All { get; } =
        new[] { A4, ShortBond, LongBond };

    internal static PaperSize Create(string name) => name switch
    {
        ShortBond => new PaperSize(ShortBond, 850, 1100),
        LongBond => new PaperSize(LongBond, 850, 1300),
        _ => new PaperSize(A4, 827, 1169)
    };
}

internal static class SoaPrintService
{
    private static readonly Color Navy = Color.FromArgb(18, 53, 88);
    private static readonly Color LightBlue = Color.FromArgb(214, 228, 244);
    private static readonly Color AlternateBlue = Color.FromArgb(235, 242, 250);
    private static readonly Color Border = Color.FromArgb(155, 174, 194);

    internal static void ShowPreview(
        IWin32Window owner,
        StatementOfAccount statement,
        TenantAssignment assignment,
        string paperSize)
    {
        using var document = new PrintDocument
        {
            DocumentName = statement.SoaNumber,
            OriginAtMargins = false
        };
        document.DefaultPageSettings.Landscape = false;
        document.DefaultPageSettings.PaperSize = SoaPaperSizes.Create(paperSize);
        document.DefaultPageSettings.Margins = new Margins(38, 38, 35, 35);
        document.PrintPage += (_, eventArgs) =>
            DrawStatement(eventArgs, statement, assignment);

        using var preview = new PrintPreviewDialog
        {
            Document = document,
            Text = $"Print {statement.SoaNumber}",
            WindowState = FormWindowState.Maximized,
            UseAntiAlias = true
        };
        preview.ShowDialog(owner);
    }

    private static void DrawStatement(
        PrintPageEventArgs eventArgs,
        StatementOfAccount statement,
        TenantAssignment assignment)
    {
        var graphics = eventArgs.Graphics ??
            throw new InvalidOperationException("The printer did not provide a drawing surface.");
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var page = eventArgs.MarginBounds;
        var left = page.Left;
        var width = page.Width;
        var right = page.Right;
        var y = (float)page.Top;

        using var companyFont = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
        using var titleFont = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
        using var sectionFont = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        using var headerFont = new Font("Segoe UI Semibold", 8.4F, FontStyle.Bold);
        using var bodyFont = new Font("Segoe UI", 8.6F);
        using var bodyBoldFont = new Font("Segoe UI Semibold", 8.8F, FontStyle.Bold);
        using var smallFont = new Font("Segoe UI", 7.5F);
        using var whiteBrush = new SolidBrush(Color.White);
        using var navyBrush = new SolidBrush(Navy);
        using var lightBlueBrush = new SolidBrush(LightBlue);
        using var alternateBrush = new SolidBrush(AlternateBlue);
        using var textBrush = new SolidBrush(Color.FromArgb(30, 43, 58));
        using var secondaryBrush = new SolidBrush(Color.FromArgb(75, 91, 108));
        using var borderPen = new Pen(Border, 0.8F);

        var companyWidth = (int)(width * 0.58F);
        graphics.FillRectangle(navyBrush, left, y, companyWidth, 54);
        DrawText(graphics, "SYRENTO", companyFont, whiteBrush,
            new RectangleF(left + 12, y + 3, companyWidth - 24, 46),
            StringAlignment.Near, StringAlignment.Center);
        DrawText(graphics, "PROPERTY MANAGEMENT SERVICES", headerFont, navyBrush,
            new RectangleF(left + 4, y + 57, companyWidth - 8, 22));
        DrawLogo(graphics, new RectangleF(right - 210, y, 210, 82));
        y += 91;

        graphics.FillRectangle(navyBrush, left, y, width, 43);
        DrawText(graphics,
            statement.SoaType == SoaTypes.Owner
                ? "OWNER STATEMENT OF ACCOUNT"
                : "TENANT REFUND STATEMENT OF ACCOUNT",
            titleFont,
            whiteBrush,
            new RectangleF(left, y, width, 43),
            StringAlignment.Center,
            StringAlignment.Center);
        y += 43;

        var informationHeight = 132;
        graphics.FillRectangle(lightBlueBrush, left, y, width, informationHeight);
        graphics.DrawRectangle(borderPen, left, y, width, informationHeight);
        var middle = left + (width / 2);
        graphics.DrawLine(borderPen, middle, y, middle, y + informationHeight);

        var recipientName = statement.SoaType == SoaTypes.Owner
            ? assignment.OwnerName ?? "Unit Owner"
            : assignment.Tenant?.FullName ?? "Tenant";
        var recipientContact = statement.SoaType == SoaTypes.Owner
            ? assignment.OwnerContactNumber
            : assignment.MainTenantContactNumber ?? assignment.Tenant?.ContactNumber;
        var recipientEmail = statement.SoaType == SoaTypes.Owner
            ? assignment.OwnerEmailAddress
            : assignment.MainTenantEmailAddress ?? assignment.Tenant?.EmailAddress;
        DrawText(graphics, "BILL TO", headerFont, navyBrush,
            new RectangleF(left + 10, y + 8, width / 2 - 20, 18));
        DrawText(graphics, recipientName, sectionFont, textBrush,
            new RectangleF(left + 10, y + 27, width / 2 - 20, 23));
        DrawLabelValue(graphics, "Contact", recipientContact, left + 10, y + 56, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);
        DrawLabelValue(graphics, "Email", recipientEmail, left + 10, y + 80, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);
        DrawLabelValue(graphics, "Account", $"{assignment.Condominium?.Name} / Unit {assignment.UnitNumber}", left + 10, y + 104, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);

        DrawText(graphics, "PROPERTY AND CONTRACT", headerFont, navyBrush,
            new RectangleF(middle + 10, y + 8, width / 2 - 20, 18));
        DrawLabelValue(graphics, "Condominium", assignment.Condominium?.Name, middle + 10, y + 32, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);
        DrawLabelValue(graphics, "Unit number", assignment.UnitNumber, middle + 10, y + 56, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);
        DrawLabelValue(graphics, statement.SoaType == SoaTypes.Owner ? "Tenant" : "Unit owner",
            statement.SoaType == SoaTypes.Owner ? assignment.Tenant?.FullName : assignment.OwnerName,
            middle + 10, y + 80, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);
        DrawLabelValue(graphics, "Contract", FormatContractPeriod(assignment), middle + 10, y + 104, width / 2 - 20, bodyFont, bodyBoldFont, textBrush);
        y += informationHeight + 18;

        graphics.FillRectangle(lightBlueBrush, left, y, width, 28);
        DrawText(graphics, "STATEMENT INFORMATION", headerFont, navyBrush,
            new RectangleF(left + 8, y, 190, 28), StringAlignment.Near, StringAlignment.Center);
        DrawText(graphics, $"SOA #: {statement.SoaNumber}", bodyBoldFont, textBrush,
            new RectangleF(left + 205, y, width * 0.48F, 28), StringAlignment.Near, StringAlignment.Center);
        DrawText(graphics, $"Date: {statement.StatementDate:MMM d, yyyy}", bodyBoldFont, textBrush,
            new RectangleF(right - 205, y, 197, 28), StringAlignment.Far, StringAlignment.Center);
        y += 43;

        DrawText(graphics, "PAYMENTS AND ADJUSTMENTS", sectionFont, textBrush,
            new RectangleF(left + 4, y, width - 8, 25));
        y += 27;

        var rows = BuildRows(statement);
        var footerReserve = 108F;
        var availableTableHeight = Math.Max(250F, page.Bottom - y - footerReserve);
        var rowHeight = Math.Max(20F, Math.Min(31F, (availableTableHeight - 32F) / Math.Max(1, rows.Count)));
        var headerHeight = 32F;
        var categoryWidth = width * 0.16F;
        var descriptionWidth = width * 0.46F;
        var remarksWidth = width * 0.22F;
        var amountWidth = width - categoryWidth - descriptionWidth - remarksWidth;

        graphics.FillRectangle(navyBrush, left, y, width, headerHeight);
        DrawTableCell(graphics, "TYPE", headerFont, whiteBrush, left, y, categoryWidth, headerHeight, StringAlignment.Center);
        DrawTableCell(graphics, "DESCRIPTION", headerFont, whiteBrush, left + categoryWidth, y, descriptionWidth, headerHeight, StringAlignment.Near);
        DrawTableCell(graphics, "REMARKS", headerFont, whiteBrush, left + categoryWidth + descriptionWidth, y, remarksWidth, headerHeight, StringAlignment.Near);
        DrawTableCell(graphics, "AMOUNT", headerFont, whiteBrush, right - amountWidth, y, amountWidth, headerHeight, StringAlignment.Far);
        y += headerHeight;

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowBrush = row.IsTotal
                ? lightBlueBrush
                : index % 2 == 0
                    ? alternateBrush
                    : Brushes.White;
            graphics.FillRectangle(rowBrush, left, y, width, rowHeight);
            var font = row.IsTotal ? bodyBoldFont : bodyFont;
            DrawTableCell(graphics, row.Category, font, textBrush, left, y, categoryWidth, rowHeight, StringAlignment.Center);
            DrawTableCell(graphics, row.Description, font, textBrush, left + categoryWidth, y, descriptionWidth, rowHeight, StringAlignment.Near);
            DrawTableCell(graphics, row.Remarks, font, secondaryBrush, left + categoryWidth + descriptionWidth, y, remarksWidth, rowHeight, StringAlignment.Near);
            DrawTableCell(graphics, $"PHP {row.Amount:N2}", font, textBrush, right - amountWidth, y, amountWidth, rowHeight, StringAlignment.Far);
            graphics.DrawRectangle(borderPen, left, y, width, rowHeight);
            y += rowHeight;
        }

        y += 16;
        var totalWidth = Math.Min(310F, width * 0.46F);
        graphics.FillRectangle(navyBrush, right - totalWidth, y, totalWidth, 38);
        DrawText(graphics,
            statement.SoaType == SoaTypes.Owner ? "PAYABLE TO OWNER" : "TOTAL REFUND",
            headerFont,
            whiteBrush,
            new RectangleF(right - totalWidth + 10, y, totalWidth * 0.56F, 38),
            StringAlignment.Near,
            StringAlignment.Center);
        DrawText(graphics, $"PHP {statement.NetAmount:N2}", bodyBoldFont, whiteBrush,
            new RectangleF(right - totalWidth + (totalWidth * 0.56F), y, totalWidth * 0.40F, 38),
            StringAlignment.Far,
            StringAlignment.Center);

        var footerY = Math.Max(y + 54, page.Bottom - 52);
        using var footerPen = new Pen(Navy, 1.2F);
        graphics.DrawLine(footerPen, left, footerY - 10, right, footerY - 10);
        DrawText(graphics, "Prepared by Syrento Property Management Services", bodyBoldFont, textBrush,
            new RectangleF(left + 3, footerY, width * 0.60F, 20));
        DrawText(graphics, $"Filed by: {statement.CreatedByUsername}", smallFont, secondaryBrush,
            new RectangleF(right - 250, footerY, 247, 20), StringAlignment.Far, StringAlignment.Near);
        DrawText(graphics, "This system-generated statement records the payment and adjustment details shown above.", smallFont, secondaryBrush,
            new RectangleF(left + 3, footerY + 22, width - 6, 18));
        eventArgs.HasMorePages = false;
    }

    private static List<StatementPrintRow> BuildRows(StatementOfAccount statement)
    {
        var rows = new List<StatementPrintRow>();
        if (statement.RentalAmount is > 0M)
        {
            rows.Add(new StatementPrintRow("Payment", "Monthly rent reference", string.Empty, statement.RentalAmount.Value));
        }
        if (statement.AdvanceAmount is > 0M)
        {
            rows.Add(new StatementPrintRow("Payment", MonthDescription(statement.AdvanceAmount.Value, statement.RentalAmount, "advance"), string.Empty, statement.AdvanceAmount.Value));
        }
        if (statement.DepositAmount is > 0M)
        {
            rows.Add(new StatementPrintRow("Payment", MonthDescription(statement.DepositAmount.Value, statement.RentalAmount, "deposit"), string.Empty, statement.DepositAmount.Value));
        }
        rows.Add(new StatementPrintRow("Subtotal", "Starting amount", string.Empty, statement.BaseAmount, true));
        rows.AddRange(statement.LineItems
            .OrderBy(item => item.SortOrder)
            .Select(item => new StatementPrintRow(
                "Deduction",
                item.Description,
                item.Remarks ?? string.Empty,
                item.Amount)));
        rows.Add(new StatementPrintRow(
            "Total",
            statement.SoaType == SoaTypes.Owner ? "Total payable to owner" : "Total tenant refund",
            string.Empty,
            statement.NetAmount,
            true));
        return rows;
    }

    private static string MonthDescription(decimal amount, decimal? rent, string type)
    {
        if (rent is not > 0M)
        {
            return $"Owner {type}";
        }

        var months = decimal.Round(amount / rent.Value, 2);
        return $"{months:0.##} {(months == 1M ? "month" : "months")} {type}";
    }

    private static string FormatContractPeriod(TenantAssignment assignment)
    {
        if (!assignment.StartOfContract.HasValue && !assignment.EndOfContract.HasValue)
        {
            return "Not specified";
        }

        return $"{assignment.StartOfContract?.ToString("MMM d, yyyy") ?? "-"} to {assignment.EndOfContract?.ToString("MMM d, yyyy") ?? "-"}";
    }

    private static void DrawLogo(Graphics graphics, RectangleF bounds)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "SyrentoBrand.png");
        if (!File.Exists(path))
        {
            using var fallbackFont = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            using var brush = new SolidBrush(Navy);
            DrawText(graphics, "SYRENTO", fallbackFont, brush, bounds, StringAlignment.Center, StringAlignment.Center);
            return;
        }

        using var image = Image.FromFile(path);
        var scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        graphics.DrawImage(
            image,
            bounds.Right - width,
            bounds.Top + ((bounds.Height - height) / 2F),
            width,
            height);
    }

    private static void DrawLabelValue(
        Graphics graphics,
        string label,
        string? value,
        float x,
        float y,
        float width,
        Font labelFont,
        Font valueFont,
        Brush brush)
    {
        var labelWidth = Math.Min(92F, width * 0.34F);
        DrawText(graphics, label, labelFont, brush, new RectangleF(x, y, labelWidth, 20));
        DrawText(graphics, value ?? "-", valueFont, brush, new RectangleF(x + labelWidth, y, width - labelWidth, 20));
    }

    private static void DrawTableCell(
        Graphics graphics,
        string text,
        Font font,
        Brush brush,
        float x,
        float y,
        float width,
        float height,
        StringAlignment alignment) =>
        DrawText(graphics, text, font, brush,
            new RectangleF(x + 6, y + 2, Math.Max(1, width - 12), Math.Max(1, height - 4)),
            alignment,
            StringAlignment.Center);

    private static void DrawText(
        Graphics graphics,
        string text,
        Font font,
        Brush brush,
        RectangleF bounds,
        StringAlignment horizontal = StringAlignment.Near,
        StringAlignment vertical = StringAlignment.Near)
    {
        using var format = new StringFormat
        {
            Alignment = horizontal,
            LineAlignment = vertical,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        graphics.DrawString(text, font, brush, bounds, format);
    }

    private sealed record StatementPrintRow(
        string Category,
        string Description,
        string Remarks,
        decimal Amount,
        bool IsTotal = false);
}
