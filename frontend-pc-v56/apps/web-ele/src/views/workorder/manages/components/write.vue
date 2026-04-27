<script lang="ts" setup>
import type { Nullable } from '@vben/types';
import type { HubConnection } from '@microsoft/signalr';

import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { onMounted, onUnmounted, type PropType, reactive, ref, watch } from 'vue';

import {
  ElDatePicker,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElOption,
  ElSelect,
  ElUpload,
  type FormInstance,
  type FormRules,
  type UploadFile,
  type UploadProps,
  type UploadUserFile,
} from 'element-plus';
import { UploadAjaxError } from 'element-plus/es/components/upload/src/ajax';

import { requestClient } from '#/api/request';

import { WorkOrderPriority } from '../enums';

type UploadRequestOptions = Parameters<
  NonNullable<UploadProps['httpRequest']>
>[0];
type UploadProgressEvent = Parameters<UploadRequestOptions['onProgress']>[0];

type UploadFileItem = UploadUserFile & {
  downloadUrl?: string;
  fileId?: string;
  isPersisted?: boolean;
  uid?: number;
};

type UploadedAttachment = {
  downloadUrl?: string;
  id: string;
  relativePath?: string;
  url?: string;
};

type UploadSettings = {
  hubUrl: string;
  maxConcurrentUploads: number;
  signalREnabled: boolean;
};

type UploadProgressMessage = {
  percent: number;
  uploadUid: string;
};

type UploadTask = {
  options: UploadRequestOptions;
  reject: (reason?: unknown) => void;
  resolve: (value: unknown) => void;
  uid: string;
};

type UploadRuntimeState = {
  controller?: AbortController;
  localPercent: number;
  options: UploadRequestOptions;
  serverPercent: number;
  uid: string;
  usingServerProgress: boolean;
};

const props = defineProps({
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});

const DEFAULT_UPLOAD_SETTINGS: UploadSettings = {
  hubUrl: '/hub/upload',
  maxConcurrentUploads: 3,
  signalREnabled: true,
};
const UPLOAD_ENDPOINT = '/basic/upload/upload/single';

const formRef = ref<FormInstance>();
const formLabelWidth = '120px';
const loading = ref(false);
const fileList = ref<UploadFileItem[]>([]);
const uploadSettings = ref<UploadSettings>(DEFAULT_UPLOAD_SETTINGS);
const hubConnection = ref<HubConnection>();
const connectionId = ref<string>();
const signalRReady = ref(false);

let activeUploadCount = 0;
let destroyed = false;
let initializePromise: null | Promise<void> = null;
const pendingUploads: UploadTask[] = [];
const uploadStateMap = new Map<string, UploadRuntimeState>();

const initData = () => ({
  id: '',
  title: '',
  priority: WorkOrderPriority.Medium,
  description: '',
  location: '',
  contactName: '',
  contactPhone: '',
  deadlineTime: '',
  attachments: '[]',
});
const formData = reactive(initData());

const priorityOptions = [
  { label: '低', value: WorkOrderPriority.Low },
  { label: '中', value: WorkOrderPriority.Medium },
  { label: '高', value: WorkOrderPriority.High },
  { label: '紧急', value: WorkOrderPriority.Critical },
];

const rules = reactive<FormRules>({
  title: [
    { required: true, message: '请输入工单标题', trigger: 'blur' },
    { min: 1, max: 200, message: '长度在 1 到 200 个字符', trigger: 'blur' },
  ],
  priority: [{ required: true, message: '请选择优先级', trigger: 'change' }],
  contactName: [{ max: 50, message: '长度不能超过 50 个字符', trigger: 'blur' }],
  contactPhone: [{ max: 20, message: '长度不能超过 20 个字符', trigger: 'blur' }],
  location: [{ max: 200, message: '长度不能超过 200 个字符', trigger: 'blur' }],
});

const getFileUid = (file: Pick<UploadFile, 'uid'> | UploadRequestOptions['file']) => {
  return String(file.uid);
};

const findFileItem = (uid: string) => {
  return fileList.value.find((item) => item.uid !== undefined && String(item.uid) === uid);
};

const buildUploadError = (message: string) => {
  return new UploadAjaxError(message, 500, 'POST', UPLOAD_ENDPOINT);
};

const emitProgress = (state: UploadRuntimeState, percent: number) => {
  state.options.onProgress({ percent } as UploadProgressEvent);
};

const updateProgress = (uid: string, percent: number, source: 'local' | 'server') => {
  const state = uploadStateMap.get(uid);
  const file = findFileItem(uid);
  if (!state || !file) {
    return;
  }

  let displayPercent = percent;
  if (source === 'server') {
    state.serverPercent = percent;
    state.usingServerProgress = true;
  } else {
    state.localPercent = percent;
    if (state.usingServerProgress) {
      return;
    }
    const fallbackLimit = signalRReady.value ? 15 : 95;
    displayPercent = Math.min(percent, fallbackLimit);
  }

  file.status = 'uploading';
  file.percentage = displayPercent;
  emitProgress(state, displayPercent);
};

