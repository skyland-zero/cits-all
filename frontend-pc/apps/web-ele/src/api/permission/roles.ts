import { getPage, requestClient } from '#/api/request';

/**
 * 分页列表
 * @param pager 分页参数
 * @param params 请求参数
 * @returns 数据
 */
export async function pageApi(pager: any, params: any) {
  return getPage('/basic/role', pager, params);
}

/**
 * 新增
 */
export async function addApi(data: any) {
  return requestClient.post<any>('/basic/role', data);
}

/**
 * 编辑
 */
export async function editApi(id: string, data: any) {
  return requestClient.put<any>(`/basic/role/${id}`, data);
}

/**
 * 删除
 */
export async function deleteApi(id: string) {
  return requestClient.delete<any>(`/basic/role/${id}`);
}

/**
 * 获取已选择的树形ID
 * @returns 已选择的id列表
 */
export async function getSelectedMenuIdsApi(id: string) {
  return requestClient.get<any>(`/basic/role/${id}/menu-ids`);
}

/**
 * 编辑
 */
export async function setMenusApi(id: string, permissions: any[]) {
  return requestClient.put<any>(`/basic/role/${id}/menus`, { permissions });
}

/**
 * 获取选项数据
 * @returns 已选择的id列表
 */
export async function getSelectedApi() {
  return requestClient.get<any>(`/basic/role/select`);
}
