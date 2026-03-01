import { getPage, requestClient } from '#/api/request';

/**
 * 分页列表
 * @param pager 分页参数
 * @param params 请求参数
 * @returns 数据
 */
export async function pageApi(pager: any, params: any) {
  return getPage('/basic/user', pager, params);
}

/**
 * 新增
 */
export async function addApi(data: any) {
  return requestClient.post<any>('/basic/user', data);
}

/**
 * 编辑
 */
export async function editApi(id: string, data: any) {
  return requestClient.put<any>(`/basic/user/${id}`, data);
}

/**
 * 删除
 */
export async function deleteApi(id: string) {
  return requestClient.delete<any>(`/basic/user/${id}`);
}
