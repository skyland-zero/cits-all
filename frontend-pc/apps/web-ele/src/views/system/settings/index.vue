<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';

import {
  CircleX as MdiDelete,
  Plus as MdiAdd,
  RotateCw as MdiReset,
  Search as MdiSearch,
  UserRoundPen as MdiEdit,
} from '@vben/icons';

import {
  ElButton,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElInputNumber,
  ElMessage,
  ElMessageBox,
  ElOption,
  ElPagination,
  ElSelect,
  ElSwitch,
  ElTable,
  ElTableColumn,
  ElTag,
} from 'element-plus';

import {
  createSystemSetting,
  deleteSystemSetting,
  getSystemSettingGroups,
  getSystemSettings,
  type SystemSettingDto,
  type SystemSettingFormData,
  type SystemSettingGroupDto,
  type SystemSettingValueType,
  updateSystemSetting,
} from '#/api/system/settings';
import { MyContainer } from '#/components';

const valueTypeOptions: Array<{
  label: string;
  value: SystemSettingValueType;
}> = [
  { label: '字符串', value: 'String' },
  { label: '数字', value: 'Number' },
  { label: '布尔值', value: 'Boolean' },
  { label: 'JSON', value: 'Json' },
];

const initQueryForm = () => ({
  currentPage: 1,
  group: '',
  keyword: '',
  maxResultCount: 20,
});

const initFormData = (): SystemSettingFormData & { id: string } => ({
  description: '',
  group: 'Basic',
  id: '',
  isEncrypted: false,
  isReadonly: false,
  key: '',
  name: '',
  sort: 0,
  value: '',
  valueType: 'String',
});

const tableData = ref<SystemSettingDto[]>([]);
const groupOptions = ref<SystemSettingGroupDto[]>([]);
const loading = ref(false);
const total = ref(0);
const dialogVisible = ref(false);
const dialogTitle = ref('');
const formLoading = ref(false);
const isEdit = ref(false);
const formRef = ref();
const queryForm = reactive(initQueryForm());
const formData = reactive(initFormData());

const rules = {
  group: [{ required: true, message: '请选择分组', trigger: 'change' }],
  key: [{ required: true, message: '请输入参数键名', trigger: 'blur' }],
  name: [{ required: true, message: '请输入参数名称', trigger: 'blur' }],
  valueType: [{ required: true, message: '请选择值类型', trigger: 'change' }],
};

const selectedGroupLabel = computed(() => {
  if (!queryForm.group) return '全部配置';
  return (
    groupOptions.value.find((item) => item.value === queryForm.group)?.label ??
    queryForm.group
  );
});

const getGroupLabel = (group: string) => {
  return (
    groupOptions.value.find((item) => item.value === group)?.label ?? group
  );
};

const getValueTypeLabel = (valueType: string) => {
  return (
    valueTypeOptions.find((item) => item.value === valueType)?.label ??
    valueType
  );
};

const normalizeValueByType = () => {
  if (formData.valueType === 'Boolean') {
    formData.value = formData.value === 'true' ? 'true' : 'false';
    return;
  }

  if (formData.value === null || formData.value === undefined) {
    formData.value = '';
  }
};

const fetchGroups = async () => {
  groupOptions.value = await getSystemSettingGroups();
};

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getSystemSettings(
      {
        currentPage: queryForm.currentPage,
        maxResultCount: queryForm.maxResultCount,
      },
      {
        group: queryForm.group,
        keyword: queryForm.keyword,
      },
    );
    tableData.value = res.items ?? [];
    total.value = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  queryForm.currentPage = 1;
  fetchData();
};

const handleReset = () => {
  Object.assign(queryForm, initQueryForm());
  fetchData();
};

const resetFormData = () => {
  Object.assign(formData, initFormData());
};

const handleAdd = () => {
  resetFormData();
  if (queryForm.group) {
    formData.group = queryForm.group;
  }
  isEdit.value = false;
  dialogTitle.value = '新增系统参数';
  dialogVisible.value = true;
};

