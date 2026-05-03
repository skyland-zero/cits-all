<script lang="ts" setup>
import type {
  AnnouncementDto,
  AnnouncementFormData,
  AnnouncementPriority,
} from '#/api/system/announcements';

import { onMounted, reactive, ref } from 'vue';

import {
  CircleX as MdiDelete,
  Eye as MdiEye,
  Plus as MdiAdd,
  RotateCw as MdiReset,
  Search as MdiSearch,
  UserRoundPen as MdiEdit,
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
  ElSwitch,
  ElTable,
  ElTableColumn,
  ElTag,
  ElTreeSelect,
  type FormInstance,
  type FormRules,
} from 'element-plus';

import {
  createAnnouncement,
  deleteAnnouncement,
  getAnnouncements,
  publishAnnouncement,
  updateAnnouncement,
} from '#/api/system/announcements';
import { getSelectedApi } from '#/api/permission/roles';
import { getTreeApi } from '#/api/permission/organizations';
import { MyContainer, RichHtml, TinymceEditor } from '#/components';

const priorityOptions: Array<{
  label: string;
  type: 'danger' | 'primary' | 'warning';
  value: AnnouncementPriority;
}> = [
  { label: '普通', type: 'primary', value: 0 },
  { label: '重要', type: 'warning', value: 10 },
  { label: '紧急', type: 'danger', value: 20 },
];

const treeSelectProps = {
  children: 'children',
  label: 'name',
  value: 'id',
};

const initPager = () => ({
  currentPage: 1,
  maxResultCount: 20,
  totalCount: 0,
});

const initQuery = () => ({
  isPublished: null as boolean | null,
  keyword: '',
  priority: null as AnnouncementPriority | null,
});

const initForm = (): AnnouncementFormData & { id: string } => ({
  contentHtml: '',
  expireTime: null,
  id: '',
  isPublished: false,
  organizationUnitIds: [],
  popupOnLogin: true,
  priority: 0,
  publishTime: null,
  roleIds: [],
  summary: '',
  title: '',
  visibleToAll: true,
});

const loading = ref(false);
const saveLoading = ref(false);
const tableData = ref<AnnouncementDto[]>([]);
const pager = reactive(initPager());
const queryForm = reactive(initQuery());
const formData = reactive(initForm());
const formRef = ref<FormInstance>();
const dialogVisible = ref(false);
const previewVisible = ref(false);
const isEdit = ref(false);
const previewRow = ref<AnnouncementDto>();
const roleOptions = ref<Array<{ label: string; value: string }>>([]);
const organizationTree = ref<any[]>([]);

const rules: FormRules = {
  contentHtml: [
    { message: '请输入公告正文', required: true, trigger: 'change' },
  ],
  title: [{ message: '请输入公告标题', required: true, trigger: 'blur' }],
};

function getPriorityOption(value: AnnouncementPriority) {
  return (
    priorityOptions.find((item) => item.value === value) ?? priorityOptions[0]!
  );
}

async function fetchOptions() {
  const [roles, organizations] = await Promise.all([
    getSelectedApi(),
    getTreeApi(),
  ]);
  roleOptions.value = roles.items ?? [];
  organizationTree.value = organizations.items ?? [];
}