const applyUploadSuccess = (uid: string, response: UploadedAttachment) => {
  const file = findFileItem(uid);
  if (!file) {
    return;
  }

  file.status = 'success';
  file.percentage = 100;
  file.url = response.url || response.relativePath;
  file.downloadUrl = response.downloadUrl;
  file.fileId = response.id;
  file.isPersisted = false;
};

const markUploadFailed = (uid: string) => {
  const file = findFileItem(uid);
  if (!file) {
    return;
  }

  file.status = 'fail';
};

const fetchUploadSettings = async () => {
  try {
    uploadSettings.value = await requestClient.get<UploadSettings>('/basic/upload/upload/settings');
  } catch (error) {
    console.error('Failed to fetch upload settings', error);
    uploadSettings.value = DEFAULT_UPLOAD_SETTINGS;
    ElMessage.warning('上传配置读取失败，已使用默认配置');
  }
};

const createHubConnection = () => {
  return new HubConnectionBuilder()
    .withUrl(uploadSettings.value.hubUrl)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Error)
    .build();
};

const registerHubEvents = (connection: HubConnection) => {
  connection.on('UploadProgress', (message: UploadProgressMessage) => {
    updateProgress(message.uploadUid, message.percent, 'server');
  });

  connection.onreconnecting(() => {
    signalRReady.value = false;
    connectionId.value = undefined;
  });

  connection.onreconnected(async () => {
    signalRReady.value = true;
    connectionId.value = await connection.invoke<string>('GetConnectionId');
  });

  connection.onclose(() => {
    signalRReady.value = false;
    connectionId.value = undefined;
    if (hubConnection.value === connection) {
      hubConnection.value = undefined;
    }
  });
};

const ensureSignalRConnection = async () => {
  if (!uploadSettings.value.signalREnabled) {
    signalRReady.value = false;
    return;
  }

  if (hubConnection.value && signalRReady.value && connectionId.value) {
    return;
  }

  try {
    const connection = createHubConnection();
    registerHubEvents(connection);
    await connection.start();
    connectionId.value = await connection.invoke<string>('GetConnectionId');
    signalRReady.value = true;
    hubConnection.value = connection;
  } catch (error) {
    console.error('Failed to initialize upload signalR', error);
    signalRReady.value = false;
    connectionId.value = undefined;
    ElMessage.warning('实时上传进度不可用，已切换为本地进度');
  }
};

const initializeUploadContext = async () => {
  if (initializePromise) {
    return initializePromise;
  }

  initializePromise = (async () => {
    loading.value = true;
    try {
      await fetchUploadSettings();
      await ensureSignalRConnection();
    } finally {
      loading.value = false;
    }
  })();

  return initializePromise;
};

const finalizeUpload = (uid: string) => {
  uploadStateMap.delete(uid);
  activeUploadCount = Math.max(0, activeUploadCount - 1);
  scheduleUploads();
};

const runUploadTask = async (task: UploadTask) => {
  const state = uploadStateMap.get(task.uid);
  if (!state) {
    task.reject(new Error('upload task not found'));
    return;
  }

  activeUploadCount += 1;
  try {
    await initializeUploadContext();

    const controller = new AbortController();
    state.controller = controller;
    const headers: Record<string, string> = {
      'X-Upload-Uid': task.uid,
    };
    if (connectionId.value) {
      headers['X-Connection-Id'] = connectionId.value;
    }

    const response = await requestClient.upload<UploadedAttachment>(
      UPLOAD_ENDPOINT,
      { file: task.options.file },
      {
        headers,
        onUploadProgress: (event) => {
          const total = event.total ?? task.options.file.size ?? 0;
          const percent = total > 0 ? Math.round((event.loaded / total) * 100) : 0;
          updateProgress(task.uid, percent, 'local');
        },
        signal: controller.signal,
      },
    );

    applyUploadSuccess(task.uid, response);
    task.options.onSuccess(response);
    task.resolve(response);
  } catch (error) {
    const canceled = state.controller?.signal.aborted ?? false;
    if (!canceled) {
      console.error('Upload failed', error);
      markUploadFailed(task.uid);
      const uploadError = buildUploadError(
        error instanceof Error ? error.message : '上传失败',
      );
      task.options.onError(uploadError);
      task.reject(uploadError);
    } else {
      task.reject(new Error('upload canceled'));
    }
  } finally {
    finalizeUpload(task.uid);
  }
};

const scheduleUploads = () => {
  while (
    activeUploadCount < uploadSettings.value.maxConcurrentUploads &&
    pendingUploads.length > 0
  ) {
    const task = pendingUploads.shift();
    if (!task) {
      return;
    }
    void runUploadTask(task);
  }
};

