<script lang="ts" setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';

import {
  CircleX as MdiDelete,
  Eye as MdiPreview,
  RotateCw as MdiReset,
  Search as MdiSearch,
  SvgDownloadIcon as MdiDownload,
} from '@vben/icons';

import {
  ElButton,
  ElDatePicker,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElMessageBox,
  ElOption,
  ElPagination,
  ElSelect,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';

import {
  batchDeleteFiles,
  cleanupTemporaryFiles,
  deleteFile,
  type FileCleanupResultDto,
  type FileManagementDto,
  type FileReplacementRecordDto,
  getFileReplacementRecords,
  getFiles,
  replaceFile,
} from '#/api/system/files';
import { MyContainer } from '#/components';

const initFormSearchData = () => ({
  contentType: '',
  dateRange: [] as string[],
  extension: '',
  isPermanent: '' as '' | boolean,
  keyword: '',
});

const initPager = () => ({
  currentPage: 1,
  maxResultCount: 20,
  totalCount: 0,
});

const tableData = ref<FileManagementDto[]>([]);
const selectedRows = ref<FileManagementDto[]>([]);
const replacementRecords = ref<FileReplacementRecordDto[]>([]);
const replacementRecordsVisible = ref(false);
const replacementRecordsLoading = ref(false);
const currentReplaceRow = ref<FileManagementDto>();
const replaceInputRef = ref<HTMLInputElement>();
const formSearchData = reactive(initFormSearchData());
const pager = reactive(initPager());
const loading = ref(false);
const replaceLoading = ref(false);

const selectedIds = computed(() => selectedRows.value.map((item) => item.id));

const buildQuery = () => ({
  contentType: formSearchData.contentType,
  endTime: formSearchData.dateRange?.[1],
  extension: formSearchData.extension,
  isPermanent: formSearchData.isPermanent,
  keyword: formSearchData.keyword,
  startTime: formSearchData.dateRange?.[0],
});

const search = async () => {
  loading.value = true;
  try {
    const res = await getFiles(pager, buildQuery());
    tableData.value = res.items ?? [];
    pager.totalCount = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
};

const onSearch = () => {
  pager.currentPage = 1;
  search();
};

const onReset = () => {
  Object.assign(pager, initPager());
  Object.assign(formSearchData, initFormSearchData());
  search();
};

const formatDateTime = (value?: null | string) => {
  if (!value) return '-';
  return value.replace('T', ' ').slice(0, 19);
};

const formatFileSize = (size: number) => {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
  if (size < 1024 * 1024 * 1024) {
    return `${(size / 1024 / 1024).toFixed(1)} MB`;
  }
  return `${(size / 1024 / 1024 / 1024).toFixed(1)} GB`;
};

const openUrl = (url?: null | string) => {
  if (!url) {
    ElMessage.warning('访问链接不存在');
    return;
  }
  window.open(url, '_blank');
};

const showCleanupResult = (result: FileCleanupResultDto) => {
  if (result.failedCount > 0) {
    ElMessage.warning(
      `删除成功 ${result.deletedCount} 个，失败 ${result.failedCount} 个`,
    );
    return;
  }

  ElMessage.success(`删除成功 ${result.deletedCount} 个文件`);
};

const onDelete = (row: FileManagementDto) => {
  ElMessageBox.confirm(
    `确定要物理删除文件【${row.originalName}】吗？该操作不可恢复。`,
    '危险操作',
    { type: 'warning' },
  )
    .then(async () => {
      await deleteFile(row.id);
      ElMessage.success('删除成功');
      search();
    })
    .catch(() => {});
};

const onBatchDelete = () => {
  if (selectedIds.value.length === 0) {
    ElMessage.warning('请选择要删除的文件');
    return;
  }

  ElMessageBox.confirm(
    `确定要物理删除选中的 ${selectedIds.value.length} 个文件吗？该操作不可恢复。`,
    '危险操作',
    { type: 'warning' },
  )
    .then(async () => {
      const result = await batchDeleteFiles(selectedIds.value);
      showCleanupResult(result);
      search();
    })
    .catch(() => {});
};

const onCleanupTemporary = () => {
  ElMessageBox.confirm('确定要物理清理 24 小时前未转正的临时文件吗？', '提示', {
    type: 'warning',
  })
    .then(async () => {
      const result = await cleanupTemporaryFiles(24);
      showCleanupResult(result);
      search();
    })
    .catch(() => {});
};

const onReplace = (row: FileManagementDto) => {
  currentReplaceRow.value = row;
  if (replaceInputRef.value) {
    replaceInputRef.value.value = '';
    replaceInputRef.value.click();
  }
};

const onReplaceFileSelected = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  const source = currentReplaceRow.value;
  if (!file || !source) return;

  try {
    await ElMessageBox.confirm(
      `确定要使用【${file.name}】替换【${source.originalName}】吗？源文件会保留并生成替换记录。`,
      '替换文件',
      { type: 'warning' },
    );
  } catch {
    currentReplaceRow.value = undefined;
    return;
  }

  replaceLoading.value = true;
  try {
    await replaceFile(source.id, file);
    ElMessage.success('替换成功，源文件已保留');
    await search();
  } finally {
    replaceLoading.value = false;
    currentReplaceRow.value = undefined;
  }
};

const onViewReplacementRecords = async (row: FileManagementDto) => {
  replacementRecordsVisible.value = true;
  replacementRecordsLoading.value = true;
  try {
    replacementRecords.value = await getFileReplacementRecords(row.id);
  } finally {
    replacementRecordsLoading.value = false;
  }
};

const onSelectionChange = (rows: FileManagementDto[]) => {
  selectedRows.value = rows;
};

watch(
  () => ({
    page: pager.currentPage,
    size: pager.maxResultCount,
  }),
  () => {
    search();
  },
);

onMounted(() => {
  search();
});
</script>

<template>
  <MyContainer :show-footer="true" :show-header="true">
    <input
      ref="replaceInputRef"
      class="hidden"
      type="file"
      @change="onReplaceFileSelected"
    />
    <template #header>
      <ElForm
        :inline="true"
        :model="formSearchData"
        class="demo-form-inline no-action-align ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="文件">
          <ElInput
            v-model="formSearchData.keyword"
            clearable
            placeholder="文件名/路径"
            style="width: 220px"
            @keyup.enter="onSearch"
          />
        </ElFormItem>
        <ElFormItem label="扩展名">
          <ElInput
            v-model="formSearchData.extension"
            clearable
            placeholder="如 .xlsx"
            style="width: 140px"
            @keyup.enter="onSearch"
          />
        </ElFormItem>
        <ElFormItem label="状态">
          <ElSelect
            v-model="formSearchData.isPermanent"
            clearable
            placeholder="全部"
            style="width: 140px"
            @change="onSearch"
          >
            <ElOption :value="true" label="正式文件" />
            <ElOption :value="false" label="临时文件" />
          </ElSelect>
        </ElFormItem>
        <ElFormItem label="上传时间">
          <ElDatePicker
            v-model="formSearchData.dateRange"
            end-placeholder="结束时间"
            range-separator="至"
            start-placeholder="开始时间"
            type="datetimerange"
            value-format="YYYY-MM-DD HH:mm:ss"
          />
        </ElFormItem>
      </ElForm>
    </template>

    <template #table-header>
      <div class="flex w-full items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <ElButton :icon="MdiSearch" type="primary" @click="onSearch">
            查询
          </ElButton>
          <ElButton :icon="MdiReset" @click="onReset">重置</ElButton>
          <div class="mx-1 h-4 border-l border-gray-200"></div>
          <ElButton
            :disabled="selectedIds.length === 0"
            :icon="MdiDelete"
            plain
            type="danger"
            @click="onBatchDelete"
          >
            批量删除
          </ElButton>
          <ElButton
            :icon="MdiReset"
            plain
            type="warning"
            @click="onCleanupTemporary"
          >
            清理临时文件
          </ElButton>
        </div>
        <span class="text-sm text-gray-500">
          已选择 {{ selectedIds.length }} 个文件
        </span>
      </div>
    </template>

    <ElTable
      v-loading="loading"
      :data="tableData"
      class="w-full"
      height="100%"
      row-key="id"
      @selection-change="onSelectionChange"
    >
      <ElTableColumn type="selection" width="45" />
      <ElTableColumn fixed label="文件名" min-width="240" prop="originalName" />
      <ElTableColumn label="扩展名" prop="extension" width="100" />
      <ElTableColumn label="大小" width="110">
        <template #default="{ row }">
          {{ formatFileSize(row.fileSize) }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="状态" width="100">
        <template #default="{ row }">
          <ElTag :type="row.isPermanent ? 'success' : 'warning'">
            {{ row.isPermanent ? '正式' : '临时' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn
        label="Content-Type"
        min-width="160"
        prop="contentType"
        show-overflow-tooltip
      />
      <ElTableColumn
        label="相对路径"
        min-width="260"
        prop="relativePath"
        show-overflow-tooltip
      />
      <ElTableColumn label="上传人" min-width="120" prop="creatorUserName" />
      <ElTableColumn label="上传时间" width="170">
        <template #default="{ row }">
          {{ formatDateTime(row.createTime) }}
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" fixed="right" label="操作" width="330">
        <template #default="{ row }">
          <ElButton
            :icon="MdiPreview"
            size="small"
            text
            type="primary"
            @click="openUrl(row.previewUrl)"
          >
            预览
          </ElButton>
          <ElButton
            :icon="MdiDownload"
            size="small"
            text
            type="primary"
            @click="openUrl(row.downloadUrl)"
          >
            下载
          </ElButton>
          <ElButton
            :icon="MdiDelete"
            size="small"
            text
            type="danger"
            @click="onDelete(row)"
          >
            删除
          </ElButton>
          <ElButton
            :icon="MdiReset"
            :loading="replaceLoading && currentReplaceRow?.id === row.id"
            size="small"
            text
            type="warning"
            @click="onReplace(row)"
          >
            替换
          </ElButton>
          <ElButton
            :icon="MdiPreview"
            size="small"
            text
            type="primary"
            @click="onViewReplacementRecords(row)"
          >
            记录
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <template #table-footer>
      <div class="flex w-full items-center justify-between">
        <span class="text-sm text-gray-500">共 {{ pager.totalCount }} 条</span>
        <ElPagination
          v-model:current-page="pager.currentPage"
          v-model:page-size="pager.maxResultCount"
          :page-sizes="[10, 20, 50, 100]"
          :total="pager.totalCount"
          layout="sizes, prev, pager, next, jumper"
        />
      </div>
    </template>

    <template #dialog>
      <ElDialog
        v-model="replacementRecordsVisible"
        :close-on-click-modal="false"
        title="替换记录"
        width="900"
      >
        <ElTable
          v-loading="replacementRecordsLoading"
          :data="replacementRecords"
          max-height="520"
        >
          <ElTableColumn
            label="源文件"
            min-width="220"
            prop="sourceOriginalName"
          />
          <ElTableColumn
            label="替换文件"
            min-width="220"
            prop="replacementOriginalName"
          />
          <ElTableColumn label="替换人" width="130" prop="replacedByUserName" />
          <ElTableColumn label="替换时间" width="170">
            <template #default="{ row }">
              {{ formatDateTime(row.replacedTime) }}
            </template>
          </ElTableColumn>
          <ElTableColumn align="center" fixed="right" label="操作" width="220">
            <template #default="{ row }">
              <ElButton
                size="small"
                text
                type="primary"
                @click="openUrl(row.sourcePreviewUrl)"
              >
                预览源文件
              </ElButton>
              <ElButton
                size="small"
                text
                type="primary"
                @click="openUrl(row.replacementPreviewUrl)"
              >
                预览替换文件
              </ElButton>
            </template>
          </ElTableColumn>
        </ElTable>
        <template #footer>
          <ElButton @click="replacementRecordsVisible = false">关闭</ElButton>
        </template>
      </ElDialog>
    </template>
  </MyContainer>
</template>
