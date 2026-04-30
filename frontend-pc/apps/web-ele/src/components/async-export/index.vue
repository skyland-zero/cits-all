<script lang="ts" setup>
import type { HubConnection } from '@microsoft/signalr';

import { computed, onMounted, onUnmounted, ref, watch } from 'vue';

import { useDebounceFn, useIntervalFn } from '@vueuse/core';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { SvgDownloadIcon } from '@vben/icons';
import { useAccessStore } from '@vben/stores';

import {
  ElButton,
  ElCheckbox,
  ElCheckboxGroup,
  ElDialog,
  ElEmpty,
  ElMessage,
  ElScrollbar,
  ElTag,
} from 'element-plus';

import {
  createExportTaskApi,
  downloadExportTaskApi,
  getExportFieldsApi,
  getExportTaskListApi,
} from '#/api/export/tasks';

import type {
  ExportFieldDto,
  ExportTaskDto,
  ExportTaskStatus,
} from '#/api/export/tasks';

interface Props {
  fields?: ExportFieldDto[];
  fileName?: string;
  moduleKey: string;
  moduleName: string;
  query?: Record<string, unknown>;
}

interface ExportTaskChangedMessage {
  changedAt: string;
  errorMessage?: null | string;
  moduleKey: string;
  status: ExportTaskStatus;
  taskId: string;
}

const props = withDefaults(defineProps<Props>(), {
  fields: () => [],
  fileName: '',
  query: () => ({}),
});

const accessStore = useAccessStore();
const visible = ref(false);
const fieldsLoading = ref(false);
const queueLoading = ref(false);
const creating = ref(false);
const fieldOptions = ref<ExportFieldDto[]>([]);
const selectedFields = ref<string[]>([]);
const tasks = ref<ExportTaskDto[]>([]);
const hubConnection = ref<HubConnection>();
const signalRReady = ref(false);
let signalRConnecting: null | Promise<void> = null;

const statusMeta: Record<
  ExportTaskStatus,
  { label: string; type: 'danger' | 'info' | 'primary' | 'success' | 'warning' }
> = {
  0: { label: '排队中', type: 'info' },
  1: { label: '生成中', type: 'warning' },
  2: { label: '可下载', type: 'success' },
  3: { label: '失败', type: 'danger' },
};

const selectedCount = computed(() => selectedFields.value.length);

const formatFileSize = (size: number) => {
  if (!size) {
    return '-';
  }

  if (size < 1024) {
    return `${size} B`;
  }

  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }

  return `${(size / 1024 / 1024).toFixed(1)} MB`;
};

const formatTime = (value?: null | string) => {
  if (!value) {
    return '-';
  }

  return value.replace('T', ' ').slice(0, 19);
};

const selectAllFields = () => {
  selectedFields.value = fieldOptions.value.map((field) => field.key);
};

const invertSelectedFields = () => {
  const selectedFieldSet = new Set(selectedFields.value);

  selectedFields.value = fieldOptions.value
    .filter((field) => !selectedFieldSet.has(field.key))
    .map((field) => field.key);
};

const loadFields = async () => {
  fieldsLoading.value = true;
  try {
    if (props.fields.length > 0) {
      fieldOptions.value = props.fields;
    } else {
      const res = await getExportFieldsApi(props.moduleKey);
      fieldOptions.value = res.items;
    }

    selectedFields.value = fieldOptions.value
      .filter((field) => field.selected)
      .map((field) => field.key);
  } finally {
    fieldsLoading.value = false;
  }
};

const loadQueue = async () => {
  queueLoading.value = true;
  try {
    const res = await getExportTaskListApi({
      maxResultCount: 20,
      moduleKey: props.moduleKey,
      skipCount: 0,
    });
    tasks.value = res.items;
  } finally {
    queueLoading.value = false;
  }
};

const scheduleLoadQueue = useDebounceFn(() => {
  if (visible.value) {
    void loadQueue();
  }
}, 300);

const { pause, resume } = useIntervalFn(loadQueue, 5000, {
  immediate: false,
});

const syncPollingState = () => {
  if (!visible.value || signalRReady.value) {
    pause();
    return;
  }

  resume();
};

