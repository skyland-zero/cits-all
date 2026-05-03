<script lang="ts" setup>
import { computed, onMounted, reactive, ref, watch } from 'vue';

import {
  RotateCw as MdiReset,
  Search as MdiSearch,
  SvgDownloadIcon as MdiDownload,
} from '@vben/icons';

import {
  ElButton,
  ElForm,
  ElFormItem,
  ElMessage,
  ElOption,
  ElPagination,
  ElSelect,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';

import {
  createImportTaskApi,
  downloadImportErrorReportApi,
  downloadImportTemplateApi,
  getImportModulesApi,
  getImportTaskListApi,
  type ImportModuleDto,
  type ImportTaskDto,
  type ImportTaskStatus,
} from '#/api/import/tasks';
import { MyContainer } from '#/components';

const statusMeta: Record<
  ImportTaskStatus,
  { label: string; type: 'danger' | 'info' | 'primary' | 'success' | 'warning' }
> = {
  0: { label: '排队中', type: 'info' },
  1: { label: '导入中', type: 'warning' },
  2: { label: '成功', type: 'success' },
  3: { label: '失败', type: 'danger' },
  4: { label: '部分成功', type: 'warning' },
};

const initPager = () => ({
  currentPage: 1,
  maxResultCount: 20,
  totalCount: 0,
});

const initQuery = () => ({
  moduleKey: '',
});

const modules = ref<ImportModuleDto[]>([]);
const tableData = ref<ImportTaskDto[]>([]);
const fileInputRef = ref<HTMLInputElement>();
const loading = ref(false);
const creating = ref(false);
const pager = reactive(initPager());
const query = reactive(initQuery());

const selectedModule = computed(() => {
  return modules.value.find((item) => item.moduleKey === query.moduleKey);
});

const fetchModules = async () => {
  const res = await getImportModulesApi();
  modules.value = res.items ?? [];
  if (!query.moduleKey && modules.value.length > 0) {
    query.moduleKey = modules.value[0]!.moduleKey;
  }
};

const search = async () => {
  loading.value = true;
  try {
    const res = await getImportTaskListApi({
      maxResultCount: pager.maxResultCount,
      moduleKey: query.moduleKey,
      skipCount: (pager.currentPage - 1) * pager.maxResultCount,
    });
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
  Object.assign(query, initQuery());
  if (modules.value.length > 0) {
    query.moduleKey = modules.value[0]!.moduleKey;
  }
  search();
};

const formatDateTime = (value?: null | string) => {
  if (!value) return '-';
  return value.replace('T', ' ').slice(0, 19);
};

const formatFileSize = (size: number) => {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
  return `${(size / 1024 / 1024).toFixed(1)} MB`;
};

const onDownloadTemplate = async () => {
  if (!selectedModule.value) {
    ElMessage.warning('请选择导入模块');
    return;
  }
  await downloadImportTemplateApi(selectedModule.value);
};

const onChooseFile = () => {
  if (!selectedModule.value) {
    ElMessage.warning('请选择导入模块');
    return;
  }
  if (fileInputRef.value) {
    fileInputRef.value.value = '';
    fileInputRef.value.click();
  }
};

const onFileSelected = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file || !selectedModule.value) return;

  if (!file.name.toLowerCase().endsWith('.xlsx')) {
    ElMessage.warning('仅支持 xlsx 文件');
    return;
  }

  creating.value = true;
  try {
    await createImportTaskApi(selectedModule.value.moduleKey, file);
    ElMessage.success('导入任务已创建');
    await search();
  } finally {
    creating.value = false;
  }
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

onMounted(async () => {
  await fetchModules();
  await search();
});
</script>

<template>
  <MyContainer :show-footer="true" :show-header="true">
    <input
      ref="fileInputRef"
      accept=".xlsx"
      class="hidden"
      type="file"
      @change="onFileSelected"
    />

    <template #header>
      <ElForm
        :inline="true"
        :model="query"
        class="demo-form-inline no-action-align ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="导入模块">
          <ElSelect
            v-model="query.moduleKey"
            placeholder="请选择导入模块"
            style="width: 220px"
            @change="onSearch"
          >
            <ElOption
              v-for="item in modules"
              :key="item.moduleKey"
              :label="item.moduleName"
              :value="item.moduleKey"
            />
          </ElSelect>
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
            :icon="MdiDownload"
            plain
            type="primary"
            @click="onDownloadTemplate"
          >
            下载模板
          </ElButton>
          <ElButton
            :loading="creating"
            plain
            type="primary"
            @click="onChooseFile"
          >
            上传 xlsx
          </ElButton>
        </div>
        <span class="text-sm text-gray-500">第一版支持：字典数据导入</span>
      </div>
    </template>

    <ElTable
      v-loading="loading"
      :data="tableData"
      class="w-full"
      height="100%"
      row-key="id"
    >
      <ElTableColumn
        fixed
        label="文件名"
        min-width="240"
        prop="originalFileName"
      />
      <ElTableColumn label="模块" min-width="120" prop="moduleName" />
      <ElTableColumn label="状态" width="110">
        <template #default="{ row }">
          <ElTag :type="statusMeta[row.status as ImportTaskStatus].type">
            {{ statusMeta[row.status as ImportTaskStatus].label }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn
        align="center"
        label="总行数"
        prop="totalCount"
        width="90"
      />
      <ElTableColumn
        align="center"
        label="成功"
        prop="successCount"
        width="90"
      />
      <ElTableColumn
        align="center"
        label="失败"
        prop="failedCount"
        width="90"
      />
      <ElTableColumn label="大小" width="110">
        <template #default="{ row }">
          {{ formatFileSize(row.fileSize) }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="创建时间" width="170">
        <template #default="{ row }">
          {{ formatDateTime(row.creationTime) }}
        </template>
      </ElTableColumn>
      <ElTableColumn
        label="错误信息"
        min-width="220"
        prop="errorMessage"
        show-overflow-tooltip
      />
      <ElTableColumn align="center" fixed="right" label="操作" width="130">
        <template #default="{ row }">
          <ElButton
            :disabled="!row.canDownloadErrorReport"
            size="small"
            text
            type="danger"
            @click="downloadImportErrorReportApi(row)"
          >
            错误报告
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
  </MyContainer>
</template>
