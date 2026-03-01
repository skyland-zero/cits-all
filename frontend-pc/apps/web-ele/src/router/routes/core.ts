import type { RouteRecordRaw } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';

import { $t } from '#/locales';

const BasicLayout = () => import('#/layouts/basic.vue');
const AuthPageLayout = () => import('#/layouts/auth.vue');
/** 全局404页面 */
const fallbackNotFoundRoute: RouteRecordRaw = {
  component: () => import('#/views/_core/fallback/not-found.vue'),
  meta: {
    hideInBreadcrumb: true,
    hideInMenu: true,
    hideInTab: true,
    title: '404',
  },
  name: 'FallbackNotFound',
  path: '/:path(.*)*',
};

/** 基本路由，这些路由是必须存在的 */
const coreRoutes: RouteRecordRaw[] = [
  /**
   * 根路由
   * 使用基础布局，作为所有页面的父级容器，子级就不必配置BasicLayout。
   * 此路由必须存在，且不应修改
   */
  {
    component: BasicLayout,
    meta: {
      hideInBreadcrumb: true,
      title: 'Root',
    },
    name: 'Root',
    path: '/',
    redirect: preferences.app.defaultHomePath,
    children: [],
  },
  {
    component: AuthPageLayout,
    meta: {
      hideInTab: true,
      title: 'Authentication',
    },
    name: 'Authentication',
    path: '/auth',
    redirect: LOGIN_PATH,
    children: [
      {
        name: 'Login',
        path: 'login',
        component: () => import('#/views/_core/authentication/login.vue'),
        meta: {
          title: $t('page.auth.login'),
        },
      },
      {
        name: 'CodeLogin',
        path: 'code-login',
        component: () => import('#/views/_core/authentication/code-login.vue'),
        meta: {
          title: $t('page.auth.codeLogin'),
        },
      },
      {
        name: 'QrCodeLogin',
        path: 'qrcode-login',
        component: () =>
          import('#/views/_core/authentication/qrcode-login.vue'),
        meta: {
          title: $t('page.auth.qrcodeLogin'),
        },
      },
      {
        name: 'ForgetPassword',
        path: 'forget-password',
        component: () =>
          import('#/views/_core/authentication/forget-password.vue'),
        meta: {
          title: $t('page.auth.forgetPassword'),
        },
      },
      {
        name: 'Register',
        path: 'register',
        component: () => import('#/views/_core/authentication/register.vue'),
        meta: {
          title: $t('page.auth.register'),
        },
      },
    ],
  },
  {
    meta: {
      title: 'Oidc',
      hideInBreadcrumb: true,
      hideInMenu: true,
      hideInTab: true,
    },
    name: 'Oidc',
    path: '/oidc',
    redirect: '/oidc/login',
    children: [
      {
        name: 'OidcCallback',
        path: 'callback',
        component: () => import('#/views/_core/oidc/callback.vue'),
        meta: {
          title: '统一认证中心-正在登录',
        },
      },
      {
        name: 'OidcLogin',
        path: 'login',
        component: () => import('#/views/_core/oidc/login.vue'),
        meta: {
          title: '统一认证中心-正在登录',
        },
      },
      {
        name: 'OidcSignoutSilentCallback',
        path: 'signout-silent-callback',
        component: () =>
          import('#/views/_core/oidc/signout-silent-callback.vue'),
        meta: {
          title: '统一认证中心-退出登录中',
        },
      },
      {
        name: 'OidcSignoutCallback',
        path: 'signout-callback',
        component: () => import('#/views/_core/oidc/signout-callback.vue'),
        meta: {
          title: '统一认证中心-退出登录中',
        },
      },
    ],
  },
];

export { coreRoutes, fallbackNotFoundRoute };
