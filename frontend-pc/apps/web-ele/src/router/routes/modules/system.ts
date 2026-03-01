import type { RouteRecordRaw } from 'vue-router';

import { BasicLayout } from '#/layouts';

const routes: RouteRecordRaw[] = [
  {
    component: BasicLayout,
    meta: {
      icon: 'lucide:layout-dashboard',
      order: -1,
      title: '系统设置',
    },
    name: 'System',
    path: '/system',
    children: [
      {
        name: 'Pages',
        path: '/system/page',
        component: () => import('#/views/system/pages/index.vue'),
        meta: {
          icon: 'lucide:area-chart',
          title: '页面管理',
        },
      },
    ],
  },
];

export default routes;
