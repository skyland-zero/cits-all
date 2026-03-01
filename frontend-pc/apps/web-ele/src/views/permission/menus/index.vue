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
  ElTag,
  ElText,
  type FormInstance,
} from 'element-plus';

import {
  addApi,
  deleteApi,
  editApi,
  getTreeApi,
  multiAddApi,
} from '#/api/permission/menus';
import { MyContainer } from '#/components';
import { filterTree } from '#/utils';

import AddAuthPoint from './components/add-auth-point.vue';
import EditAuthPoint from './components/edit-auth-point.vue';
import WriteGroup from './components/write-group.vue';
import WriteMenu from './components/write-menu.vue';
import { actionEnum } from './enums';

const tableData = ref<any>([]);
let tempTableData: any[] = [];
const selectTableData = ref<any>([]);
const currentRow = ref<any | null>(null);
const searchFormRef = ref<FormInstance>();
const dialogFormVisible = ref(false);
const actionType = ref(actionEnum.none);
const dialogTitle = ref('');
const writeGroupRef = ref<InstanceType<typeof WriteGroup>>();
const writeMenuRef = ref<InstanceType<typeof WriteMenu>>();
const addAuthRef = ref<InstanceType<typeof AddAuthPoint>>();
const editAuthRef = ref<InstanceType<typeof EditAuthPoint>>();
const saveLoading = ref(false);
const loading = ref(false);

const formInline = reactive({
  name: '',
});

const filterMenuTree = (menuNodes: any[]) => {
  return menuNodes
    .map((node: any) => {
      // 核心过滤逻辑
      if (node.type !== 1) return null;

      const filteredNode: any = {
        ...node,
        children: node.children ? filterMenuTree(node.children) : [],
      };

      // 可选字段处理
      if (!filteredNode.redirect) delete filteredNode.redirect;
      return filteredNode;
    })
    .filter((node: any) => node !== null) // 过滤无效节点
    .sort((a: any, b: any) => a.order - b.order); // 排序
};

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
const onAdd = (type: actionEnum, parentId?: string) => {
  console.log('on add');
  actionType.value = type;
  dialogTitle.value = `新建${type === actionEnum.addGroup ? '分组' : '菜单'}`;
  currentRow.value = null;
  if (parentId) {
    currentRow.value = { parentId };
  }
  selectTableData.value = filterMenuTree(tempTableData);
  dialogFormVisible.value = true;
};

/**
 * 新建权限点
 * @param parentId 菜单id
 */
const onAddAuth = (row: any) => {
  actionType.value = actionEnum.addPoint;
  dialogTitle.value = `新建权限点`;
  currentRow.value = row;
  dialogFormVisible.value = true;
};

/**
 * 点击编辑
 * @param row
 */
const onEdit = (row: any) => {
  console.log('on edit', row);
  switch (row.type) {
    case 1: {
      actionType.value = actionEnum.editGroup;
      dialogTitle.value = '编辑分组';
      break;
    }
    case 2: {
      actionType.value = actionEnum.editMenu;
      dialogTitle.value = '编辑菜单';
      break;
    }
    case 3: {
      actionType.value = actionEnum.editPoint;
      dialogTitle.value = '编辑权限点';
      break;
    }
  }

  currentRow.value = row;
  console.log(row);
  selectTableData.value = filterMenuTree(tempTableData);
  dialogFormVisible.value = true;
};

/**
 * 删除
 * @param row
 */
const onDelete = (row: any) => {
  console.log('delete', row);
  actionType.value = actionEnum.deleteGroup;
  dialogTitle.value = '提示';
  currentRow.value = row;
  dialogFormVisible.value = true;
};

/**
 * 保存数据
 */
