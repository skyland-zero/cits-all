import { baseRequestClient, requestClient } from '#/api/request';

export namespace AuthApi {
  /** 登录接口参数 */
  export interface LoginParams {
    password?: string;
    username?: string;
  }

  /** 登录接口返回值 */
  export interface LoginResult {
    accessToken: string;
    refreshToken: string;
  }

  export interface RefreshTokenResult {
    accessToken: string;
    refreshToken: string;
  }
}

/**
 * 登录
 */
export async function loginApi(data: AuthApi.LoginParams) {
  return requestClient.post<AuthApi.LoginResult>('/basic/account/login', data);
}

/**
 * 刷新accessToken
 */
export async function refreshTokenApi(refreshToken: null | string) {
  return baseRequestClient.post<AuthApi.RefreshTokenResult>(
    `/basic/account/refresh-token?refreshToken=${encodeURIComponent(refreshToken ?? '')}`,
  );
}

/**
 * 退出登录
 */
export async function logoutApi() {
  return baseRequestClient.post('/basic/account/logout', {
    withCredentials: true,
  });
}

/**
 * 获取用户权限码
 */
export async function getAccessCodesApi() {
  return requestClient.get<string[]>('/basic/user-permission/permission-codes');
}
