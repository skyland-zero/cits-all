import type { RouteRecordRaw } from 'vue-router';

import { BasicLayout } from '#/layouts';

const routes: RouteRecordRaw[] = [
  {
    component: BasicLayout,
    meta: {
      icon: 'lucide:monitor',
      order: 10,
      title: '系统监控',
    },
    name: 'Monitor',
    path: '/monitor',
    children: [
      {
        name: 'ServerMonitor',
        path: '/monitor/server',
        component: () => import('#/views/monitor/server/index.vue'),
        meta: {
          icon: 'lucide:cpu',
          title: '服务器监控',
        },
      },
    ],
  },
];

export default routes;
