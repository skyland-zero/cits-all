import type { UserInfo } from '@vben/types';

import { ref } from 'vue';
import { useRouter } from 'vue-router';

import {  LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { resetAllStores, useAccessStore, useUserStore } from '@vben/stores';

import { UserManager, WebStorageStateStore } from 'oidc-client-ts';
import { defineStore } from 'pinia';

import { getAccessCodesApi, getUserInfoApi } from '#/api';

export const useOidcStore = defineStore('oidc', () => {
  const accessStore = useAccessStore();
  const userStore = useUserStore();
  const router = useRouter();

  const loginLoading = ref(false);

  const userManager = new UserManager({
    authority: 'https://localhost:63277',
    scope: 'NextApi offline_access',
    client_id: 'NextApi_App',
    redirect_uri: `${window.location.origin}/oidc/callback`,
    post_logout_redirect_uri: `${window.location.origin}/oidc/signout-callback`,
    response_type: 'code',
    userStore: new WebStorageStateStore({ store: window.localStorage }),
  });

  userManager.events.addAccessTokenExpiring(() => {
    // eslint-disable-next-line no-console
    console.log('token expiring');
    refreshToken();
  });

  userManager.events.addAccessTokenExpired(() => {
    // eslint-disable-next-line no-console
    console.log('token expired');
  });

  userManager.events.addSilentRenewError((e) => {
    // eslint-disable-next-line no-console
    console.log('silent renew error', e.message);
  });

  /**
   * 跳转到授权中心登录
   */
  async function signIn() {
    return userManager.signinRedirect({
      extraQueryParams: {
        identity_provider: 'Local',
        hardcoded_identity_id: '1',
      },
    });
  }

  async function callback(onSuccess?: () => Promise<void> | void) {
    let userInfo: null | UserInfo = null;
    try {
      loginLoading.value = true;
      const user = await userManager.signinCallback();
      if (user) {
        // 将accesstoken存储到accessstore中
        accessStore.setAccessToken(user.access_token);

        // 获取用户信息并存储到 accessStore 中

        const [fetchUserInfoResult, accessCodes] = await Promise.all([
          fetchUserInfo(user.access_token),
          getAccessCodesApi(),
        ]);
        userInfo = fetchUserInfoResult;
        userStore.setUserInfo(userInfo);
        accessStore.setAccessCodes(accessCodes);

        if (accessStore.loginExpired) {
          accessStore.setLoginExpired(false);
        } else {
          onSuccess
            ? await onSuccess?.()
            : await router.push(userInfo.homePath ||  preferences.app.defaultHomePath);
        }
      } else {
        // eslint-disable-next-line no-console
        console.log('登录失败');
      }
      // eslint-disable-next-line no-console
      console.log('callback', user);
    } catch (error) {
      // eslint-disable-next-line no-console
      console.log('error', error);
    } finally {
      loginLoading.value = false;
    }

    return {
      userInfo,
    };
  }

  async function fetchUserInfo(accessToken: string) {
    let userInfo: null | UserInfo = null;
    userInfo = await getUserInfoApi();
    userInfo.token = accessToken;
    userStore.setUserInfo(userInfo);
    return userInfo;
  }

  async function logout(redirect: boolean = true) {
    // oidc退出登录
    await userManager.signoutRedirect();
    // eslint-disable-next-line no-console
    console.log(redirect);
  }

  async function signoutSilentCallback() {
    await userManager.signoutSilentCallback();
  }

  async function signoutCallback() {
    await userManager.signoutCallback();
    // 清理本地缓存
    resetAllStores();
    accessStore.setLoginExpired(false);
    // 回登录页
    await router.replace({
      path: LOGIN_PATH,
      query: {},
    });
  }

  /**
   * 静默刷新token
   */
  async function refreshToken() {
    const user = await userManager.signinSilent();
    // eslint-disable-next-line no-console
    console.log('refreshToken user', user);
    if (user) {
      accessStore.setAccessToken(user.access_token);
      return user.access_token;
    }
    throw Object.assign({}, user, { user });
  }

  function $reset() {
    loginLoading.value = false;
  }

  return {
    $reset,
    signIn,
    callback,
    logout,
    signoutCallback,
    signoutSilentCallback,
    refreshToken,
  };
});
