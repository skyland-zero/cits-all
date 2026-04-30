<script lang="ts" setup>
import { computed, onMounted, ref, watch } from 'vue';

import { useIntervalFn } from '@vueuse/core';

import { SvgDownloadIcon } from '@vben/icons';

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

const props = withDefaults(defineProps<Props>(), {
  fields: () => [],
  fileName: '',
  query: () => ({}),
});

const visible = ref(false);
const fieldsLoading = ref(false);
const queueLoading = ref(false);
const creating = ref(false);
const fieldOptions = ref<ExportFieldDto[]>([]);
const selectedFields = ref<string[]>([]);
const tasks = ref<ExportTaskDto[]>([]);

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

const hasRunningTask = computed(() =>
  tasks.value.some((task) => task.status === 0 || task.status === 1),
);

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

const open = async () => {
  visible.value = true;
  await Promise.all([loadFields(), loadQueue()]);
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

const { pause, resume } = useIntervalFn(loadQueue, 5000, {
  immediate: false,
});

watch(
  () => visible.value,
  (nextVisible) => {
    if (nextVisible) {
      resume();
    } else {
      pause();
    }
  },
);

watch(hasRunningTask, (running) => {
  if (visible.value && running) {
    resume();
    return;
  }

  if (!running) {
    pause();
  }
});

onMounted(() => {
  if (props.fields.length > 0) {
    fieldOptions.value = props.fields;
    selectedFields.value = props.fields
      .filter((field) => field.selected)
      .map((field) => field.key);
  }
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
    width="760"
  >
    <div class="async-export">
      <section class="async-export__fields">
        <div class="async-export__section-head">
          <span>导出字段</span>
          <span>{{ selectedCount }}/{{ fieldOptions.length }}</span>
        </div>
        <ElScrollbar height="178px" v-loading="fieldsLoading">
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
        <div class="async-export__actions">
          <ElButton :loading="creating" type="primary" @click="createTask">
            创建导出任务
          </ElButton>
          <ElButton :loading="queueLoading" @click="loadQueue">
            刷新队列
          </ElButton>
        </div>
      </section>

      <section class="async-export__queue" v-loading="queueLoading">
        <div class="async-export__section-head">
          <span>{{ moduleName }}导出记录</span>
          <span>最近 20 条</span>
        </div>
        <ElScrollbar height="260px">
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
  </ElDialog>
</template>

<style scoped>
.async-export {
  display: grid;
  grid-template-columns: 250px minmax(0, 1fr);
  gap: 20px;
}

.async-export__fields {
  min-width: 0;
}

.async-export__queue {
  min-width: 0;
}

.async-export__section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
  color: #111827;
  font-size: 14px;
  font-weight: 600;
}

.async-export__section-head span:last-child {
  color: #6b7280;
  font-size: 12px;
  font-weight: 400;
}

.async-export__checks {
  display: grid;
  gap: 8px;
}

.async-export__actions {
  display: flex;
  gap: 8px;
  margin-top: 14px;
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
  }
}
</style>