const handleUpload = (options: UploadRequestOptions) => {
  return new Promise((resolve, reject) => {
    if (destroyed) {
      reject(buildUploadError('上传组件已销毁'));
      return;
    }

    const uid = getFileUid(options.file);
    uploadStateMap.set(uid, {
      localPercent: 0,
      options,
      serverPercent: 0,
      uid,
      usingServerProgress: false,
    });
    pendingUploads.push({ options, reject, resolve, uid });
    scheduleUploads();
  });
};

const removePendingUpload = (uid: string) => {
  const index = pendingUploads.findIndex((task) => task.uid === uid);
  if (index >= 0) {
    pendingUploads[index]?.reject(new Error('upload canceled'));
    pendingUploads.splice(index, 1);
    uploadStateMap.delete(uid);
  }
};

const deleteTemporaryAttachment = async (fileId: string) => {
  try {
    await requestClient.delete(`/basic/upload/upload/${fileId}`);
  } catch (error) {
    console.error('Failed to delete temporary attachment', error);
    ElMessage.warning('附件临时文件删除失败，将由后台自动清理');
  }
};

const handleRemove: UploadProps['onRemove'] = (uploadFile) => {
  const uid = getFileUid(uploadFile);
  const state = uploadStateMap.get(uid);
  if (state?.controller) {
    state.controller.abort();
    uploadStateMap.delete(uid);
  } else {
    removePendingUpload(uid);
  }

  const file = uploadFile as UploadFileItem;
  if (file.fileId && !file.isPersisted) {
    void deleteTemporaryAttachment(file.fileId);
  }
};

const onUploadSuccess: UploadProps['onSuccess'] = (response, uploadFile) => {
  const attachment = response as UploadedAttachment;
  applyUploadSuccess(getFileUid(uploadFile), attachment);
};

const setValues = (currentRow: any) => {
  Object.assign(formData, currentRow);
  if (currentRow.attachmentList && Array.isArray(currentRow.attachmentList)) {
    fileList.value = currentRow.attachmentList.map((item: any, index: number) => ({
      downloadUrl: item.downloadUrl,
      fileId: item.id,
      isPersisted: true,
      name: item.originalName,
      status: 'success',
      uid: -(index + 1),
      url: item.url || item.relativePath,
    }));
  } else {
    fileList.value = [];
  }
};

const submit = async () => {
  const formEl = formRef.value;
  if (!formEl) {
    return;
  }

  const valid = await formEl.validate().catch((error) => {
    console.log('valid', error);
  });
  if (!valid) {
    return;
  }

  const attachmentsData = fileList.value
    .filter((file) => file.status === 'success' && file.fileId)
    .map((file) => ({
      id: file.fileId,
      name: file.name,
    }));
  formData.attachments = JSON.stringify(attachmentsData);
  return formData;
};

watch(
  () => props.currentRow,
  (currentRow) => {
    if (!currentRow) {
      Object.assign(formData, initData());
      fileList.value = [];
      return;
    }
    setValues(currentRow);
  },
  {
    deep: true,
    immediate: true,
  },
);

onMounted(() => {
  void initializeUploadContext();
});

onUnmounted(() => {
  destroyed = true;
  pendingUploads.length = 0;
  for (const state of uploadStateMap.values()) {
    state.controller?.abort();
  }
  uploadStateMap.clear();
  if (hubConnection.value) {
    void hubConnection.value.stop();
  }
});

defineExpose({
  submit,
});
</script>

<template>
  <ElForm ref="formRef" :model="formData" :rules="rules" v-loading="loading">
    <ElFormItem :label-width="formLabelWidth" label="标题：" prop="title">
      <ElInput v-model="formData.title" autocomplete="off" placeholder="请输入工单标题" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="优先级：" prop="priority">
      <ElSelect v-model="formData.priority" placeholder="请选择优先级" style="width: 100%">
        <ElOption v-for="item in priorityOptions" :key="item.value" :label="item.label" :value="item.value" />
      </ElSelect>
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="地点：" prop="location">
      <ElInput v-model="formData.location" autocomplete="off" placeholder="故障/作业地点" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="联系人：" prop="contactName">
      <ElInput v-model="formData.contactName" autocomplete="off" placeholder="联系人姓名" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="联系电话：" prop="contactPhone">
      <ElInput v-model="formData.contactPhone" autocomplete="off" placeholder="联系人电话" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="截止时间：" prop="deadlineTime">
      <ElDatePicker v-model="formData.deadlineTime" type="datetime" placeholder="选择处理截止时间" style="width: 100%"
        value-format="YYYY-MM-DD HH:mm:ss" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="描述：" prop="description">
      <ElInput v-model="formData.description" type="textarea" :rows="4" placeholder="请输入工单描述" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="附件：">
      <ElUpload v-model:file-list="fileList" :http-request="handleUpload" :on-remove="handleRemove"
        :on-success="onUploadSuccess" multiple list-type="text">
        <template #trigger>
          <el-button type="primary">点击上传</el-button>
        </template>
      </ElUpload>
    </ElFormItem>
  </ElForm>
</template>
