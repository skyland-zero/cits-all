import { requestClient } from '#/api/request';

const RICH_TEXT_IMAGE_UPLOAD_ENDPOINT = '/basic/upload/upload/single';

export interface UploadedRichTextImage {
  downloadUrl?: string;
  extension?: string;
  id: string;
  relativePath?: string;
  url?: string;
}

export async function uploadRichTextImage(
  blob: Blob,
  filename: string,
  onProgress?: (percent: number) => void,
) {
  const file =
    blob instanceof File
      ? blob
      : new File([blob], filename || `image-${Date.now()}.png`, {
          type: blob.type || 'image/png',
        });

  return await requestClient.upload<UploadedRichTextImage>(
    RICH_TEXT_IMAGE_UPLOAD_ENDPOINT,
    { file },
    {
      onUploadProgress: (event) => {
        const total = event.total ?? file.size ?? 0;
        if (total <= 0) return;
        onProgress?.(Math.min(100, Math.round((event.loaded / total) * 100)));
      },
    },
  );
}
