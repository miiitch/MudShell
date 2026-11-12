using MudBlazor;
using MudShell.Components.Navigation.Models;
using MudShell.Components.Sidebar;

namespace MyApplication.Client.Components;

public static class ShowcaseCatalog
{
    public sealed record Family(string Id, string Title, string Description, string Icon, string Href, string[] Components);

    public static readonly Family[] Families =
    [
        new("navigation", "Structure and navigation", "Find your way at every level, pin and reorder shortcuts.",
            Icons.Material.Outlined.AccountTree, "/components/navigation",
            ["MdsAppShell", "MdsSidebar", "MdsSidebarNav", "MdsSidebarSubMenu", "MdsContextNavPanel", "MdsBottomNav", "MdsMobileDrilldownNav"]),
        new("composition", "Page composition", "Organize actions, content and their states.",
            Icons.Material.Outlined.ViewAgenda, "/components/composition",
            ["MdsPageHeader", "MdsMainToolbar", "MdsMainToolbarDivider", "MdsMainButton", "MdsMainSection", "MdsMainPart", "MdsMainStatusBadge", "MdsMainEmptyState"]),
        new("layouts", "Layouts and workspaces", "Arrange space, tabs and scrolling regions.",
            Icons.Material.Outlined.ViewQuilt, "/components/layouts",
            ["MdsFillLayout", "MdsFillRegion", "MdsSplitPane", "MdsTabbedFill", "MdsTabHeader"]),
        new("documents", "Documents and filters", "Browse cards and refine a selection.",
            Icons.Material.Outlined.Description, "/components/documents",
            ["MdsDocumentCard", "MdsFilterTabBar"]),
        new("conversation", "Conversation and assistants", "From the first message to a streamed response.",
            Icons.Material.Outlined.Chat, "/components/conversation",
            ["MdsChatBar", "MdsChatLanding", "MdsChatComposer", "MdsChatTranscript", "MdsChatMessage", "MdsChatStream", "MdsChatThinking", "MdsChatPartRenderer", "MdsChatFloating"]),
        new("data", "Metrics and data", "Read values, trends and distributions.",
            Icons.Material.Outlined.BarChart, "/components/data",
            ["MdsStatTile", "MdsStatStrip", "MdsDistributionBar", "MdsSparkline", "MdsRunStrip"])
    ];

    public static readonly MbxNavItem[] PrimaryItems =
    [
        new(Icons.Material.Outlined.Home, "Home", "/"),
        new(Icons.Material.Outlined.Widgets, "Components", "/components"),
        new(Icons.Material.Outlined.Dashboard, "Examples", "/examples"),
        new(Icons.Material.Outlined.Code, "Technical", "/technical")
    ];

    public static MbxNavTree Tree => new(
    [
        new(id: "components", text: "Components", icon: Icons.Material.Outlined.Widgets, href: "/components", children:
            Families.Select(f => new MbxNavNode(id: $"family-{f.Id}", text: f.Title, icon: f.Icon,
                href: f.Href, children:
                [
                    new(id: $"{f.Id}-demo", text: "Demo", href: f.Href, pin: MbxNavPin.Top),
                    new(id: $"{f.Id}-details", text: f.Id == "navigation" ? "Mobile navigation" : "Variants and usage",
                        href: f.Id == "navigation" ? "/components/navigation/mobile" : $"{f.Href}#details", pin: MbxNavPin.Bottom)
                ])).ToList()),
        new(id: "examples", text: "Examples", icon: Icons.Material.Outlined.Dashboard, href: "/examples", children:
        [
            new(id: "examples-workspace", text: "Workspace", children:
            [
                new(id: "example-list", text: "List and preview", href: "/examples/workspace", pin: MbxNavPin.Top),
                new(id: "example-detail", text: "Detail page", href: "/examples/workspace/detail", pin: MbxNavPin.Bottom)
            ]),
            new(id: "examples-screens", text: "Complete screens", children:
            [
                new(id: "example-library", text: "Library", href: "/examples/library", pin: MbxNavPin.Top),
                new(id: "example-weather", text: "Weather", href: "/examples/weather", pin: MbxNavPin.Top),
                new(id: "example-cities", text: "European cities", href: "/examples/cities", pin: MbxNavPin.Bottom)
            ])
        ]),
        new(id: "technical", text: "Technical", icon: Icons.Material.Outlined.Code, href: "/technical", children:
        [
            new(id: "technical-rendering", text: "Blazor compatibility", children:
            [
                new(id: "technical-ssr", text: "Static SSR", href: "/modes/ssr", pin: MbxNavPin.Top),
                new(id: "technical-server", text: "Server", href: "/modes/server", pin: MbxNavPin.Top),
                new(id: "technical-wasm", text: "WebAssembly", href: "/modes/wasm", pin: MbxNavPin.Top),
                new(id: "technical-auto", text: "Auto", href: "/modes/auto", pin: MbxNavPin.Bottom)
            ]),
            new(id: "technical-appearance", text: "Integration and appearance", children:
            [
                new(id: "technical-guide", text: "Integration guide", href: "/technical", pin: MbxNavPin.Top),
                new(id: "technical-density", text: "Density", href: "/density", pin: MbxNavPin.Bottom)
            ])
        ])
    ]);

    public static string CanonicalPath(string path)
    {
        path = path.TrimEnd('/');
        if (path.EndsWith("/bg", StringComparison.OrdinalIgnoreCase))
            path = path[..^3];
        return path.ToLowerInvariant() switch
        {
            "/demo" => "/components/navigation",
            "/main-components" => "/components/composition",
            "/chat" => "/components/conversation",
            "/chat-floating" => "/components/conversation/floating",
            "/fill-demo" => "/examples/workspace",
            "/fill-demo/detail" => "/examples/workspace/detail",
            "/library" => "/examples/library",
            "/meteo" => "/examples/weather",
            "/europe-cities" => "/examples/cities",
            _ => path
        };
    }

    public static bool HasContext(string path)
    {
        path = CanonicalPath(path);
        return path.StartsWith("/components", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/examples", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/modes", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/technical", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/density", StringComparison.OrdinalIgnoreCase);
    }
}
