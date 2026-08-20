import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  docsSidebar: [
    'getting-started',
    'theming',
    'palettes',
    'responsive',
    'architecture',
    {
      type: 'category',
      label: 'Components',
      items: [
        'components/app-shell',
        'components/sidebar',
        'components/bottom-nav',
        'components/chat-bar',
        'components/document-card',
        'components/filter-tab-bar',
        'components/page-header',
      ],
    },
  ],
};

export default sidebars;
