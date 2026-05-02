/**
 * 该文件可自行根据业务逻辑进行调整
 */
import type { RequestClientOptions } from '@vben/request';

import { useAppConfig } from '@vben/hooks';
import { preferences } from '@vben/preferences';
import {
  authenticateResponseInterceptor,
  errorMessageResponseInterceptor,
  RequestClient,
} from '@vben/request';
import { useAccessStore } from '@vben/stores';

import { ElMessage } from 'element-plus';

import { useAuthStore } from '#/store';

import { refreshTokenApi } from './core';

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
const LOGIN_EXPIRED_MESSAGE = '登录认证过期，请重新登录后继续。';

function createLoginExpiredError(cause: unknown) {
  return Object.assign(new Error(LOGIN_EXPIRED_MESSAGE), {
    cause,
    response: {
      data: { message: LOGIN_EXPIRED_MESSAGE },
      status: 401,
    },
  });
}

function getResponseErrorMessage(responseData: any) {
  const error = responseData?.error;

  if (typeof error === 'string') {
    return error;
  }

  if (typeof error?.message === 'string') {
    return error.message;
  }

  if (typeof responseData?.message === 'string') {
    return responseData.message;
  }

  return '';
}

function createRequestClient(baseURL: string, options?: RequestClientOptions) {
  const client = new RequestClient({
    ...options,
    baseURL,
  });

  /**
   * 重新认证逻辑
   */
  async function doReAuthenticate() {
    console.warn('Access token or refresh token is invalid or expired. ');
    const accessStore = useAccessStore();
    const authStore = useAuthStore();
    accessStore.setAccessToken(null);
    accessStore.setRefreshToken(null);
    if (
      preferences.app.loginExpiredMode === 'modal' &&
      accessStore.isAccessChecked
    ) {
      accessStore.setLoginExpired(true);
    } else {
      await authStore.logout();
    }
  }

  /**
   * 刷新token逻辑
   */
  async function doRefreshToken() {
    const accessStore = useAccessStore();
    let accessToken: string;
    let refreshToken: string;

    try {
      const result = await refreshTokenApi(accessStore.refreshToken);
      accessToken = result.accessToken;
      refreshToken = result.refreshToken;
    } catch (error) {
      throw createLoginExpiredError(error);
    }

    accessStore.setAccessToken(accessToken);
    accessStore.setRefreshToken(refreshToken);

    return accessToken;
  }

  function formatToken(token: null | string) {
    return token ? `Bearer ${token}` : null;
  }

  // 请求头处理
  client.addRequestInterceptor({
    fulfilled: async (config) => {
      const accessStore = useAccessStore();

      config.headers.Authorization = formatToken(accessStore.accessToken);
      config.headers['Accept-Language'] = preferences.app.locale;
      return config;
    },
  });

  // 当前后端使用 HTTP 状态码 + 裸 DTO 返回成功结果。
  client.addResponseInterceptor({
    fulfilled: async (response) => {
      if (response.status >= 200 && response.status < 300) {
        return response.data;
      }

      throw Object.assign({}, response, { response });
    },
  });

  // token过期的处理
  client.addResponseInterceptor(
    authenticateResponseInterceptor({
      client,
      doReAuthenticate,
      doRefreshToken,
      enableRefreshToken: preferences.app.enableRefreshToken,
      formatToken,
    }),
  );

  // 通用的错误处理,如果没有进入上面的错误处理逻辑，就会进入这里
  client.addResponseInterceptor(
    errorMessageResponseInterceptor((msg: string, error) => {
      // 这里可以根据业务进行定制,你可以拿到 error 内的信息进行定制化处理，根据不同的 code 做不同的提示，而不是直接使用 message.error 提示 msg
      // 当前mock接口返回的错误字段是 error 或者 message
      const responseData = error?.response?.data ?? {};
      const errorMessage = getResponseErrorMessage(responseData);
      // 如果没有错误信息，则会根据状态码进行提示
      ElMessage.error(errorMessage || msg);
    }),
  );

  return client;
}

export const requestClient = createRequestClient(apiURL, {
  responseReturn: 'data',
});

export const baseRequestClient = new RequestClient({ baseURL: apiURL });

export const getPage = (url: string, pager: any, params: any) => {
  if (!pager.currentPage) {
    pager.currentPage = 1;
  }
  if (!pager.maxResultCount) {
    pager.maxResultCount = 20;
  }

  pager.skipCount = (pager.currentPage - 1) * pager.maxResultCount;

  return requestClient.get<any>(url, { params: { ...pager, ...params } });
};
