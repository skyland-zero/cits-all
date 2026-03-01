<!-- eslint-disable no-console -->
<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';

import { MdiAdd, MdiDelete, MdiEdit } from '@vben/icons';

import {
  ElButton,
  ElCol,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElRow,
  ElTable,
  ElTableColumn,
  type FormInstance,
} from 'element-plus';

import {
  addApi,
  deleteApi,
  editApi,
  getTreeApi,
} from '#/api/permission/organizations';
import { MyContainer } from '#/components';
import { filterTree } from '#/utils';

import Write from './components/write.vue';

const tableData = ref<any>([]);
let tempTableData: any[] = [];
const currentRow = ref<any | null>(null);
const searchFormRef = ref<FormInstance>();
const dialogFormVisible = ref(false);
const actionType = ref('');
const dialogTitle = ref('');
const writeRef = ref<InstanceType<typeof Write>>();
const saveLoading = ref(false);
const loading = ref(false);

const formInline = reactive({
  name: '',
});

const fetchData = async () => {
  loading.value = true;
  const res = await getTreeApi();
  tableData.value = res.items;
  tempTableData = res.items;
  loading.value = false;
};

const search = async () => {
  if (formInline.name) {
    loading.value = true;
    const temp = filterTree(tempTableData, formInline.name, 'name');
    tableData.value = temp;
    loading.value = false;
  }
};

const onSubmit = () => {
  // eslint-disable-next-line no-console
  console.log('submit!');
  search();
};

const onReset = () => {
  // eslint-disable-next-line no-console
  console.log('reset!');
  formInline.name = '';
  fetchData();
};

/**
 * 点击新建
 */
const onAdd = (parentId?: string) => {
  console.log('on add');
  actionType.value = 'add';
  dialogTitle.value = '新建';
  currentRow.value = null;
  if (parentId) {
    currentRow.value = { parentId };
  }
  dialogFormVisible.value = true;
};
/**
 * 点击编辑
 * @param row
 */
const onEdit = (row: any) => {
  console.log('on edit', row);
  actionType.value = 'edit';
  dialogTitle.value = '编辑';
  currentRow.value = row;
  dialogFormVisible.value = true;
};

/**
 * 删除
 * @param row
 */
const onDelete = (row: any) => {
  console.log('delete', row);
  actionType.value = 'delete';
  dialogTitle.value = '提示';
  currentRow.value = row;
  dialogFormVisible.value = true;
};

/**
 * 保存数据
 */
const save = async () => {
  if (actionType.value === 'add' || actionType.value === 'edit') {
    const write = writeRef.value;
    const formData = await write?.submit();
    if (formData) {
      saveLoading.value = true;
      console.log(formData);
      const api =
        actionType.value === 'add'
          ? addApi(formData)
          : editApi(currentRow.value.id, formData);
      api
        .then(() => {
          ElMessage({
            message: '新建成功',
            type: 'success',
          });
          dialogFormVisible.value = false;
          search();
        })
        .catch(() => {})
        .finally(() => {
          saveLoading.value = false;
        });
    }
  } else if (actionType.value === 'delete') {
    saveLoading.value = true;
    deleteApi(currentRow.value.id)
      .then(() => {
        ElMessage({
          message: '删除成功',
          type: 'success',
        });
        dialogFormVisible.value = false;
        search();
      })
      .catch(() => {})
      .finally(() => {
        saveLoading.value = false;
      });
  }
};

const close = () => {
  console.log('close');
  dialogFormVisible.value = false;
};

onMounted(() => {
  fetchData();
});
</script>

<template>
  <MyContainer :show-header="true">
    <template #header>
      <ElForm
        ref="searchFormRef"
        :inline="true"
        :model="formInline"
        class="demo-form-inline ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="部门名称">
          <ElInput
            v-model="formInline.name"
            clearable
            placeholder="请输入名称"
          />
        </ElFormItem>
        <ElFormItem>
          <ElButton type="primary" @click="onSubmit">查询</ElButton>
          <ElButton type="" @click="onReset">重置</ElButton>
        </ElFormItem>
      </ElForm>
    </template>
    <template #table-header>
      <ElRow>
        <ElCol :span="12" class="pl-[8px]">
          <ElButton
            :icon="MdiAdd"
            plain
            size="small"
            type="primary"
            @click="onAdd()"
          >
            新增
          </ElButton>
        </ElCol>
        <ElCol :span="12">
          <div class="grid-content ep-bg-purple-light"></div>
        </ElCol>
      </ElRow>
    </template>
    <ElTable
      :data="tableData"
      :default-expand-all="true"
      :tree-props="{ children: 'children' }"
      class="w-full"
      height="100%"
      lazy
      row-key="id"
      v-loading="loading"
    >
      <ElTableColumn fixed label="部门名称" prop="name" />
      <ElTableColumn label="部门编码" prop="code" />
      <ElTableColumn label="说明" prop="description" />
      <ElTableColumn align="center" label="排序" prop="sort" width="70" />
      <ElTableColumn align="center" class="operation" label="操作" width="190">
        <template #default="scope">
          <ElButton
            :icon="MdiAdd"
            size="small"
            text
            type="primary"
            @click="onAdd(scope.row.id)"
          >
            新增
          </ElButton>
          <ElButton
            :icon="MdiEdit"
            size="small"
            text
            type="primary"
            @click="onEdit(scope.row)"
          >
            编辑
          </ElButton>
          <ElButton
            :icon="MdiDelete"
            size="small"
            text
            type="danger"
            @click="onDelete(scope.row)"
          >
            删除
          </ElButton>
        </template>
      </ElTableColumn>
    </ElTable>
    <template #dialog>
      <ElDialog
        v-model="dialogFormVisible"
        :close-on-click-modal="false"
        :show-close="false"
        :title="dialogTitle"
        destroy-on-close
        draggable
        lock-scroll
        width="500"
      >
        <Write
          v-if="actionType === 'add' || actionType === 'edit'"
          ref="writeRef"
          :current-row="currentRow"
          :tree-select-data="tableData"
        />
        <span v-if="actionType === 'delete'">
          确定要删除部门【{{ currentRow.name }}】?
        </span>
        <template #footer>
          <div class="dialog-footer">
            <ElButton :loading="saveLoading" @click="close">关闭</ElButton>
            <ElButton
              v-if="actionType !== 'detail'"
              :loading="saveLoading"
              type="primary"
              @click="save"
            >
              保存
            </ElButton>
          </div>
        </template>
      </ElDialog>
    </template>
  </MyContainer>
</template>
