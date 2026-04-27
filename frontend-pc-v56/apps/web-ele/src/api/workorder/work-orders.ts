import { getPage, requestClient } from '#/api/request';

/**
 * 分页列表
 * @param pager 分页参数
 * @param params 请求参数
 * @returns 数据
 */
export async function pageApi(pager: any, params: any) {
  return getPage('/workorder/work-order', pager, params);
}

/**
 * 新增
 */
export async function addApi(data: any) {
  return requestClient.post<any>('/workorder/work-order', data);
}

/**
 * 编辑
 */
export async function editApi(id: string, data: any) {
  return requestClient.post<any>(`/workorder/work-order/${id}/update`, data);
}

/**
 * 删除
 */
export async function deleteApi(id: string) {
  return requestClient.post<any>(`/workorder/work-order/${id}/delete`);
}

/**
 * 获取详情
 */
export async function getApi(id: string) {
  return requestClient.get<any>(`/workorder/work-order/${id}`);
}

/**
 * 获取统计数据
 */
export async function statsApi() {
  return requestClient.get<any>('/workorder/work-order/stats');
}