const handleEdit = (row: SystemSettingDto) => {
  Object.assign(formData, {
    description: row.description ?? '',
    group: row.group,
    id: row.id,
    isEncrypted: row.isEncrypted,
    isReadonly: row.isReadonly,
    key: row.key,
    name: row.name,
    sort: row.sort,
    value: row.value ?? '',
    valueType: row.valueType,
  });
  normalizeValueByType();
  isEdit.value = true;
  dialogTitle.value = '编辑系统参数';
  dialogVisible.value = true;
};

const handleDelete = (row: SystemSettingDto) => {
  ElMessageBox.confirm(`确定要删除系统参数 [${row.name}] 吗？`, '提示', {
    type: 'warning',
  })
    .then(async () => {
      await deleteSystemSetting(row.id);
      ElMessage.success('删除成功');
      fetchData();
    })
    .catch(() => {});
};

const submitForm = () => {
  formRef.value.validate(async (valid: boolean) => {
    if (!valid) return;
    normalizeValueByType();

    formLoading.value = true;
    try {
      const payload: SystemSettingFormData = {
        description: formData.description,
        group: formData.group,
        isEncrypted: formData.isEncrypted,
        isReadonly: formData.isReadonly,
        key: formData.key,
        name: formData.name,
        sort: formData.sort,
        value: formData.value,
        valueType: formData.valueType,
      };

      if (isEdit.value) {
        await updateSystemSetting(formData.id, payload);
        ElMessage.success('修改成功');
      } else {
        await createSystemSetting(payload);
        ElMessage.success('新增成功');
      }

      dialogVisible.value = false;
      fetchData();
    } finally {
      formLoading.value = false;
    }
  });
};

const handleSizeChange = (val: number) => {
  queryForm.maxResultCount = val;
  fetchData();
};

const handleCurrentChange = (val: number) => {
  queryForm.currentPage = val;
  fetchData();
};

onMounted(async () => {
  await fetchGroups();
  await fetchData();
});
</script>

