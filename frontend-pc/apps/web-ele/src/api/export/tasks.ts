import { requestClient } from '#/api/request';

export interface ExportFieldDto {
  key: string;
  label: string;
  selected: boolean;
}

export type ExportTaskStatus = 0 | 1 | 2 | 3;

export interface ExportTaskDto {
  id: string;
  moduleKey: string;
  moduleName: string;
  fileName: string;
  status: ExportTaskStatus;
  totalCount: number;
  fileSize: number;
  creationTime: string;
  startedTime?: null | string;
  finishedTime?: null | string;
  errorMessage?: null | string;
  canDownload: boolean;
}

export interface CreateExportTaskInput {
  fields: string[];
  fileName?: string;
  moduleKey: string;
  query: Record<string, unknown>;
}

export interface GetExportTasksInput {
  maxResultCount?: number;
  moduleKey?: string;
  skipCount?: number;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
}

export async function getExportFieldsApi(moduleKey: string) {
  return requestClient.get<{ items: ExportFieldDto[] }>(
    `/export-tasks/fields/${moduleKey}`,
  );
}

export async function createExportTaskApi(data: CreateExportTaskInput) {
  return requestClient.post<ExportTaskDto>('/export-tasks', data);
}

export async function getExportTaskListApi(params: GetExportTasksInput) {
  return requestClient.get<PagedResultDto<ExportTaskDto>>('/export-tasks', {
    params,
  });
}

export async function downloadExportTaskApi(task: ExportTaskDto) {
  const blob = await requestClient.download<Blob>(
    `/export-tasks/${task.id}/download`,
  );
  const objectUrl = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = objectUrl;
  link.download = task.fileName;
  link.click();
  URL.revokeObjectURL(objectUrl);
}
