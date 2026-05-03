import { getPage, requestClient } from '#/api/request';

export type SystemSettingValueType = 'Boolean' | 'Json' | 'Number' | 'String';

export interface SystemSettingDto {
  id: string;
  key: string;
  name: string;
  value?: null | string;
  valueType: SystemSettingValueType;
  group: string;
  description?: null | string;
  isEncrypted: boolean;
  isReadonly: boolean;
  sort: number;
}

export interface SystemSettingGroupDto {
  value: string;
  label: string;
}

export interface SystemSettingFormData {
  key: string;
  name: string;
  value?: null | string;
  valueType: SystemSettingValueType;
  group: string;
  description?: null | string;
  isEncrypted: boolean;
  isReadonly: boolean;
  sort: number;
}

const Api = {
  Groups: '/system/settings/groups',
  Settings: '/system/settings',
};

export function getSystemSettings(pager: any, params: any) {
  return getPage(Api.Settings, pager, params);
}

export function getSystemSettingGroups() {
  return requestClient.get<SystemSettingGroupDto[]>(Api.Groups);
}

export function createSystemSetting(data: SystemSettingFormData) {
  return requestClient.post(Api.Settings, data);
}

export function updateSystemSetting(id: string, data: SystemSettingFormData) {
  return requestClient.put(`${Api.Settings}/${id}`, data);
}

export function deleteSystemSetting(id: string) {
  return requestClient.delete(`${Api.Settings}/${id}`);
}
