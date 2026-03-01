<!-- eslint-disable no-console -->
<script lang="ts" setup>
import { onMounted, reactive, ref, watch } from 'vue';

import { MdiAdd, MdiDelete, MdiEdit, MdiViewDashboardEdit } from '@vben/icons';

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
  type FormInstance,
} from 'element-plus';

import {
  addApi,
  deleteApi,
  editApi,
  pageApi,
  setMenusApi,
} from '#/api/permission/roles';
import { MyContainer } from '#/components';

import EditMenus from './components/edit-menus.vue';
import Write from './components/write.vue';
import { actionEnum } from './enums';

const tableData = ref<any>([]);
const currentRow = ref<any | null>(null);
const searchFormRef = ref<FormInstance>();
const dialogFormVisible = ref(false);
const actionType = ref(actionEnum.none);
const dialogTitle = ref('');
const writeRef = ref<InstanceType<typeof Write>>();
const setMenusRef = ref<InstanceType<typeof EditMenus>>();
const saveLoading = ref(false);
const loading = ref(false);
const activeRowId = ref(null);

const initFormSearchData = () => ({
  name: null,
  code: null,
  isDefault: null,
  isStatic: null,
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
  const res = await pageApi(pager, formSearchData);
  tableData.value = res.items;
  pager.totalCount = res.totalCount;
  loading.value = false;
};

/**
 * 重置
 */
const onReset = () => {
  // eslint-disable-next-line no-console
  console.log('reset!');
  Object.assign(pager, initPager()); // 重置分页
  Object.assign(formSearchData, initFormSearchData()); // 重置搜索条件
  search();
};

/**
 * 点击新建
 */
const onAdd = (parentId?: string) => {
  console.log('on add');
  actionType.value = actionEnum.add;
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
  actionType.value = actionEnum.edit;
  dialogTitle.value = '编辑';
  currentRow.value = row;
  dialogFormVisible.value = true;
};

const onSetMenus = (row: any) => {
  actionType.value = actionEnum.editMenus;
  dialogTitle.value = '菜单配置';
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
    case actionEnum.editMenus: {
      saveLoading.value = true;
      const menus = setMenusRef.value;
      if (menus) {
        const data = await menus.submit();
        if (data) {
          setMenusApi(currentRow.value.id, data)
            .then(() => {
              ElMessage({
                message: '保存成功',
                type: 'success',
              });
              dialogFormVisible.value = false;
            })
            .catch(() => {})
            .finally(() => {
              saveLoading.value = false;
            });
        }
      }

      break;
    }
  }
};

const close = () => {
  console.log('close');
  dialogFormVisible.value = false;
};

// 鼠标进入行时触发
const handleMouseEnter = (row: any) => {
  activeRowId.value = row.id;
};

// 鼠标离开行时触发
const handleMouseLeave = () => {
  activeRowId.value = null;
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
  <MyContainer :show-footer="true" :show-header="true">
    <template #header>
      <ElForm
        ref="searchFormRef"
        :inline="true"
        :model="formSearchData"
        class="demo-form-inline ml-[18px] mr-[18px] mt-[18px]"
      >
        <ElFormItem label="角色名称">
          <ElInput
            v-model="formSearchData.name"
            clearable
            placeholder="请输入名称"
            style="width: 200px"
          />
        </ElFormItem>
        <ElFormItem label="角色编码">
          <ElInput
            v-model="formSearchData.code"
            clearable
            placeholder="请输入编码"
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
      @cell-mouse-enter="handleMouseEnter"
      @cell-mouse-leave="handleMouseLeave"
    >
      <ElTableColumn fixed label="角色名称" prop="name" />
      <ElTableColumn label="角色编码" prop="code" />
      <ElTableColumn label="说明" prop="description" />
      <ElTableColumn align="center" class="operation" label="" width="300">
        <template #default="scope">
          <Transition>
            <div v-show="activeRowId === scope.row.id" class="action-buttons">
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
                :icon="MdiViewDashboardEdit"
                size="small"
                text
                type="primary"
                @click="onSetMenus(scope.row)"
              >
                设置权限
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
            </div>
          </Transition>
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
        <EditMenus
          v-if="actionType === actionEnum.editMenus"
          ref="setMenusRef"
          :current-row="currentRow"
        />
        <span v-if="actionType === actionEnum.delete">
          确定要删除角色【{{ currentRow.name }}】?
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