async function fetchData() {
  loading.value = true;
  try {
    const res = await getAnnouncements(pager, queryForm);
    tableData.value = res.items ?? [];
    pager.totalCount = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}

function handleSearch() {
  pager.currentPage = 1;
  fetchData();
}

function handleReset() {
  Object.assign(queryForm, initQuery());
  Object.assign(pager, initPager());
  fetchData();
}

function resetForm() {
  Object.assign(formData, initForm());
  formRef.value?.clearValidate();
}

function openAdd() {
  resetForm();
  isEdit.value = false;
  dialogVisible.value = true;
}

function openEdit(row: AnnouncementDto) {
  Object.assign(formData, {
    contentHtml: row.contentHtml,
    expireTime: row.expireTime ?? null,
    id: row.id,
    isPublished: row.isPublished,
    organizationUnitIds: row.organizationUnitIds ?? [],
    popupOnLogin: row.popupOnLogin,
    priority: row.priority,
    publishTime: row.publishTime ?? null,
    roleIds: row.roleIds ?? [],
    summary: row.summary ?? '',
    title: row.title,
    visibleToAll: row.visibleToAll,
  });
  isEdit.value = true;
  dialogVisible.value = true;
}

function openPreview(row: AnnouncementDto) {
  previewRow.value = row;
  previewVisible.value = true;
}

async function handleSave() {
  const valid = await formRef.value?.validate();
  if (!valid) return;
  saveLoading.value = true;
  try {
    const data = { ...formData };
    if (data.visibleToAll) {
      data.roleIds = [];
      data.organizationUnitIds = [];
    }

    if (isEdit.value) {
      await updateAnnouncement(formData.id, data);
      ElMessage.success('公告已更新');
    } else {
      await createAnnouncement(data);
      ElMessage.success('公告已创建');
    }
    dialogVisible.value = false;
    await fetchData();
  } finally {
    saveLoading.value = false;
  }
}

async function handleDelete(row: AnnouncementDto) {
  await ElMessageBox.confirm(`确认删除公告“${row.title}”？`, '提示', {
    type: 'warning',
  });
  await deleteAnnouncement(row.id);
  ElMessage.success('公告已删除');
  await fetchData();
}

async function handleTogglePublish(row: AnnouncementDto) {
  await publishAnnouncement(row.id, !row.isPublished);
  ElMessage.success(row.isPublished ? '公告已下线' : '公告已发布');
  await fetchData();
}

onMounted(async () => {
  await fetchOptions();
  await fetchData();
});
</script>

<template>
  <MyContainer :show-header="true">
    <template #header>
      <ElForm :inline="true" :model="queryForm" class="m-[18px] mb-0">
        <ElFormItem label="关键词">
          <ElInput
            v-model="queryForm.keyword"
            clearable
            placeholder="标题/摘要"
            @keyup.enter="handleSearch"
          />
        </ElFormItem>
        <ElFormItem label="状态">
          <ElSelect
            v-model="queryForm.isPublished"
            clearable
            placeholder="全部"
            style="width: 120px"
          >
            <ElOption label="已发布" :value="true" />
            <ElOption label="草稿" :value="false" />
          </ElSelect>
        </ElFormItem>
        <ElFormItem label="级别">
          <ElSelect
            v-model="queryForm.priority"
            clearable
            placeholder="全部"
            style="width: 120px"
          >
            <ElOption
              v-for="item in priorityOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </ElSelect>
        </ElFormItem>
        <ElFormItem>
          <ElButton :icon="MdiSearch" type="primary" @click="handleSearch"
            >查询</ElButton
          >
          <ElButton :icon="MdiReset" @click="handleReset">重置</ElButton>
        </ElFormItem>
      </ElForm>
    </template>

    <template #table-header>
      <div class="px-2">
        <ElButton
          :icon="MdiAdd"
          plain
          size="small"
          type="primary"
          @click="openAdd"
          >新增公告</ElButton
        >
      </div>
    </template>

    <ElTable v-loading="loading" :data="tableData" height="100%" row-key="id">
      <ElTableColumn
        label="标题"
        min-width="220"
        prop="title"
        show-overflow-tooltip
      />
      <ElTableColumn label="级别" width="90">
        <template #default="{ row }">
          <ElTag :type="getPriorityOption(row.priority).type">{{
            getPriorityOption(row.priority).label
          }}</ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="状态" width="90">
        <template #default="{ row }">
          <ElTag :type="row.isPublished ? 'success' : 'info'">{{
            row.isPublished ? '已发布' : '草稿'
          }}</ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="登录弹窗" width="100">
        <template #default="{ row }">
          <ElTag :type="row.popupOnLogin ? 'warning' : 'info'">{{
            row.popupOnLogin ? '开启' : '关闭'
          }}</ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn label="可见范围" width="120">
        <template #default="{ row }">
          {{ row.visibleToAll ? '全员' : '指定范围' }}
        </template>
      </ElTableColumn>
      <ElTableColumn label="发布时间" min-width="160" prop="publishTime" />
      <ElTableColumn fixed="right" label="操作" width="280">
        <template #default="{ row }">
          <ElButton :icon="MdiEye" link type="primary" @click="openPreview(row)"
            >预览</ElButton
          >
          <ElButton :icon="MdiEdit" link type="primary" @click="openEdit(row)"
            >编辑</ElButton
          >
          <ElButton link type="primary" @click="handleTogglePublish(row)">{{
            row.isPublished ? '下线' : '发布'
          }}</ElButton>
          <ElButton
            :icon="MdiDelete"
            link
            type="danger"
            @click="handleDelete(row)"
            >删除</ElButton
          >
        </template>
      </ElTableColumn>
    </ElTable>

    <template #footer>
      <ElPagination
        v-model:current-page="pager.currentPage"
        v-model:page-size="pager.maxResultCount"
        :total="pager.totalCount"
        background
        layout="total, sizes, prev, pager, next, jumper"
        @change="fetchData"
      />
    </template>

    <ElDialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑公告' : '新增公告'"
      width="980px"
    >
      <ElForm
        ref="formRef"
        label-width="100px"
        :model="formData"
        :rules="rules"
      >
        <ElFormItem label="公告标题" prop="title">
          <ElInput
            v-model="formData.title"
            maxlength="200"
            placeholder="请输入公告标题"
            show-word-limit
          />
        </ElFormItem>
        <ElFormItem label="公告摘要" prop="summary">
          <ElInput
            v-model="formData.summary"
            maxlength="500"
            placeholder="请输入摘要"
            show-word-limit
            type="textarea"
          />
        </ElFormItem>
        <div class="grid grid-cols-2 gap-x-4">
          <ElFormItem label="公告级别" prop="priority">
            <ElSelect v-model="formData.priority" class="w-full">
              <ElOption
                v-for="item in priorityOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </ElSelect>
          </ElFormItem>
          <ElFormItem label="发布状态" prop="isPublished">
            <ElSwitch
              v-model="formData.isPublished"
              active-text="发布"
              inactive-text="草稿"
              inline-prompt
            />
          </ElFormItem>
          <ElFormItem label="发布时间" prop="publishTime">
            <ElDatePicker
              v-model="formData.publishTime"
              class="w-full"
              placeholder="立即发布可不填"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
          </ElFormItem>
          <ElFormItem label="过期时间" prop="expireTime">
            <ElDatePicker
              v-model="formData.expireTime"
              class="w-full"
              placeholder="长期有效可不填"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm:ss"
            />
          </ElFormItem>
          <ElFormItem label="登录弹窗" prop="popupOnLogin">
            <ElSwitch
              v-model="formData.popupOnLogin"
              active-text="开启"
              inactive-text="关闭"
              inline-prompt
            />
          </ElFormItem>
          <ElFormItem label="全员可见" prop="visibleToAll">
            <ElSwitch
              v-model="formData.visibleToAll"
              active-text="是"
              inactive-text="否"
              inline-prompt
            />
          </ElFormItem>
        </div>
        <template v-if="!formData.visibleToAll">
          <ElFormItem label="可见角色" prop="roleIds">
            <ElSelect
              v-model="formData.roleIds"
              class="w-full"
              clearable
              collapse-tags
              collapse-tags-tooltip
              multiple
              placeholder="请选择角色"
            >
              <ElOption
                v-for="item in roleOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </ElSelect>
          </ElFormItem>
          <ElFormItem label="可见部门" prop="organizationUnitIds">
            <ElTreeSelect
              v-model="formData.organizationUnitIds"
              check-strictly
              class="w-full"
              clearable
              collapse-tags
              collapse-tags-tooltip
              :data="organizationTree"
              multiple
              :props="treeSelectProps"
              :render-after-expand="false"
            />
          </ElFormItem>
        </template>
        <ElFormItem label="公告正文" prop="contentHtml">
          <TinymceEditor
            v-model="formData.contentHtml"
            class="w-full"
            :height="520"
            placeholder="可直接从 Word 粘贴公告内容"
          />
        </ElFormItem>
      </ElForm>
      <template #footer>
        <ElButton @click="dialogVisible = false">取消</ElButton>
        <ElButton :loading="saveLoading" type="primary" @click="handleSave"
          >保存</ElButton
        >
      </template>
    </ElDialog>

    <ElDialog
      v-model="previewVisible"
      :title="previewRow?.title || '公告预览'"
      width="860px"
    >
      <div v-if="previewRow" class="space-y-4">
        <div class="flex items-center gap-2 text-sm text-gray-500">
          <ElTag :type="getPriorityOption(previewRow.priority).type">{{
            getPriorityOption(previewRow.priority).label
          }}</ElTag>
          <span>{{ previewRow.publishTime || previewRow.creationTime }}</span>
        </div>
        <div
          v-if="previewRow.summary"
          class="rounded bg-gray-50 p-3 text-sm text-gray-600 dark:bg-gray-800"
        >
          {{ previewRow.summary }}
        </div>
        <RichHtml :html="previewRow.contentHtml" />
      </div>
    </ElDialog>
  </MyContainer>
</template>
