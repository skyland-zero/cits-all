<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { PropType } from 'vue';

import { computed } from 'vue';

import { ElDescriptions, ElDescriptionsItem, ElImage, ElTag } from 'element-plus';

import { WorkOrderStatus, WorkOrderPriority } from './enums';

const props = defineProps({
  currentRow: {
    type: Object as PropType<any>,
    default: () => ({}),
  },
});

const imageExtensions = new Set([
  '.apng',
  '.avif',
  '.bmp',
  '.gif',
  '.ico',
  '.jpeg',
  '.jpg',
  '.png',
  '.svg',
  '.webp',
]);

const getPriorityLabel = (val: number) => {
  switch (val) {
    case WorkOrderPriority.Low: return '低';
    case WorkOrderPriority.Medium: return '中';
    case WorkOrderPriority.High: return '高';
    case WorkOrderPriority.Critical: return '紧急';
    default: return '未知';
  }
};

const getStatusLabel = (val: number) => {
  switch (val) {
    case WorkOrderStatus.Draft: return '草稿';
    case WorkOrderStatus.PendingAssignment: return '待分派';
    case WorkOrderStatus.InProgress: return '处理中';
    case WorkOrderStatus.PendingApproval: return '待审核';
    case WorkOrderStatus.Completed: return '已完成';
    case WorkOrderStatus.Canceled: return '已作废';
    default: return '未知';
  }
};

const getPriorityTagType = (val: number) => {
  switch (val) {
    case WorkOrderPriority.Low: return 'info';
    case WorkOrderPriority.Medium: return undefined;
    case WorkOrderPriority.High: return 'warning';
    case WorkOrderPriority.Critical: return 'danger';
    default: return 'info';
  }
};

const getStatusTagType = (val: number) => {
  switch (val) {
    case WorkOrderStatus.Draft: return 'info';
    case WorkOrderStatus.PendingAssignment: return 'warning';
    case WorkOrderStatus.InProgress: return undefined;
    case WorkOrderStatus.PendingApproval: return 'success';
    case WorkOrderStatus.Completed: return 'success';
    case WorkOrderStatus.Canceled: return 'danger';
    default: return 'info';
  }
};

const formatDate = (date: string | null) => {
  if (!date) return '-';
  return new Date(date).toLocaleString();
};

const formatFileSize = (size?: number) => {
  if (!size || size <= 0) {
    return '-';
  }
  if (size < 1024) {
    return `${size} B`;
  }
  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }
  if (size < 1024 * 1024 * 1024) {
    return `${(size / 1024 / 1024).toFixed(1)} MB`;
  }
  return `${(size / 1024 / 1024 / 1024).toFixed(1)} GB`;
};

const getFileTypeLabel = (extension?: string) => {
  return extension ? extension.replace('.', '').toUpperCase() : '-';
};

const isImageFile = (extension?: string) => {
  return extension ? imageExtensions.has(extension.toLowerCase()) : false;
};

const imageAttachments = computed(() => {
  return (props.currentRow.attachmentList ?? []).filter((file: any) =>
    isImageFile(file.extension),
  );
});

const otherAttachments = computed(() => {
  return (props.currentRow.attachmentList ?? []).filter(
    (file: any) => !isImageFile(file.extension),
  );
});

const imagePreviewUrls = computed(() => {
  return imageAttachments.value
    .map((file: any) => file.url || file.relativePath)
    .filter(Boolean);
});
</script>

<template>
  <div class="p-4">
    <ElDescriptions title="基本信息" :column="2" border>
      <ElDescriptionsItem label="工单编号">{{ props.currentRow.orderNo }}</ElDescriptionsItem>
      <ElDescriptionsItem label="标题">{{ props.currentRow.title }}</ElDescriptionsItem>

      <ElDescriptionsItem label="状态">
        <ElTag :type="getStatusTagType(props.currentRow.currentStatus)" size="small">
          {{ getStatusLabel(props.currentRow.currentStatus) }}
        </ElTag>
      </ElDescriptionsItem>

      <ElDescriptionsItem label="优先级">
        <ElTag :type="getPriorityTagType(props.currentRow.priority)" size="small">
          {{ getPriorityLabel(props.currentRow.priority) }}
        </ElTag>
      </ElDescriptionsItem>

      <ElDescriptionsItem label="联系人">{{ props.currentRow.contactName || '-' }}</ElDescriptionsItem>
      <ElDescriptionsItem label="联系电话">{{ props.currentRow.contactPhone || '-' }}</ElDescriptionsItem>

      <ElDescriptionsItem label="地点" :span="2">{{ props.currentRow.location || '-' }}</ElDescriptionsItem>

      <ElDescriptionsItem label="创建时间">{{ formatDate(props.currentRow.createdTime) }}</ElDescriptionsItem>
      <ElDescriptionsItem label="截止时间">{{ formatDate(props.currentRow.deadlineTime) }}</ElDescriptionsItem>

      <ElDescriptionsItem label="描述" :span="2">
        <div class="whitespace-pre-wrap">{{ props.currentRow.description || '-' }}</div>
      </ElDescriptionsItem>

      <ElDescriptionsItem label="附件" :span="2">
        <div v-if="props.currentRow.attachmentList?.length" class="space-y-2">
          <div v-if="imageAttachments.length" class="grid grid-cols-2 gap-4 md:grid-cols-3">
            <div v-for="file in imageAttachments" :key="file.id" class="rounded border p-2">
              <ElImage :initial-index="0" :preview-src-list="imagePreviewUrls" :src="file.url || file.relativePath"
                fit="cover" preview-teleported style="height: 160px; width: 100%" />
              <div class="mt-2 truncate text-sm text-gray-700">{{ file.originalName }}</div>
              <div class="text-xs text-gray-500">
                {{ getFileTypeLabel(file.extension) }} / {{ formatFileSize(file.fileSize) }}
              </div>
              <a :href="file.downloadUrl || file.url || file.relativePath" class="mt-1 inline-block text-sm text-blue-500 hover:underline"
                rel="noopener noreferrer" target="_blank">
                下载原图
              </a>
            </div>
          </div>

          <div v-for="file in otherAttachments" :key="file.id" class="flex items-center gap-3">
            <a :href="file.url || file.relativePath" class="text-blue-500 hover:underline" rel="noopener noreferrer"
              target="_blank">
              {{ file.originalName }}
            </a>
            <span class="text-xs text-gray-500">
              {{ getFileTypeLabel(file.extension) }} / {{ formatFileSize(file.fileSize) }}
            </span>
            <a :href="file.downloadUrl || file.url || file.relativePath"
              class="text-gray-500 hover:underline" rel="noopener noreferrer" target="_blank">
              下载
            </a>
          </div>
        </div>
        <span v-else>-</span>
      </ElDescriptionsItem>
    </ElDescriptions>
  </div>
</template>
