import type { RouteRecordRaw } from 'vue-router';

import { BasicLayout } from '#/layouts';

const routes: RouteRecordRaw[] = [
  {
    component: BasicLayout,
    meta: {
      icon: 'lucide:layout-dashboard',
      order: -1,
      title: '工单管理',
    },
    name: 'WorkOrder',
    path: '/workorder',
    children: [
      {
        name: 'WorkOrderManages',
        path: '/workorder/manages/index',
        component: () => import('#/views/workorder/manages/index.vue'),
        meta: {
          icon: 'lucide:area-chart',
          title: '工单管理',
          keepAlive: true,
        },
      },
      {
        name: 'WorkOrderCreate',
        path: '/workorder/manages/create',
        component: () => import('#/views/workorder/manages/create.vue'),
        meta: {
          title: '新增工单',
          hideInMenu: true,
          currentActiveMenu: '/workorder/manages/index',
        },
      },
    ],
  },
];

export default routes;
