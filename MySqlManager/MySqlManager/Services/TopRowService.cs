using Microsoft.AspNetCore.Components;

namespace MySqlManager.Services;

public class TopRowService
{
    public RenderFragment? TopRow { get; set; }
    
    public event Action? OnTopRowChanged;

    private void SetTopRow(RenderFragment content)
    {
        TopRow = content;
        OnTopRowChanged?.Invoke();
    }

    public void SetTopRowForTableView(string? database, string? table, string? active = null)
    {
        SetTopRow(_builder =>
        {
            var seq = -1;
            _builder.OpenElement(++seq, "div");
            _builder.AddAttribute(++seq, "class", "container-fluid");
            
            _builder.OpenElement(++seq, "ul");
            _builder.AddAttribute(++seq, "class", "list-unstyled d-flex flex-row align-items-center");
            
            // "Browse" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueBrowse = $"/database/{database}/table/{table}";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary ms-0 me-2 mt-3 {(active == "browse" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueBrowse);
            _builder.AddContent(++seq, "Browse");
            _builder.CloseElement();
            _builder.CloseElement();
            
            // "Information" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueInformation = $"/database/{database}/table/{table}/information";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary ms-0 me-2 mt-3 {(active == "information" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueInformation);
            _builder.AddContent(++seq, "Information");
            _builder.CloseElement();
            _builder.CloseElement();
            
            // "SQL" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueSql = $"/database/{database}/table/{table}/sql";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary me-2 mt-3 {(active == "sql" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueSql);
            _builder.AddContent(++seq, "SQL");
            _builder.CloseElement();
            _builder.CloseElement();
            
            // "Search" link
            //_builder.OpenElement(++seq, "li");
            //_builder.OpenElement(++seq, "a");
            //var hrefValueSearch = $"/database/{database}/table/{table}/search";
            //_builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary me-2 mt-3 {(active == "search" ? "active" : "")}");
            //_builder.AddAttribute(++seq, "href", hrefValueSearch);
            //_builder.AddContent(++seq, "Search");
            //_builder.CloseElement();
            //_builder.CloseElement();
            
            // "Insert" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueInsert = $"/database/{database}/table/{table}/insert";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary me-2 mt-3 {(active == "insert" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueInsert);
            _builder.AddContent(++seq, "Insert");
            _builder.CloseElement();
            _builder.CloseElement();
            
            // "Import" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueImport = $"/database/{database}/table/{table}/import";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary me-2 mt-3 {(active == "import" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueImport);
            _builder.AddContent(++seq, "Import");
            _builder.CloseElement();
            _builder.CloseElement();
            
            // "Export" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueExport = $"/database/{database}/table/{table}/export";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary mt-3 {(active == "export" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueExport);
            _builder.AddContent(++seq, "Export");
            _builder.CloseElement();
            _builder.CloseElement();
            
            _builder.CloseElement(); // ul
            _builder.CloseElement(); // div
        });
    }

    public void SetTopRowForDatabaseView(string? database, string? active = null)
    {
        Console.WriteLine("TopRowService SetTopRowForDatabaseView ");
        SetTopRow(_builder =>
        {
            var seq = -1;
            _builder.OpenElement(++seq, "div");
            _builder.AddAttribute(++seq, "class", "container-fluid");
        
            _builder.OpenElement(++seq, "ul");
            _builder.AddAttribute(++seq, "class", "list-unstyled d-flex flex-row align-items-center");

            // "Overview" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueOverview = $"/database/{database}";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary ms-0 me-2 mt-3 {(active == "overview" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueOverview);
            _builder.AddContent(++seq, "Overview");
            _builder.CloseElement();
            _builder.CloseElement();
        
            // "Import" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueImport = $"/database/{database}/import";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary me-2 mt-3 {(active == "import" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueImport);
            _builder.AddContent(++seq, "Import");
            _builder.CloseElement();
            _builder.CloseElement();
            
            // "Export" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            var hrefValueExport = $"/database/{database}/export";
            _builder.AddAttribute(++seq, "class", $"btn btn-outline-secondary me-2 mt-3 {(active == "export" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueExport);
            _builder.AddContent(++seq, "Export");
            _builder.CloseElement();
            _builder.CloseElement();

            _builder.CloseElement(); // ul
            _builder.CloseElement(); // div
        });
    }

    public void ClearTopRow()
    {
        TopRow = null;
        OnTopRowChanged?.Invoke();
    }
}