const createHubConnection = () => {
  return new HubConnectionBuilder()
    .withUrl(`/hub/export-tasks?moduleKey=${encodeURIComponent(props.moduleKey)}`, {
      accessTokenFactory: () => accessStore.accessToken ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Error)
    .build();
};

const registerHubEvents = (connection: HubConnection) => {
  connection.on('ExportTaskChanged', (message: ExportTaskChangedMessage) => {
    if (message.moduleKey.toLowerCase() !== props.moduleKey.toLowerCase()) {
      return;
    }

    scheduleLoadQueue();
  });

  connection.onreconnecting(() => {
    if (hubConnection.value !== connection) {
      return;
    }

    signalRReady.value = false;
    syncPollingState();
  });

  connection.onreconnected(() => {
    if (hubConnection.value !== connection) {
      return;
    }

    signalRReady.value = true;
    pause();
    void loadQueue();
  });

  connection.onclose(() => {
    if (hubConnection.value !== connection) {
      return;
    }

    signalRReady.value = false;
    hubConnection.value = undefined;
    signalRConnecting = null;
    syncPollingState();
  });
};

const ensureSignalRConnection = async () => {
  if (hubConnection.value && signalRReady.value) {
    return;
  }

  if (signalRConnecting) {
    return signalRConnecting;
  }

  signalRConnecting = (async () => {
    const connection = createHubConnection();
    registerHubEvents(connection);
    hubConnection.value = connection;

    try {
      await connection.start();
      signalRReady.value = true;
      pause();
      await loadQueue();
    } catch (error) {
      console.error('Failed to initialize export task signalR', error);
      signalRReady.value = false;
      hubConnection.value = undefined;
      await connection.stop().catch(() => undefined);
      syncPollingState();
    } finally {
      signalRConnecting = null;
    }
  })();

  return signalRConnecting;
};

const stopSignalRConnection = async () => {
  const connection = hubConnection.value;
  hubConnection.value = undefined;
  signalRReady.value = false;
  signalRConnecting = null;

  if (!connection) {
    return;
  }

  connection.off('ExportTaskChanged');
  await connection.stop().catch((error) => {
    console.error('Failed to stop export task signalR', error);
  });
};

const open = async () => {
  visible.value = true;
  await Promise.all([loadFields(), loadQueue()]);
  await ensureSignalRConnection();
  syncPollingState();
};

const createTask = async () => {
  if (selectedFields.value.length === 0) {
    ElMessage.warning('请至少选择一个导出字段');
    return;
  }

  creating.value = true;
  try {
    await createExportTaskApi({
      fields: selectedFields.value,
      fileName: props.fileName || `${props.moduleName}列表`,
      moduleKey: props.moduleKey,
      query: props.query,
    });
    ElMessage.success('导出任务已创建');
    await loadQueue();
  } finally {
    creating.value = false;
  }
};

const downloadTask = async (task: ExportTaskDto) => {
  try {
    await downloadExportTaskApi(task);
  } catch (error) {
    ElMessage.error(error instanceof Error ? error.message : '下载失败');
  }
};

watch(
  () => visible.value,
  (nextVisible) => {
    if (!nextVisible) {
      pause();
      void stopSignalRConnection();
    }
  },
);

onMounted(() => {
  if (props.fields.length > 0) {
    fieldOptions.value = props.fields;
    selectedFields.value = props.fields
      .filter((field) => field.selected)
      .map((field) => field.key);
  }
});

onUnmounted(() => {
  pause();
  void stopSignalRConnection();
});
</script>

<template>
  <ElButton :icon="SvgDownloadIcon" plain @click="open">
    <slot>导出</slot>
  </ElButton>

  <ElDialog
    v-model="visible"
    append-to-body
    destroy-on-close
    draggable
    title="导出队列"
    width="860"
  >
    <div class="async-export">
      <section class="async-export__fields">
        <div class="async-export__section-head">
          <div class="async-export__section-title">
            <span>导出字段</span>
            <span class="async-export__section-extra">
              {{ selectedCount }}/{{ fieldOptions.length }}
            </span>
          </div>
          <div class="async-export__field-actions">
            <ElButton
              :disabled="fieldOptions.length === 0"
              plain
              size="small"
              @click="selectAllFields"
            >
              全选
            </ElButton>
            <ElButton
              :disabled="fieldOptions.length === 0"
              plain
              size="small"
              @click="invertSelectedFields"
            >
              反选
            </ElButton>
          </div>
        </div>
        <ElScrollbar
          class="async-export__scrollbar"
          height="100%"
          v-loading="fieldsLoading"
        >
          <ElCheckboxGroup v-model="selectedFields" class="async-export__checks">
            <ElCheckbox
              v-for="field in fieldOptions"
              :key="field.key"
              :label="field.key"
            >
              {{ field.label }}
            </ElCheckbox>
          </ElCheckboxGroup>
        </ElScrollbar>
      </section>

      <section class="async-export__queue" v-loading="queueLoading">
        <div class="async-export__section-head">
          <span>导出记录</span>
          <span class="async-export__section-extra">最近 20 条</span>
        </div>
        <ElScrollbar class="async-export__scrollbar" height="100%">
          <ElEmpty v-if="tasks.length === 0" description="暂无导出任务" />
          <div v-else class="async-export__list">
            <div
              v-for="task in tasks"
              :key="task.id"
              class="async-export__item"
            >
              <div class="async-export__item-main">
                <div class="async-export__file">
                  {{ task.fileName }}
                </div>
                <div class="async-export__meta">
                  {{ formatTime(task.creationTime) }}
                  <span>数据 {{ task.totalCount }} 条</span>
                  <span>{{ formatFileSize(task.fileSize) }}</span>
                </div>
                <div v-if="task.errorMessage" class="async-export__error">
                  {{ task.errorMessage }}
                </div>
              </div>
              <div class="async-export__item-actions">
                <ElTag :type="statusMeta[task.status].type" effect="light">
                  {{ statusMeta[task.status].label }}
                </ElTag>
                <ElButton
                  :disabled="!task.canDownload"
                  link
                  type="primary"
                  @click="downloadTask(task)"
                >
                  下载
                </ElButton>
              </div>
            </div>
          </div>
        </ElScrollbar>
      </section>
    </div>

    <template #footer>
      <div class="dialog-footer async-export__footer">
        <ElButton :loading="creating" type="primary" @click="createTask">
          创建导出任务
        </ElButton>
        <ElButton :loading="queueLoading" @click="loadQueue">
          刷新队列
        </ElButton>
      </div>
    </template>
  </ElDialog>
</template>

<style scoped>
.async-export {
  display: grid;
  grid-template-columns: 320px minmax(0, 1fr);
  gap: 20px;
  height: min(560px, calc(100vh - 180px));
  overflow: hidden;
}

.async-export__fields,
.async-export__queue {
  display: flex;
  min-width: 0;
  min-height: 0;
  flex-direction: column;
}

.async-export__section-head {
  display: flex;
  flex: 0 0 auto;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
  color: #111827;
  font-size: 14px;
  font-weight: 600;
}

.async-export__section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
}

