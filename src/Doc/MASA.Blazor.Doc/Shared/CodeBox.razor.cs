﻿using BlazorComponent;
using BlazorComponent.Doc.Models;
using MASA.Blazor.Doc.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MASA.Blazor.Doc.Shared;

public partial class CodeBox
{
    private const int Await = 2000;

    private readonly static(string type, string lang) Template = ("template", "html");
    private readonly static(string type, string lang) Code = ("code", "csharp");
    private readonly static(string type, string lang) Style = ("style", "css");

    private readonly Dictionary<(string type, string lang), string> _items = new()
    {
        {Template, null},
        {Code, null},
        {Style, null},
    };

    private StringNumber _activeItem;
    private bool _clicked;
    private bool _expend;
    private bool _showComponent;

    private RenderFragment Component { get; set; }

    [Inject]
    public IJSRuntime Js { get; set; }

    [Inject]
    public ILanguageService Lang { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public bool CodeExpand { get; set; }

    [Parameter]
    public string ComponentName { get; set; }

    [Parameter]
    public DemoItemModel Demo { get; set; }

    [Parameter]
    public int Index { get; set; }

    private string EditUrl =>
        $"https://github.com/ant-design-blazor/ant-design-blazor/edit/master/site/MASA.Blazor.Doc/Demos/Components/{ComponentName}/demo/{Demo.Name}.razor";

    protected override void OnInitialized()
    {
        _showComponent = Index < 2;

        if (Demo.Type != null)
        {
            Component = Service.GetShowCase(Demo.Type);
        }

        if (Demo.Code == null) return;

        var styleIndex = Demo.Code.IndexOf("<style>", StringComparison.Ordinal);
        var codeIndex = Demo.Code.IndexOf("@code", StringComparison.Ordinal);

        if (styleIndex > -1)
        {
            var length = codeIndex > -1 ? codeIndex - styleIndex : 0;
            _items[Style] = length > 0
                ? Demo.Code.Substring(styleIndex, length).Trim()
                : Demo.Code.Substring(styleIndex).Trim();
        }

        if (codeIndex > -1)
        {
            var length = styleIndex > -1 ? styleIndex : codeIndex;
            _items[Template] = Demo.Code.Substring(0, length).Trim();
            _items[Code] = Demo.Code.Substring(codeIndex).Trim();
        }
        else
        {
            _items[Template] = Demo.Code.Trim();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!_showComponent)
            {
                await Task.Delay(Index * 16);
                _showComponent = true;
                StateHasChanged();
            }
        }
    }

    private async Task Copy(string text)
    {
        _clicked = true;

        await Js.InvokeVoidAsync(JsInteropConstants.Copy, text);

        await Task.Delay(Await);

        _clicked = false;
    }
}