import type { RouteRecordRaw } from 'vue-router';

import { BasicLayout } from '#/layouts';

const routes: RouteRecordRaw[] = [
  {
    // component: BasicLayout,
    meta: {
      icon: 'lucide:layout-dashboard',
      order: -1,
      title: '权限设置',
    },
    name: 'Permission',
    path: '/permission',
    children: [
      {
        name: 'Users',
        path: '/permission/user',
        component: () => import('#/views/permission/users/index.vue'),
        meta: {
          icon: 'lucide:area-chart',
          title: '用户管理',
        },
      },
      {
        name: 'Roles',
        path: '/permission/role',
        component: () => import('#/views/permission/roles/index.vue'),
        meta: {
          icon: 'lucide:area-chart',
          title: '角色管理',
        },
      },
      {
        name: 'Organizations',
        path: '/permission/organization',
        component: () => import('#/views/permission/organizations/index.vue'),
        meta: {
          icon: 'lucide:area-chart',
          title: '部门管理',
        },
      },
      {
        name: 'Menus',
        path: '/permission/menu',
        component: () => import('#/views/permission/menus/index.vue'),
        meta: {
          icon: 'lucide:area-chart',
          title: '菜单管理',
        },
      },
    ],
  },
];

export default routes;
