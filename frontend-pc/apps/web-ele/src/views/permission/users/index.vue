<!-- eslint-disable no-console -->
<script lang="ts" setup>
import { onMounted, reactive, ref, watch } from 'vue';

import { MdiAdd, MdiDelete, MdiEdit } from '@vben/icons';

import {
  ElButton,
  ElCol,
  ElDialog,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElPagination,
  ElRow,
  ElTable,
  ElTableColumn,
  ElTag,
  type FormInstance,
} from 'element-plus';

import { addApi, deleteApi, editApi, pageApi } from '#/api/permission/users';
import { MyContainer } from '#/components';

import UnitTree from './components/unit-tree.vue';
import Write from './components/write.vue';
import { actionEnum } from './enums';

const tableData = ref<any>([]);
const currentRow = ref<any | null>(null);
const searchFormRef = ref<FormInstance>();
const dialogFormVisible = ref(false);
const actionType = ref(actionEnum.none);
const dialogTitle = ref('');
const writeRef = ref<InstanceType<typeof Write>>();
const unitTreeRef = ref<InstanceType<typeof UnitTree>>();
const saveLoading = ref(false);
const loading = ref(false);
const organizationUnitId = ref('');

const initFormSearchData = () => ({
  userName: null,
  surname: null,
});
const initPager = () => ({
  totalCount: 0,
  maxResultCount: 20,
  currentPage: 1,
});
const formSearchData = reactive(initFormSearchData());
const pager = reactive(initPager());

/**
 * 搜索
 */
const search = async () => {
  loading.value = true;
  console.log('formSearchData', formSearchData);
  const unit = { organizationUnitId: organizationUnitId.value };
  const formData = { ...formSearchData, ...unit };
  const res = await pageApi(pager, formData);
  tableData.value = res.items;
  pager.totalCount = res.totalCount;
  loading.value = false;
};

/**
 * 重置
 */
const onReset = async () => {
  // eslint-disable-next-line no-console
  console.log('reset!');
  Object.assign(pager, initPager()); // 重置分页
  Object.assign(formSearchData, initFormSearchData()); // 重置搜索条件
  await unitTreeRef.value?.onReset();
  search();
};

/**
 * 点击新建
 */
const onAdd = () => {
  console.log('on add');
  actionType.value = actionEnum.add;
  dialogTitle.value = '新建';
  currentRow.value = null;
  if (organizationUnitId.value) {
    currentRow.value = { organizationUnitId };
  }
  dialogFormVisible.value = true;
};
/**
 * 点击编辑
 * @param row
 */
const onEdit = (row: any) => {
  console.log('on edit', row);
  actionType.value = actionEnum.edit;
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
  actionType.value = actionEnum.delete;
  dialogTitle.value = '提示';
  currentRow.value = row;
  dialogFormVisible.value = true;
};

/**
 * 保存数据
 */
const save = async () => {
  switch (actionType.value) {
    case actionEnum.add:
    case actionEnum.edit: {
      const write = writeRef.value;
      const formData = await write?.submit();
      if (formData) {
        saveLoading.value = true;
        console.log(formData);
        const api =
          actionType.value === actionEnum.add
            ? addApi(formData)
            : editApi(currentRow.value.id, formData);
        api
          .then(() => {
            ElMessage({
              message: '保存成功',
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

      break;
    }
    case actionEnum.delete: {
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

      break;
    }
  }
};

const close = () => {
  console.log('close');
  dialogFormVisible.value = false;
};

const unitCurrentChange = (data: any) => {
  console.log('unitCurrentChange', data);
  organizationUnitId.value = data?.id;
  search();
};

// 深度监听对象变化
watch(
  () => ({
    page: pager.currentPage,
    size: pager.maxResultCount,
  }),
  ({ page, size }) => {
    console.log('分页参数变化:', { page, size });
    search();
  },
);

onMounted(() => {
  search();
});
</script>

<template>
  <MyContainer :show-aside="true" :show-footer="true" :show-left-aside="true">
    <template #left-aside>
      <UnitTree ref="unitTreeRef" @current-change="unitCurrentChange" />
    </template>
    <template #header>
      <ElForm
        ref="searchFormRef"
        :inline="true"
        :model="formSearchData"
        class="demo-form-inline ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="账号">
          <ElInput
            v-model="formSearchData.userName"
            clearable
            placeholder="请输入账号"
            style="width: 200px"
          />
        </ElFormItem>
        <ElFormItem label="姓名">
          <ElInput
            v-model="formSearchData.surname"
            clearable
            placeholder="请输入姓名"
            style="width: 200px"
          />
        </ElFormItem>
        <ElFormItem>
          <ElButton type="primary" @click="search">查询</ElButton>
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
      class="w-full"
      height="100%"
      row-key="id"
      v-loading="loading"
    >
      <ElTableColumn fixed label="账号" prop="userName" />
      <ElTableColumn label="姓名" prop="surname" />
      <ElTableColumn align="center" label="启用" prop="isActive" width="70">
        <template #default="scope">
          <ElTag
            :type="scope.row.isActive ? 'primary' : 'danger'"
            disable-transitions
          >
            {{ scope.row.isActive ? '启用' : '禁用' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" class="operation" label="操作" width="300">
        <template #default="scope">
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
    <template #table-footer>
      <ElPagination
        v-model:current-page="pager.currentPage"
        v-model:page-size="pager.maxResultCount"
        :total="pager.totalCount"
        layout="prev, pager, next, jumper, total"
      />
    </template>
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
          v-if="actionType === actionEnum.add || actionType === actionEnum.edit"
          ref="writeRef"
          :current-row="currentRow"
          :tree-select-data="tableData"
        />
        <span v-if="actionType === actionEnum.delete">
          确定要删除用户【{{ currentRow.name }}】?
        </span>
        <template #footer>
          <div class="dialog-footer">
            <ElButton :loading="saveLoading" @click="close">关闭</ElButton>
            <ElButton :loading="saveLoading" type="primary" @click="save">
              保存
            </ElButton>
          </div>
        </template>
      </ElDialog>
    </template>
  </MyContainer>
</template>
