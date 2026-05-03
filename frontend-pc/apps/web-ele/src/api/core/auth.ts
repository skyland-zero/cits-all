import { baseRequestClient, requestClient } from '#/api/request';

type RawResponse<T> = {
  data: T;
};

function hasRawResponseData<T>(
  response: RawResponse<T> | T,
): response is RawResponse<T> {
  return (
    typeof response === 'object' && response !== null && 'data' in response
  );
}

export namespace AuthApi {
  /** 登录接口参数 */
  export interface LoginParams {
    password?: string;
    username?: string;
  }

  /** 登录接口返回值 */
  export interface LoginResult {
    accessToken: string;
    mustChangePassword?: boolean;
    refreshToken: string;
  }

  export interface RefreshTokenResult {
    accessToken: string;
    mustChangePassword?: boolean;
    refreshToken: string;
  }

  export interface ChangePasswordParams {
    newPassword: string;
    oldPassword: string;
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
  const response = await baseRequestClient.post<
    AuthApi.RefreshTokenResult | RawResponse<AuthApi.RefreshTokenResult>
  >(
    `/basic/account/refresh-token?refreshToken=${encodeURIComponent(refreshToken ?? '')}`,
  );

  return hasRawResponseData<AuthApi.RefreshTokenResult>(response)
    ? response.data
    : response;
}

/**
 * 退出登录
 */
export async function logoutApi() {
  return requestClient.post('/basic/account/logout');
}

/**
 * 修改当前用户密码
 */
export async function changePasswordApi(data: AuthApi.ChangePasswordParams) {
  return requestClient.post('/basic/account/change-password', data);
}

/**
 * 获取用户权限码
 */
export async function getAccessCodesApi() {
  return requestClient.get<string[]>('/basic/user-permission/permission-codes');
}
