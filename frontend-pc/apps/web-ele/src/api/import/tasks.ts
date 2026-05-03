import { requestClient } from '#/api/request';

export type ImportTaskStatus = 0 | 1 | 2 | 3 | 4;

export interface ImportModuleDto {
  moduleKey: string;
  moduleName: string;
}

export interface ImportTaskDto {
  id: string;
  moduleKey: string;
  moduleName: string;
  originalFileName: string;
  status: ImportTaskStatus;
  totalCount: number;
  successCount: number;
  failedCount: number;
  fileSize: number;
  creationTime: string;
  startedTime?: null | string;
  finishedTime?: null | string;
  errorMessage?: null | string;
  canDownloadErrorReport: boolean;
}

export interface GetImportTasksInput {
  maxResultCount?: number;
  moduleKey?: string;
  skipCount?: number;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
}

export function getImportModulesApi() {
  return requestClient.get<{ items: ImportModuleDto[] }>(
    '/import-tasks/modules',
  );
}

export async function downloadImportTemplateApi(module: ImportModuleDto) {
  const blob = await requestClient.download<Blob>(
    `/import-tasks/template/${module.moduleKey}`,
  );
  downloadBlob(blob, `${module.moduleName}导入模板.xlsx`);
}

export function createImportTaskApi(moduleKey: string, file: File) {
  const formData = new FormData();
  formData.append('file', file);
  return requestClient.post<ImportTaskDto>(
    `/import-tasks/${moduleKey}`,
    formData,
  );
}

export function getImportTaskListApi(params: GetImportTasksInput) {
  return requestClient.get<PagedResultDto<ImportTaskDto>>('/import-tasks', {
    params,
  });
}

export async function downloadImportErrorReportApi(task: ImportTaskDto) {
  const blob = await requestClient.download<Blob>(
    `/import-tasks/${task.id}/error-report`,
  );
  downloadBlob(blob, `${task.moduleName}导入错误报告.xlsx`);
}

function downloadBlob(blob: Blob, fileName: string) {
  const objectUrl = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = objectUrl;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(objectUrl);
}
