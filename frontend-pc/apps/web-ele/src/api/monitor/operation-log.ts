import { requestClient } from '#/api/request';

export interface GetOperationLogsInput {
  startTime?: string;
  endTime?: string;
  module?: string;
  operationType?: string;
  operatorId?: string;
  status?: boolean;
  cursor?: string;
  cursorTime?: string;
  cursorId?: string;
  maxResultCount?: number;
}

export interface OperationLogCursorItemDto {
  id: string;
  module?: string;
  operationType?: string;
  operatorId?: string;
  operatorName?: string;
  departmentPath?: string;
  operationIp?: string;
  operationLocation?: string;
  status: boolean;
  operationTime: string;
  elapsedMilliseconds: number;
  requestPath?: string;
  requestMethod?: string;
}

export interface OperationLogCursorResultDto {
  items: OperationLogCursorItemDto[];
  hasMore: boolean;
  nextCursor?: string;
  nextCursorTime?: string;
  nextCursorId?: string;
}

export interface OperationLogDetailDto extends OperationLogCursorItemDto {
  requestParameters?: string;
  responseParameters?: string;
  errorMessage?: string;
}

/**
 * 获取操作日志列表 (游标分页)
 */
export async function getOperationLogListApi(params: GetOperationLogsInput) {
  return requestClient.get<OperationLogCursorResultDto>('/operation-log', {
    params,
  });
}

/**
 * 获取操作日志详情
 */
export async function getOperationLogDetailApi(id: string) {
  return requestClient.get<OperationLogDetailDto>(`/operation-log/${id}`);
}
