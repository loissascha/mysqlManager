using Microsoft.AspNetCore.Components;

namespace MySqlManager.Services;

public class TopRowService
{
    private RenderFragment? TopRow { get; set; }

    private void SetTopRow(RenderFragment content)
    {
        TopRow = content;
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
        string hrefValueBrowse = $"/database/{database}/table/{table}";
        _builder.AddAttribute(++seq, "class", $"btn btn-primary ms-0 me-2 mt-3 {(active == "browse" ? "active" : "")}");
        _builder.AddAttribute(++seq, "href", hrefValueBrowse);
        _builder.AddContent(++seq, "Browse");
        _builder.CloseElement();
        _builder.CloseElement();
        
        // "SQL" link
        _builder.OpenElement(++seq, "li");
        _builder.OpenElement(++seq, "a");
        var hrefValueSql = $"/database/{database}/table/{table}/sql";
        _builder.AddAttribute(++seq, "class", $"btn btn-primary me-2 mt-3 {(active == "sql" ? "active" : "")}");
        _builder.AddAttribute(++seq, "href", hrefValueSql);
        _builder.AddContent(++seq, "SQL");
        _builder.CloseElement();
        _builder.CloseElement();
        
        // "Search" link
        _builder.OpenElement(++seq, "li");
        _builder.OpenElement(++seq, "a");
        var hrefValueSearch = $"/database/{database}/table/{table}/search";
        _builder.AddAttribute(++seq, "class", $"btn btn-primary me-2 mt-3 {(active == "search" ? "active" : "")}");
        _builder.AddAttribute(++seq, "href", hrefValueSearch);
        _builder.AddContent(++seq, "Search");
        _builder.CloseElement();
        _builder.CloseElement();
        
        // "Insert" link
        _builder.OpenElement(++seq, "li");
        _builder.OpenElement(++seq, "a");
        var hrefValueInsert = $"/database/{database}/table/{table}/insert";
        _builder.AddAttribute(++seq, "class", $"btn btn-primary mt-3 {(active == "insert" ? "active" : "")}");
        _builder.AddAttribute(++seq, "href", hrefValueInsert);
        _builder.AddContent(++seq, "Insert");
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
            string hrefValueOverview = $"/database/{database}";
            _builder.AddAttribute(++seq, "class", $"btn btn-primary ms-0 me-2 mt-3 {(active == "overview" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueOverview);
            _builder.AddContent(++seq, "Overview");
            _builder.CloseElement();
            _builder.CloseElement();
        
            // "Import" link
            _builder.OpenElement(++seq, "li");
            _builder.OpenElement(++seq, "a");
            string hrefValueImport = $"/database/{database}/import";
            _builder.AddAttribute(++seq, "class", $"btn btn-primary me-2 mt-3 {(active == "import" ? "active" : "")}");
            _builder.AddAttribute(++seq, "href", hrefValueImport);
            _builder.AddContent(++seq, "Import");
            _builder.CloseElement();
            _builder.CloseElement();

            _builder.CloseElement(); // ul
            _builder.CloseElement(); // div
        });
    }
}