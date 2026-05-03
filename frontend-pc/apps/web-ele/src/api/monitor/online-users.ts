import { getPage, requestClient } from '#/api/request';

export interface OnlineUserSessionDto {
  id: string;
  sessionId: string;
  userId: string;
  userName: string;
  surname: string;
  ip?: null | string;
  userAgent?: null | string;
  loginTime: string;
  lastActiveTime: string;
  expireTime: string;
  isRevoked: boolean;
  revokedTime?: null | string;
  revokedReason?: null | string;
  isOnline: boolean;
}

const Api = {
  OnlineUsers: '/monitor/online-users',
};

export function getOnlineUsers(pager: any, params: any) {
  return getPage(Api.OnlineUsers, pager, params);
}

export function revokeOnlineUserSession(sessionId: string, reason?: string) {
  return requestClient.post(`${Api.OnlineUsers}/${sessionId}/revoke`, {
    reason,
  });
}

export function revokeOnlineUser(userId: string, reason?: string) {
  return requestClient.post(`${Api.OnlineUsers}/users/${userId}/revoke`, {
    reason,
  });
}
