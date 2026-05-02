import { getPage, requestClient } from '#/api/request';

// API Constants
const Api = {
  Types: '/system/dict/types',
  Items: '/system/dict/items',
  ItemsByCode: '/system/dict/items/code',
};

// --- Dict Types ---

export function getDictTypes(pager: any, params: any) {
  return getPage(Api.Types, pager, params);
}

export function createDictType(data: any) {
  return requestClient.post(Api.Types, data);
}

export function updateDictType(id: string, data: any) {
  return requestClient.put(`${Api.Types}/${id}`, data);
}

export function deleteDictType(id: string) {
  return requestClient.delete(`${Api.Types}/${id}`);
}

// --- Dict Items ---

export function getDictItems(pager: any, params: any) {
  return getPage(Api.Items, pager, params);
}

export function createDictItem(data: any) {
  return requestClient.post(Api.Items, data);
}

export function updateDictItem(id: string, data: any) {
  return requestClient.put(`${Api.Items}/${id}`, data);
}

export function deleteDictItem(id: string) {
  return requestClient.delete(`${Api.Items}/${id}`);
}

export function getDictItemsByCode(code: string) {
  return requestClient.get(`${Api.ItemsByCode}/${code}`);
}
