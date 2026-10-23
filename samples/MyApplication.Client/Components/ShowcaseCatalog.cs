using MudBlazor;
using MudShell.Components.Navigation.Models;
using MudShell.Components.Sidebar;

namespace MyApplication.Client.Components;

public static class ShowcaseCatalog
{
    public sealed record Family(string Id, string Title, string Description, string Icon, string Href, string[] Components);

    public static readonly Family[] Families =
    [
        new("navigation", "Structure et navigation", "Se repérer à chaque niveau, épingler et réorganiser ses raccourcis.",
            Icons.Material.Outlined.AccountTree, "/components/navigation",
            ["MdsAppShell", "MdsSidebar", "MdsSidebarNav", "MdsSidebarSubMenu", "MdsContextNavPanel", "MdsBottomNav", "MdsMobileDrilldownNav"]),
        new("composition", "Composition de page", "Hiérarchiser les actions, le contenu et leurs états.",
            Icons.Material.Outlined.ViewAgenda, "/components/composition",
            ["MdsPageHeader", "MdsMainToolbar", "MdsMainToolbarDivider", "MdsMainButton", "MdsMainSection", "MdsMainPart", "MdsMainStatusBadge", "MdsMainEmptyState"]),
        new("layouts", "Layouts et espaces de travail", "Répartir l’espace, les onglets et le défilement.",
            Icons.Material.Outlined.ViewQuilt, "/components/layouts",
            ["MdsFillLayout", "MdsFillRegion", "MdsSplitPane", "MdsTabbedFill", "MdsTabHeader"]),
        new("documents", "Documents et filtres", "Parcourir des cartes et affiner une sélection.",
            Icons.Material.Outlined.Description, "/components/documents",
            ["MdsDocumentCard", "MdsFilterTabBar"]),
        new("conversation", "Conversation et assistants", "De la première saisie à la réponse progressive.",
            Icons.Material.Outlined.Chat, "/components/conversation",
            ["MdsChatBar", "MdsChatLanding", "MdsChatComposer", "MdsChatTranscript", "MdsChatMessage", "MdsChatStream", "MdsChatThinking", "MdsChatPartRenderer", "MdsChatFloating"]),
        new("data", "Indicateurs et données", "Lire les valeurs, les tendances et les distributions.",
            Icons.Material.Outlined.BarChart, "/components/data",
            ["MdsStatTile", "MdsStatStrip", "MdsDistributionBar", "MdsSparkline", "MdsRunStrip"])
    ];

    public static readonly MbxNavItem[] PrimaryItems =
    [
        new(Icons.Material.Outlined.Home, "Accueil", "/"),
        new(Icons.Material.Outlined.Widgets, "Composants", "/components"),
        new(Icons.Material.Outlined.Dashboard, "Exemples", "/examples"),
        new(Icons.Material.Outlined.Code, "Technique", "/technical")
    ];

    public static MbxNavTree Tree => new(
    [
        new(id: "components", text: "Composants", icon: Icons.Material.Outlined.Widgets, href: "/components", children:
            Families.Select(f => new MbxNavNode(id: $"family-{f.Id}", text: f.Title, icon: f.Icon,
                href: f.Href, children:
                [
                    new(id: $"{f.Id}-demo", text: "Démonstration", href: f.Href, pin: MbxNavPin.Top),
                    new(id: $"{f.Id}-details", text: f.Id == "navigation" ? "Navigation mobile" : "Variantes et utilisation",
                        href: f.Id == "navigation" ? "/components/navigation/mobile" : $"{f.Href}#details", pin: MbxNavPin.Bottom)
                ])).ToList()),
        new(id: "examples", text: "Exemples", icon: Icons.Material.Outlined.Dashboard, href: "/examples", children:
        [
            new(id: "examples-workspace", text: "Espace de travail", children:
            [
                new(id: "example-list", text: "Liste et aperçu", href: "/examples/workspace", pin: MbxNavPin.Top),
                new(id: "example-detail", text: "Fiche détaillée", href: "/examples/workspace/detail", pin: MbxNavPin.Bottom)
            ]),
            new(id: "examples-screens", text: "Écrans complets", children:
            [
                new(id: "example-library", text: "Bibliothèque", href: "/examples/library", pin: MbxNavPin.Top),
                new(id: "example-weather", text: "Météo", href: "/examples/weather", pin: MbxNavPin.Top),
                new(id: "example-cities", text: "Villes européennes", href: "/examples/cities", pin: MbxNavPin.Bottom)
            ])
        ]),
        new(id: "technical", text: "Technique", icon: Icons.Material.Outlined.Code, href: "/technical", children:
        [
            new(id: "technical-rendering", text: "Compatibilité Blazor", children:
            [
                new(id: "technical-ssr", text: "SSR statique", href: "/modes/ssr", pin: MbxNavPin.Top),
                new(id: "technical-server", text: "Server", href: "/modes/server", pin: MbxNavPin.Top),
                new(id: "technical-wasm", text: "WebAssembly", href: "/modes/wasm", pin: MbxNavPin.Top),
                new(id: "technical-auto", text: "Auto", href: "/modes/auto", pin: MbxNavPin.Bottom)
            ]),
            new(id: "technical-appearance", text: "Intégration et apparence", children:
            [
                new(id: "technical-guide", text: "Guide d’intégration", href: "/technical", pin: MbxNavPin.Top),
                new(id: "technical-density", text: "Densité", href: "/density", pin: MbxNavPin.Bottom)
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
