import { getPage, requestClient } from '#/api/request';

/**
 * 新增
 */
export async function addApi(data: any) {
  return requestClient.post<any>('/basic/menu', data);
}

/**
 * 新增
 */
export async function multiAddApi(data: any) {
  return requestClient.post<any>('/basic/menu/multi-create', data);
}

/**
 * 编辑
 */
export async function editApi(id: string, data: any) {
  return requestClient.put<any>(`/basic/menu/${id}`, data);
}

/**
 * 删除
 */
export async function deleteApi(id: string) {
  return requestClient.delete<any>(`/basic/menu/${id}`);
}

/**
 *
 * @returns 获取树形结构
 */
export async function getLitePageApi(pager: any, params: any) {
  return getPage('/basic/menu/lite-list', pager, params);
}

/**
 *
 * @returns 获取树形结构
 */
export async function getTreeApi() {
  return requestClient.get<any>('/basic/menu/tree');
}

/**
 *
 * @returns 获取详情
 */
export async function getApi(id: string) {
  return requestClient.get<any>(`/basic/menu/${id}`);
}

/**
 *
 * @returns 获取树形选择数据
 */
export async function getTreeSelectApi() {
  return requestClient.get<any>('/basic/menu/tree-select');
}