.async-export__section-extra {
  color: #6b7280;
  font-size: 12px;
  font-weight: 400;
}

.async-export__field-actions {
  display: flex;
  flex: 0 0 auto;
  gap: 6px;
}

.async-export__field-actions :deep(.el-button + .el-button) {
  margin-left: 0;
}

.async-export__scrollbar {
  flex: 1 1 auto;
  min-height: 0;
}

.async-export__checks {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px 12px;
}

.async-export__checks :deep(.el-checkbox) {
  min-width: 0;
  margin-right: 0;
}

.async-export__checks :deep(.el-checkbox__label) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.async-export__footer {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: flex-end;
  background: var(--el-bg-color);
}

.async-export__list {
  display: grid;
  gap: 10px;
}

.async-export__item {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 14px;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #edf0f3;
}

.async-export__file {
  overflow: hidden;
  color: #111827;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.async-export__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 5px;
  color: #6b7280;
  font-size: 12px;
}

.async-export__error {
  margin-top: 6px;
  color: #dc2626;
  font-size: 12px;
}

.async-export__item-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

@media (max-width: 768px) {
  .async-export {
    grid-template-columns: 1fr;
    height: min(640px, calc(100vh - 160px));
  }

  .async-export__checks {
    grid-template-columns: 1fr;
  }
}
</style>
