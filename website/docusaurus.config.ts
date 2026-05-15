import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'MudShell',
  tagline: 'Opinionated Blazor components built on top of MudBlazor 9',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://miiitch.github.io',
  baseUrl: '/MudBlazorExt/',

  organizationName: 'miiitch',
  projectName: 'MudBlazorExt',
  trailingSlash: false,

  onBrokenLinks: 'throw',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/miiitch/MudBlazorExt/edit/main/website/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'MudShell',
      logo: {
        alt: 'MudShell Logo',
        src: 'img/logo.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://www.nuget.org/packages/MudShell',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/miiitch/MudBlazorExt',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Getting Started', to: '/docs/getting-started'},
            {label: 'Components', to: '/docs/components/app-shell'},
            {label: 'Theming', to: '/docs/theming'},
          ],
        },
        {
          title: 'More',
          items: [
            {label: 'GitHub', href: 'https://github.com/miiitch/MudBlazorExt'},
            {label: 'NuGet', href: 'https://www.nuget.org/packages/MudShell'},
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} miiitch. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'bash'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