const save = async () => {
  switch (actionType.value) {
    case actionEnum.addGroup:
    case actionEnum.editGroup: {
      const write = writeGroupRef.value;
      const formData = await write?.submit();
      if (formData) {
        saveLoading.value = true;
        const api =
          actionType.value === actionEnum.addGroup
            ? addApi(formData)
            : editApi(currentRow.value.id, formData);
        api
          .then(() => {
            ElMessage({
              message: '保存成功',
              type: 'success',
            });
            dialogFormVisible.value = false;
            onReset();
          })
          .catch(() => {})
          .finally(() => {
            saveLoading.value = false;
          });
      }

      break;
    }
    case actionEnum.addMenu:
    case actionEnum.editMenu: {
      const write = writeMenuRef.value;
      const formData = await write?.submit();
      if (formData) {
        saveLoading.value = true;
        const api =
          actionType.value === actionEnum.addMenu
            ? addApi(formData)
            : editApi(currentRow.value.id, formData);
        api
          .then(() => {
            ElMessage({
              message: '保存成功',
              type: 'success',
            });
            dialogFormVisible.value = false;
            onReset();
          })
          .catch(() => {})
          .finally(() => {
            saveLoading.value = false;
          });
      }

      break;
    }
    case actionEnum.addPoint: {
      const point = addAuthRef.value;
      if (point) {
        const data = await point.submit();
        console.log('addPoint', data);
        await multiAddApi(data)
          .then(() => {
            ElMessage({
              message: '保存成功',
              type: 'success',
            });
            dialogFormVisible.value = false;
            onReset();
          })
          .catch(() => {})
          .finally(() => {
            saveLoading.value = false;
          });
      }
      break;
    }
    case actionEnum.deleteGroup:
    case actionEnum.deleteMenu: {
      saveLoading.value = true;
      deleteApi(currentRow.value.id)
        .then(() => {
          ElMessage({
            message: '删除成功',
            type: 'success',
          });
          dialogFormVisible.value = false;
          onReset();
        })
        .catch(() => {})
        .finally(() => {
          saveLoading.value = false;
        });
      break;
    }
    case actionEnum.editPoint: {
      const write = editAuthRef.value;
      const formData = await write?.submit();
      if (formData) {
        saveLoading.value = true;
        const api = editApi(currentRow.value.id, formData);
        api
          .then(() => {
            ElMessage({
              message: '保存成功',
              type: 'success',
            });
            dialogFormVisible.value = false;
            onReset();
          })
          .catch(() => {})
          .finally(() => {
            saveLoading.value = false;
          });
      }
      break;
    }
    default: {
      break;
    }
    // No default
  }
};

const close = () => {
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
        <ElFormItem label="名称">
          <ElInput
            v-model="formInline.name"
            clearable
            placeholder="请输入名称"
          />
        </ElFormItem>
        <ElFormItem>
          <ElButton type="primary" @click="onSubmit">查询</ElButton>
          <ElButton @click="onReset">重置</ElButton>
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
            @click="onAdd(actionEnum.addGroup)"
          >
            新增分组
          </ElButton>
          <ElButton
            :icon="MdiAdd"
            plain
            size="small"
            type="primary"
            @click="onAdd(actionEnum.addMenu)"
          >
            新增菜单
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
      <ElTableColumn fixed label="名称" prop="name" />
      <ElTableColumn align="center" label="类型" prop="typeStr" width="70" />
      <ElTableColumn label="路由/编码" prop="path">
        <template #default="scope">
          <ElText class="mx-1" type="primary">{{ scope.row.path }}</ElText>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" label="排序" prop="order" width="70" />
      <ElTableColumn align="center" label="启用" prop="enabled" width="70">
        <template #default="scope">
          <ElTag
            :type="scope.row.enabled ? 'primary' : 'danger'"
            disable-transitions
          >
            {{ scope.row.enabled ? '启用' : '禁用' }}
          </ElTag>
        </template>
      </ElTableColumn>
      <ElTableColumn align="center" class="operation" label="操作" width="290">
        <template #default="scope">
          <ElButton
            v-if="scope.row.type === 1"
            :icon="MdiAdd"
            size="small"
            text
            type="primary"
            @click="onAdd(actionEnum.addMenu, scope.row.id)"
          >
            新增菜单
          </ElButton>
          <ElButton
            v-if="scope.row.type === 1"
            :icon="MdiAdd"
            size="small"
            text
            type="primary"
            @click="onAdd(actionEnum.addGroup, scope.row.id)"
          >
            新增分组
          </ElButton>
          <ElButton
            v-if="scope.row.type === 2"
            :icon="MdiAdd"
            size="small"
            text
            type="primary"
            @click="onAddAuth(scope.row)"
          >
            新增权限点
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
        width="700"
      >
        <WriteGroup
          v-if="
            actionType === actionEnum.addGroup ||
            actionType === actionEnum.editGroup
          "
          ref="writeGroupRef"
          :current-row="currentRow"
          :tree-select-data="selectTableData"
        />
        <WriteMenu
          v-if="
            actionType === actionEnum.addMenu ||
            actionType === actionEnum.editMenu
          "
          ref="writeMenuRef"
          :current-row="currentRow"
          :tree-select-data="selectTableData"
        />
        <AddAuthPoint
          v-if="actionType === actionEnum.addPoint"
          ref="addAuthRef"
          :current-row="currentRow"
        />
        <EditAuthPoint
          v-if="actionType === actionEnum.editPoint"
          ref="editAuthRef"
          :current-row="currentRow"
        />
        <span v-if="actionType === actionEnum.deleteGroup">
          确定要删除【{{ currentRow.name }}】?
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
