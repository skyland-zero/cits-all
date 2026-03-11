import type { RouteRecordRaw } from 'vue-router';

import { BasicLayout } from '#/layouts';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    component: BasicLayout,
    meta: {
      icon: 'lucide:layout-dashboard',
      order: 10,
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
          title: '工单管理',
        },
      },
      {
        name: 'WorkOrderCreate',
        path: '/workorder/manages/create',
        component: () => import('#/views/workorder/manages/create.vue'),
        meta: {
          title: '新增工单',
          hideInMenu: true,
          activeMenu: '/workorder/manages/index',
        },
      },
    ],
  },
];

export default routes;
