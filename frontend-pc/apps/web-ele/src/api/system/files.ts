import { getPage, requestClient } from '#/api/request';

export interface FileManagementDto {
  id: string;
  originalName: string;
  fileHash: string;
  storageName: string;
  extension: string;
  fileSize: number;
  contentType: string;
  relativePath: string;
  rootIdentifier: string;
  createTime: string;
  isPermanent: boolean;
  accessVersion: number;
  creatorUserName: string;
  previewUrl?: null | string;
  downloadUrl?: null | string;
}

export interface FileCleanupResultDto {
  deletedCount: number;
  failedCount: number;
  failedMessages: string[];
}

export interface FileReplacementRecordDto {
  id: string;
  sourceFileId: string;
  replacementFileId: string;
  sourceOriginalName: string;
  sourceRelativePath: string;
  sourceFileSize: number;
  sourceExtension: string;
  sourcePreviewUrl?: null | string;
  sourceDownloadUrl?: null | string;
  replacementOriginalName: string;
  replacementRelativePath: string;
  replacementFileSize: number;
  replacementExtension: string;
  replacementPreviewUrl?: null | string;
  replacementDownloadUrl?: null | string;
  replacedTime: string;
  replacedByUserId: string;
  replacedByUserName: string;
}

const Api = {
  Files: '/system/files',
};

export function getFiles(pager: any, params: any) {
  return getPage(Api.Files, pager, params);
}

export function deleteFile(id: string) {
  return requestClient.delete(`${Api.Files}/${id}`);
}

export function batchDeleteFiles(ids: string[]) {
  return requestClient.post<FileCleanupResultDto>(`${Api.Files}/batch-delete`, {
    ids,
  });
}

export function cleanupTemporaryFiles(olderThanHours = 24) {
  return requestClient.post<FileCleanupResultDto>(
    `${Api.Files}/cleanup-temporary`,
    { olderThanHours },
  );
}

export function replaceFile(id: string, file: File) {
  const formData = new FormData();
  formData.append('file', file);
  return requestClient.post<FileManagementDto>(
    `${Api.Files}/${id}/replace`,
    formData,
  );
}

export function getFileReplacementRecords(id: string) {
  return requestClient.get<FileReplacementRecordDto[]>(
    `${Api.Files}/${id}/replacement-records`,
  );
}