<template>
  <MyContainer :show-footer="true" :show-header="true">
    <template #header>
      <ElForm
        :inline="true"
        :model="queryForm"
        class="demo-form-inline no-action-align ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="配置分组">
          <ElSelect
            v-model="queryForm.group"
            clearable
            placeholder="全部分组"
            style="width: 180px"
            @change="handleSearch"
          >
            <ElOption
              v-for="item in groupOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </ElSelect>
        </ElFormItem>
        <ElFormItem label="关键字">
          <ElInput
            v-model="queryForm.keyword"
            clearable
            placeholder="键名/名称/说明"
            style="width: 240px"
            @keyup.enter="handleSearch"
          />
        </ElFormItem>
      </ElForm>
    </template>

    <template #table-header>
      <div class="flex w-full items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <ElButton :icon="MdiSearch" type="primary" @click="handleSearch">
            查询
          </ElButton>
          <ElButton :icon="MdiReset" @click="handleReset">重置</ElButton>
          <div class="mx-1 h-4 border-l border-gray-200"></div>
          <ElButton :icon="MdiAdd" plain type="primary" @click="handleAdd">
            新增参数
          </ElButton>
        </div>
        <span class="text-sm text-gray-500">{{ selectedGroupLabel }}</span>
      </div>
    </template>

    <ElTable
      v-loading="loading"
      :data="tableData"
      class="w-full"
      height="100%"
      row-key="id"
    >
      <ElTableColumn fixed label="参数键名" min-width="240" prop="key" />
      <ElTableColumn label="参数名称" min-width="160" prop="name" />
      <ElTableColumn label="分组" width="120">
        <template #default="{ row }">
          <ElTag>{{ getGroupLabel(row.group) }}</ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="类型" width="100">
        <template #default="{ row }">
          {{ getValueTypeLabel(row.valueType) }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="参数值" min-width="220" show-overflow-tooltip>
        <template #default="{ row }">
          <span v-if="row.isEncrypted">******</span>
          <span v-else>{{ row.value }}</span>
        </template>
      </ElTableColumn>
      <ElTableColumn label="说明" min-width="220" prop="description" />
      <ElTableColumn align="center" label="只读" prop="isReadonly" width="80">
        <template #default="{ row }">
          <ElTag :type="row.isReadonly ? 'warning' : 'info'">
            {{ row.isReadonly ? '是' : '否' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" label="排序" prop="sort" width="80" />
      <ElTableColumn align="center" fixed="right" label="操作" width="170">
        <template #default="{ row }">
          <ElButton
            :icon="MdiEdit"
            size="small"
            text
            type="primary"
            @click="handleEdit(row)"
          >
            编辑
          </ElButton>
          <ElButton
            :disabled="row.isReadonly"
            :icon="MdiDelete"
            size="small"
            text
            type="danger"
            @click="handleDelete(row)"
          >
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>

    <template #table-footer>
      <div class="flex w-full items-center justify-between">
        <span class="text-sm text-gray-500">共 {{ total }} 条</span>
        <ElPagination
          v-model:current-page="queryForm.currentPage"
          v-model:page-size="queryForm.maxResultCount"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="sizes, prev, pager, next, jumper"
          @current-change="handleCurrentChange"
          @size-change="handleSizeChange"
        />
      </div>
    </template>

    <template #dialog>
      <ElDialog
        v-model="dialogVisible"
        :close-on-click-modal="false"
        :show-close="false"
        :title="dialogTitle"
        destroy-on-close
        draggable
        lock-scroll
        width="640"
      >
        <ElForm
          ref="formRef"
          :model="formData"
          :rules="rules"
          label-width="100px"
        >
          <ElFormItem label="参数键名" prop="key">
            <ElInput
              v-model="formData.key"
              :disabled="isEdit"
              placeholder="如：security.password.minLength"
            />
          </ElFormItem>
          <ElFormItem label="参数名称" prop="name">
            <ElInput v-model="formData.name" placeholder="请输入参数名称" />
          </ElFormItem>
          <ElFormItem label="配置分组" prop="group">
            <ElSelect v-model="formData.group" class="w-full">
              <ElOption
                v-for="item in groupOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </ElSelect>
          </ElFormItem>
          <ElFormItem label="值类型" prop="valueType">
            <ElSelect
              v-model="formData.valueType"
              class="w-full"
              @change="normalizeValueByType"
            >
              <ElOption
                v-for="item in valueTypeOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </ElSelect>
          </ElFormItem>
          <ElFormItem label="参数值" prop="value">
            <ElSelect
              v-if="formData.valueType === 'Boolean'"
              v-model="formData.value"
              class="w-full"
            >
              <ElOption label="true" value="true" />
              <ElOption label="false" value="false" />
            </ElSelect>
            <ElInput
              v-else-if="formData.valueType === 'Json'"
              v-model="formData.value"
              :rows="5"
              placeholder="请输入合法 JSON"
              type="textarea"
            />
            <ElInput
              v-else
              v-model="formData.value"
              placeholder="请输入参数值"
            />
          </ElFormItem>
          <ElFormItem label="参数说明" prop="description">
            <ElInput
              v-model="formData.description"
              :rows="3"
              placeholder="请输入参数说明"
              type="textarea"
            />
          </ElFormItem>
          <div class="grid grid-cols-3 gap-3">
            <ElFormItem label="敏感配置">
              <ElSwitch v-model="formData.isEncrypted" />
            </ElFormItem>
            <ElFormItem label="只读">
              <ElSwitch v-model="formData.isReadonly" />
            </ElFormItem>
            <ElFormItem label="排序">
              <ElInputNumber v-model="formData.sort" :min="0" />
            </ElFormItem>
          </div>
        </ElForm>
        <template #footer>
          <div class="dialog-footer">
            <ElButton :loading="formLoading" @click="dialogVisible = false">
              关闭
            </ElButton>
            <ElButton :loading="formLoading" type="primary" @click="submitForm">
              保存
            </ElButton>
          </div>
        </template>
      </ElDialog>
    </template>
  </MyContainer>
</template>
