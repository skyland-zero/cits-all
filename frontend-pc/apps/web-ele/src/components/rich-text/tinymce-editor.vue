<script lang="ts" setup>
import { computed, shallowRef } from 'vue';

import Editor from '@tinymce/tinymce-vue';
import 'tinymce/tinymce';
import 'tinymce/models/dom/model';
import 'tinymce/themes/silver';
import 'tinymce/icons/default';
import 'tinymce/plugins/advlist';
import 'tinymce/plugins/autoresize';
import 'tinymce/plugins/code';
import 'tinymce/plugins/fullscreen';
import 'tinymce/plugins/image';
import 'tinymce/plugins/link';
import 'tinymce/plugins/lists';
import 'tinymce/plugins/preview';
import 'tinymce/plugins/table';
import 'tinymce/plugins/wordcount';
import 'tinymce/skins/content/default/content.min.css';
import 'tinymce/skins/ui/oxide/content.min.css';
import 'tinymce/skins/ui/oxide/skin.min.css';
import 'tinymce-i18n/langs8/zh-CN.js';

import { uploadRichTextImage } from './upload';

type TinyMceBlobInfo = {
  blob: () => Blob;
  filename: () => string;
};

type TinyMceEditor = {
  getDoc: () => Document;
  on: (name: string, callback: () => void) => void;
};

const props = withDefaults(
  defineProps<{
    disabled?: boolean;
    height?: number | string;
    modelValue?: string;
    placeholder?: string;
    readonly?: boolean;
  }>(),
  {
    disabled: false,
    height: 460,
    modelValue: '',
    placeholder: '请输入正文内容',
    readonly: false,
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();

const editorRef = shallowRef<TinyMceEditor>();
const uploadedImageMap = new Map<string, string>();

const content = computed({
  get: () => props.modelValue ?? '',
  set: (value) => emit('update:modelValue', value),
});

function rememberUploadedImage(url: string, fileId: string) {
  uploadedImageMap.set(url, fileId);
}

function applyUploadedImageIds(editor = editorRef.value) {
  if (!editor) return;

  editor
    .getDoc()
    .querySelectorAll<HTMLImageElement>('img')
    .forEach((img) => {
      if (img.dataset.fileId) return;

      const src = img.getAttribute('src') ?? '';
      const dataMceSrc = img.getAttribute('data-mce-src') ?? '';
      const fileId =
        uploadedImageMap.get(src) ?? uploadedImageMap.get(dataMceSrc);
      if (fileId) {
        img.dataset.fileId = fileId;
      }
    });
}

async function handleImageUpload(
  blobInfo: TinyMceBlobInfo,
  progress: (percent: number) => void,
) {
  const uploaded = await uploadRichTextImage(
    blobInfo.blob(),
    blobInfo.filename(),
    progress,
  );
  const imageUrl =
    uploaded.url || uploaded.downloadUrl || uploaded.relativePath;

  if (!imageUrl) {
    throw new Error('图片上传成功，但未返回可访问地址');
  }

  rememberUploadedImage(imageUrl, uploaded.id);
  window.setTimeout(() => applyUploadedImageIds(), 0);
  return imageUrl;
}

function handleSetup(editor: TinyMceEditor) {
  editorRef.value = editor;
  editor.on('init SetContent Change NodeChange Undo Redo', () => {
    applyUploadedImageIds(editor);
  });
}

const editorInit = computed(() => ({
  automatic_uploads: true,
  autoresize_bottom_margin: 16,
  branding: false,
  content_style:
    'body { font-family: Inter, "Microsoft YaHei", Arial, sans-serif; font-size: 14px; line-height: 1.7; } img { max-width: 100%; height: auto; } table { border-collapse: collapse; width: 100%; } table td, table th { border: 1px solid #dcdfe6; padding: 6px 8px; }',
  convert_urls: false,
  extended_valid_elements:
    'img[src|alt|title|width|height|class|style|data-file-id|data-mce-src]',
  height: props.height,
  images_upload_handler: handleImageUpload,
  language: 'zh-CN',
  menubar: 'file edit view insert format table tools help',
  paste_data_images: true,
  paste_merge_formats: true,
  paste_webkit_styles:
    'color font-size font-family background-color text-decoration text-align',
  placeholder: props.placeholder,
  plugins:
    'advlist lists link image table code preview fullscreen wordcount autoresize',
  promotion: false,
  readonly: props.readonly || props.disabled,
  setup: handleSetup,
  toolbar:
    'undo redo | blocks fontfamily fontsize | bold italic underline forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image table | removeformat code preview fullscreen',
}));
</script>

<template>
  <Editor v-model="content" license-key="gpl" :init="editorInit" />
</template>

<style scoped>
:deep(.tox-tinymce) {
  border-color: var(--el-border-color);
  border-radius: var(--el-border-radius-base);
}
</style>
