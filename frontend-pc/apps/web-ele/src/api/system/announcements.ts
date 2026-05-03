import { getPage, requestClient } from '#/api/request';

export type AnnouncementPriority = 0 | 10 | 20;

export interface AnnouncementDto {
  contentHtml: string;
  creationTime?: string;
  creatorUserName?: string;
  expireTime?: null | string;
  id: string;
  isPublished: boolean;
  isRead?: boolean;
  organizationUnitIds: string[];
  popupOnLogin: boolean;
  priority: AnnouncementPriority;
  publishTime?: null | string;
  roleIds: string[];
  summary?: null | string;
  title: string;
  visibleToAll: boolean;
}

export interface AnnouncementFormData {
  contentHtml: string;
  expireTime?: null | string;
  isPublished: boolean;
  organizationUnitIds: string[];
  popupOnLogin: boolean;
  priority: AnnouncementPriority;
  publishTime?: null | string;
  roleIds: string[];
  summary?: string;
  title: string;
  visibleToAll: boolean;
}

export interface AnnouncementQuery {
  isPublished?: boolean | null;
  keyword?: string;
  priority?: AnnouncementPriority | null;
}

export async function getAnnouncements(pager: any, params: AnnouncementQuery) {
  return getPage('/system/announcements', pager, params);
}

export async function createAnnouncement(data: AnnouncementFormData) {
  return requestClient.post('/system/announcements', data);
}

export async function updateAnnouncement(
  id: string,
  data: AnnouncementFormData,
) {
  return requestClient.post(`/system/announcements/${id}/update`, data);
}

export async function deleteAnnouncement(id: string) {
  return requestClient.post(`/system/announcements/${id}/delete`);
}

export async function publishAnnouncement(id: string, isPublished: boolean) {
  return requestClient.post(`/system/announcements/${id}/publish`, {
    isPublished,
  });
}

export async function getUnreadAnnouncements() {
  return requestClient.get<AnnouncementDto[]>(
    '/system/announcements/current/unread',
  );
}

export async function getLoginPopupAnnouncements() {
  return requestClient.get<AnnouncementDto[]>(
    '/system/announcements/current/login-popups',
  );
}

export async function markAnnouncementRead(id: string) {
  return requestClient.post(`/system/announcements/${id}/read`);
}
