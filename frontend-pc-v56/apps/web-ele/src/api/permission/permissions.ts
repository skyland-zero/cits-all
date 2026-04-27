import { requestClient } from '#/api/request';

/**
 *
 * @returns 获取树形结构
 */
export async function getTreeSelectApi() {
  return requestClient.get<any>('/basic/permission/tree-select');
}
