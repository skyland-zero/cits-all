import { requestClient } from '#/api/request';

export interface LoginLogDto {
  id: string;
  ip: string;
  browser: string;
  os: string;
  device: string;
  browserInfo: string;
  status: boolean;
  location: string;
  message: string;
  userId: string;
  userName: string;
  realName: string;
  loginTime: string;
  userAgent: string;
}

export interface GetLoginLogsInput {
  skipCount: number;
  maxResultCount: number;
  userName?: string;
  ip?: string;
  status?: boolean;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

/**
 * 获取登录日志列表
 */
export async function getLoginLogListApi(params: GetLoginLogsInput) {
  return requestClient.get<PagedResultDto<LoginLogDto>>('/basic/login-log', {
    params,
  });
}
