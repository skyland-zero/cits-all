import { requestClient } from '#/api/request';

/**
 * 新增
 */
export async function addApi(data: any) {
  return requestClient.post<any>('/basic/page', data);
}

/**
 * 编辑
 */
export async function editApi(id: string, data: any) {
  return requestClient.put<any>('/basic/page/' + id, data);
}

/**
 * 删除
 */
export async function deleteApi(id: string) {
  return requestClient.delete<any>('/basic/page/' + id);
}

/**
 * 获取树形结构
 * @returns
 */
export async function getTreeApi() {
  return requestClient.get<any>('/basic/page/tree');
}

/**
 * 获取树形选择数据
 * @returns
 */
export async function getTreeSelectApi() {
  return requestClient.get<any>('/basic/page/tree-select');
}


