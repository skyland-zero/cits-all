import { requestClient } from '#/api/request';

/**
 * 新增
 */
export async function addApi(data: any) {
  return requestClient.post<any>('/basic/organization-unit', data);
}

/**
 * 编辑
 */
export async function editApi(id: string, data: any) {
  return requestClient.put<any>(`/basic/organization-unit/${id}`, data);
}

/**
 * 删除
 */
export async function deleteApi(id: string) {
  return requestClient.delete<any>(`/basic/organization-unit/${id}`);
}

/**
 *
 * @returns 获取树形结构
 */
export async function getTreeApi() {
  return requestClient.get<any>('/basic/organization-unit/tree');
}
